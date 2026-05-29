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
| `HoleMaskClickBlocker.cs` | 基于 `ICanvasRaycastFilter` 的点击穿透组件，孔洞内穿透/孔洞外拦截 |

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
| `holeShape` | `RoundedRect` | 孔洞形状：`Rect` / `RoundedRect` / `Circle` |

### Shader 参数（运行时由脚本自动设置）

| 参数 | 类型 | 说明 |
|------|------|------|
| `_HoleCenter` | Vector2（归一化） | 孔洞中心（屏幕坐标归一化） |
| `_HoleSize` | Vector2（归一化） | 孔洞宽高（归一化） |
| `_CornerRadius` | Float（归一化） | 圆角半径（归一化） |
| `_HoleShape` | Float | 0=直角矩形，1=圆角矩形，2=正圆 |
| `_Color` | Color | 遮罩颜色 |

---

## 运行时调试

在 Inspector 中勾选 `Enable Runtime Debug`，即可在 Play 模式实时拖动以下参数观察效果：
- `debugTarget`：拖入任意 RectTransform，孔洞自动对齐目标
- `debugPadding`：实时调整目标四周边距
- `debugCornerRadiusPixels`：实时调整像素圆角
- `debugHoleShape`：切换直角矩形 / 圆角矩形 / 正圆
- `debugHoleCenter`：调整孔洞位置
- `debugHoleSize`：调整孔洞大小
- `debugCornerRadius`：调整圆角

如果只想调某个真实按钮，优先使用 `debugTarget + debugPadding + debugCornerRadiusPixels`。调好后把数值填入对应 `TutorialStepConfigSO` 的 `holePadding` / `cornerRadius` / `holeShape`。

---

## 注意事项

1. **Shader 编译**：首次导入后需等待 Unity 编译 Shader，如 Material 显示粉色，重启 Editor 或手动 Import All。
2. **Canvas 模式**：Screen Space - Overlay 和 Screen Space - Camera 均支持，World Space 未测试。
3. **Material 实例**：`HoleMaskController` 在运行时通过 `new Material(shader)` 创建实例，不会影响共享 Material。`OnDestroy` 会自动清理。
4. **多遮罩**：可以同时存在多个 HoleMaskController 实例，各自独立计算，互不干扰。
5. **点击穿透必须用 `ICanvasRaycastFilter`**：不要用 `IPointerClickHandler` 在回调里 return。回调阶段射线已经被遮罩吃掉，下层按钮不会再收到事件。
6. **点击检测用像素坐标**：Shader 可以吃归一化参数，但 C# 的 `IsRaycastLocationValid` 收到的是屏幕像素坐标。C# SDF 应直接用像素空间，避免 Canvas Scaler 下“视觉洞里点不到”的误差。
7. **手指 / 光圈 Prefab 要关闭 raycastTarget**：引导特效如果是 UI Image，默认会挡住孔洞里的按钮。实例化后用 `GetComponentsInChildren<Graphic>(true)` 递归关闭所有 `raycastTarget`。

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
  │     ├── HoleMaskController.SetHoleTarget(target, config.holePadding, config.cornerRadius, config.holeShape)
  │     └── 实例化手指 Prefab，定位到 target 旁边
  └── 完成：ClearStep()
        ├── HoleMaskController.HideHole()
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
    public float   cornerRadius  = 20f;
    public HoleShape holeShape   = HoleShape.RoundedRect;
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
        holeMask.SetHoleTarget(target, config.holePadding, config.cornerRadius, config.holeShape);
    else
        holeMask?.HideHole();

    // 2. 手指特效 Prefab
    DestroyFingerObject();
    if (config != null && !string.IsNullOrEmpty(config.prefabPath))
        SpawnFingerPrefab(config, target);
}

private void ClearStep()
{
    holeMask?.HideHole();
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
        // 用目标视觉包围盒中心定位，兼容左上角锚点 / 右上角锚点等非中心 pivot。
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        var parentRect = parent as RectTransform;
        var canvas = target.GetComponentInParent<Canvas>()?.rootCanvas;
        Camera cam = canvas != null ? canvas.worldCamera : null;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);
        if (parentRect != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, cam, out var localPoint))
        {
            rect.anchoredPosition = localPoint + (Vector2)config.localPosition;
        }
        else
        {
            rect.position = worldCenter + config.localPosition;
        }

        rect.localScale = config.localScale;
    }

    // 引导特效本身不能挡住孔洞里的按钮。
    foreach (var graphic in _fingerObj.GetComponentsInChildren<Graphic>(true))
        graphic.raycastTarget = false;
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

`HoleMaskClickBlocker` 必须实现 `ICanvasRaycastFilter`，让孔洞在射线检测阶段穿透：

```csharp
public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
{
    // false = 当前遮罩不接收射线，下层按钮继续参与 GraphicRaycaster 检测
    if (_maskController != null && _maskController.IsPointInHole(sp))
        return false;

    // true = 孔洞外由遮罩接收射线，拦截误触
    return true;
}
```

`IsPointInHole` 使用像素坐标：

```csharp
private bool IsPointInHole(Vector2 screenPoint)
{
    Vector2 diff = screenPoint - _holeScreenCenter;
    float dist = RoundedBoxSDF(diff, _holeScreenHalfSize, _cornerRadiusPixels);
    return dist < 0f;
}
```

## 来源: `10_流水/光与朽项目/Claude-2026-04-14.md` · 提取日期 2026-05-20

## 光与朽实战补充：引导遮罩四个高频坑

| 坑 | 现象 | 修法 |
| --- | --- | --- |
| inactive 遮罩节点 `Awake()` 自隐藏 | `Show()` 激活后立刻又被关掉 | `Show()` 懒初始化，`Awake()` 不强制 `SetActive(false)` |
| 面板刚打开就读坐标 | 孔洞或手指在错误位置 | 事件后 `yield return null` 等 Layout rebuild |
| 目标按钮锚点不在中心 | 返回按钮等左上锚点 UI 指向偏移 | 用 `GetWorldCorners()` 算视觉中心，不直接用 `target.position` |
| 手指 / 光圈挡点击 | 孔洞正确但按钮难点 | 实例化后递归关闭所有子 `Graphic.raycastTarget` |

配置驱动的新手引导，必须同时提供“孔洞调试”和“手指偏移调试”。两者属于不同对象：遮罩组件调洞，TutorialDirector 调手指。
