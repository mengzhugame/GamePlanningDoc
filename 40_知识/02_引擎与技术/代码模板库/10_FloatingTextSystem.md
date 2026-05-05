# 战斗飘字系统 — FloatingTextSystem

> **来源工程**：LightVSDecay（光与朽 · `Assets/Scripts/UI/FloatingText/`）
> **提取日期**：2026-04-29
> **复用价值**：⭐⭐⭐⭐⭐
> **依赖**：TextMeshPro（必需）+ [[02_SingletonPattern|Singleton 基类]](必需) + 一个 Canvas

---

## 适用场景

任何需要**世界坐标 → 屏幕坐标飘字**的游戏：
- 战斗伤害（普通 / 暴击 / 元素 / Boss 弱点）
- 玩家受击 / 治疗 / 护盾
- 处决文字（"EXECUTE!"、"COMBO X3"）
- 状态文本（"STUN"、"BLOCK"、"MISS"）
- 拾取奖励（"+50 金币"、"+1 蓝图"——可与 [[09_CoinFlyAnimation]] 配合）

**为什么不直接 Instantiate 一个 Text 让它飘：**
- 大量飘字（同屏 50+）会触发频繁 GC，掉帧
- 每种类型样式不同（颜色 / 大小 / 缓动），写在调用方会变成 if-else 地狱
- 优先级回收：暴击 > 普通——同屏溢出时优先回收普通

---

## 三层架构

```
┌─────────────────────────────────────────────────────────┐
│  业务层（敌人 / 玩家 / 拾取物）                          │
│   ↓                                                    │
│  FloatingTextManager.ShowDamage / ShowExecution / ...   │
│   ↓                                                    │
│  FloatingTextConfig（ScriptableObject）                 │
│  ├── 各类型 Prefab 引用                                 │
│  ├── 各类型样式（颜色 / 大小 / 动画参数）                │
│  └── 各类型优先级（决定回收顺序）                        │
│   ↓                                                    │
│  FloatingText（单实例组件，挂在 Prefab 上）              │
│   控制：位置（重力+水平偏移）/ 透明度 / 缩放（EaseOutBack）│
└─────────────────────────────────────────────────────────┘
```

**职责切分**：
- `FloatingTextManager`：单例 + 多类型对象池 + 优先级回收 + 业务接口
- `FloatingTextConfig`：所有视觉数据（ScriptableObject，策划在 Inspector 配）
- `FloatingText`：单条飘字的运行时（Awake 缓存组件、Update 走动画曲线、Complete 回收）
- `FloatingTextType`：枚举（飘字"语义类型"，决定走哪套样式）

---

## 核心特性

1. **多 Prefab 对象池**：每种类型独立队列，避免不同字体大小/描边粗细在同一池里来回 setup
2. **预热策略**：启动时为高频类型（普通伤害 / 暴击 / Boss 受击）预先实例化 Prefab
3. **优先级回收**：同屏溢出时，优先回收"低优先级 + 剩余时间最少"的飘字
4. **世界 → 屏幕坐标自动转换**：支持 `Screen Space - Overlay` / `Screen Space - Camera` / `World Space` 三种 Canvas
5. **两种动画曲线**：位置走"初速度 + 重力 + 水平随机"（物理感）、缩放走 `EaseOutBack`（弹性回弹）
6. **样式 ScriptableObject 化**：所有颜色/字号/动画时长可视化配置，策划改完不需要程序重新打包

---

## 使用方法

### 业务层最常用调用

```csharp
// 怪物受击 — 普通伤害
FloatingTextManager.Instance.ShowDamage(enemy.transform.position, dmg, isCrit: false);

// 怪物受击 — 暴击
FloatingTextManager.Instance.ShowDamage(enemy.transform.position, dmg, isCrit: true);

// Boss 弱点命中
FloatingTextManager.Instance.ShowBossCoreDamage(boss.corePosition, dmg, isCrit: critRoll);

// Boss 护甲伤害（小字银灰，有意"不爽"）
FloatingTextManager.Instance.ShowBossShieldDamage(boss.shieldPosition, dmg);

// 处决（特殊文字）
FloatingTextManager.Instance.ShowExecution(enemy.transform.position);

// 状态
FloatingTextManager.Instance.ShowStatus(enemy.transform.position, "STUN!");

// 玩家受击 / 恢复
FloatingTextManager.Instance.ShowPlayerHealthDamage(player.position, dmg);
FloatingTextManager.Instance.ShowPlayerShieldDamage(player.position, dmg);
FloatingTextManager.Instance.ShowPlayerHealthRestore(player.position, heal);

// 关卡切换 / 死亡时清空
FloatingTextManager.Instance.ReturnAll();
```

### 扩展自定义类型（3 步）

1. `FloatingTextType` 枚举加一项（如 `Heal`）
2. `FloatingTextConfig` 新增对应 `healStyle`、`healPriority`、`healPrefab` 字段，并在 `GetStyle/GetPriority/GetPrefab` switch 里加 case
3. `FloatingTextManager` 加一个 `ShowHeal(...)` 公开方法

---

## 代码实现

### 1. FloatingTextType.cs（枚举）

```csharp
namespace YourGame.UI.FloatingText
{
    public enum FloatingTextType
    {
        Normal,              // 普通伤害（白色小字）
        Crit,                // 暴击（红色大字 + 弹跳）
        Status,              // 状态文本（黄色，如 STUN）
        BossShield,          // Boss 护甲伤害（银灰小字 + 盾图标）
        BossCore,            // Boss 弱点伤害（红色大字）
        PlayerHealthDamage,  // 玩家受伤（红）
        PlayerShieldDamage,  // 玩家护盾受伤（青）
        PlayerHealthRestore, // 玩家恢复（绿）
        PlayerShieldRestore, // 玩家护盾恢复（亮青）
        Execution,           // 处决（"EXECUTE!"）
        Chain,               // 连锁伤害（电弧黄）
        Explosion,           // 爆炸 / AoE（橙）
        // 项目特定的可继续扩展……
    }
}
```

### 2. FloatingTextStyle + FloatingTextConfig（ScriptableObject）

```csharp
using UnityEngine;

namespace YourGame.UI.FloatingText
{
    [System.Serializable]
    public class FloatingTextStyle
    {
        [Header("颜色与字体")]
        public Color textColor = Color.white;
        public Color outlineColor = Color.black;
        [Range(16f, 72f)] public float fontSize = 32f;
        public bool isBold = false;
        [Range(0f, 0.5f)] public float outlineWidth = 0.2f;

        [Header("动画 — 位置")]
        [Range(0.3f, 2f)]   public float duration = 0.6f;          // 总时长
        [Range(0f, 300f)]   public float initialUpSpeed = 150f;    // 初始向上速度
        [Range(0f, 200f)]   public float horizontalRandomRange = 80f;
        [Range(0f, 500f)]   public float gravity = 0f;             // 重力（下落加速度）

        [Header("动画 — 透明度")]
        [Range(0.3f, 0.9f)] public float fadeStartPercent = 0.5f;  // 几成时间开始淡出

        [Header("动画 — 缩放（EaseOutBack 回弹）")]
        public bool useScaleAnimation = false;
        [Range(0.5f, 2f)]   public float initialScale = 1f;
        [Range(1f, 3f)]     public float peakScale = 1.5f;
        [Range(0.1f, 0.5f)] public float scalePeakPercent = 0.2f;

        [Header("整体大小倍率")]
        [Range(0.5f, 2f)]   public float sizeMultiplier = 1f;
    }

    [CreateAssetMenu(fileName = "FloatingTextConfig", menuName = "YourGame/FloatingTextConfig")]
    public class FloatingTextConfig : ScriptableObject
    {
        [Header("对象池")]
        [Range(10, 50)] public int prewarmCount = 20;
        [Range(20, 100)] public int maxPoolSize = 40;

        [Header("Prefab 引用（基础）")]
        public GameObject normalPrefab;
        public GameObject critPrefab;
        public GameObject bossShieldPrefab;
        public GameObject bossCorePrefab;
        public GameObject statusPrefab;

        [Header("优先级（越大越不容易被回收）")]
        public int normalPriority      = 0;
        public int critPriority        = 3;
        public int statusPriority      = 1;
        public int bossShieldPriority  = 1;
        public int bossCorePriority    = 2;

        [Header("各类型样式（在 Inspector 调）")]
        public FloatingTextStyle normalStyle = new FloatingTextStyle();
        public FloatingTextStyle critStyle = new FloatingTextStyle();
        public FloatingTextStyle bossShieldStyle = new FloatingTextStyle();
        public FloatingTextStyle bossCoreStyle = new FloatingTextStyle();
        public FloatingTextStyle statusStyle = new FloatingTextStyle();
        // …其他样式按需扩展

        public FloatingTextStyle GetStyle(FloatingTextType type)
        {
            switch (type)
            {
                case FloatingTextType.Crit:       return critStyle;
                case FloatingTextType.BossShield: return bossShieldStyle;
                case FloatingTextType.BossCore:   return bossCoreStyle;
                case FloatingTextType.Status:     return statusStyle;
                default:                          return normalStyle;
            }
        }

        public int GetPriority(FloatingTextType type)
        {
            switch (type)
            {
                case FloatingTextType.Crit:       return critPriority;
                case FloatingTextType.BossShield: return bossShieldPriority;
                case FloatingTextType.BossCore:   return bossCorePriority;
                case FloatingTextType.Status:     return statusPriority;
                default:                          return normalPriority;
            }
        }

        public GameObject GetPrefab(FloatingTextType type)
        {
            switch (type)
            {
                case FloatingTextType.Crit:       return critPrefab       != null ? critPrefab       : normalPrefab;
                case FloatingTextType.BossShield: return bossShieldPrefab != null ? bossShieldPrefab : normalPrefab;
                case FloatingTextType.BossCore:   return bossCorePrefab   != null ? bossCorePrefab   : normalPrefab;
                case FloatingTextType.Status:     return statusPrefab     != null ? statusPrefab     : normalPrefab;
                default:                          return normalPrefab;
            }
        }
    }
}
```

### 3. FloatingText.cs（单条飘字组件，挂在 Prefab 上）

```csharp
using UnityEngine;
using TMPro;

namespace YourGame.UI.FloatingText
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FloatingText : MonoBehaviour
    {
        private TextMeshProUGUI textMesh;
        private RectTransform   rectTransform;
        private CanvasGroup     canvasGroup;
        private Canvas          parentCanvas;
        private Camera          worldCamera;

        private FloatingTextType currentType;
        private int   priority;
        private float elapsedTime, duration;
        private bool  isPlaying;
        private Vector2 velocity;
        private float gravity, fadeStartPercent;
        private bool  useScaleAnimation;
        private float initialScale, peakScale, scalePeakPercent;
        private System.Action<FloatingText> onComplete;

        public FloatingTextType CurrentType => currentType;
        public int   Priority         => priority;
        public bool  IsPlaying        => isPlaying;
        public float RemainingPercent => isPlaying ? 1f - (elapsedTime / duration) : 0f;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (!isPlaying) return;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            if (t >= 1f) { Complete(); return; }

            // 位置：初速度 + 重力 + 水平随机
            velocity.y -= gravity * Time.deltaTime;
            rectTransform.anchoredPosition += velocity * Time.deltaTime;

            // 透明度：fadeStartPercent 后线性淡出
            canvasGroup.alpha = t < fadeStartPercent
                ? 1f
                : 1f - (t - fadeStartPercent) / (1f - fadeStartPercent);

            // 缩放：EaseOutBack 回弹
            if (useScaleAnimation)
            {
                float scale = t < scalePeakPercent
                    ? Mathf.Lerp(initialScale, peakScale, EaseOutBack(t / scalePeakPercent))
                    : Mathf.Lerp(peakScale, 1f, (t - scalePeakPercent) / (1f - scalePeakPercent));
                rectTransform.localScale = Vector3.one * scale;
            }
        }

        public void Play(string text, Vector3 worldPos,
                         FloatingTextType type, FloatingTextStyle style, int typePriority,
                         Canvas targetCanvas, Camera projectionCamera,
                         System.Action<FloatingText> completeCallback)
        {
            currentType = type;
            priority    = typePriority;
            parentCanvas = targetCanvas;
            worldCamera  = projectionCamera;
            onComplete   = completeCallback;

            // 文本与样式
            textMesh.text          = text;
            textMesh.color         = style.textColor;
            textMesh.fontSize      = style.fontSize;
            textMesh.fontStyle     = style.isBold ? FontStyles.Bold : FontStyles.Normal;
            textMesh.outlineColor  = style.outlineColor;
            textMesh.outlineWidth  = style.outlineWidth;

            // 世界坐标 → 屏幕坐标 → Canvas 局部坐标
            Camera cam = worldCamera != null ? worldCamera : Camera.main;
            Vector3 screenPos = cam != null ? cam.WorldToScreenPoint(worldPos) : worldPos;
            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            Camera uiCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
            if (canvasRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out var localPoint))
                rectTransform.anchoredPosition = localPoint;
            else
                rectTransform.position = screenPos;

            // 动画参数
            duration         = style.duration;
            gravity          = style.gravity;
            fadeStartPercent = style.fadeStartPercent;
            float horizontalSpeed = Random.Range(-style.horizontalRandomRange, style.horizontalRandomRange);
            velocity = new Vector2(horizontalSpeed, style.initialUpSpeed);

            useScaleAnimation = style.useScaleAnimation;
            initialScale      = style.initialScale;
            peakScale         = style.peakScale;
            scalePeakPercent  = style.scalePeakPercent;
            rectTransform.localScale = useScaleAnimation
                ? Vector3.one * initialScale
                : Vector3.one;

            canvasGroup.alpha = 1f;
            isPlaying    = true;
            elapsedTime  = 0f;
            gameObject.SetActive(true);
        }

        public void ForceStop() => Complete();

        public void Reset()
        {
            isPlaying = false;
            elapsedTime = 0f;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (rectTransform != null) rectTransform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }

        private void Complete()
        {
            isPlaying = false;
            gameObject.SetActive(false);
            onComplete?.Invoke(this);
        }

        // EaseOutBack —— 超出后回弹（弹性感）
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
```

### 4. FloatingTextManager.cs（单例 + 对象池）

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourGame.UI.FloatingText
{
    public class FloatingTextManager : Singleton<FloatingTextManager>  // ← 来自 02_SingletonPattern
    {
        [Header("配置")]
        [SerializeField] private FloatingTextConfig config;
        [SerializeField] private Canvas targetCanvas;

        private Dictionary<FloatingTextType, Queue<FloatingText>> typePools = new();
        private List<FloatingText> activeTexts = new();
        private Transform poolContainer;
        private int totalCreated;
        private bool isInitialized;
        private Camera worldProjectionCamera;

        // 等一帧再初始化，确保所有 UI 组件已就绪
        private IEnumerator Start()
        {
            yield return null;
            if (Instance == this && !isInitialized) Initialize();
        }

        private void Initialize()
        {
            if (config == null) { Debug.LogError("FloatingTextConfig 未设置"); return; }
            if (targetCanvas == null) targetCanvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
            if (targetCanvas == null) { Debug.LogError("找不到 Canvas"); return; }

            worldProjectionCamera = Camera.main;

            // 池容器铺满整个 Canvas（避免 Anchor 偏移影响子物体世界坐标转换）
            var containerGO = new GameObject("[FloatingTextPool]");
            containerGO.transform.SetParent(transform, false);
            var rt = containerGO.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;
            poolContainer = containerGO.transform;

            // 各类型空池
            foreach (FloatingTextType type in System.Enum.GetValues(typeof(FloatingTextType)))
                typePools[type] = new Queue<FloatingText>();

            // 预热高频类型（按你的项目调整）
            PrewarmType(FloatingTextType.Normal, config.prewarmCount / 2);
            PrewarmType(FloatingTextType.Crit,   config.prewarmCount / 4);
            PrewarmType(FloatingTextType.BossShield, 5);
            PrewarmType(FloatingTextType.BossCore,   5);

            isInitialized = true;
        }

        private void PrewarmType(FloatingTextType type, int count)
        {
            var prefab = config.GetPrefab(type);
            if (prefab == null) return;
            for (int i = 0; i < count; i++)
            {
                var ft = CreateInstance(type, prefab);
                if (ft != null) { ft.gameObject.SetActive(false); typePools[type].Enqueue(ft); }
            }
        }

        private FloatingText CreateInstance(FloatingTextType type, GameObject prefab)
        {
            if (prefab == null || poolContainer == null) return null;
            var go = Instantiate(prefab, poolContainer);
            go.name = $"FloatingText_{type}_{totalCreated:D3}";
            var ft = go.GetComponent<FloatingText>() ?? go.AddComponent<FloatingText>();
            totalCreated++;
            return ft;
        }

        // ─── 业务接口 ────────────────────────────────────────

        public void ShowDamage(Vector3 worldPos, float damage, bool isCrit = false)
            => Show(worldPos, Mathf.RoundToInt(damage).ToString(),
                    isCrit ? FloatingTextType.Crit : FloatingTextType.Normal);

        public void ShowBossCoreDamage(Vector3 worldPos, float damage, bool isCrit = false)
            => Show(worldPos, Mathf.RoundToInt(damage).ToString(),
                    isCrit ? FloatingTextType.Crit : FloatingTextType.BossCore);

        public void ShowBossShieldDamage(Vector3 worldPos, float damage)
            => Show(worldPos, Mathf.RoundToInt(damage).ToString(), FloatingTextType.BossShield);

        public void ShowExecution(Vector3 worldPos)
            => Show(worldPos, "EXECUTE!", FloatingTextType.Execution);

        public void ShowStatus(Vector3 worldPos, string statusText)
            => Show(worldPos, statusText, FloatingTextType.Status);

        public void ShowPlayerHealthDamage(Vector3 worldPos, int damage)
            => Show(worldPos, $"-{damage}", FloatingTextType.PlayerHealthDamage);

        // 通用入口
        public void Show(Vector3 worldPosition, string text, FloatingTextType type)
        {
            if (!isInitialized) Initialize();
            if (!isInitialized) return;

            var ft = GetInstance(type);
            if (ft == null) return;

            var style    = config.GetStyle(type);
            var priority = config.GetPriority(type);
            ft.Play(text, worldPosition, type, style, priority, targetCanvas, worldProjectionCamera, OnFloatingTextComplete);
            activeTexts.Add(ft);
        }

        public void ReturnAll()
        {
            foreach (var ft in new List<FloatingText>(activeTexts))
                if (ft != null) ft.ForceStop();
            activeTexts.Clear();
        }

        // ─── 对象池获取 ───────────────────────────────────────

        private FloatingText GetInstance(FloatingTextType requestType)
        {
            if (!typePools.ContainsKey(requestType)) typePools[requestType] = new Queue<FloatingText>();
            var pool = typePools[requestType];

            // 1. 池里有 → 直接取
            if (pool.Count > 0) return pool.Dequeue();

            // 2. 没超上限 → 动态创建
            if (totalCreated < config.maxPoolSize && poolContainer != null)
            {
                var prefab = config.GetPrefab(requestType);
                if (prefab != null) return CreateInstance(requestType, prefab);
            }

            // 3. 优先级回收（找一个比当前优先级低 + 剩余时间最少的）
            return TryRecycleLowPriority(requestType);
        }

        private FloatingText TryRecycleLowPriority(FloatingTextType requestType)
        {
            int requestPriority = config.GetPriority(requestType);
            FloatingText candidate = null;
            float minScore = float.MaxValue;

            foreach (var ft in activeTexts)
            {
                if (ft == null || !ft.IsPlaying) continue;
                if (ft.Priority > requestPriority) continue;   // 不抢更高优先级
                float score = ft.Priority * 100f + ft.RemainingPercent * 100f;
                if (score < minScore) { minScore = score; candidate = ft; }
            }

            if (candidate != null)
            {
                activeTexts.Remove(candidate);
                candidate.Reset();
            }
            return candidate;
        }

        private void OnFloatingTextComplete(FloatingText ft)
        {
            if (ft == null) return;
            activeTexts.Remove(ft);
            ft.Reset();
            var type = ft.CurrentType;
            if (!typePools.ContainsKey(type)) typePools[type] = new Queue<FloatingText>();
            typePools[type].Enqueue(ft);
        }
    }
}
```

---

## Prefab 设置

### 飘字 Prefab 必备组件

```
[FloatingTextNormal] (Prefab，挂在 Canvas 下任意位置)
├── RectTransform        — anchor / pivot 自定（推荐居中）
├── TextMeshProUGUI      — 文本组件，FloatingText 会读写
├── CanvasGroup          — FloatingText 自动添加（用于淡出）
└── FloatingText 脚本   — 挂在根节点
```

### 各类型 Prefab 命名建议

- `FloatingTextNormal`：普通伤害基础 Prefab（必填）
- `FloatingTextCrit`：暴击专属（字号大、有描边）
- `FloatingTextBossCore`：Boss 弱点（红色 + 眼睛图标）
- `FloatingTextBossShield`：Boss 护甲（小字银灰 + 盾图标）

字号、颜色、动画参数都在 `FloatingTextConfig` ScriptableObject 里改，**不要在 Prefab 上硬写**——便于策划只改 Config 就调全局观感。

---

## 关键设计决策（踩坑总结）

1. **多 Prefab，不是单 Prefab + 运行时换样式**——TMP 的字体/描边切换在 WebGL 上有 GC 开销，不如准备多份 Prefab
2. **`Update()` 自驱动，不用协程**——飘字数量大时（同屏 50+），协程的状态机开销比 Update 高
3. **优先级 = 类型优先级 × 100 + 剩余百分比**——优先回收低优先级且快结束的；保护暴击/Boss 飘字不被普通飘字挤掉
4. **池容器用 RectTransform 铺满 Canvas**——避免父物体的 anchor 偏移影响子飘字的世界坐标转换
5. **`Initialize` 等一帧再做（Start 协程）**——`Awake` 里 Canvas 可能还没就绪
6. **`ScreenPointToLocalPointInRectangle` 兼容三种 Canvas 模式**：
   - `ScreenSpaceOverlay`：`uiCamera = null`
   - `ScreenSpaceCamera` / `WorldSpace`：`uiCamera = canvas.worldCamera`
7. **`ReturnAll()` 必须在关卡切换、玩家死亡时调用**——否则切场景时正在播放的飘字会带着旧的 worldCamera 引用进新场景，错位
8. **`EaseOutBack` 缓动是暴击的灵魂**——别用 `EaseOutQuad`，回弹感差很多

---

## 与其他模板的关系

| 模板 | 配合 |
|------|------|
| [[02_SingletonPattern]] | `FloatingTextManager` 继承自这里的 `Singleton<T>` 基类 |
| [[09_CoinFlyAnimation]] | 拾取金币时：先飞行（09），落点用本模板飘"+1"（白色小字） |
| [[07_UIAnimationHelper]] | 拼装组合反馈时（HP 栏被打 → punch 栏 + 数字滚动 + 飘字） |
| [[04_AudioManager]] | 暴击 / 处决飘字配合音效更爽 |
| [[40_知识/02_引擎与技术/数据埋点与BattleLog工程落地]] | 暴击次数可作为 BattleLog 字段，在飘字触发处埋点 |

---

## 注意事项

- **TMP 必须导入**：项目设置 → Package Manager → 装 TextMeshPro
- **`FloatingTextConfig` 资产**：在 Project 窗口右键 → Create → YourGame → FloatingTextConfig，配好各类型 Prefab 和样式
- **Canvas 缩放模式**：本模板假设 Canvas 用 `Constant Pixel Size` 或 `Scale With Screen Size`，`Constant Physical Size` 下 fontSize 单位会变化
- **大量飘字（>40 同屏）建议**：调高 `maxPoolSize`、关掉 `useScaleAnimation`（CPU 省一半）
- **关卡切换/玩家死亡** 必须调 `ReturnAll()`，不然旧飘字会带着失效的 Camera 引用错位
