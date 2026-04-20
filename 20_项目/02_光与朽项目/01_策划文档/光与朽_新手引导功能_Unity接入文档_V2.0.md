# 光与朽 — 新手引导功能 Unity 接入文档 V2.0

> 文档版本：V2.0  
> 最后更新：2026-04-14  
> 适用工程：`D:\Project\LightVSDecay`

---

## 一、系统概览

### 1.1 引导分层

| 层次 | 适用场景 | 遮罩 | 手指 Prefab |
|------|----------|------|-------------|
| **战斗层** | 激光操控引导、大招引导 | 无（不阻挡操作） | 有 |
| **主界面层** | 科技树引导（4步）、装备引导（3步） | SDF 挖孔遮罩（防误触） | 有 |

### 1.2 核心文件

| 文件 | 说明 |
|------|------|
| `Assets/Scripts/UI/Tutorial/BattleTutorialDirector.cs` | 战斗场景引导控制器 |
| `Assets/Scripts/UI/Tutorial/MainSceneTutorialDirector.cs` | 主界面引导控制器 |
| `Assets/Scripts/UI/Tutorial/TutorialSpotlightOverlay.cs` | SDF 挖孔遮罩（已重构，单图层 Shader） |
| `Assets/Scripts/Data/SO/TutorialStepConfigSO.cs` | 引导步骤 ScriptableObject 配置 |
| `Assets/Scripts/UI/UIAnimationHelper.cs` | UI 通用动画工具（Scale Punch + Rolling Number） |
| `Assets/Shaders/UIHoleMask.shader` | SDF 圆角矩形挖孔 Shader |

### 1.3 存档标志位（MetaData）

所有引导是否已看过，由 `MetaData`（`SessionData.cs` 内部类）的布尔字段控制：

| 字段 | 含义 |
|------|------|
| `hasSeenLaserTutorial` | 激光操控引导已完成 |
| `hasSeenOverloadTutorial` | 大招引导已完成 |
| `hasViewedTechTreeTutorial` | 科技树引导（4步）已完成 |
| `hasViewedEquipmentTutorial` | 装备引导（3步）已完成 |
| `hasShownTechTreeUnlockTip` | 科技树解锁提示（TipsPanel）已显示 |
| `hasShownEquipmentUnlockTip` | 装备解锁提示（TipsPanel）已显示 |

---

## 二、TutorialStepConfigSO — 引导步骤配置资产

### 2.1 创建方式

右键 `Assets/Resources/Data/Tutorial/` → **Create → LightVsDecay → Tutorial Step Config**

### 2.2 字段说明

| 字段 | 类型 | 说明 |
|------|------|------|
| `stepID` | string | 步骤唯一标识（日志调试用，如 `tech_select_node`） |
| `prefabPath` | string | Resources 相对路径，如 `Effects/Tutorial/FingerPointerWithRing` |
| `parentPath` | string | 父节点场景路径（留空则挂在 target 的父节点下） |
| `localPosition` | Vector3 | 相对父节点/目标的本地坐标偏移 |
| `localScale` | Vector3 | Prefab 本地缩放（默认 1,1,1） |
| `useSpotlightOverlay` | bool | 是否启用 SDF 挖孔遮罩（战斗场景设为 false） |
| `holePadding` | Vector2 | 挖孔四边扩展边距（像素），默认 (24, 24) |
| `overlayMessage` | string | 遮罩上显示的引导文案（留空不显示） |

### 2.3 配置资产命名规范

存放路径：`Assets/Resources/Data/Tutorial/`

| 文件名                              | 对应步骤        |
| -------------------------------- | ----------- |
| `TutorialConfig_TechSelectNode`  | 科技树：点击第一个节点 |
| `TutorialConfig_TechUpgradeNode` | 科技树：点击升级按钮  |
| `TutorialConfig_TechCloseDetail` | 科技树：关闭详情面板  |
| `TutorialConfig_TechBackToMain`  | 科技树：返回主界面   |
| `TutorialConfig_EquipSelectItem` | 装备：点击装备物品   |
| `TutorialConfig_EquipEquipItem`  | 装备：点击装备按钮   |
| `TutorialConfig_EquipBackToMain` | 装备：返回主界面    |
| `TutorialConfig_LaserSwipe`      | 战斗：激光操控滑动   |
| `TutorialConfig_Overload`        | 战斗：点击大招按钮   |

---

## 三、手指特效 Prefab 规格

### 3.1 FingerPointerWithRing（主界面引导用）

- **用途**：科技树、装备系统引导，指向具体按钮
- **组成**：手指图片（带点击动画） + 圆环光圈（循环缩放）
- **Canvas 层级**：挂在 `Canvas/TutorialLayer`（或目标按钮父节点）
- **Pivot**：手指尖端为 Pivot（0.3, 0.9）
- **动画**：圆环 Scale 1.0 → 1.2 → 1.0，循环，周期 1s
- **Resources 路径**：`Effects/Tutorial/FingerPointerWithRing`

### 3.2 FingerSwipeGuide（激光操控引导用）

- **用途**：战斗场景，教玩家左右滑动控制激光
- **组成**：手指图片 + 左弧线箭头 + 右弧线箭头（同时显示）
- **Canvas 层级**：挂在战斗 Canvas 中央
- **动画**：手指左右摇摆，循环
- **Resources 路径**：`Effects/Tutorial/FingerSwipeGuide`

### 3.3 FingerPointer（大招按钮引导用）

- **用途**：战斗场景，指向大招按钮
- **组成**：手指图片 + 简单跳动动画
- **Resources 路径**：`Effects/Tutorial/FingerPointer`

---

## 四、BattleTutorialDirector — 战斗引导接入

### 4.1 挂载位置

在 **战斗场景**（GameScene）的 `BattleUICanvas` 下任意 GameObject 上添加 `BattleTutorialDirector` 组件。

### 4.2 Inspector 赋值

| 字段 | 赋值 |
|------|------|
| `Laser Config` | `TutorialConfig_LaserSwipe` 资产 |
| `Swipe Threshold` | 8（默认，像素/帧，防误触） |
| `Overload Config` | `TutorialConfig_Overload` 资产 |
| `Overload Button Rect` | 大招按钮的 RectTransform 拖入 |

### 4.3 工作流程

```
Start()（等一帧）
  ├── !hasSeenLaserTutorial → 实例化 FingerSwipeGuide Prefab（屏幕中央）
  └── 缓存 _shouldShowOverloadTutorial（避免 OverloadManager 竞争条件）

Update()
  └── 检测滑动（移动端：deltaPosition.magnitude > 8px；编辑器：Mouse X > 0.05）
        → CompleteLaserTutorial()（删 Prefab，写 PlayerPrefs）

OnOverloadStateChanged(Ready)
  └── !_shouldShowOverloadTutorial → 实例化 FingerPointer Prefab（大招按钮旁）

OnOverloadStateChanged(Active)
  └── CompleteOverloadTutorial()（删 Prefab，写 PlayerPrefs）

OnGameVictory / OnGameDefeat
  └── 清除所有引导 Prefab
```

### 4.4 注意事项

- `BattleTutorialDirector` **不使用** SDF 挖孔遮罩，战斗中不阻挡任何操作。
- 大招引导的 `_shouldShowOverloadTutorial` 在 `Start()` 中缓存，因为 `OverloadManager.ShowReadyBubble()` 会在 `OnEnterReady()` 内部（`GameEvents` 触发之前）提前写 `hasSeenOverloadTutorial = true`，不缓存会导致引导永远不显示。

---

## 五、MainSceneTutorialDirector — 主界面引导接入

### 5.1 挂载位置

在 **主界面场景**（MainScene）的 `MainSceneCanvas` 下任意 GameObject 上添加 `MainSceneTutorialDirector` 组件。

### 5.2 Inspector 赋值

#### SDF 挖孔遮罩
| 字段                  | 赋值                                              |
| ------------------- | ----------------------------------------------- |
| `Spotlight Overlay` | 场景中 `TutorialSpotlightOverlay` 组件所在的 GameObject |

#### 科技树引导配置（4步）
| 字段 | 赋值 |
|------|------|
| `Tech Select Node Config` | `TutorialConfig_TechSelectNode` |
| `Tech Upgrade Node Config` | `TutorialConfig_TechUpgradeNode` |
| `Tech Close Detail Config` | `TutorialConfig_TechCloseDetail` |
| `Tech Back To Main Config` | `TutorialConfig_TechBackToMain` |

#### 科技树面板引用
| 字段 | 赋值 |
|------|------|
| `Tech Tree Panel` | 场景中 `TechTreePanel` 组件 |
| `Tech Tree Detail Panel` | 场景中 `TechTreeNodeDetailPanel` 组件 |
| `First Tech Node Id` | `"node_fp_cap1"`（首个需引导的节点 ID） |

#### 装备引导配置（3步）
| 字段 | 赋值 |
|------|------|
| `Equip Select Item Config` | `TutorialConfig_EquipSelectItem` |
| `Equip Equip Item Config` | `TutorialConfig_EquipEquipItem` |
| `Equip Back To Main Config` | `TutorialConfig_EquipBackToMain` |

#### 装备面板引用
| 字段 | 赋值 |
|------|------|
| `Equipment Panel` | 场景中 `EquipmentPanel` 组件 |
| `Item Info Panel` | 场景中 `ItemInfoPanel` 组件 |

### 5.3 科技树引导流程（4步状态机）

```
进入科技树界面（OnMainSceneStateChanged → KeJi）
  └── IsTechTreeUnlocked && !hasViewedTechTreeTutorial
        ↓
Step 1: SelectNode
  ShowStep(techSelectNodeConfig, firstNodeRect)
  → SDF 挖孔 + FingerPointerWithRing 指向第一个科技节点
        ↓ 玩家点击节点（DetailShown 事件）
Step 2: UpgradeNode
  ShowStep(techUpgradeNodeConfig, upgradeButtonRect)
  → SDF 挖孔 + 手指 指向升级按钮
        ↓ 玩家点击升级（NodeUpgraded 事件）
Step 3: CloseDetail
  ShowStep(techCloseDetailConfig, closeButtonRect)
  → SDF 挖孔 + 手指 指向关闭按钮
        ↓ 玩家关闭详情（DetailClosed 事件）
Step 4: BackToMain
  ShowStep(techBackToMainConfig, backButtonRect)
  → SDF 挖孔 + 手指 指向返回按钮
        ↓ 玩家点击返回（BackClicked 事件）
Completed
  hasViewedTechTreeTutorial = true → PlayerPrefs.Save()
  ClearStep()
```

### 5.4 装备引导流程（3步状态机）

```
进入装备界面（OnMainSceneStateChanged → ZhuangBei）
  └── IsEquipmentUnlocked && !hasViewedEquipmentTutorial
        ↓
Step 1: SelectItem
  ShowStep(equipSelectItemConfig, bestItemRect)
  → SDF 挖孔 + 手指 指向背包中最佳装备
        ↓ 玩家点击装备（InfoShown 事件）
Step 2: EquipItem
  ShowStep(equipEquipItemConfig, equipButtonRect)
  → SDF 挖孔 + 手指 指向"装备"按钮
        ↓ 玩家点击装备（Equipped 事件）
Step 3: BackToMain
  ShowStep(equipBackToMainConfig, backButtonRect)
  → SDF 挖孔 + 手指 指向返回按钮
        ↓ 玩家点击返回（BackClicked 事件）
Completed
  hasViewedEquipmentTutorial = true → PlayerPrefs.Save()
  ClearStep()
```

### 5.5 解锁提示（TipsPanel）

`MainSceneTutorialDirector.Start()` 延迟 0.5s 后检查：

- `IsTechTreeUnlocked && !hasShownTechTreeUnlockTip` → 显示"科技树系统已解锁！永久强化你的战力！"
- `IsEquipmentUnlocked && !hasShownEquipmentUnlockTip` → 显示"装备系统已解锁！强化你的光棱塔！"

---

## 六、TutorialSpotlightOverlay — SDF 挖孔遮罩

### 6.1 挂载位置

在 **主界面 Canvas** 下创建 GameObject `TutorialSpotlightOverlay`，添加同名组件。

组件会在 `Awake()` 中自动创建一个全屏 Image，使用 `UI/HoleMask` Shader。

### 6.2 对外接口（MainSceneTutorialDirector 使用）

```csharp
// 显示：在 target 区域挖孔，可选文案和边距
spotlightOverlay.Show(RectTransform target, string message = null, Vector2? padding = null, bool trackTarget = true);

// 隐藏
spotlightOverlay.Hide();

// 仅更新文案
spotlightOverlay.SetMessage(string message);
```

### 6.3 Shader 参数说明

`Assets/Shaders/UIHoleMask.shader`（Shader 名：`UI/HoleMask`）

| 属性 | 说明 |
|------|------|
| `_Color` | 遮罩颜色，默认 (0,0,0,0.8) 半透明黑 |
| `_HoleCenter` | 挖孔中心（归一化屏幕坐标） |
| `_HoleSize` | 挖孔尺寸（归一化屏幕坐标） |
| `_CornerRadius` | 圆角半径（归一化，基于较短边） |

### 6.4 点击穿透

`TutorialSpotlightOverlay` 实现了 `IPointerClickHandler` 和 `IPointerDownHandler`：

- 点击在孔洞内 → `IsPointInHole()` 返回 true → 事件穿透给孔洞后方按钮
- 点击在遮罩区域 → 拦截，不传递事件

---

## 七、UIAnimationHelper — UI 通用动画

文件：`Assets/Scripts/UI/UIAnimationHelper.cs`（静态类，namespace `LightVsDecay.UI`）

### 7.1 Scale Punch（Q弹缩放）

```csharp
// 基础用法：Scale 1.0 → 1.2 → 1.0，耗时 0.2s
StartCoroutine(UIAnimationHelper.PlayScalePunch(rectTransform));

// 自定义参数
StartCoroutine(UIAnimationHelper.PlayScalePunch(
    rectTransform,
    punchScale: 1.3f,      // 放大倍率
    duration: 0.25f,       // 总时长（秒）
    useUnscaledTime: true  // true = UI 安全（推荐），false = 跟随 TimeScale
));
```

### 7.2 Rolling Number（数字滚动）

```csharp
// 整数滚动
StartCoroutine(UIAnimationHelper.RollInt(text, from: 100, to: 150, duration: 0.5f, prefix: "攻击力："));

// 浮点数滚动
StartCoroutine(UIAnimationHelper.RollFloat(text, from: 10f, to: 15.5f, prefix: "暴击率：", suffix: "%"));
```

### 7.3 组合效果（Q弹 + 滚动，装备属性更新标准用法）

```csharp
// 整数属性（如攻击力、生命值）
StartCoroutine(UIAnimationHelper.PunchThenRollInt(text, from, to, prefix: "攻击力："));

// 浮点数属性（如暴击率）
StartCoroutine(UIAnimationHelper.PunchThenRollFloat(text, from, to, prefix: "暴击率：", suffix: "%"));
```

### 7.4 当前使用位置

| 文件 | 用法 |
|------|------|
| `EquipmentPanel.cs` | `ApplyIntStat` / `ApplyFloatStat` → `UIAnimationHelper.PunchThenRollInt/Float` |
| `HUDPanel.cs` | `CoinPunchCoroutine` → `UIAnimationHelper.PlayScalePunch(useUnscaledTime: false)` |

---

## 八、场景 Hierarchy 参考结构

### 8.1 战斗场景（GameScene）

```
BattleUICanvas
  ├── HUDPanel               [HUDPanel]
  ├── ...
  └── BattleTutorialRoot     [BattleTutorialDirector]
        （无子节点，手指 Prefab 在运行时动态创建）
```

### 8.2 主界面场景（MainScene）

```
MainSceneCanvas
  ├── TechTreePanel          [TechTreePanel]
  ├── EquipmentPanel         [EquipmentPanel]
  ├── TipsPanelController    [TipsPanelController]（已有）
  ├── TutorialSpotlightOverlay  [TutorialSpotlightOverlay]
  │     （全屏 Image 由 Awake 动态创建，无需预设子节点）
  └── TutorialDirectorRoot   [MainSceneTutorialDirector]
        （无子节点，手指 Prefab 在运行时动态创建）
```

> **Canvas 排序**：`TutorialSpotlightOverlay` 的 Sort Order 应高于所有游戏 UI，但低于系统弹窗（如退出确认框）。

---

## 九、Resources 资产放置规范

```
Assets/Resources/
  ├── Data/Tutorial/
  │     ├── TutorialConfig_LaserSwipe.asset
  │     ├── TutorialConfig_Overload.asset
  │     ├── TutorialConfig_TechSelectNode.asset
  │     ├── TutorialConfig_TechUpgradeNode.asset
  │     ├── TutorialConfig_TechCloseDetail.asset
  │     ├── TutorialConfig_TechBackToMain.asset
  │     ├── TutorialConfig_EquipSelectItem.asset
  │     ├── TutorialConfig_EquipEquipItem.asset
  │     └── TutorialConfig_EquipBackToMain.asset
  └── Effects/Tutorial/
        ├── FingerPointerWithRing.prefab    （主界面引导用）
        ├── FingerSwipeGuide.prefab         （激光操控引导用）
        └── FingerPointer.prefab            （大招按钮引导用）
```

---

## 十、调试与测试

### 10.1 重置引导状态（编辑器调试）

在 `MetaData.Reset()` 中所有引导字段已重置为 `false`，可在菜单或 Inspector 调用 `ProgressManager.Instance.Meta.Reset()` 重置全部引导。

### 10.2 验证清单

- [ ] 首次进入战斗 → 激光操控引导 Prefab 出现在屏幕中央
- [ ] 玩家在屏幕上滑动 → 引导消失，不再显示
- [ ] 大招首次就绪 → 手指 Prefab 出现在大招按钮旁
- [ ] 玩家点击大招（Active 状态）→ 手指消失，标志位写入
- [ ] 战斗结束（胜/败）→ 所有引导 Prefab 销毁
- [ ] 科技树系统解锁后首次进入主界面 → TipsPanel 显示解锁提示
- [ ] 进入科技树界面 → Step 1 挖孔+手指正确指向首个节点
- [ ] 按流程走完 4 步 → hasViewedTechTreeTutorial 写入，引导不再显示
- [ ] 装备系统同上，3 步完整走通
- [ ] 挖孔区域内点击 → 穿透到目标按钮
- [ ] 挖孔区域外点击 → 点击被拦截
- [ ] 不同分辨率（720p / 1080p / 2K）→ 挖孔位置正确

### 10.3 DrawCall 参考

替换 4 面板几何方案为 SDF 单图层后，遮罩部分 DrawCall 从 4 降为 1。预计整体 Battle UI DrawCall 减少 3。
