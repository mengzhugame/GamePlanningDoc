# Unity C# 代码规范

> 创建日期: 2026-02-07
> 适用范围: 所有Unity游戏项目
> 来源: 光与朽 + 美妆叠叠乐项目经验总结

---

## 一、文件规范

### 1.1 文件头注释（必须）
```csharp
// ============================================================
// ClassName.cs
// 文件位置: Assets/Scripts/模块/ClassName.cs
// 用途：简要说明这个类的作用
// 更新：如有重大更新，在这里记录
// ============================================================
```

### 1.2 文件命名
- 类名与文件名必须一致
- 使用 PascalCase（大驼峰）
- 接口以 `I` 开头：`IPoolable.cs`
- 抽象类可用 `Base` 后缀：`BasePowerUp.cs`

---

## 二、命名规范

### 2.1 类与接口
```csharp
public class GameManager { }           // PascalCase
public interface IPoolable { }         // I前缀
public abstract class BasePanel { }    // Base前缀（可选）
public enum GameState { }              // PascalCase
```

### 2.2 方法
```csharp
public void StartGame() { }            // PascalCase，动词开头
private void HandleEnemyDied() { }     // 事件处理用Handle前缀
protected virtual void OnSingletonAwake() { }  // 生命周期用On前缀
```

### 2.3 变量
```csharp
// 私有字段：camelCase
private float gameTimer = 0f;
private bool isPlaying = false;

// 公共属性：PascalCase
public float GameTimer => gameTimer;
public bool IsPlaying => isPlaying;

// 常量：UPPER_SNAKE_CASE
private const float FROST_SPREAD_RADIUS = 1.5f;
private const string PREF_BGM_VOLUME = "BGMVolume";

// SerializeField：camelCase
[SerializeField] private GameObject bossPrefab;
[SerializeField] private float spawnOffset = 1.5f;
```

### 2.4 布尔变量命名
```csharp
// 使用 is/has/can/should 前缀
private bool isPlaying;
private bool hasInstance;
private bool canShoot;
private bool shouldAutoSave;
```

---

## 三、代码组织

### 3.1 区域分隔符
```csharp
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// 配置引用
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Header("═══ 基础配置 ═══")]
[SerializeField] private GameSettings settings;
```

### 3.2 代码顺序（推荐）
```csharp
public class ExampleClass : MonoBehaviour
{
    // 1. 常量
    private const float MAX_SPEED = 10f;
    
    // 2. 静态字段
    private static ExampleClass instance;
    
    // 3. SerializeField 字段
    [SerializeField] private float speed;
    
    // 4. 私有字段
    private float timer;
    
    // 5. 公共属性
    public float Speed => speed;
    
    // 6. Unity 生命周期
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    private void OnDestroy() { }
    
    // 7. 公共方法
    public void DoSomething() { }
    
    // 8. 私有方法
    private void HandleInternal() { }
    
    // 9. 事件回调
    private void OnEventTriggered() { }
    
    // 10. 调试方法（条件编译）
#if UNITY_EDITOR
    private void OnGUI() { }
#endif
}
```

---

## 四、Inspector 属性

### 4.1 使用 Header 分组
```csharp
[Header("═══ 基础配置 ═══")]
[Tooltip("游戏设置")]
[SerializeField] private GameSettings settings;

[Header("═══ 调试 ═══")]
[SerializeField] private bool showDebugInfo = false;
```

### 4.2 使用 Tooltip 说明
```csharp
[Tooltip("敌人检测层（Enemy Layer - 普通敌人 + Boss护甲）")]
[SerializeField] private LayerMask enemyLayer;
```

### 4.3 使用 Range 限制
```csharp
[Range(0f, 1f)]
[SerializeField] private float critRate = 0.1f;
```

---

## 五、事件与委托

### 5.1 事件命名
```csharp
// 使用 On 前缀
public static event Action OnGameStart;
public static event Action<int> OnLevelUp;
public static event Action<Vector3, int> OnEnemyDied;
```

### 5.2 事件触发方法
```csharp
// 使用 Trigger 前缀
public static void TriggerGameStart() => OnGameStart?.Invoke();
public static void TriggerLevelUp(int level) => OnLevelUp?.Invoke(level);
```

### 5.3 订阅与取消
```csharp
private void OnEnable()
{
    GameEvents.OnGameStart += HandleGameStart;
}

private void OnDisable()
{
    GameEvents.OnGameStart -= HandleGameStart;
}
```

---

## 六、Expression Body（简化语法）

### 6.1 只读属性
```csharp
// 推荐
public float CurrentWidth => baseWidth * multiplier;

// 不推荐
public float CurrentWidth
{
    get { return baseWidth * multiplier; }
}
```

### 6.2 简单方法
```csharp
// 推荐
public void TriggerVictory() => Victory();

// 不推荐
public void TriggerVictory()
{
    Victory();
}
```

---

## 七、注释规范

### 7.1 XML 文档注释
```csharp
/// <summary>
/// 从池中获取对象
/// </summary>
/// <param name="position">生成位置</param>
/// <param name="rotation">生成旋转</param>
/// <returns>对象实例，如果池已满返回null</returns>
public T Get(Vector3 position, Quaternion rotation)
```

### 7.2 内联注释
```csharp
// 等待一帧，让所有 Start() 执行完毕
yield return null;

// 【修改】自动获取控制器并调用 Show
if (skillChooseController == null)
{
    skillChooseController = skillChoosePanel.GetComponent<SkillChooseOnePanel>();
}
```

### 7.3 TODO 注释
```csharp
// TODO: 优化性能，考虑使用 Job System
// FIXME: 偶尔出现空引用，需要排查
// HACK: 临时解决方案，后续重构
```

---

## 八、调试代码规范

### 8.1 使用条件编译
```csharp
#if UNITY_EDITOR
private void OnGUI()
{
    // 仅编辑器显示的调试UI
}
#endif
```

### 8.2 使用调试开关
```csharp
[Header("调试")]
[SerializeField] private bool showDebugInfo = false;

private void LogDebug(string message)
{
    if (showDebugInfo)
    {
        Debug.Log($"[{GetType().Name}] {message}");
    }
}
```

### 8.3 发布前清理
- 移除 `Debug.Log`（或使用条件编译）
- 关闭 `showDebugInfo`
- 移除调试用的 `[ContextMenu]`

---

## 九、性能规范

### 9.1 避免在 Update 中分配内存
```csharp
// 不推荐
void Update()
{
    var enemies = FindObjectsOfType<Enemy>(); // 每帧分配
}

// 推荐
private List<Enemy> cachedEnemies = new List<Enemy>();
void Update()
{
    // 使用缓存
}
```

### 9.2 使用对象池
```csharp
// 不推荐
Instantiate(bulletPrefab);
Destroy(bullet);

// 推荐
var bullet = bulletPool.Get(pos, rot);
bulletPool.Return(bullet);
```

### 9.3 缓存组件引用
```csharp
// 不推荐
void Update()
{
    GetComponent<Rigidbody>().velocity = newVel;
}

// 推荐
private Rigidbody rb;
void Awake() => rb = GetComponent<Rigidbody>();
void Update() => rb.velocity = newVel;
```

---

## 十、ScriptableObject 规范

### 10.1 CreateAssetMenu 属性
```csharp
[CreateAssetMenu(fileName = "GameSettings", menuName = "YourGame/Game Settings", order = 0)]
public class GameSettings : ScriptableObject
```

### 10.2 提供便捷方法
```csharp
/// <summary>
/// 计算指定等级升级所需经验
/// </summary>
public int CalculateExpToNextLevel(int level)
{
    return expBase + (level - 1) * expGrowth;
}
```

### 10.3 编辑器工具
```csharp
#if UNITY_EDITOR
[ContextMenu("打印经验表")]
public void PrintExpTable()
{
    // 方便测试和验证
}

[ContextMenu("验证数值")]
public void ValidateSettings()
{
    // 检查配置是否合理
}
#endif
```

---

## 十一、Git 提交规范

### 11.1 提交消息格式
```
<type>: <subject>

<body>

<footer>
```

### 11.2 Type 类型
- `feat`: 新功能
- `fix`: Bug修复
- `refactor`: 重构（不改变功能）
- `style`: 格式调整（不影响代码逻辑）
- `docs`: 文档更新
- `perf`: 性能优化
- `test`: 测试相关
- `chore`: 构建/工具相关

### 11.3 示例
```
feat: 添加Boss战激光穿透功能

- 新增 Focus 技能穿透机制
- 支持穿透衰减配置
- Boss 真实伤害选项

Closes #123
```

---

## 十二、检查清单

### 代码审查清单
- [ ] 文件头注释是否完整
- [ ] 命名是否符合规范
- [ ] 是否使用了 Header/Tooltip
- [ ] 事件是否正确订阅/取消
- [ ] 是否有未清理的调试代码
- [ ] 是否有性能问题（Update中分配等）
- [ ] 是否使用对象池（高频创建销毁）
- [ ] 是否缓存了组件引用

### 发布前清单
- [ ] 关闭所有 showDebugInfo
- [ ] 移除调试用的 Debug.Log
- [ ] 检查 PlayerPrefs 键名
- [ ] 验证 ScriptableObject 配置

---

*此规范持续更新，根据项目经验不断完善*
