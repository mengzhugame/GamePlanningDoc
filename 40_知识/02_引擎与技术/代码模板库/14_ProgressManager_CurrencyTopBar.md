# 局外资源管理 + 顶部资源栏 — ProgressManager + TopAreaController

> **来源工程**：LightVSDecay（光与朽 · `Logic/ProgressManager.cs` + `UI/TopAreaController.cs`）+ BeautyStacking（美妆叠叠乐 · `Core/CurrencyManager.cs` + `Runtime/UI/CurrencyDisplayUI.cs`）双工程合并提取
> **提取日期**：2026-04-29
> **复用价值**：⭐⭐⭐⭐⭐
> **依赖**：[[02_SingletonPattern|Singleton 基类]]、[[11_GameEvents]]、`PlayerPrefs`

---

## 适用场景

任何需要**多种局外资源 + 顶部 HUD 实时刷新**的休闲小游戏：

- **资源种类**：金币 / 钻石 / 体力 / 蓝图（图纸）/ 经验……可扩展
- **特殊机制**：体力自然恢复（隔 N 分钟 +1）+ 离线补算（关游戏 1 小时再开能补回 6 点）+ 看广告换体力 / 金币 / 蓝图
- **UI 刷新**：事件驱动（修改资源 → 触发事件 → 顶部栏 + 各处展示同步刷新），而不是每帧轮询
- **持久化**：所有资源走 `PlayerPrefs` 或 JSON 存档，自动保存

---

## 三层架构

```
ProgressManager（局外资源单例）
  ├── 数据层 — MetaData：gems / goldCoins / energy / blueprints / 章节进度
  ├── 体力系统 — 自然恢复（每帧累计计时器） + 离线补算（启动时根据时间戳算）
  ├── 广告系统 — 三种"看广告换资源"接口（体力/金币/蓝图），各自独立每日上限
  └── 公共事件 — OnGoldCoinsChanged / OnEnergyChanged / OnBlueprintsChanged
        ↓
TopAreaController（顶部栏单例）
  ├── 监听 ProgressManager.OnXxxChanged → 实时刷新 TMP 文本
  ├── 资源点击 → 弹出 TopBarTipsPanel（"体力不足？看广告补 +2"）
  └── 导航按钮 — 主界面显示设置按钮，子面板显示返回按钮
        ↓
TopBarTipsPanel（资源不足提示）
  └── 调用 ProgressManager.WatchAdForXxx() → AdManager → 成功回调 → 自动累加 → 自动刷新
```

---

## 使用方法

### Step 1：MetaData 持久化容器

```csharp
[System.Serializable]
public class MetaData
{
    public int gems;
    public int goldCoins;
    public int energy;
    public int blueprints;

    public int adWatchCountToday;            // 今日广告次数（按类型分多个字段）
    public int adGoldWatchCountToday;
    public int adBlueprintWatchCountToday;
    public string adLastResetDate = "";      // 用于每日重置

    public string lastSaveTimestamp = "";    // 用于离线补算

    public void Load(int maxEnergy)
    {
        gems       = PlayerPrefs.GetInt("Meta_Gems", 0);
        goldCoins  = PlayerPrefs.GetInt("Meta_Gold", 0);
        energy     = PlayerPrefs.GetInt("Meta_Energy", maxEnergy);
        blueprints = PlayerPrefs.GetInt("Meta_BP", 0);
        adWatchCountToday = PlayerPrefs.GetInt("Meta_AdEnergy", 0);
        adGoldWatchCountToday = PlayerPrefs.GetInt("Meta_AdGold", 0);
        adBlueprintWatchCountToday = PlayerPrefs.GetInt("Meta_AdBP", 0);
        adLastResetDate = PlayerPrefs.GetString("Meta_AdLastReset", "");
        lastSaveTimestamp = PlayerPrefs.GetString("Meta_LastSave", "");
    }

    public void Save()
    {
        PlayerPrefs.SetInt("Meta_Gems", gems);
        PlayerPrefs.SetInt("Meta_Gold", goldCoins);
        PlayerPrefs.SetInt("Meta_Energy", energy);
        PlayerPrefs.SetInt("Meta_BP", blueprints);
        PlayerPrefs.SetInt("Meta_AdEnergy", adWatchCountToday);
        PlayerPrefs.SetInt("Meta_AdGold", adGoldWatchCountToday);
        PlayerPrefs.SetInt("Meta_AdBP", adBlueprintWatchCountToday);
        PlayerPrefs.SetString("Meta_AdLastReset", adLastResetDate);
        PlayerPrefs.SetString("Meta_LastSave", lastSaveTimestamp);
        PlayerPrefs.Save();
    }
}
```

### Step 2：ProgressManager（核心）

```csharp
using System;
using UnityEngine;

namespace YourGame.Logic
{
    public class ProgressManager : PersistentSingleton<ProgressManager>
    {
        [Header("配置")]
        [SerializeField] private int maxEnergy = 5;
        [SerializeField] private float energyRecoveryInterval = 360f;   // 6 分钟 +1
        [SerializeField] private int maxDailyAdWatches = 5;
        [SerializeField] private int adEnergyReward = 2;
        [SerializeField] private int adGoldReward = 500;
        [SerializeField] private int adBlueprintReward = 3;

        private MetaData meta = new MetaData();
        private float energyRecoveryTimer = 0f;

        // ─── 事件（业务订阅这些） ───────────────────────────
        public static event Action<int> OnGoldCoinsChanged;
        public static event Action<int, int> OnEnergyChanged;
        public static event Action<int> OnBlueprintsChanged;
        public static event Action<int> OnGemsChanged;

        // ─── 公共属性 ───────────────────────────────────────
        public int Gems       => meta.gems;
        public int GoldCoins  => meta.goldCoins;
        public int Energy     => meta.energy;
        public int Blueprints => meta.blueprints;
        public int MaxEnergy  => maxEnergy;
        public bool IsEnergyFull => meta.energy >= maxEnergy;

        // ─── 生命周期 ──────────────────────────────────────

        protected override void OnSingletonAwake()
        {
            meta.Load(maxEnergy);
            CalculateOfflineRecovery();   // 离线期间能恢复多少体力
            CheckDailyAdReset();           // 跨天则重置广告次数
        }

        private void Update() => UpdateEnergyRecovery();

        private void OnApplicationQuit() => SaveTimestamp();
        private void OnApplicationPause(bool pause) { if (pause) SaveTimestamp(); }

        private void SaveTimestamp()
        {
            meta.lastSaveTimestamp = DateTime.UtcNow.ToString("O");
            meta.Save();
        }

        // ─── 体力自然恢复 ──────────────────────────────────

        private void UpdateEnergyRecovery()
        {
            if (IsEnergyFull) { energyRecoveryTimer = 0f; return; }

            energyRecoveryTimer += Time.deltaTime;
            while (energyRecoveryTimer >= energyRecoveryInterval && meta.energy < maxEnergy)
            {
                energyRecoveryTimer -= energyRecoveryInterval;
                meta.energy++;
                meta.Save();
                OnEnergyChanged?.Invoke(meta.energy, maxEnergy);
            }
        }

        // ─── 离线补算 ───────────────────────────────────────

        private void CalculateOfflineRecovery()
        {
            if (string.IsNullOrEmpty(meta.lastSaveTimestamp)) return;
            if (IsEnergyFull) return;

            try
            {
                var lastSave = DateTime.Parse(meta.lastSaveTimestamp, null,
                                  System.Globalization.DateTimeStyles.RoundtripKind);
                double elapsedSec = (DateTime.UtcNow - lastSave).TotalSeconds;
                if (elapsedSec <= 0) return;

                int recoverable = Mathf.FloorToInt((float)elapsedSec / energyRecoveryInterval);
                if (recoverable > 0)
                {
                    int canRecover = maxEnergy - meta.energy;
                    int recovered = Mathf.Min(recoverable, canRecover);
                    meta.energy += recovered;

                    // 计时器从余量继续，避免"刚回到游戏就立刻又 +1"的不爽感
                    float usedTime = recoverable * energyRecoveryInterval;
                    energyRecoveryTimer = Mathf.Clamp((float)elapsedSec - usedTime, 0f, energyRecoveryInterval);

                    meta.Save();
                    OnEnergyChanged?.Invoke(meta.energy, maxEnergy);
                }
                else
                {
                    energyRecoveryTimer = Mathf.Clamp((float)elapsedSec, 0f, energyRecoveryInterval);
                }
            }
            catch (Exception e) { Debug.LogWarning($"[Progress] 离线补算失败: {e.Message}"); }
        }

        // ─── 广告每日重置 ──────────────────────────────────

        private void CheckDailyAdReset()
        {
            string today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            if (meta.adLastResetDate != today)
            {
                meta.adWatchCountToday = 0;
                meta.adGoldWatchCountToday = 0;
                meta.adBlueprintWatchCountToday = 0;
                meta.adLastResetDate = today;
                meta.Save();
            }
        }

        // ─── 资源操作（公共接口） ───────────────────────────

        public void AddGoldCoins(int amount)
        {
            if (amount <= 0) return;
            meta.goldCoins += amount;
            meta.Save();
            OnGoldCoinsChanged?.Invoke(meta.goldCoins);
        }

        public bool ConsumeGoldCoins(int amount)
        {
            if (meta.goldCoins < amount) return false;
            meta.goldCoins -= amount;
            meta.Save();
            OnGoldCoinsChanged?.Invoke(meta.goldCoins);
            return true;
        }

        public bool ConsumeEnergy(int amount = 1)
        {
            if (meta.energy < amount) return false;
            meta.energy -= amount;
            meta.Save();
            OnEnergyChanged?.Invoke(meta.energy, maxEnergy);
            return true;
        }

        public void AddBlueprints(int amount)
        {
            if (amount <= 0) return;
            meta.blueprints += amount;
            meta.Save();
            OnBlueprintsChanged?.Invoke(meta.blueprints);
        }

        // ─── 看广告换资源（与 AdManager 解耦） ──────────────

        public bool CanWatchAdForEnergy => meta.adWatchCountToday < maxDailyAdWatches;
        public bool CanWatchAdForGold => meta.adGoldWatchCountToday < maxDailyAdWatches;
        public bool CanWatchAdForBlueprint => meta.adBlueprintWatchCountToday < maxDailyAdWatches;

        /// <summary>由 AdManager 在广告完播回调里调用</summary>
        public void GrantAdEnergyReward()
        {
            meta.adWatchCountToday++;
            meta.energy += adEnergyReward;   // 溢出允许（不卡 maxEnergy 上限）
            meta.Save();
            OnEnergyChanged?.Invoke(meta.energy, maxEnergy);
        }

        public void GrantAdGoldReward()
        {
            meta.adGoldWatchCountToday++;
            AddGoldCoins(adGoldReward);
        }

        public void GrantAdBlueprintReward()
        {
            meta.adBlueprintWatchCountToday++;
            AddBlueprints(adBlueprintReward);
        }
    }
}
```

### Step 3：TopAreaController（顶部资源栏）

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YourGame.UI
{
    public class TopAreaController : MonoBehaviour
    {
        public static TopAreaController Instance { get; private set; }

        [Header("资源文字")]
        [SerializeField] private TextMeshProUGUI gemText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI blueprintText;

        [Header("资源按钮（点击弹提示）")]
        [SerializeField] private Button energyButton;
        [SerializeField] private Button goldButton;
        [SerializeField] private Button blueprintButton;

        [Header("提示面板")]
        [SerializeField] private TopBarTipsPanel tipsPanel;

        // 给金币飞行动画暴露目标位置（[[09_CoinFlyAnimation]] 用）
        public RectTransform GoldBarRect => goldText != null ? goldText.transform.parent as RectTransform : null;
        public Vector3 GoldBarWorldPos => GoldBarRect != null ? GoldBarRect.position : Vector3.zero;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (energyButton != null) energyButton.onClick.AddListener(() => tipsPanel?.Show(ResType.Energy));
            if (goldButton != null) goldButton.onClick.AddListener(() => tipsPanel?.Show(ResType.Gold));
            if (blueprintButton != null) blueprintButton.onClick.AddListener(() => tipsPanel?.Show(ResType.Blueprint));
        }

        private void Start() => Refresh();

        // ★ 关键：用事件驱动刷新，不要每帧 Update 轮询
        private void OnEnable()
        {
            ProgressManager.OnGoldCoinsChanged += OnGoldChanged;
            ProgressManager.OnEnergyChanged += OnEnergyChanged;
            ProgressManager.OnBlueprintsChanged += OnBlueprintsChanged;
        }

        private void OnDisable()
        {
            ProgressManager.OnGoldCoinsChanged -= OnGoldChanged;
            ProgressManager.OnEnergyChanged -= OnEnergyChanged;
            ProgressManager.OnBlueprintsChanged -= OnBlueprintsChanged;
        }

        public void Refresh()
        {
            if (ProgressManager.Instance == null) return;
            if (gemText != null) gemText.text = ProgressManager.Instance.Gems.ToString();
            if (goldText != null) goldText.text = ProgressManager.Instance.GoldCoins.ToString();
            if (energyText != null) energyText.text = $"{ProgressManager.Instance.Energy}/{ProgressManager.Instance.MaxEnergy}";
            if (blueprintText != null) blueprintText.text = ProgressManager.Instance.Blueprints.ToString();
        }

        private void OnGoldChanged(int newAmount)
        {
            if (goldText != null) goldText.text = newAmount.ToString();
            // 配合 [[07_UIAnimationHelper]]：金币变化时整栏 punch
            StartCoroutine(UIAnimationHelper.PlayScalePunch(GoldBarRect, 1.15f, 0.12f, useUnscaledTime: false));
        }

        private void OnEnergyChanged(int current, int max)
        {
            if (energyText != null) energyText.text = $"{current}/{max}";
        }

        private void OnBlueprintsChanged(int newAmount)
        {
            if (blueprintText != null) blueprintText.text = newAmount.ToString();
        }
    }

    public enum ResType { Gold, Energy, Blueprint }
}
```

---

## 关键设计决策（踩坑总结）

1. **事件驱动 UI，不要每帧 Update**——`OnEnable` 订阅 `ProgressManager.OnXxxChanged`，资源变化时触发刷新；`Update` 轮询会让 UI 永远比实际值晚一帧
2. **资源修改唯一入口在 Manager**——业务永远走 `ProgressManager.AddGoldCoins(100)`，绝不直接 `meta.goldCoins += 100`。否则事件不触发，UI 不刷新，存档不保存
3. **体力溢出允许**（`meta.energy += reward` 不夹 `maxEnergy`）——看广告 +2 体力可以让你超过 5 点上限，这是产品设计（用户感觉"赚到"，实际玩到下限就会重新自然恢复）
4. **离线补算**——`OnApplicationQuit` 写时间戳，`OnSingletonAwake` 读时间戳算差值。如果两小时回来，按 6 分钟/点算能补 20 点（夹 maxEnergy）；如果只过 1 分钟，恢复不了 1 点但**计时器要继承**——否则玩家关游戏 30 秒回来发现计时器从 0 开始重数，体验差
5. **每日广告次数按 UTC 跨天判断**——用 `DateTime.UtcNow.Date.ToString("yyyy-MM-dd")` 比 `DateTime.Now.Date` 更稳（避免玩家改设备时区刷次数）
6. **三种广告资源各自独立次数字段**——共享一个 `adWatchCountToday` 会让用户看完体力广告就拿不了金币广告。各自 5 次更友好
7. **`OnApplicationPause(true)` 也存档**——iOS / 微信小游戏切后台不一定走 `OnApplicationQuit`，必须 Pause 时也写时间戳
8. **`PersistentSingleton` 而非 `Singleton`**——`ProgressManager` 必须跨场景保留，进战斗场景不能丢局外数据
9. **TopAreaController 暴露 `GoldBarWorldPos` / `GoldBarRect`**——给金币飞行动画 ([[09_CoinFlyAnimation]]) 提供吸附目标位置；这是"资源栏对接战斗反馈"的关键 API
10. **`PlayerPrefs.Save()` 显式调用**——不要靠 Unity 自动 Save（不可靠）；每次写完 `Set*` 后都 Save 一次
11. **`AddGoldCoins` 内部触发 punch**——把视觉反馈封装在事件回调里，业务调用 `AddGoldCoins(100)` 即得到完整爽感链路

---

## 与 BeautyStacking CurrencyManager 的合并差异

| 特性 | BeautyStacking 简单版 | 本模板（合并版） |
|------|---------------------|---------------|
| 资源种类 | 单一 makeupCoins | 多种（金币 / 钻石 / 体力 / 蓝图） |
| 体力机制 | 无 | 自然恢复 + 离线补算 + 广告补 |
| 广告系统 | 无（外部直接调用） | 内置每日上限 + 三种独立计数 |
| 数据持久化 | 走 SaveManager（外部） | PlayerPrefs 直接管理 + 时间戳 |

**适用建议**：
- 如果你是叠放/收纳/消除等"无体力"休闲游戏 → 用 BeautyStacking 简单版（10 行 `CurrencyManager` 即可）
- 如果是 Roguelite / 关卡型 / 需要回流付费 → 用本模板的完整版

---

## 与其他模板的关系

| 模板 | 用法 |
|------|------|
| [[02_SingletonPattern]] | `ProgressManager` 继承 `PersistentSingleton<T>` |
| [[06_WXAdsManager]] | 看广告成功回调 → 调用 `ProgressManager.GrantAdXxxReward()` |
| [[09_CoinFlyAnimation]] | `TopAreaController.GoldBarWorldPos` 作为金币吸附目标 |
| [[07_UIAnimationHelper]] | 资源变化事件回调里 `PlayScalePunch(GoldBarRect)` |
| [[11_GameEvents]] | 局内金币变化用 `GameEvents.TriggerCoinChanged`；局外金币用本模板的 `OnGoldCoinsChanged`（区分本场战斗 vs. 永久资产） |

---

## 注意事项

- **PlayerPrefs 在微信小游戏的容量限制**：单 key value 上限 1MB，所有 key 总和上限 10MB。如果资源种类很多或要存大量章节进度，要换 JSON 文件存档（参见 [[05_SaveManager]]）
- **不要把局内 `coins`（本场战斗收集的）和局外 `goldCoins`（永久余额）混淆**——前者是 SessionData，跟随场景重置；后者是 MetaData，永久保留
- **TopBarTipsPanel 没在本模板写完整**——它的核心是"显示资源不足提示 + 引导看广告"，与 [[06_WXAdsManager]] 联动，逻辑较薄，按需自实现
- **`maxEnergy` 在 Inspector 配置**——不要硬编码；不同关卡 / 章节可能上限不同
- **`energyRecoveryInterval = 360f` 是 6 分钟**——休闲小游戏标准值；动作类游戏可以降到 3 分钟（增加 DAU），SLG 可以到 10 分钟（拉付费）
