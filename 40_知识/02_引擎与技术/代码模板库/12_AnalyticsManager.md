# AnalyticsManager 微信场景分析埋点

> **来源工程**：LightVSDecay（光与朽 · `Core/AnalyticsManager.cs`）
> **提取日期**：2026-04-29
> **复用价值**：⭐⭐⭐⭐⭐
> **依赖**：WeChatWASM SDK（仅微信小游戏构建时需要）
> **方法论文档**：[[40_知识/02_引擎与技术/数据埋点与BattleLog工程落地]]——本模板是该方法论的代码层落地

---

## 适用场景

任何**微信小游戏**项目的"上线 MVP 数据埋点"。无需引入第三方 SDK（友盟/Talking Data），直接走微信后台「场景分析」即可拿到：

- 新手漏斗（`app_launch → first_battle_start → first_battle_win/lose`）
- 广告意愿（各广告位 `ad_click_*`）
- 章节通关漏斗
- D1/D3/D7 留存（微信内置）

**关键限制**：场景分析**不支持自定义参数**——只能记录"事件发生了几次"。需要"死在第几波 / 选了哪个技能"这类带参数事件，要走二期友盟 SDK。

---

## 核心设计

```
业务层（GameManager / SettlementPanel / AdManager）
  ↓ AnalyticsManager.LogScene(...) / LogSceneOnce(...) / TryLogFirstBattle*(...)
  ↓
AnalyticsManager（统一入口，三种语义级别）
  ├── LogScene             — 反复触发（每场战斗、每次点击）
  ├── LogSceneOnce         — 永久一次性（首次完成新手引导）→ PlayerPrefs 持久化
  └── LogSceneOncePerSession — 本次启动一次（app_launch）→ HashSet 内存
  ↓
BranchConfigMap（sceneId → branchId 映射）
  ↓ #if WEIXINMINIGAME
WX.ReportUserBehaviorBranchAnalytics
```

---

## 使用方法

### 1. 应用启动时

```csharp
// 在 Bootstrap 场景或 Splash 完成时调一次
AnalyticsManager.LogAppLaunch();   // 内部走 LogSceneOncePerSession，本次启动只上报一次
```

### 2. 关键节点埋点

```csharp
// 新手引导步骤完成（永久一次性）
AnalyticsManager.LogSceneOnce("tutorial_techtree_upgrade");

// 战斗里程碑（每次都报）
AnalyticsManager.LogScene("battle_start");
AnalyticsManager.LogScene("chapter1_clear");

// 广告点击（点击意愿）
AnalyticsManager.LogScene(AnalyticsSceneIds.AdClickRevive);
```

### 3. 首战漏斗（封装版）

为什么单独包一层 `TryLog*`：首战开始/结果是"漏斗根节点"，必须保证"开始了才报结果"且"全局只报一次"。

```csharp
// 在 GameEvents 触发处自动调用（业务层无感）
GameEvents.TriggerGameStart()    → AnalyticsManager.TryLogFirstBattleStart()
GameEvents.TriggerGameVictory() → AnalyticsManager.TryLogFirstBattleResult(true)
GameEvents.TriggerGameDefeat()  → AnalyticsManager.TryLogFirstBattleResult(false)
```

### 4. Editor / DEVELOPMENT_BUILD 调试

```csharp
// 自动打日志，不真实上报
[Analytics] LogScene: app_launch
[Analytics] LogScene: first_battle_start
```

---

## 代码实现

```csharp
// ============================================================
// AnalyticsManager.cs
// ============================================================

using System.Collections.Generic;
using UnityEngine;

#if WEIXINMINIGAME || UNITY_WEBGL
using WeChatWASM;
#endif

namespace YourGame.Core
{
    /// <summary>
    /// 逻辑埋点 ID（业务层用这个常量，不要散字符串）
    /// </summary>
    public static class AnalyticsSceneIds
    {
        public const string AppLaunch = "app_launch";
        public const string FirstBattleStart = "first_battle_start";
        public const string FirstBattleWin = "first_battle_win";
        public const string FirstBattleLose = "first_battle_lose";

        public const string AdClickRevive = "ad_click_revive";
        public const string AdClickDouble = "ad_click_double";
        public const string AdClickReroll = "ad_click_reroll";
        public const string AdClickEnergy = "ad_click_energy";
        public const string AdClickGold = "ad_click_gold";
        public const string AdClickBlueprint = "ad_click_blueprint";

        // 业务可继续扩展...
    }

    public sealed class AnalyticsManager : MonoBehaviour
    {
        private const int EventTypeExposure = 1;   // 微信事件类型：曝光
        private const int EventTypeClick = 2;      // 微信事件类型：点击

        private const string PlayerIdKey = "Analytics_UserId";
        private const string OncePrefix = "Analytics_Once_";
        private const string FirstBattleStartedKey = "Analytics_FirstBattle_Started";
        private const string FirstBattleResultKey = "Analytics_FirstBattle_ResultReported";

        private static readonly HashSet<string> SessionReportedScenes = new();

        // 微信后台拿到的 branchId 在这里集中配置（替换为你项目的真实 ID）
        private static readonly Dictionary<string, BranchAnalyticsConfig> BranchConfigMap = new()
        {
            { AnalyticsSceneIds.AppLaunch,         new("BCxxxxxxxxxxxxxxxx", EventTypeExposure) },
            { AnalyticsSceneIds.FirstBattleStart,  new("BCyyyyyyyyyyyyyyyy", EventTypeExposure) },
            { AnalyticsSceneIds.FirstBattleWin,    new("BCzzzzzzzzzzzzzzzz", EventTypeExposure) },
            { AnalyticsSceneIds.FirstBattleLose,   new("BCaaaaaaaaaaaaaaaa", EventTypeExposure) },
            { AnalyticsSceneIds.AdClickRevive,     new("BCbbbbbbbbbbbbbbbb", EventTypeClick) },
            { AnalyticsSceneIds.AdClickDouble,     new("BCcccccccccccccccc", EventTypeClick) },
            { AnalyticsSceneIds.AdClickReroll,     new("BCdddddddddddddddd", EventTypeClick) },
            { AnalyticsSceneIds.AdClickEnergy,     new("BCeeeeeeeeeeeeeeee", EventTypeClick) },
            { AnalyticsSceneIds.AdClickGold,       new("BCffffffffffffffff", EventTypeClick) },
            { AnalyticsSceneIds.AdClickBlueprint,  new("BCgggggggggggggggg", EventTypeClick) },
        };

        public static AnalyticsManager Instance { get; private set; }
        public static string UserId { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureUserId();
        }

        // ─── 三种语义级别 ─────────────────────────────────────

        /// <summary>本次启动只上报一次（HashSet 内存，重启会重置）</summary>
        public static void LogAppLaunch() => LogSceneOncePerSession(AnalyticsSceneIds.AppLaunch);

        /// <summary>每次都上报（战斗里程碑、广告点击）</summary>
        public static void LogScene(string sceneId)
            => EnsureInstance().ReportSceneInternal(sceneId);

        /// <summary>永久只上报一次（玩家级别一次性事件，PlayerPrefs 持久化）</summary>
        public static void LogSceneOnce(string sceneId)
        {
            string key = OncePrefix + sceneId;
            if (PlayerPrefs.GetInt(key, 0) == 1) return;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            LogScene(sceneId);
        }

        /// <summary>本次启动只上报一次（HashSet 内存）</summary>
        public static void LogSceneOncePerSession(string sceneId)
        {
            if (!SessionReportedScenes.Add(sceneId)) return;
            LogScene(sceneId);
        }

        // ─── 首战漏斗封装（保证"开始过才报结果"+"全局一次"）─────

        public static void TryLogFirstBattleStart()
        {
            if (PlayerPrefs.GetInt(FirstBattleStartedKey, 0) == 1) return;
            PlayerPrefs.SetInt(FirstBattleStartedKey, 1);
            PlayerPrefs.Save();
            LogScene(AnalyticsSceneIds.FirstBattleStart);
        }

        public static void TryLogFirstBattleResult(bool victory)
        {
            if (PlayerPrefs.GetInt(FirstBattleStartedKey, 0) != 1 ||
                PlayerPrefs.GetInt(FirstBattleResultKey, 0) == 1) return;
            PlayerPrefs.SetInt(FirstBattleResultKey, 1);
            PlayerPrefs.Save();
            LogScene(victory ? AnalyticsSceneIds.FirstBattleWin : AnalyticsSceneIds.FirstBattleLose);
        }

        // ─── 调试入口 ─────────────────────────────────────────

        public static void ClearLocalFlags()
        {
            PlayerPrefs.DeleteKey(OncePrefix + AnalyticsSceneIds.AppLaunch);
            PlayerPrefs.DeleteKey(FirstBattleStartedKey);
            PlayerPrefs.DeleteKey(FirstBattleResultKey);
            PlayerPrefs.Save();
            SessionReportedScenes.Clear();
        }

        // ─── 内部 ─────────────────────────────────────────────

        private static AnalyticsManager EnsureInstance()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("[AnalyticsManager]");
            Instance = go.AddComponent<AnalyticsManager>();
            return Instance;
        }

        private static void EnsureUserId()
        {
            if (!string.IsNullOrEmpty(UserId)) return;
            UserId = PlayerPrefs.GetString(PlayerIdKey, string.Empty);
            if (!string.IsNullOrEmpty(UserId)) return;
            UserId = System.Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(PlayerIdKey, UserId);
            PlayerPrefs.Save();
        }

        private void ReportSceneInternal(string sceneKey)
        {
            if (string.IsNullOrEmpty(sceneKey)) return;
            EnsureUserId();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Analytics] LogScene: {sceneKey}");
#endif

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD && (WEIXINMINIGAME || UNITY_WEBGL)
            if (!BranchConfigMap.TryGetValue(sceneKey, out var config) || string.IsNullOrEmpty(config.BranchId))
            {
                Debug.LogWarning($"[Analytics] 未配置微信 branchId，跳过: {sceneKey}");
                return;
            }

            try
            {
                WX.ReportUserBehaviorBranchAnalytics(new ReportUserBehaviorBranchAnalyticsOption
                {
                    branchId = config.BranchId,
                    eventType = config.EventType,
                    branchDim = string.Empty
                });
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Analytics] 上报失败 {sceneKey}: {e.Message}");
            }
#endif
        }

        private readonly struct BranchAnalyticsConfig
        {
            public readonly string BranchId;
            public readonly int EventType;
            public BranchAnalyticsConfig(string branchId, int eventType)
            { BranchId = branchId; EventType = eventType; }
        }
    }
}
```

---

## 接入步骤

### Step 1：微信后台创建场景

登录微信小程序后台 → 「统计」→「场景分析」→「新建场景」，逐一创建：

| 场景名 | 事件类型 | 用途 |
|--------|---------|------|
| `app_launch` | 曝光 | 启动入口 |
| `first_battle_start` | 曝光 | 首战漏斗开端 |
| `first_battle_win` / `first_battle_lose` | 曝光 | 首战漏斗结果 |
| `ad_click_*` | **点击** | 各广告位的意愿测量 |
| `chapter*_clear` | 曝光 | 章节通关漏斗 |
| `tutorial_*_done` | 曝光 | 新手引导漏斗 |

每个场景创建完会拿到一个 `branchId`（形如 `BCxxxx...`），填到代码 `BranchConfigMap` 中。

### Step 2：业务接入

按 [[40_知识/02_引擎与技术/数据埋点与BattleLog工程落地#三、最少必要的场景清单（22 个就够）|22 个最少必要场景]] 在对应位置调用 `LogScene` / `LogSceneOnce`。

### Step 3：发布前验证

- Editor 跑：日志应有 `[Analytics] LogScene: ...`
- 微信开发者工具体验版跑：「数据助手」可以实时看到事件触发（约 10 分钟延迟）

---

## 关键设计决策（踩坑总结）

1. **三种语义级别要分清**——用错了会让漏斗数据严重失真
   - **永久一次性**（`LogSceneOnce`）：玩家这辈子只能报一次。新手引导步骤、首通章节、首次解锁某功能
   - **本次启动一次**（`LogSceneOncePerSession`）：本次启动期间只报一次。`app_launch`
   - **每次都报**（`LogScene`）：战斗开始、关卡通过、广告点击
2. **首战漏斗用 `TryLog*` 而不是 `LogSceneOnce`** — 因为要保证"开始过才能报结果"的依赖关系，纯 `Once` 无法表达
3. **`branchId` 集中放 `Dictionary`** — 不要散在各处。后台改 ID 时只改一处
4. **sceneId 集中放 `AnalyticsSceneIds`** — 字符串笔误是这套系统的头号杀手
5. **`#if !UNITY_EDITOR && !DEVELOPMENT_BUILD && (WEIXINMINIGAME || UNITY_WEBGL)`** — 编译期剔除 SDK 调用，比运行时判断省 CPU 且确定性高
6. **没配 `branchId` 时只 LogWarning 不报错** — 开发期还没建场景的事件就跳过；不要让缺配置阻断游戏运行
7. **PlayerPrefs 而不是文件** — 微信小游戏的 PlayerPrefs 走 `wx.setStorageSync`，比文件 IO 快得多
8. **`UserId` 用 `Guid.NewGuid().ToString("N")` 不是带破折号版本** — N 格式是 32 字符纯字母数字，对接第三方系统更友好
9. **`ClearLocalFlags()` 调试入口** — QA 验证漏斗时，每次都新建账号太麻烦；用这个入口可以"重置首战标记"，重新走一遍漏斗

---

## 与其他模板的关系

| 模板 | 用法 |
|------|------|
| [[11_GameEvents]] | `TriggerGameStart` 内部调用 `AnalyticsManager.TryLogFirstBattleStart()` |
| [[06_WXAdsManager]] | 每次广告点击时埋 `AdClick*`；广告完播时埋 `AdReward*`（意愿 vs 完播分开） |
| [[40_知识/02_引擎与技术/数据埋点与BattleLog工程落地]] | 方法论层（22 场景清单 + 4 看板设计 + 工时估算） |

---

## 注意事项

- **`branchId` 是"分支 ID"，不是"场景 ID"** — 微信后台一个"场景"下可以有多个"分支"对应不同的事件类型（曝光 / 点击）。新建场景时记得选对类型
- **后台数据有 ~10 分钟延迟** — 验证时不要急着说"事件没上报"
- **微信场景分析有事件数量上限** — 单日上限官方未明确，超过会被限流。22 个核心事件足够，不要为了"以防万一"加几十个
- **不要把这个当成全栈分析工具** — 它只能数事件次数，不能做用户分群、漏斗对比、A/B 测试。需要那些就走友盟（二期）
