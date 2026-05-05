# UGUI 挖孔遮罩模板库

SDF Shader 单图层挖孔方案，适用于新手引导「聚光灯/高亮」效果。

## 优势（对比传统四面板几何切割）

| 特性 | 四面板几何切割 | SDF Shader（本方案）|
|------|--------------|-------------------|
| DrawCall | 4 个 Image = 4次 | 1 个 Image = 1次 |
| 圆角支持 | 不支持 | 支持，可调 |
| 边缘抗锯齿 | 硬边锯齿 | smoothstep 软边 |
| 点击穿透 | 需要特殊处理 | C# SDF 镜像算法 |
| 分辨率适配 | 需重算布局 | 归一化坐标自适应 |

---

## 文件清单

| 文件 | 说明 |
|------|------|
| `UIHoleMask.shader` | SDF 挖孔 Shader（放入项目 `Assets/Shaders/` 目录） |
| `HoleMaskController.cs` | 遮罩控制器，管理 Shader 参数 + C# 点击检测 |
| `HoleMaskClickBlocker.cs` | 点击穿透组件，孔洞内穿透/孔洞外拦截 |

---

## 接入步骤

### 1. 导入 Shader

将 `UIHoleMask.shader` 复制到项目 `Assets/Shaders/` 目录（或任意 Shaders 子目录）。

### 2. 创建遮罩 GameObject

在 Canvas 下创建一个全屏 Image：
- `anchorMin = (0, 0)`，`anchorMax = (1, 1)`，`offsetMin/Max = (0, 0)`
- 挂载 `HoleMaskController` 脚本
- 挂载 `HoleMaskClickBlocker` 脚本（可选，用于拦截遮罩区域点击）
- Image 的 `Raycast Target` 设为 `true`

### 3. 调用接口

```csharp
// 获取遮罩控制器
HoleMaskController holeMask = GetComponent<HoleMaskController>();

// 设置挖孔目标（传入需要高亮的 RectTransform）
holeMask.SetHoleTarget(targetRectTransform);

// 隐藏孔洞（全屏遮罩）
holeMask.HideHole();
```

### 4. 修改命名空间

两个 .cs 文件的 `namespace Template.UI` 请根据项目改为实际命名空间，例如：
- `namespace YourProject.UI`
- `namespace LightVsDecay.UI.Tutorial`

---

## 参数说明

### Inspector 参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `maskColor` | `(0,0,0,0.8)` | 遮罩颜色和不透明度 |
| `cornerRadius` | `20f` (像素) | 挖孔圆角半径 |
| `holePadding` | `(20,20)` (像素) | 挖孔区域在目标四周的扩展边距 |

### Shader 参数（运行时由脚本自动设置）

| 参数 | 类型 | 说明 |
|------|------|------|
| `_HoleCenter` | Vector2（归一化） | 孔洞中心（屏幕坐标归一化） |
| `_HoleSize` | Vector2（归一化） | 孔洞宽高（归一化） |
| `_CornerRadius` | Float（归一化） | 圆角半径（归一化） |
| `_Color` | Color | 遮罩颜色 |

---

## 运行时调试

在 Inspector 中勾选 `Enable Runtime Debug`，即可在 Play 模式实时拖动以下参数观察效果：
- `debugHoleCenter`：调整孔洞位置
- `debugHoleSize`：调整孔洞大小
- `debugCornerRadius`：调整圆角

调试完毕后，使用右键菜单「复制当前参数到调试字段」可将自动计算的结果固化。

---

## 注意事项

1. **Shader 编译**：首次导入后需等待 Unity 编译 Shader，如 Material 显示粉色，重启 Editor 或手动 Import All。
2. **Canvas 模式**：Screen Space - Overlay 和 Screen Space - Camera 均支持，World Space 未测试。
3. **Material 实例**：`HoleMaskController` 在运行时通过 `new Material(shader)` 创建实例，不会影响共享 Material。`OnDestroy` 会自动清理。
4. **多遮罩**：可以同时存在多个 HoleMaskController 实例，各自独立计算，互不干扰。

---

## 进阶：配置驱动的 TutorialDirector 接入方案

以下是在光与朽（LightVSDecay）项目中验证的完整接入模式，适合有多个引导步骤、需要设计师自由配置挖孔位置的项目。

### 架构概览

```
TutorialStepConfigSO (ScriptableObject)
  ├── prefabPath        → 手指特效 Resources 路径
  ├── parentPath        → Prefab 挂载父节点路径
  ├── localPosition     → 手指相对目标的偏移
  ├── useSpotlightOverlay → 是否启用 SDF 挖孔
  ├── holePadding       → 挖孔扩展边距
  └── overlayMessage    → 遮罩上的引导文案

TutorialDirector (MonoBehaviour)
  ├── 每步：ShowStep(config, target)
  │     ├── HoleMaskController.Show(target, config.holePadding)
  │     └── 实例化手指 Prefab，定位到 target 旁边
  └── 完成：ClearStep()
        ├── HoleMaskController.Hide()
        └── Destroy 手指 Prefab
```

### TutorialStepConfigSO 模板

```csharp
[CreateAssetMenu(menuName = "YourProject/Tutorial Step Config")]
public class TutorialStepConfigSO : ScriptableObject
{
    public string  stepID;
    public string  prefabPath = "Effects/Tutorial/FingerPointer";
    public string  parentPath = "";          // 留空 = 挂在 target 的父节点
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localScale    = Vector3.one;
    public bool    useSpotlightOverlay = true;
    public Vector2 holePadding   = new Vector2(24f, 24f);
    [TextArea(1, 3)]
    public string  overlayMessage;
}
```

### TutorialDirector 核心方法

```csharp
private void ShowStep(TutorialStepConfigSO config, RectTransform target)
{
    if (target == null) return;

    // 1. SDF 挖孔遮罩
    if (config != null && config.useSpotlightOverlay && holeMask != null)
        holeMask.Show(target, config.overlayMessage, config.holePadding);
    else
        holeMask?.Hide();

    // 2. 手指特效 Prefab
    DestroyFingerObject();
    if (config != null && !string.IsNullOrEmpty(config.prefabPath))
        SpawnFingerPrefab(config, target);
}

private void ClearStep()
{
    holeMask?.Hide();
    DestroyFingerObject();
}

private void SpawnFingerPrefab(TutorialStepConfigSO config, RectTransform target)
{
    var prefab = Resources.Load<GameObject>(config.prefabPath);
    if (prefab == null) return;

    _fingerObj = Instantiate(prefab);
    Transform parent = string.IsNullOrEmpty(config.parentPath)
        ? target.parent
        : GameObject.Find(config.parentPath)?.transform ?? target.parent;

    _fingerObj.transform.SetParent(parent, false);
    var rect = _fingerObj.transform as RectTransform;
    if (rect != null)
    {
        // 与 target 同父时，以 target 的 anchoredPosition 为基准
        if (target.parent == parent)
            rect.anchoredPosition = target.anchoredPosition + (Vector2)config.localPosition;
        else
            rect.localPosition = config.localPosition;
        rect.localScale = config.localScale;
    }
}
```

### HoleMaskController 需增加的接口

在 `HoleMaskController.cs` 中增加带 `padding` 和 `message` 的 `Show` 重载：

```csharp
/// <summary>
/// 显示挖孔遮罩，聚焦 target，自动跟踪位置
/// </summary>
public void Show(RectTransform target, string message = null, Vector2? padding = null, bool track = true)
{
    if (target == null) return;
    _target   = target;
    _padding  = padding ?? holePadding;
    _tracking = track;
    UpdateHoleShader();
    SetMessage(message);
    gameObject.SetActive(true);
}

public void Hide()
{
    _target = null;
    gameObject.SetActive(false);
}
```

### 完整引导生命周期

```
场景加载
  └── TutorialDirector.Start()
        └── 检查 PlayerPrefs 标志位
              ├── 已完成 → 不启动引导
              └── 未完成 → 等待触发条件（如打开科技树界面）
                    ↓
              ShowStep(step1Config, targetRect)
                    → SDF 挖孔 + 手指 Prefab 出现
              玩家操作（点击/滑动/等待）
                    → 检测到完成条件
              ShowStep(step2Config, nextTargetRect)
                    → 切换到下一步（自动销毁上一步手指 Prefab）
              ...所有步骤完成
              ClearStep()
                    → 隐藏遮罩 + 销毁 Prefab
              写 PlayerPrefs 标志位（永久不再显示）
```

### 点击穿透配置

`HoleMaskClickBlocker`（或直接在 `HoleMaskController` 内实现 `IPointerClickHandler`）：

```csharp
public void OnPointerDown(PointerEventData eventData)
{
    // 点击在孔洞内 → 穿透
    if (IsPointInHole(eventData.position)) return;

    // 点击在遮罩区域 → 拦截
    eventData.Use();
}

private bool IsPointInHole(Vector2 screenPoint)
{
    // 与 Shader 完全一致的 C# SDF 镜像
    Vector2 uv   = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
    Vector2 diff = uv - _holeCenter;
    float   dist = RoundedBoxSDF(diff, _holeSize * 0.5f, _cornerRadius);
    return dist < 0f;
}

private static float RoundedBoxSDF(Vector2 p, Vector2 halfSize, float r)
{
    Vector2 q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - halfSize + new Vector2(r, r);
    return Mathf.Min(Mathf.Max(q.x, q.y), 0f) + new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude - r;
}
```
