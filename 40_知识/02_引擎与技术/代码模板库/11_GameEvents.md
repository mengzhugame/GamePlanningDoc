# GameEvents 全局事件总线

> **来源工程**：LightVSDecay（光与朽 · `Core/GameEvents.cs`）
> **提取日期**：2026-04-29
> **复用价值**：⭐⭐⭐⭐⭐
> **依赖**：无（纯 C# `static event Action`）

---

## 适用场景

任何中型以上 Unity 项目都应该在第一天就铺好这套：**Manager 之间不互相引用，全部走静态事件解耦**。

不用这套时的典型病症：
- `BattleManager` 引用 `UIManager` 引用 `AudioManager` 引用 `BattleManager` ——循环依赖，单测无门
- 加一个新 Manager 要改 5 个 Manager 的 `using`
- 切场景时 `null reference` 满地飞

用这套的好处：
- Manager 只需 `OnEnable` 订阅、`OnDisable` 取消订阅，互相不知道存在
- 新增事件只改 `GameEvents.cs` 一处
- 单元测试可以直接 `GameEvents.TriggerXxx(...)` 模拟任何场景

---

## 设计哲学（4 条铁律）

1. **静态事件 + Trigger 方法成对** — `OnEnemyDied` 是 `event`，`TriggerEnemyDied` 是触发函数。`event` 限制了"只能 += / -=，不能直接 = null"，强制走 `ClearAllEvents()`
2. **事件按业务域分组** — 游戏状态 / 玩家进度 / 玩家状态 / 敌人 / Boss / 波次……每组用 `━━━` 分隔注释，不要混在一起
3. **`ClearAllEvents()` 在场景切换或 GameManager 重启时调** — 防止 lambda 闭包持有失效的 GameObject 引用导致 leak
4. **触发参数尽量简单** — 复杂数据用 ScriptableObject 引用 + ID 传入，不要传整个对象（避免事件订阅方持有强引用）

---

## 使用方法

### 监听事件（订阅方）

```csharp
public class HUDPanel : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.OnHullHPChanged += UpdateHpBar;
        GameEvents.OnGameStart += OnGameStart;
        GameEvents.OnGameDefeat += OnGameDefeat;
    }

    private void OnDisable()
    {
        GameEvents.OnHullHPChanged -= UpdateHpBar;
        GameEvents.OnGameStart -= OnGameStart;
        GameEvents.OnGameDefeat -= OnGameDefeat;
    }

    private void UpdateHpBar(int current, int max) { /* ... */ }
}
```

**铁律**：
- `OnEnable` 订阅、`OnDisable` 取消，**永远成对出现**——不在 `Awake` 订阅是因为 `Awake` 期间事件触发时其他 Manager 可能还没就绪
- 用具名方法不用匿名 lambda——匿名 lambda 无法 `-=` 取消订阅，会留下幽灵订阅

### 触发事件（发布方）

```csharp
// Manager 内部状态变化时调 Trigger
public class PlayerStatsManager : Singleton<PlayerStatsManager>
{
    public void TakeDamage(int dmg)
    {
        currentHull -= dmg;
        GameEvents.TriggerHullHPChanged(currentHull, maxHull);

        if (currentHull <= 0)
            GameEvents.TriggerPlayerDeathRequested();
    }
}
```

### 场景切换时清空（GameManager 调用）

```csharp
public class GameManager : MonoBehaviour
{
    public void RestartScene()
    {
        GameEvents.ClearAllEvents();   // 防止旧场景的 lambda 持有失效引用
        SceneManager.LoadScene("GameScene");
    }
}
```

---

## 代码实现（精简模板）

```csharp
// ============================================================
// GameEvents.cs
// 文件位置: Assets/Scripts/Core/GameEvents.cs
// ============================================================

using System;
using UnityEngine;

namespace YourGame.Core
{
    public enum GameState { Menu, Playing, Paused, Victory, Defeat }

    public static class GameEvents
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 游戏状态
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public static event Action<GameState> OnGameStateChanged;
        public static event Action OnGameStart;
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;
        public static event Action OnGameVictory;
        public static event Action OnGameDefeat;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 玩家进度
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public static event Action<int, int> OnExpChanged;        // (current, required)
        public static event Action<int> OnLevelUp;                // (newLevel)
        public static event Action<int> OnCoinChanged;            // (totalCoins)
        public static event Action<int> OnComboChanged;           // (combo)
        public static event Action OnComboReset;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 玩家状态
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public static event Action<int, int> OnHullHPChanged;     // (current, max)
        public static event Action<int, int> OnShieldHPChanged;
        public static event Action OnShieldBroken;
        public static event Action OnLowHealthStart;              // 血量 < 20%
        public static event Action OnLowHealthEnd;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 敌人 / 战斗
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public static event Action<EnemyType, Vector3, int, int> OnEnemyDied;  // (type, pos, xp, coin)
        public static event Action<int> OnXPOrbCollected;
        public static event Action OnBossDeath;
        public static event Action OnBossFightStart;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 波次（按需添加）
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public static event Action<int, int> OnWaveStart;         // (current, total)
        public static event Action<int, int> OnWaveComplete;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 触发方法（与事件成对）
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public static void TriggerGameStateChanged(GameState s) => OnGameStateChanged?.Invoke(s);
        public static void TriggerGameStart() => OnGameStart?.Invoke();
        public static void TriggerGamePaused() => OnGamePaused?.Invoke();
        public static void TriggerGameResumed() => OnGameResumed?.Invoke();
        public static void TriggerGameVictory() => OnGameVictory?.Invoke();
        public static void TriggerGameDefeat() => OnGameDefeat?.Invoke();

        public static void TriggerExpChanged(int cur, int req) => OnExpChanged?.Invoke(cur, req);
        public static void TriggerLevelUp(int lv) => OnLevelUp?.Invoke(lv);
        public static void TriggerCoinChanged(int total) => OnCoinChanged?.Invoke(total);
        public static void TriggerComboChanged(int c) => OnComboChanged?.Invoke(c);
        public static void TriggerComboReset() => OnComboReset?.Invoke();

        public static void TriggerHullHPChanged(int cur, int max) => OnHullHPChanged?.Invoke(cur, max);
        public static void TriggerShieldHPChanged(int cur, int max) => OnShieldHPChanged?.Invoke(cur, max);
        public static void TriggerShieldBroken() => OnShieldBroken?.Invoke();
        public static void TriggerLowHealthStart() => OnLowHealthStart?.Invoke();
        public static void TriggerLowHealthEnd() => OnLowHealthEnd?.Invoke();

        public static void TriggerEnemyDied(EnemyType type, Vector3 pos, int xp, int coin)
            => OnEnemyDied?.Invoke(type, pos, xp, coin);
        public static void TriggerXPOrbCollected(int xp) => OnXPOrbCollected?.Invoke(xp);
        public static void TriggerBossDeath() => OnBossDeath?.Invoke();
        public static void TriggerBossFightStart() => OnBossFightStart?.Invoke();

        public static void TriggerWaveStart(int cur, int total) => OnWaveStart?.Invoke(cur, total);
        public static void TriggerWaveComplete(int done, int total) => OnWaveComplete?.Invoke(done, total);

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 清除所有订阅（场景切换 / GameManager 重启时调用）
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public static void ClearAllEvents()
        {
            OnGameStateChanged = null;
            OnGameStart = null; OnGamePaused = null; OnGameResumed = null;
            OnGameVictory = null; OnGameDefeat = null;

            OnExpChanged = null; OnLevelUp = null; OnCoinChanged = null;
            OnComboChanged = null; OnComboReset = null;

            OnHullHPChanged = null; OnShieldHPChanged = null;
            OnShieldBroken = null; OnLowHealthStart = null; OnLowHealthEnd = null;

            OnEnemyDied = null; OnXPOrbCollected = null;
            OnBossDeath = null; OnBossFightStart = null;

            OnWaveStart = null; OnWaveComplete = null;
        }

        // 调试：检查某个事件的订阅者数量
        public static int GetWaveCompleteListenerCount()
            => OnWaveComplete?.GetInvocationList()?.Length ?? 0;
    }
}
```

---

## 关键设计决策（踩坑总结）

1. **`event` 关键字而不是裸 `Action`** — 裸 `Action` 允许外部 `GameEvents.OnEnemyDied = null;`（清掉所有订阅），`event` 强制只能 `+= / -=`。这是核心保护
2. **每个 event 配对一个 `Trigger*` 方法** — 让外部触发必须走显式入口（方便加日志、加埋点、加防抖），不能直接 `OnEnemyDied?.Invoke(...)`
3. **`ClearAllEvents()` 必须有** — 切场景时 lambda 闭包会持有旧场景对象引用；如果不清，会让 GC 无法回收，且可能调用已销毁对象的方法导致 NullReferenceException
4. **场景切换后必须重新订阅** — `OnEnable` 在场景切换时不一定会再触发（如果对象本来就是 DontDestroyOnLoad），需要在场景加载完成事件里手动 resubscribe（参见 [[13_AudioManagerPro]] 的 `ResubscribeToGameEvents`）
5. **Trigger 在 Manager 内部调用，不暴露外部** — 业务方应该改 Manager 状态，Manager 自己 Trigger；不要让 UI 直接 `GameEvents.TriggerCoinChanged(100)`——这会让数据流向无法追踪
6. **不要把过大的对象作为参数** — `OnEnemyDied(EnemyType, Vector3, int, int)` 是好的；`OnEnemyDied(Enemy enemy)` 是差的——后者让订阅方可以读到 enemy 的所有状态，破坏封装
7. **波次/Boss/特殊事件按业务域分组** — 不要把所有 30+ 事件平铺；`━━━` 注释分组让人能 30 秒找到自己想要的事件
8. **调试事件订阅者数量**（`GetXxxListenerCount`）— 排查"事件触发了但没人响应"问题时的关键利器

---

## 与单例 / 直接引用的对比

| 方式 | 何时用 | 缺点 |
|------|--------|------|
| **GameEvents（本模板）** | Manager 间松耦合通信、UI 监听数据变化、跨场景事件 | 静态状态、不支持多游戏实例（一般游戏不需要）|
| **直接 `Singleton.Instance.Method()`** | 强相关 Manager 之间（如 `BattleManager.Instance.SpawnEnemy()`）| 制造硬依赖，单测难 |
| **C# Action 字段（实例事件）** | 单个组件的局部事件（按钮点击、动画完成）| 跨 Manager 通信会让代码到处传引用 |

**经验法则**：跨 Manager / 跨场景 → GameEvents；同一系统内的局部事件 → 实例 Action。

---

## 与其他模板的关系

| 模板 | 用法 |
|------|------|
| [[09_CoinFlyAnimation]] | `CoinPickupSpawner` 监听 `OnEnemyDied` 自动 spawn 金币 |
| [[13_AudioManagerPro]] | 监听 `OnLevelUp / OnGameVictory / OnEnemyDied` 等播音效 |
| [[14_ProgressManager_CurrencyTopBar]] | `Trigger CoinChanged / ExpChanged / EnergyChanged` 由它发起 |
| [[10_FloatingTextSystem]] | 飘字管理器监听受击事件触发飘字 |
| [[12_AnalyticsManager]] | `TriggerGameStart` 内部连带调用 `AnalyticsManager.TryLogFirstBattleStart()` |

---

## 注意事项

- **静态事件不参与 Unity 生命周期** — 进入 Play Mode 后状态保持，重新 Play 时如果 `Domain Reload` 关闭（Unity 2019+ 默认）会保留旧订阅。Editor 启动时调一次 `ClearAllEvents()` 防御
- **多线程** — 静态事件不是线程安全的，所有 Trigger 必须在主线程调用（Unity 默认就是主线程，一般无需特殊处理）
- **不要把事件总线当成神** — 一切都用事件会让代码流向混乱。原则：**数据变化 → 事件**；**操作请求 → 直接调用 Manager**
