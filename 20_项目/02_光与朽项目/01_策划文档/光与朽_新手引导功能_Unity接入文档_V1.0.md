# 光与朽 新手引导功能 Unity接入文档 V1.0

## 1. 本次代码接入内容

本轮已落地的功能包括：

- 首次启动自动进入战斗，且不扣体力
- 科技树解锁条件改为：第一局结算后解锁
- 装备系统解锁条件改为：通关第二章·难度1后解锁
- 主菜单沿用现有 `TipsPanal` + 红点进行系统解锁提示
- 技能三选一首次控池：
  - 首次固定出现 `极寒光束 / 聚能透镜 / 分裂棱镜`
  - 选择 `极寒光束` 后，后续技能池移除 `聚能透镜`
  - 选择 `聚能透镜` 后，后续技能池移除 `极寒光束`
- 技能三选一常驻 Tips / 流派角标 / 协同发光支持
- 科技树首次进入引导：
  - 顶部常驻轮播 Tips
  - 手指+光圈指向第一个火力节点
  - 指向升级按钮
  - 指向关闭按钮
  - 指向顶部返回按钮
- 装备首次进入引导：
  - 指向背包中最高品质且可装备物品
  - 指向装备按钮
  - 装备后属性做 `Punch + Rolling Number`
  - 指向顶部返回按钮
- 大招首次提示改为纯文字气泡
- 新增无人机三选一常驻说明组件

## 2. 本次新增/修改脚本

主要新增脚本：

- `D:\Project\LightVSDecay\Assets\Scripts\UI\Tutorial\TutorialSpotlightOverlay.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\Tutorial\MainSceneTutorialDirector.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\Tutorial\DroneChoiceTipsPresenter.cs`

主要修改脚本：

- `D:\Project\LightVSDecay\Assets\Scripts\Data\Runtime\SessionData.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\Logic\SystemUnlockManager.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\Panels\MainMenuPanel.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\Data\SO\SkillDataBase.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\Panels\SkillChooseOnePanel.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\MainSceneUIManager.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\TopAreaController.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\TechTree\TechTreePanel.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\TechTree\TechTreeNodeUI.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\TechTree\TechTreeNodeDetailPanel.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\Equipment\EquipmentPanel.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\Equipment\EquipmentItemUI.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\UI\Equipment\ItemInfoPanel.cs`
- `D:\Project\LightVSDecay\Assets\Scripts\Logic\Player\OverloadManager.cs`

## 3. Unity场景接线

### 3.1 主场景 Tutorial 遮罩

在主菜单场景 Canvas 下新增一个全屏节点，建议命名：

- `TutorialOverlay`

挂载组件：

- `RectTransform`
- `CanvasGroup`
- `TutorialSpotlightOverlay`

子节点建议结构：

- `TopBlock`
- `BottomBlock`
- `LeftBlock`
- `RightBlock`
- `Ring`
- `Finger`
- `MessageRoot`
  - `MessageText`

`TutorialSpotlightOverlay` 字段配置：

- `Root Rect` -> `TutorialOverlay` 自己
- `Top Block` -> `TopBlock`
- `Bottom Block` -> `BottomBlock`
- `Left Block` -> `LeftBlock`
- `Right Block` -> `RightBlock`
- `Ring Rect` -> `Ring`
- `Finger Rect` -> `Finger`
- `Message Root` -> `MessageRoot`
- `Message Text` -> `MessageText`

注意：

- `TopBlock / BottomBlock / LeftBlock / RightBlock` 需要是 4 个普通半透明 Image
- 中间不要放整块全屏 Raycast Image，否则会挡住扣洞穿透
- `Ring` 和 `Finger` 建议默认隐藏视觉即可，脚本会在运行时移动

### 3.2 主场景引导总控

在主菜单场景任意常驻对象上挂载：

- `MainSceneTutorialDirector`

字段配置：

- `Spotlight Overlay` -> `TutorialOverlay`
- `Tech Tree Panel` -> 科技树主面板对象
- `Tech Tree Detail Panel` -> 科技树详情弹窗对象
- `First Tech Node Id` -> `node_fp_cap1`
- `Equipment Panel` -> 装备主面板对象
- `Item Info Panel` -> 装备二级信息面板对象

## 4. 科技树界面配置

### 4.1 顶部轮播 Tips

在科技树界面增加一个顶部横幅文本，建议命名：

- `TutorialTipsText`

挂在 `TechTreePanel` 上的字段：

- `Tutorial Tips Text` -> `TutorialTipsText`

默认文案已内置：

- 红色分支：提升激光伤害与大招效果
- 绿色分支：提升基地血量与护盾上限
- 黄色分支：增加金币收益与掉落率
- 蓝色分支：加快大招充能与击退效果

可选参数：

- `Tutorial Tip Interval` 默认 `2`

### 4.2 科技树首次引导目标

第一个引导节点固定为：

- `node_fp_cap1`

请确认该节点对象上的 `TechTreeNodeUI.nodeData.nodeId` 与资源一致。

## 5. 装备界面配置

### 5.1 属性滚动动画

`EquipmentPanel` 已支持自动对发生变化的总属性文字做演出。

可调参数：

- `Stat Roll Duration` 默认 `0.5`
- `Stat Punch Scale` 默认 `1.2`
- `Stat Punch Duration` 默认 `0.2`

无需额外代码接线，只要以下文本已绑定：

- `Attack Text`
- `Hp Text`
- `Shield Text`
- `Crit Text`
- `Charge Text`

### 5.2 装备首次引导目标

装备首次引导会自动选择：

- 背包中最高品质
- 且当前可装备

无需额外配置排序规则。

## 6. 技能三选一界面配置

`SkillChooseOnePanel` 新增了常驻说明字段。

### 6.1 面板字段

新增字段：

- `Tips Banner Text`

请拖入技能三选一界面顶部横幅文本。

### 6.2 每张技能卡字段

`SkillCardUI` 新增可选字段：

- `Glow Frame`
- `Lane Badge`
- `Lane Text`

作用：

- `Glow Frame`：已有同流派技能时显示协同发光
- `Lane Badge`：流派颜色底
- `Lane Text`：显示 `极寒流派 / 聚能流派 / 分裂流派 / 冲击流派`

如果这些字段暂时不拖，技能面板仍可正常运行，只是不显示新增视觉。

## 7. 大招提示气泡配置

`OverloadManager` 新增字段：

- `Ready Bubble Root`
- `Ready Bubble Text`
- `Ready Bubble Message`

建议在 HUD 的大招按钮上方新增：

- `OverloadReadyBubble`
  - 半透明背景框
  - `TMP_Text`

字段绑定：

- `Ready Bubble Root` -> `OverloadReadyBubble`
- `Ready Bubble Text` -> 文字节点

建议默认状态：

- `OverloadReadyBubble` 设为隐藏

## 8. 无人机三选一界面配置

由于当前代码库里没有明确找到现成的“无人机三选一主控制脚本”，本轮提供了一个独立组件：

- `DroneChoiceTipsPresenter`

挂载到无人机三选一界面根节点上即可。

字段配置：

- `Top Tips Text` -> 顶部 Tips 文本
- `Tip Items` -> 3 个说明项

每个 `Tip Item` 可配置：

- `Root`
- `Description Text`
- `Description`

推荐 3 条默认说明：

- 补给无人机：提供稳定收益，适合保守补强
- 问号无人机：结果随机，可能血赚也可能空手
- 契约无人机：高风险高收益，适合优势局扩大收益

## 9. 需要准备的美术资源

本轮代码已经预留好接线位，建议补齐下列资源：

- 手指图片 `Finger`
- 光圈图片 `Ring`
- 左右弧形箭头
- 技能三选一 Tips 横幅底图
- 技能颜色卡底图
- 技能协同流光边框
- 大招气泡半透明背景框
- 无人机顶部 Tips 横幅底图
- 无人机浮动说明背景框

## 10. 存档字段说明

本轮新增的引导存档字段已进入 `MetaData`：

- `isFirstPlay`
- `hasSeenSkillTutorial`
- `hasSeenOverloadTutorial`
- `hasViewedTechTreeTutorial`
- `hasViewedEquipmentTutorial`

如果测试时需要重置引导，清档后或调用 `MetaData.Reset()` 即可。

## 11. 测试建议

建议按以下顺序验收：

1. 清档后首次进入游戏，确认自动进战斗且不扣体力
2. 第一局结算返回主菜单，确认科技树解锁提示 + 红点
3. 首次进入科技树，确认节点/升级/关闭/返回完整引导
4. 首次技能三选一，确认固定出现 `极寒光束 / 聚能透镜 / 分裂棱镜`
5. 选择 `极寒光束` 后，多次升级，确认后续不再出现 `聚能透镜`
6. 大招首次就绪时，确认只出现文字气泡，不额外播放金色外扩动画
7. 通关第二章·难度1后返回主菜单，确认装备解锁提示 + 红点
8. 首次进入装备界面，确认选中最高品质可装备物品，并完成装备/属性演出/返回引导

