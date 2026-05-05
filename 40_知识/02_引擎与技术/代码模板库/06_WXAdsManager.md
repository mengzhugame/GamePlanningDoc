# WXAdsManager 微信激励视频广告管理器（成熟版）

> **来源工程**：LightVSDecay（光与朽，2026-04 提取）
> **取代**：原来自美妆叠叠乐的简化版（仅单文件 + Action 回调），简化版思路保留在最末「附录：极简版」一节
> **复用价值**：⭐⭐⭐⭐⭐
> **文档同步**：[[40_知识/02_引擎与技术/Unity通用技术栈复用指南#二、微信小游戏专属补丁（必抄 2 个）|何时抄]] · [[40_知识/02_引擎与技术/数据埋点与BattleLog工程落地#四、IAA 埋点：意愿 vs. 实际渗透要分开|埋点联动]]

---

## 适用场景

微信小游戏接入激励视频广告（Rewarded Video Ad）。**已上线项目共识：6 广告位 + 共享每日总上限 + 场次内频控 + 异常超时兜底**。

---

## 核心特性（与简化版的差异）

| 特性 | 简化版（旧 06） | 成熟版（本文） |
|------|---------------|--------------|
| 广告位数 | 4 | 6（技能重置 / 结算双倍 / 复活 / 体力 / 金币 / 蓝图）|
| 频控 | 无 | 共享每日总上限 + 场次内"每场限 1 次" + 各广告位独立每日上限 |
| 预加载 | 无 | 启动时一次性预加载所有 6 个广告位 |
| 异常兜底 | 无 | 180s `WaitForSecondsRealtime` 超时强制 fail，避免回调丢失永久卡住 |
| 重试 | 无 | Show 失败后自动 Load 一次再 Show，仍失败才 fail |
| 占位模式 | `#if !UNITY_EDITOR` | Inspector 上 `usePlaceholderAds` 布尔开关，运行时切换 |
| 文件数 | 1（WXAdsManager.cs）| 3（AdManager.cs + WeChatAdsPlugin.cs + AdType.cs）|

---

## 三层架构

```
┌────────────────────────────────────────────────────────┐
│  业务层（任意 Panel / Manager）                         │
│   ↓ 调用                                              │
│  AdManager（决策层 — 频控、可用性、记次）              │
│   ↓ 转发                                              │
│  WeChatAdsPlugin（桥接层 — 微信 SDK + 超时 + 重试）   │
└────────────────────────────────────────────────────────┘
```

**职责切分铁律**：
- `AdManager` **不接触微信 SDK**——只做"是否可看 / 看了之后记次 / 完成后回调"
- `WeChatAdsPlugin` **不接触业务**——只把 `(adIndex, onSuccess, onFail)` 转成 `WX.CreateRewardedVideoAd`
- 桥接层用 `#if !UNITY_EDITOR && UNITY_WEBGL` 包裹，Editor 内只走业务层占位

---

## 使用方法

### 业务层调用（最常用）

```csharp
// 死亡复活
public void OnRevivePressed()
{
    if (!AdManager.Instance.CanWatchAd(AdType.Revive))
    {
        ShowToast("今日复活次数已达上限");
        return;
    }

    AdManager.Instance.ShowRewardedAd(AdType.Revive,
        onSuccess: () =>
        {
            playerHP.RestoreFull();
            ResumeGame();
        },
        onFail: () =>
        {
            ShowToast("广告未观看完整");
        });
}

// 结算双倍领取
AdManager.Instance.ShowRewardedAd(AdType.SettlementDouble,
    onSuccess: () => GrantDoubleReward(),
    onFail:    null);

// 查询场次内是否已复活（避免重复显示按钮）
bool hasRevived = AdManager.Instance.HasRevivedThisGame();

// 关卡前判定能否提供复活（波次 < 4 时不允许）
bool canOffer = AdManager.Instance.CanOfferRevive(currentWave);
```

### Inspector 配置

`AdManager` 必须在 Bootstrap 场景或 Resources 中存在 Prefab：
- `usePlaceholderAds = true`：开发期走占位（直接 `onSuccess`）
- `usePlaceholderAds = false`：发布前必须切换为 false，调用真实 SDK

---

## 代码实现

### 1. AdType.cs（枚举，集中定义所有广告位）

```csharp
namespace YourGame.Ads
{
    public enum AdType
    {
        SkillReroll,        // #1 技能重置（不限次）
        SettlementDouble,   // #2 结算双倍（每场限1次）
        Revive,             // #3 死亡复活（每场限1次）
        EnergyTopUp,        // #4 体力补充（每日5次）
        GoldTopUp,          // #5 金币补充（每日3次）
        BlueprintTopUp,     // #6 蓝图补充（每日3次）
    }
}
```

### 2. AdManager.cs（决策层）

```csharp
using System;
using System.Collections;
using UnityEngine;

namespace YourGame.Ads
{
    public sealed class AdManager : MonoBehaviour
    {
        private const string SharedDailyTotalKey = "ad_shared_total_{0}";
        private const string DailyCountKey       = "ad_daily_count_{0}_{1}";
        private const string PREFAB_PATH         = "Prefab/AdManager";

        private static AdManager instance;

        [Header("Debug")]
        [Tooltip("true=编辑器占位模式（直接成功，不调用真实广告）；发布微信前设为 false")]
        [SerializeField] private bool usePlaceholderAds = true;
        [SerializeField] private bool showDebugInfo = false;

        private bool hasRevivedThisGame;
        private bool hasSettlementDoubleThisGame;

        public static AdManager Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindObjectOfType<AdManager>();
                if (instance != null) return instance;

                var prefab = Resources.Load<GameObject>(PREFAB_PATH);
                if (prefab != null)
                {
                    Instantiate(prefab).name = "[AdManager]";
                }
                else
                {
                    var go = new GameObject("[AdManager]");
                    instance = go.AddComponent<AdManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public int SharedRewardedDailyLimit => 10;
        public bool HasRevivedThisGame() => hasRevivedThisGame;

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(PreloadAllAdsDelayed());
        }

        private IEnumerator PreloadAllAdsDelayed()
        {
            yield return null;  // 等一帧，确保 JS 桥就绪
            if (!usePlaceholderAds) WeChatAdsPlugin.Instance.PreloadAll();
        }

        // —— 你的游戏需要在 GameStart 事件里调 ResetRunState ——
        public void ResetRunState()
        {
            hasRevivedThisGame = false;
            hasSettlementDoubleThisGame = false;
        }

        // ─── 公共查询 ────────────────────────────────────────

        public bool CanWatchAd(AdType adType)
        {
            if (adType == AdType.SkillReroll) return true;

            if (IsSharedRewardedType(adType) && GetSharedRewardedDailyCount() >= SharedRewardedDailyLimit)
                return false;

            switch (adType)
            {
                case AdType.SettlementDouble: return !hasSettlementDoubleThisGame;
                case AdType.Revive:           return !hasRevivedThisGame;
                case AdType.EnergyTopUp:
                case AdType.GoldTopUp:
                case AdType.BlueprintTopUp:   return GetDailyCount(adType) < GetDailyLimit(adType);
                default:                      return false;
            }
        }

        public bool CanOfferRevive(int currentWave) => currentWave >= 4 && CanWatchAd(AdType.Revive);

        public int GetDailyCount(AdType adType)
            => IsSharedRewardedType(adType)
                ? GetSharedRewardedDailyCount()
                : PlayerPrefs.GetInt(GetDailyCountKey(adType), 0);

        public int GetDailyLimit(AdType adType)
        {
            switch (adType)
            {
                case AdType.SkillReroll:      return int.MaxValue;
                case AdType.SettlementDouble:
                case AdType.Revive:           return SharedRewardedDailyLimit;
                case AdType.EnergyTopUp:      return 5;
                case AdType.GoldTopUp:
                case AdType.BlueprintTopUp:   return 3;
                default:                      return 0;
            }
        }

        // ─── 展示广告 ────────────────────────────────────────

        public void ShowRewardedAd(AdType adType, Action onSuccess, Action onFail = null)
        {
            if (!CanWatchAd(adType)) { onFail?.Invoke(); return; }

            if (usePlaceholderAds)
            {
                GrantWatchCount(adType);
                onSuccess?.Invoke();
                return;
            }

            WeChatAdsPlugin.Instance.ShowAd((int)adType,
                onSuccess: () => { GrantWatchCount(adType); onSuccess?.Invoke(); },
                onFail:    () => onFail?.Invoke());
        }

        // ─── 私有 ────────────────────────────────────────────

        private void GrantWatchCount(AdType adType)
        {
            if (IsSharedRewardedType(adType))
                PlayerPrefs.SetInt(GetSharedRewardedDailyKey(), GetSharedRewardedDailyCount() + 1);
            else
            {
                int cur = PlayerPrefs.GetInt(GetDailyCountKey(adType), 0);
                PlayerPrefs.SetInt(GetDailyCountKey(adType), cur + 1);
            }

            switch (adType)
            {
                case AdType.Revive:           hasRevivedThisGame          = true; break;
                case AdType.SettlementDouble: hasSettlementDoubleThisGame = true; break;
            }

            PlayerPrefs.Save();
        }

        private bool IsSharedRewardedType(AdType adType)
            => adType == AdType.SettlementDouble || adType == AdType.Revive;

        private int GetSharedRewardedDailyCount()
            => PlayerPrefs.GetInt(GetSharedRewardedDailyKey(), 0);

        private string GetSharedRewardedDailyKey()
            => string.Format(SharedDailyTotalKey, DateTime.Now.ToString("yyyyMMdd"));

        private string GetDailyCountKey(AdType adType)
            => string.Format(DailyCountKey, DateTime.Now.ToString("yyyyMMdd"), adType);
    }
}
```

### 3. WeChatAdsPlugin.cs（桥接层）

```csharp
using System;
using System.Collections;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_WEBGL
using WeChatWASM;
#endif

namespace YourGame.Ads
{
    public class WeChatAdsPlugin : MonoBehaviour
    {
        private const float ShowTimeoutSeconds = 180f;
        private static WeChatAdsPlugin instance;

        // 广告位 ID 集中常量（按 AdType 顺序对齐）
        private static readonly string[] AdUnitIds =
        {
            "adunit-xxxxxxxxxxxxxxxx",  // 0: SkillReroll
            "adunit-yyyyyyyyyyyyyyyy",  // 1: SettlementDouble
            "adunit-zzzzzzzzzzzzzzzz",  // 2: Revive
            "adunit-aaaaaaaaaaaaaaaa",  // 3: EnergyTopUp
            "adunit-bbbbbbbbbbbbbbbb",  // 4: GoldTopUp
            "adunit-cccccccccccccccc",  // 5: BlueprintTopUp
        };

#if !UNITY_EDITOR && UNITY_WEBGL
        private readonly WXRewardedVideoAd[] rewardedAds = new WXRewardedVideoAd[AdUnitIds.Length];
        private readonly Action[] pendingSuccessCallbacks = new Action[AdUnitIds.Length];
        private readonly Action[] pendingFailCallbacks    = new Action[AdUnitIds.Length];
        private readonly bool[]     isShowing             = new bool[AdUnitIds.Length];
        private readonly Coroutine[] showTimeoutCoroutines = new Coroutine[AdUnitIds.Length];
#endif

        public static WeChatAdsPlugin Instance
        {
            get
            {
                if (instance != null) return instance;
                var go = new GameObject("[WeChatAdsPlugin]");
                instance = go.AddComponent<WeChatAdsPlugin>();
                DontDestroyOnLoad(go);
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PreloadAll()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            for (int i = 0; i < AdUnitIds.Length; i++)
            {
                var ad = GetOrCreateAd(i);
                if (ad == null) continue;
                int idx = i;
                ad.Load(success: _ => { }, failed: res => Debug.LogWarning($"预加载失败 idx={idx}: {res?.errMsg}"));
            }
#endif
        }

        public void ShowAd(int adTypeIndex, Action onSuccess, Action onFail)
        {
            if (adTypeIndex < 0 || adTypeIndex >= AdUnitIds.Length) { onFail?.Invoke(); return; }

#if !UNITY_EDITOR && UNITY_WEBGL
            if (isShowing[adTypeIndex]) { onFail?.Invoke(); return; }   // 防重复点击

            var ad = GetOrCreateAd(adTypeIndex);
            if (ad == null) { onFail?.Invoke(); return; }

            pendingSuccessCallbacks[adTypeIndex] = onSuccess;
            pendingFailCallbacks[adTypeIndex]    = onFail;
            isShowing[adTypeIndex] = true;
            StartShowTimeout(adTypeIndex);

            ShowAdInternal(adTypeIndex, allowReloadRetry: true);
#else
            // Editor 内模拟成功（占位模式由 AdManager 那层处理）
            onSuccess?.Invoke();
#endif
        }

#if !UNITY_EDITOR && UNITY_WEBGL
        private WXRewardedVideoAd GetOrCreateAd(int idx)
        {
            if (rewardedAds[idx] != null) return rewardedAds[idx];

            try
            {
                var ad = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
                {
                    adUnitId = AdUnitIds[idx],
                    multiton = true
                });

                int captured = idx;
                ad.OnClose(res =>
                {
                    if (res == null || res.isEnded) CompleteSuccess(captured);
                    else                            CompleteFail(captured, "用户未完整观看");
                });
                ad.OnError(res =>
                {
                    if (isShowing[captured]) CompleteFail(captured, res?.errMsg ?? "unknown");
                });

                rewardedAds[idx] = ad;
                return ad;
            }
            catch (Exception e) { Debug.LogError($"创建广告异常: {e.Message}"); return null; }
        }

        private void ShowAdInternal(int idx, bool allowReloadRetry)
        {
            var ad = rewardedAds[idx];
            if (ad == null) { CompleteFail(idx, "实例为空"); return; }

            ad.Show(
                success: _ => { },
                failed: res =>
                {
                    if (!allowReloadRetry) { CompleteFail(idx, "Show 重试失败"); return; }
                    ad.Load(
                        success: _ => ShowAdInternal(idx, allowReloadRetry: false),
                        failed:  _ => CompleteFail(idx, "Load 失败"));
                });
        }

        private void CompleteSuccess(int idx)
        {
            if (!isShowing[idx]) return;
            var cb = pendingSuccessCallbacks[idx];
            ClearPending(idx);
            cb?.Invoke();
        }

        private void CompleteFail(int idx, string reason)
        {
            if (!isShowing[idx]) return;
            Debug.LogWarning($"广告未完成 idx={idx}: {reason}");
            var cb = pendingFailCallbacks[idx];
            ClearPending(idx);
            cb?.Invoke();
        }

        private void ClearPending(int idx)
        {
            StopShowTimeout(idx);
            isShowing[idx] = false;
            pendingSuccessCallbacks[idx] = null;
            pendingFailCallbacks[idx]    = null;
        }

        private void StartShowTimeout(int idx)
        {
            StopShowTimeout(idx);
            showTimeoutCoroutines[idx] = StartCoroutine(ShowTimeoutRoutine(idx));
        }

        private void StopShowTimeout(int idx)
        {
            if (showTimeoutCoroutines[idx] == null) return;
            StopCoroutine(showTimeoutCoroutines[idx]);
            showTimeoutCoroutines[idx] = null;
        }

        private IEnumerator ShowTimeoutRoutine(int idx)
        {
            yield return new WaitForSecondsRealtime(ShowTimeoutSeconds);
            if (isShowing[idx]) CompleteFail(idx, "广告关闭回调超时");
        }
#endif
    }
}
```

---

## 关键设计决策（踩坑总结）

1. **决策与桥接分两层** — 业务调 `AdManager`，永远不直接调微信 SDK。这样 Editor 测试、占位模式、单元测试都不需要 mock 微信
2. **`multiton = true`** — 创建 `WXRewardedVideoAd` 必须传，否则同一个广告位多次创建会被微信合并，回调会丢
3. **180s `WaitForSecondsRealtime` 超时** — 微信偶尔会丢 `OnClose` 回调；没有兜底就会让 `isShowing` 永远为 true，按钮永远点不动。用 `Realtime` 是因为暂停游戏时也得倒计时
4. **Show 失败自动 Load 一次再 Show** — 用户偶发"广告还没就绪"时第一次失败，第二次就能跑通；不要 fail 一次就提示用户
5. **共享每日总上限（10 次）** — 微信平台官方推荐值，超过会被微信限流甚至拒投
6. **场次内"每场限 1 次"用内存变量，不入 PlayerPrefs** — 退游再进就该重置（这是产品语义，不是 bug）
7. **GameStart 事件里调 `ResetRunState()`** — 否则每局开始都带着上一局的 `hasRevivedThisGame=true`，按钮永久变灰

---

## 与简化版（旧 06）的迁移指南

如果你的项目用的是旧 06 简化版，迁移到本版本：

1. 把 `WXAdsManager.cs` 拆成 `AdManager.cs`（业务接口保持兼容）+ `WeChatAdsPlugin.cs`（新增）+ `AdType.cs`（枚举值要按你项目调整）
2. 业务层 `WXAdsManager.Instance.ShowAd(...)` → `AdManager.Instance.ShowRewardedAd(...)`
3. 新增频控管理：在 GameStart 事件里调 `AdManager.Instance.ResetRunState()`
4. Inspector 上的 `usePlaceholderAds` 默认 true，发布前手动切 false（比 `#if !UNITY_EDITOR` 更灵活）

---

## 附录：极简版（旧 06，仅供参考）

如果你的项目只有 1–2 个广告位、没有频控需求、不在乎超时兜底，可以用极简版（约 80 行）。但**任何上线项目都建议直接用成熟版**——多写的 200 行会救你下次半夜的 Bug。

```csharp
// 极简版关键差异：
// - 单文件 WXAdsManager.cs
// - 没有 AdType 枚举（直接用 string adUnitId）
// - 没有频控、预加载、超时兜底
// - Editor 走 #if !UNITY_EDITOR 直接 onSuccess
public class WXAdsManager : MonoBehaviour
{
    public void ShowAd(string adUnitId, Action onSuccess)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        var ad = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam { adUnitId = adUnitId });
        ad.OnClose(res => { if (res?.isEnded == true) onSuccess?.Invoke(); });
        ad.Show();
#else
        onSuccess?.Invoke();
#endif
    }
}
```

---

## 注意事项

- 广告位 ID 在微信小程序后台「流量主」申请，审核通过后才能正常展示
- 真机测试比模拟器更可靠，模拟器无法加载真实广告
- 上线前必须把 `AdManager.usePlaceholderAds` 切为 false，否则永远走占位
- 配套 [[40_知识/02_引擎与技术/数据埋点与BattleLog工程落地#四、IAA 埋点：意愿 vs. 实际渗透要分开|IAA 埋点]]：每个广告位埋两个事件（`ad_click_*` 测意愿、`ad_reward_*` 测完播）
