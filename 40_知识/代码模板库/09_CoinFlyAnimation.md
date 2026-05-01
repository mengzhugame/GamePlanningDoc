# 战斗金币掉落动画 — CoinPickup System

> **来源工程**：LightVSDecay（光与朽 · `Assets/Scripts/Logic/Coin/`）
> **提取日期**：2026-04-29
> **复用价值**：⭐⭐⭐⭐⭐
> **依赖**：[[02_SingletonPattern|Singleton 基类]]、`GameEvents.OnEnemyDied` 事件、TrailRenderer（拖尾，可选）

---

## 适用场景

**怪物死亡 → 掉落金币 → 飞向资源栏**的完整链路。比 [[07_UIAnimationHelper#与 09 的协同|07]] 的简单飞行更进一步：

- **3 阶段动画**：散落（scatter）→ 悬浮（hover）→ 贝塞尔吸附（absorb）
- **视觉数压缩**：实际掉 50 金币也只生成 8 个金币动画（避免屏幕上同时几十个金币导致 GC）
- **GameEvents 驱动**：自动监听 `OnEnemyDied`，业务层零调用即可工作
- **对象池**：单例 Spawner + 预热 24 个 + 上限 80 个

如果只是"物品 A→B 单段飞行"的简单需求，用 `SimpleFlyAnimation`（旧 09，附录保留）即可。

---

## 三层架构

```
GameEvents.OnEnemyDied(type, pos, xp, coin)
        ↓
CoinPickupSpawner（单例，监听事件）
  ├── 对象池：Queue<CoinPickup> 预热 24 + 动态扩到 80
  ├── 视觉数压缩：50 → 7 个金币，每个代表 ~7 块
  └── targetPositionGetter：Func<Vector3>，每帧重新取（资源栏移动也能跟踪）
        ↓
CoinPickup（单实例组件，挂在 Prefab 上）
  ├── Phase 1 — Scatter：从死亡点向随机方向抛物线散开（0.22s，EaseOutCubic）
  ├── Phase 2 — Hover：在散开点正弦悬停（0.45–0.7s 随机）
  ├── Phase 3 — Absorb：贝塞尔曲线吸附到资源栏（0.32–0.48s，EaseInCubic + TrailRenderer）
  └── 每个金币随机 scale + Sprite Flipbook 相位（_FrameOffset）避免动画整齐划一
```

---

## 使用方法

### 接入步骤（5 分钟）

```csharp
// 1. 在战斗场景启动时，告诉 Spawner 资源栏在哪
CoinPickupSpawner.Instance.SetTargetPositionGetter(
    () => TopAreaController.Instance.GoldBarWorldPos);

// 2. 设置"金币到达资源栏"时怎么累加（与音效配合）
CoinPickupSpawner.Instance.SetCoinArriveNotifier(visualValue =>
{
    ProgressManager.Instance.AddCoins(visualValue);
    AudioManager.Instance?.PlayCoinCollect();   // 带冷却的金币音效
    StartCoroutine(UIAnimationHelper.PlayScalePunch(
        TopAreaController.Instance.GoldBarRect, 1.15f, 0.12f, useUnscaledTime: false));
});

// 3. 业务层：只需正常触发 GameEvents.OnEnemyDied
GameEvents.TriggerEnemyDied(enemyType, deathPos, xpReward, coinReward);
// → Spawner 自动捕获，自动 spawn 金币动画
```

### 主动调用（不依赖事件）

```csharp
// 偶尔手动 spawn（如宝箱开启）
CoinPickupSpawner.Instance.SpawnCoins(chestPos, coinAmount: 30);

// 关卡切换 / 玩家死亡时清空所有正在飞的金币
CoinPickupSpawner.Instance.ReturnAllCoins();
```

---

## 代码实现

### 1. CoinPickupSpawner.cs（对象池 + 事件监听）

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YourGame.Logic.Coin
{
    public class CoinPickupSpawner : Singleton<CoinPickupSpawner>   // ← 02 基类
    {
        [Header("Prefab")]
        [SerializeField] private CoinPickup coinPrefab;

        [Header("Pool")]
        [SerializeField] private int prewarmCount = 24;
        [SerializeField] private int maxCount = 80;

        [Header("Visual Count")]
        [Tooltip("无论实际掉多少金币，最多生成几个金币动画")]
        [SerializeField] private int maxVisualCoinCount = 8;

        private readonly Queue<CoinPickup> availablePool = new();
        private readonly HashSet<CoinPickup> activeCoins = new();
        private Transform poolContainer;
        private Func<Vector3> targetPositionGetter;
        private Action<int> coinArriveNotifier;
        private int totalCreated;

        protected override void OnSingletonAwake()
        {
            var container = new GameObject("[CoinPickupPool]");
            container.transform.SetParent(transform, false);
            poolContainer = container.transform;

            for (int i = 0; i < prewarmCount; i++)
            {
                var coin = CreateCoin();
                if (coin == null) break;
                coin.gameObject.SetActive(false);
                availablePool.Enqueue(coin);
            }
        }

        private void OnEnable()  => GameEvents.OnEnemyDied += OnEnemyDied;
        private void OnDisable() => GameEvents.OnEnemyDied -= OnEnemyDied;

        public void SetTargetPositionGetter(Func<Vector3> getter)
            => targetPositionGetter = getter;

        public void SetCoinArriveNotifier(Action<int> notifier)
            => coinArriveNotifier = notifier;

        public void SpawnCoins(Vector3 position, int coinAmount)
        {
            if (coinAmount <= 0 || coinPrefab == null) return;

            int visualCount = CalculateVisualCount(coinAmount);
            int baseValue = coinAmount / visualCount;
            int remainder = coinAmount % visualCount;

            for (int i = 0; i < visualCount; i++)
            {
                var coin = GetCoin();
                if (coin == null) break;
                int visualValue = baseValue + (i < remainder ? 1 : 0);
                coin.transform.SetParent(poolContainer, false);
                activeCoins.Add(coin);
                coin.Play(position, visualValue, targetPositionGetter, HandleCoinArrived, ReturnCoin);
            }
        }

        public void ReturnAllCoins()
        {
            var coins = new List<CoinPickup>(activeCoins);
            foreach (var c in coins) ReturnCoin(c);
        }

        // 核心压缩规则：避免实际金额很大时同屏生成几十个金币
        private int CalculateVisualCount(int coinAmount)
        {
            if (coinAmount <= 4)  return coinAmount;
            if (coinAmount <= 12) return 4;
            if (coinAmount <= 24) return 5;
            if (coinAmount <= 49) return 6;
            if (coinAmount <= 79) return 7;
            return Mathf.Min(8, maxVisualCoinCount);
        }

        private void OnEnemyDied(EnemyType type, Vector3 pos, int xp, int coin)
        {
            if (coin > 0) SpawnCoins(pos, coin);
        }

        private void HandleCoinArrived(CoinPickup coin, int visualValue)
            => coinArriveNotifier?.Invoke(visualValue);

        private CoinPickup GetCoin()
        {
            if (availablePool.Count > 0) return availablePool.Dequeue();
            if (totalCreated >= maxCount) return null;
            return CreateCoin();
        }

        private CoinPickup CreateCoin()
        {
            var coin = Instantiate(coinPrefab, poolContainer);
            coin.name = $"CoinPickup_{totalCreated:D3}";
            totalCreated++;
            return coin;
        }

        private void ReturnCoin(CoinPickup coin)
        {
            if (coin == null || !activeCoins.Contains(coin)) return;
            activeCoins.Remove(coin);
            coin.gameObject.SetActive(false);
            coin.transform.SetParent(poolContainer, false);
            availablePool.Enqueue(coin);
        }
    }
}
```

### 2. CoinPickup.cs（单个金币的三阶段动画）

```csharp
using System;
using System.Collections;
using UnityEngine;

namespace YourGame.Logic.Coin
{
    public class CoinPickup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TrailRenderer trailRenderer;  // 可选，吸附阶段才打开

        [Header("Scatter（散落阶段）")]
        [SerializeField] private float scatterDuration = 0.22f;
        [SerializeField] private float scatterDistanceMin = 0.35f;
        [SerializeField] private float scatterDistanceMax = 0.9f;
        [SerializeField] private float scatterArcHeight = 0.2f;

        [Header("Hover（悬浮阶段）")]
        [SerializeField] private float hoverDurationMin = 0.45f;
        [SerializeField] private float hoverDurationMax = 0.7f;
        [SerializeField] private float hoverAmplitude = 0.06f;
        [SerializeField] private float hoverFrequency = 5f;

        [Header("Absorb（吸附阶段，贝塞尔曲线）")]
        [SerializeField] private float absorbDelayMin = 0f;
        [SerializeField] private float absorbDelayMax = 0.18f;
        [SerializeField] private float absorbDurationMin = 0.32f;
        [SerializeField] private float absorbDurationMax = 0.48f;
        [SerializeField] private float absorbArcHeight = 0.8f;
        [SerializeField] private float arriveThreshold = 0.15f;

        [Header("Scale & Flipbook（差异化）")]
        [SerializeField] private Vector2 scaleRange = new(0.9f, 1.08f);
        [SerializeField] private string frameOffsetProperty = "_FrameOffset";

        private MaterialPropertyBlock propertyBlock;
        private Func<Vector3> targetPositionGetter;
        private Action<CoinPickup, int> arriveCallback;
        private Action<CoinPickup> recycleCallback;
        private Coroutine playCoroutine;
        private Vector3 baseScale;
        private int visualValue;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            propertyBlock = new MaterialPropertyBlock();
            baseScale = transform.localScale;
        }

        public void Play(Vector3 startPos, int value,
                         Func<Vector3> targetGetter,
                         Action<CoinPickup, int> onArrive,
                         Action<CoinPickup> onRecycle)
        {
            visualValue = Mathf.Max(1, value);
            targetPositionGetter = targetGetter;
            arriveCallback = onArrive;
            recycleCallback = onRecycle;

            ResetState();
            transform.position = startPos;
            gameObject.SetActive(true);
            playCoroutine = StartCoroutine(PlayRoutine(startPos));
        }

        private void ResetState()
        {
            if (trailRenderer != null) { trailRenderer.Clear(); trailRenderer.emitting = false; }
            transform.localScale = baseScale * UnityEngine.Random.Range(scaleRange.x, scaleRange.y);

            // 让每个金币的 Sprite 翻页动画相位不同，避免整屏同步晃动
            if (spriteRenderer != null)
            {
                spriteRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(frameOffsetProperty, UnityEngine.Random.Range(0f, 4f));
                spriteRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private IEnumerator PlayRoutine(Vector3 startPos)
        {
            // 随机散开方向 + 距离 + 各阶段时长
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            if (dir.sqrMagnitude < 0.01f) dir = Vector2.up;
            Vector3 scatterTarget = startPos + (Vector3)(dir * UnityEngine.Random.Range(scatterDistanceMin, scatterDistanceMax));

            yield return PlayScatter(startPos, scatterTarget);
            yield return PlayHover(scatterTarget, UnityEngine.Random.Range(hoverDurationMin, hoverDurationMax));
            yield return new WaitForSeconds(UnityEngine.Random.Range(absorbDelayMin, absorbDelayMax));
            yield return PlayAbsorb(scatterTarget, UnityEngine.Random.Range(absorbDurationMin, absorbDurationMax));

            arriveCallback?.Invoke(this, visualValue);
            recycleCallback?.Invoke(this);
            playCoroutine = null;
        }

        private IEnumerator PlayScatter(Vector3 from, Vector3 to)
        {
            float elapsed = 0f;
            while (elapsed < scatterDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / scatterDuration);
                Vector3 pos = Vector3.LerpUnclamped(from, to, EaseOutCubic(t));
                pos.y += Mathf.Sin(t * Mathf.PI) * scatterArcHeight;
                transform.position = pos;
                yield return null;
            }
            transform.position = to;
        }

        private IEnumerator PlayHover(Vector3 center, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector3 pos = center;
                pos.y += Mathf.Sin(elapsed * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude;
                transform.position = pos;
                yield return null;
            }
        }

        private IEnumerator PlayAbsorb(Vector3 from, float duration)
        {
            Vector3 target = targetPositionGetter?.Invoke() ?? from;
            Vector3 ctrl = Vector3.Lerp(from, target, 0.5f) + Vector3.up * absorbArcHeight;

            if (trailRenderer != null) trailRenderer.emitting = true;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target = targetPositionGetter?.Invoke() ?? target;   // 资源栏移动时跟踪
                ctrl = Vector3.Lerp(from, target, 0.5f) + Vector3.up * absorbArcHeight;
                transform.position = QuadBezier(from, ctrl, target, EaseInCubic(t));

                if (Vector3.Distance(transform.position, target) <= arriveThreshold)
                {
                    transform.position = target;
                    yield break;
                }
                yield return null;
            }
        }

        private static float EaseOutCubic(float t) { float i = 1f - t; return 1f - i * i * i; }
        private static float EaseInCubic(float t) => t * t * t;

        private static Vector3 QuadBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float i = 1f - t;
            return i * i * p0 + 2f * i * t * p1 + t * t * p2;
        }
    }
}
```

---

## 关键设计决策（踩坑总结）

1. **三阶段，不是单段直飞** — 散落让玩家看到"哦掉了好多金币"，悬浮给一拍 anticipation，吸附才让数字跳。三段总时长 1.0–1.6s 是经过验证的爽感节奏，太快没视觉记忆，太慢拖沓
2. **视觉数压缩（`CalculateVisualCount`）** — 这是性能与爽感的关键：50 金币真生成 50 个动画 = GC 灾难 + 视觉杂乱；压成 7 个每个代表 7 块，玩家感觉"掉了好多"但 CPU 一点不疼
3. **`targetPositionGetter` 用 `Func<Vector3>` 不用 `Vector3`** — 资源栏在 Canvas 缩放或新手引导时会移动，每帧重新取才能正确跟踪
4. **`EaseOutCubic`（散落） + `EaseInCubic`（吸附）配对** — 散落"飞快慢停"、吸附"慢起飞快"，两段对比制造"被吸入"的物理感
5. **贝塞尔曲线（吸附阶段）用二次** — 控制点 = 中点 + 向上偏移，让金币飞出弧线再下落到资源栏，比直线 Lerp 多一层"重力被克服"的视觉
6. **每个金币 random scale + Sprite Flipbook `_FrameOffset`** — 避免一群金币像复制粘贴一样整齐划一旋转
7. **`TrailRenderer` 只在吸附阶段开** — 散落和悬浮时打开会让屏幕杂乱；只在吸附时开，强化"被吸"的拖尾感
8. **`SetParent(poolContainer, false)`** — 池容器统一挂载，避免敌人销毁时把金币也带走；`worldPositionStays=false` 是关键
9. **不在 Update 里轮询 `OnEnemyDied`** — 直接订阅事件 + 委托回调；如果改为轮询，就丢失了"事件驱动"的解耦优势

---

## Prefab 设置

```
[CoinPickup] (Prefab)
├── Transform — localScale 给个基准（如 1,1,1），运行时会乘 scaleRange 随机
├── SpriteRenderer — 金币 Sprite，材质用支持 Flipbook 的 Shader（含 _FrameOffset 属性）
├── TrailRenderer — Time 0.15–0.25s，颜色金黄渐透；初始 emitting=false（脚本控制）
└── CoinPickup 脚本 — 配上面所有 Inspector 字段
```

**Spawner Prefab**（推荐，不放也行）：
```
[CoinPickupSpawner] (放在 Bootstrap 场景的 GameObject 上，或 Resources/Prefab/ 下让 Singleton 自动加载)
└── CoinPickupSpawner 脚本 — 拖入 coinPrefab 引用
```

---

## 与其他模板的关系

| 模板 | 配合 |
|------|------|
| [[02_SingletonPattern]] | `CoinPickupSpawner` 继承自这里的 `Singleton<T>` |
| [[11_GameEvents]] | 监听 `OnEnemyDied` 事件触发掉落 |
| [[07_UIAnimationHelper#与 09 的协同]] | 资源栏的 punch 反馈在 `coinArriveNotifier` 里调用 |
| [[10_FloatingTextSystem]] | 金币到达时同步飘"+N"飘字（白色小字） |
| [[13_AudioManagerPro]] | 金币到达音效（带冷却防止 spam） |
| [[14_ProgressManager_CurrencyTopBar]] | 金币累加 + 资源栏事件刷新 |

---

## 注意事项

- **WebGL/微信小游戏**：`MaterialPropertyBlock` 是免费的——比 `material.SetFloat`（会复制材质）省 90% Draw Call
- **大批量金币（>30 个同屏）建议**：`maxVisualCoinCount` 调到 6–8；预热数 `prewarmCount` 调到 30–40
- **关卡结束/玩家死亡** 必须调 `ReturnAllCoins()`，否则正在飞的金币会带着失效的 `targetPositionGetter` 引用错位
- **Spawner 如果不在场景里预放**：放到 `Resources/Prefab/CoinPickupSpawner.prefab`，让 Singleton 在第一次 `Instance` 调用时自动加载

---

## 附录：极简版（旧 09，单段飞行）

如果项目只有"物品 A→B 单段飞行"需求（如 BeautyStacking 的物品飞向订单包），旧的 `SimpleFlyAnimation` 仍然适用。它没有三阶段、没有对象池、没有事件监听，纯协程一个文件搞定。详情见旧版本（已被本文取代，关键技巧已并入"踩坑总结"）：

```csharp
// 极简版核心 API（保留参考）
SimpleFlyAnimation.Instance.FlyToTarget(item, targetPos,
    duration: 0.3f,
    onComplete: () => Destroy(item),
    onNearComplete: () => currencyManager.AddGold(1));   // 80% 提前回调，让数字看起来"刚好同步"
```

**何时选极简版**：物品类型简单、不需要事件驱动、单屏并发 < 5 个。
**何时选完整版（本文）**：战斗类游戏、敌人死亡掉落、需要对象池 + 三阶段爽感。
