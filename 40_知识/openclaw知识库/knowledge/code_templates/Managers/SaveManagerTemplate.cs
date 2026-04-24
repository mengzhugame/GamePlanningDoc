// ============================================================
// SaveManagerTemplate.cs
// 文件位置: Assets/Scripts/Managers/SaveManager.cs
// 用途：存档管理器模板 - 本地存储和数据管理
// 使用：复制后修改命名空间和数据结构
// ============================================================

using System;
using UnityEngine;

namespace YourGame.Core
{
    /// <summary>
    /// 玩家数据结构（根据项目需求修改）
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public string playerID;
        public string playerName;
        public int coins;
        public int level;
        public int currentChapter;
        public long lastPlayTime;
        
        // 版本控制，用于数据迁移
        public int dataVersion = 1;
        
        public PlayerData()
        {
            playerID = Guid.NewGuid().ToString();
            playerName = "Player";
            coins = 0;
            level = 1;
            currentChapter = 1;
            lastPlayTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    /// <summary>
    /// 存档管理器
    /// 负责玩家数据的保存、加载、管理
    /// </summary>
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 配置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 自动保存配置 ═══")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 60f;

        [Header("═══ 调试 ═══")]
        [SerializeField] private bool showDebugInfo = false;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 常量
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private const string SAVE_KEY = "PlayerSaveData";
        private const int CURRENT_DATA_VERSION = 1;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 运行时数据
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private PlayerData currentData;
        private float lastSaveTime;
        private bool isDirty = false;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 事件
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public event Action<PlayerData> OnDataLoaded;
        public event Action OnDataSaved;
        public event Action<string> OnSaveError;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共属性
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public PlayerData Data => currentData;
        public bool HasSaveData => PlayerPrefs.HasKey(SAVE_KEY);

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 生命周期
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected override void OnSingletonAwake()
        {
            Load();
        }

        private void Start()
        {
            if (enableAutoSave)
            {
                InvokeRepeating(nameof(AutoSave), autoSaveInterval, autoSaveInterval);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && isDirty)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            if (isDirty)
            {
                Save();
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 保存/加载
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void Save()
        {
            try
            {
                currentData.lastPlayTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string json = JsonUtility.ToJson(currentData);
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();

                isDirty = false;
                lastSaveTime = Time.time;
                OnDataSaved?.Invoke();

                if (showDebugInfo)
                {
                    Debug.Log($"[SaveManager] 数据已保存: {json}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 保存失败: {e.Message}");
                OnSaveError?.Invoke(e.Message);
            }
        }

        public void Load()
        {
            try
            {
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    currentData = JsonUtility.FromJson<PlayerData>(json);

                    // 数据迁移
                    if (currentData.dataVersion < CURRENT_DATA_VERSION)
                    {
                        MigrateData(currentData);
                    }

                    if (showDebugInfo)
                    {
                        Debug.Log($"[SaveManager] 数据已加载: {json}");
                    }
                }
                else
                {
                    // 创建新存档
                    currentData = new PlayerData();
                    Save();

                    if (showDebugInfo)
                    {
                        Debug.Log("[SaveManager] 创建新存档");
                    }
                }

                OnDataLoaded?.Invoke(currentData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 加载失败: {e.Message}，创建新存档");
                currentData = new PlayerData();
                Save();
            }
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            currentData = new PlayerData();
            isDirty = false;

            if (showDebugInfo)
            {
                Debug.Log("[SaveManager] 存档已删除");
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 数据修改
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void AddCoins(int amount)
        {
            currentData.coins += amount;
            MarkDirty();
        }

        public bool SpendCoins(int amount)
        {
            if (currentData.coins >= amount)
            {
                currentData.coins -= amount;
                MarkDirty();
                return true;
            }
            return false;
        }

        public void SetLevel(int level)
        {
            currentData.level = level;
            MarkDirty();
        }

        public void SetChapter(int chapter)
        {
            currentData.currentChapter = chapter;
            MarkDirty();
        }

        private void MarkDirty()
        {
            isDirty = true;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 自动保存
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void AutoSave()
        {
            if (isDirty)
            {
                Save();

                if (showDebugInfo)
                {
                    Debug.Log("[SaveManager] 自动保存完成");
                }
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 数据迁移
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void MigrateData(PlayerData data)
        {
            // 根据版本号进行数据迁移
            // if (data.dataVersion < 2) { ... }

            data.dataVersion = CURRENT_DATA_VERSION;

            if (showDebugInfo)
            {
                Debug.Log($"[SaveManager] 数据迁移完成: v{data.dataVersion}");
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 调试
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
        [ContextMenu("打印存档数据")]
        private void PrintSaveData()
        {
            if (currentData != null)
            {
                Debug.Log($"[SaveManager] 当前存档:\n{JsonUtility.ToJson(currentData, true)}");
            }
        }

        [ContextMenu("删除存档")]
        private void EditorDeleteSave()
        {
            DeleteSave();
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 200, 150));
            GUILayout.Label("=== SaveManager ===");
            GUILayout.Label($"Coins: {currentData?.coins ?? 0}");
            GUILayout.Label($"Level: {currentData?.level ?? 0}");
            GUILayout.Label($"Chapter: {currentData?.currentChapter ?? 0}");
            GUILayout.Label($"Dirty: {isDirty}");

            if (GUILayout.Button("Save Now")) Save();
            if (GUILayout.Button("Add 100 Coins")) AddCoins(100);

            GUILayout.EndArea();
        }
#endif
    }
}
