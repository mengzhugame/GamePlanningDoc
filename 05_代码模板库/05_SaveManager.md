# SaveManager 存档管理器

## 适用场景
微信小游戏或手游项目的玩家数据持久化方案。提供本地缓存（PlayerPrefs）+ 服务器同步双保险，网络断线时自动队列待同步数据，联网后自动续传。

## 核心特性
1. **继承 PersistentSingleton**，跨场景持久化，避免重复创建
2. **本地优先策略**：所有写操作先写本地，再异步同步服务器
3. **时间戳冲突解决**：服务器与本地数据谁更新用谁
4. **IServerDataManager 接口**：方便替换真实服务器（微信云开发/自建后端）与 Mock 测试
5. **自动保存**：可配置间隔（默认5分钟），游戏关键节点强制触发
6. **事件系统**：`OnDataLoaded`、`OnDataSaved`、`OnSaveError`、`OnNetworkStatusChanged`，UI 层监听即可响应

## 架构图

```
SaveManager
  ├── PlayerPrefs（本地缓存，JSON序列化）
  ├── IServerDataManager（接口）
  │     ├── MockServerDataManager（测试/开发用）
  │     └── RealServerDataManager（接入真实后端时替换）
  └── 自动保存 Coroutine（5分钟一次）
```

## 使用方法

```csharp
// 游戏启动时初始化（在 GameManager 的 Start/Awake 里调用）
SaveManager.Instance.InitializePlayerData(playerID, playerName, success =>
{
    if (success) Debug.Log("数据加载完成");
});

// 在关键节点保存（如关卡完成）
SaveManager.Instance.SaveData();

// 监听事件
SaveManager.Instance.OnDataLoaded += (data) => UpdateUI(data);
SaveManager.Instance.OnSaveError  += (err)  => ShowErrorToast(err);

// 获取玩家信息
string id   = SaveManager.Instance.GetPlayerID();
string name = SaveManager.Instance.GetPlayerName();
PlayerGameData data = SaveManager.Instance.CurrentPlayerData;
```

## 代码实现

```csharp
// SaveManager.cs
using System;
using System.Collections;
using UnityEngine;

public class SaveManager : PersistentSingleton<SaveManager>
{
    [Header("服务器配置")]
    [SerializeField] private MockServerDataManager mockServerManager;
    
    [Header("自动保存")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5分钟

    public PlayerGameData CurrentPlayerData { get; private set; }

    private IServerDataManager serverManager;
    private bool hasPendingSync = false;
    private float lastSaveTime = 0f;

    public bool IsOnline { get; private set; }

    // 事件
    public event Action<PlayerGameData> OnDataLoaded;
    public event Action OnDataSaved;
    public event Action<bool> OnNetworkStatusChanged;
    public event Action<string> OnSaveError;

    private const string LOCAL_DATA_KEY = "LocalPlayerData";

    protected override void Awake()
    {
        base.Awake();
        if (mockServerManager == null)
            mockServerManager = gameObject.AddComponent<MockServerDataManager>();
        serverManager = mockServerManager;
        UpdateNetworkStatus();
        StartCoroutine(NetworkStatusCheckLoop());
    }

    private void Start()
    {
        if (enableAutoSave) StartCoroutine(AutoSaveLoop());
    }

    // ===== 初始化 =====

    public void InitializePlayerData(string playerID, string playerName, Action<bool> onComplete = null)
    {
        PlayerGameData local = LoadFromLocal();

        if (local != null && local.playerID == playerID)
        {
            CurrentPlayerData = local;
            OnDataLoaded?.Invoke(CurrentPlayerData);
            if (IsOnline) SyncWithServer(playerID, onComplete);
            else onComplete?.Invoke(true);
        }
        else
        {
            LoadFromServer(playerID, playerName, onComplete);
        }
    }

    // ===== 保存 =====

    /// <summary>
    /// 保存数据（本地 + 服务器）
    /// </summary>
    public void SaveData(bool forceSync = false, Action onSuccess = null, Action<string> onFailure = null)
    {
        if (CurrentPlayerData == null) { onFailure?.Invoke("数据为空"); return; }

        SaveToLocal();

        if (IsOnline || forceSync)
        {
            SaveToServer(onSuccess, onFailure);
        }
        else
        {
            hasPendingSync = true;
            onSuccess?.Invoke();
        }

        lastSaveTime = Time.time;
    }

    private void SaveToLocal()
    {
        try
        {
            PlayerPrefs.SetString(LOCAL_DATA_KEY, CurrentPlayerData.ToJson());
            PlayerPrefs.Save();
        }
        catch (Exception e) { Debug.LogError($"[SaveManager] 本地保存失败: {e.Message}"); }
    }

    private PlayerGameData LoadFromLocal()
    {
        try
        {
            if (PlayerPrefs.HasKey(LOCAL_DATA_KEY))
                return PlayerGameData.FromJson(PlayerPrefs.GetString(LOCAL_DATA_KEY));
        }
        catch (Exception e) { Debug.LogError($"[SaveManager] 本地加载失败: {e.Message}"); }
        return null;
    }

    private void SaveToServer(Action onSuccess, Action<string> onFailure)
    {
        serverManager.SavePlayerData(CurrentPlayerData,
            () => { hasPendingSync = false; OnDataSaved?.Invoke(); onSuccess?.Invoke(); },
            err  => { hasPendingSync = true; OnSaveError?.Invoke(err); onFailure?.Invoke(err); });
    }

    private void LoadFromServer(string playerID, string playerName, Action<bool> onComplete)
    {
        if (!IsOnline) { CreateNewPlayerData(playerID, playerName); onComplete?.Invoke(false); return; }

        serverManager.CheckPlayerExists(playerID, exists =>
        {
            if (exists)
                serverManager.LoadPlayerData(playerID,
                    data  => { CurrentPlayerData = data; SaveToLocal(); OnDataLoaded?.Invoke(data); onComplete?.Invoke(true); },
                    error => { CreateNewPlayerData(playerID, playerName); onComplete?.Invoke(false); });
            else
                serverManager.CreateNewPlayer(playerID, playerName,
                    data  => { CurrentPlayerData = data; SaveToLocal(); OnDataLoaded?.Invoke(data); onComplete?.Invoke(true); },
                    error => { CreateNewPlayerData(playerID, playerName); onComplete?.Invoke(false); });
        });
    }

    private void CreateNewPlayerData(string playerID, string playerName)
    {
        CurrentPlayerData = new PlayerGameData { playerID = playerID, playerName = playerName };
        SaveToLocal();
        OnDataLoaded?.Invoke(CurrentPlayerData);
        hasPendingSync = true;
    }

    private void SyncWithServer(string playerID, Action<bool> onComplete)
    {
        serverManager.LoadPlayerData(playerID,
            serverData =>
            {
                if (serverData.lastSaveTime > CurrentPlayerData.lastSaveTime)
                {
                    CurrentPlayerData = serverData; SaveToLocal(); OnDataLoaded?.Invoke(serverData);
                }
                else if (CurrentPlayerData.lastSaveTime > serverData.lastSaveTime)
                {
                    SaveToServer(null, null);
                }
                onComplete?.Invoke(true);
            },
            error => onComplete?.Invoke(false));
    }

    // ===== 网络管理 =====

    private void UpdateNetworkStatus()
    {
        bool wasOnline = IsOnline;
        IsOnline = serverManager.IsOnline();
        if (wasOnline != IsOnline)
        {
            OnNetworkStatusChanged?.Invoke(IsOnline);
            if (IsOnline && hasPendingSync) SaveToServer(null, null);
        }
    }

    private IEnumerator NetworkStatusCheckLoop()
    {
        while (true) { yield return new WaitForSeconds(5f); UpdateNetworkStatus(); }
    }

    private IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            if (CurrentPlayerData != null && Time.time - lastSaveTime >= autoSaveInterval)
                SaveData();
        }
    }

    // ===== 快捷访问 =====
    public string GetPlayerID()   => CurrentPlayerData?.playerID   ?? "";
    public string GetPlayerName() => CurrentPlayerData?.playerName ?? "未知玩家";
}
```
