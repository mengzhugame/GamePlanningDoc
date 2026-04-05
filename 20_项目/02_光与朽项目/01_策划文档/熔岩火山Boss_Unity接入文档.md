# 熔岩火山 Boss — Unity 接入与配置操作文档

> 版本：V1.0 | 日期：2026-03-31
> 对应代码：`VolcanoBossController.cs` / `BossEyeController.cs`

---

## 一、Prefab 层级结构（必须严格按此搭建）

```
VolcanoBoss (GameObject)
├── VolcanoBossController.cs   ← 挂载此脚本
├── Rigidbody2D
├── Collider2D
│
├── VisualRoot               ← 纯视觉根节点，不挂物理组件
│   ├── Body01               ← 内部流体层（静态，无需改动）
│   ├── Body02               ← 岩石外壳层（SpriteRenderer，阶段切换用）
│   ├── Body03               ← 顶部喷发层（SpriteRenderer，材质动画用）
│   ├── Eye                  ← 眼睛节点（挂 BossEyeController）
│   └── CraterParticles      ← 火山口粒子系统（ParticleSystem）
│
└── BossHealth               ← 血量组件（已有）
```

**关键点**：
- `VisualRoot` 是移动震动动画的目标节点，不能缺省。其 `localPosition` 默认为 `(0, 0, 0)`，代码会在此基础上叠加 Perlin Noise 偏移。
- `Body02` 和 `Body03` 必须是 `VisualRoot` 的子节点，否则Inspector 拖拽后渲染器会找不到。
- `CraterParticles` 挂在 `Body03` 或 `VisualRoot` 下均可，建议直接放在火山口位置。

---

## 二、Body02 岩石外壳 — 阶段贴图配置

| Inspector 字段 | 类型 | 说明 |
|---|---|---|
| `Body 02 Renderer` | `SpriteRenderer` | 拖入 Body02 节点的 SpriteRenderer |
| `Body 02 Phase Sprites` (Size = 3) | `Sprite[]` | 依次填入：[0]=阶段1外壳, [1]=阶段2破损外壳, [2]=阶段3严重破损外壳 |

**操作步骤**：
1. 在 Project 面板找到美术资源，例如 `Boss_Body02_Phase1.png`、`Boss_Body02_Phase2.png`、`Boss_Body02_Phase3.png`。
2. 确认每张图的 Sprite Mode = `Single`，Pixels Per Unit 与其他 Boss 贴图一致。
3. 将 Body02 节点拖入 `Body 02 Renderer` 槽。
4. 展开 `Body 02 Phase Sprites`，Element 0/1/2 分别拖入对应阶段贴图。

> 阶段切换是**硬切（瞬间替换）**，在阶段转换特效播放期间切换，视觉上不会突兀。

---

## 三、Body03 顶部喷发层 — 阶段贴图 + 材质颜色动画

### 3.1 贴图配置（同 Body02）

| Inspector 字段 | 类型 | 说明 |
|---|---|---|
| `Body 03 Renderer` | `SpriteRenderer` | 拖入 Body03 节点的 SpriteRenderer |
| `Body 03 Phase Sprites` (Size = 3) | `Sprite[]` | 依次填入：[0]=阶段1顶部, [1]=阶段2顶部, [2]=阶段3顶部 |

### 3.2 HDR 颜色动画配置

Body03 使用 `WobblyLiquidSprite.shadergraph`，通过改变 `_Color`（HDR）属性实现发光强度变化。

| Inspector 字段 | 推荐颜色（HDR） | 对应状态 |
|---|---|---|
| `Body 03 Idle Color` | 橙红 `(1.0, 0.3, 0.0)` × Intensity 1.5 | 待机呼吸 |
| `Body 03 Charge Telegraph Color` | 深红 `(1.0, 0.1, 0.0)` × Intensity 2.5 | 冲撞前摇闪红 |
| `Body 03 Charge Active Color` | 亮橙 `(1.0, 0.5, 0.0)` × Intensity 3.0 | 冲撞中 |
| `Body 03 Press Color` | 暗红 `(0.8, 0.0, 0.0)` × Intensity 2.0 | 绝境碾压 |
| `Body 03 Summon Color` | 深橙 `(1.0, 0.4, 0.0)` × Intensity 2.0 | 汲取融合召唤 |
| `Color Lerp Speed` | `3.0`（默认） | 颜色过渡速度 |

**在 Unity Inspector 中设置 HDR 颜色的方法**：
1. 点击颜色字段旁的色块，弹出 Color Picker。
2. 勾选顶部的 `HDR` 选项（如未显示，确认字段有 `[ColorUsage(true, true)]`）。
3. 调节 `Intensity` 滑条（> 1 才有 Bloom 效果）。

> `_Color` 属性已通过 `body03Renderer.material`（运行时实例）修改，不会影响项目中其他使用同一材质的对象。

---

## 四、待机动画 · 沉重呼吸缩放

| Inspector 字段 | 默认值 | 说明 |
|---|---|---|
| `Breathing Amplitude` | `0.015` | 缩放幅度（Y轴 ±1.5%，X轴 ±1%） |
| `Breathing Period` | `4.0` | 一次完整呼吸的秒数 |

**调优建议**：
- 幅度过大（> 0.03）会破坏岩石坚硬感，建议不超过 `0.02`。
- 周期 2~6 秒均可接受，越慢越有"沉睡火山"的压迫感。
- 缩放作用在 `VisualRoot` 的 Scale 上，叠加在 Body 移动震动之上，互不干扰。

---

## 五、移动动画 · VisualRoot 身体震动

| Inspector 字段 | 默认值 | 说明 |
|---|---|---|
| `Visual Root` | `Transform` | **必须拖入 VisualRoot 节点** |
| `Move Shake Amplitude` | `0.025` | 移动时 localPosition 的震动幅度（世界单位） |
| `Move Shake Speed` | `18.0` | Perlin Noise 采样频率（越高震动越快） |

**配置要点**：
- `Visual Root` 如不赋值，移动震动功能静默跳过，不会报错，但没有效果。
- 震动使用 Perlin Noise（非随机），保证连续、有机的震动感而不是突变。
- Boss 停止移动时，代码会自动将 `VisualRoot.localPosition` 归零并触发一次 `CameraShake.ImpactShake`。

**Camera Shake 接入**：
- 代码直接调用 `CameraShake.Instance?.Shake()` / `CameraShake.Instance?.ImpactShake()`。
- 确认场景中 Camera 上挂有 `CameraShake` 组件且为单例，无需额外配置。

---

## 六、火山口粒子系统配置

| Inspector 字段 | 默认值 | 说明 |
|---|---|---|
| `Crater Particles` | `ParticleSystem` | 拖入 CraterParticles 节点的 ParticleSystem |
| `Crater Emission Phase1` | `8` | 阶段1每秒粒子数 |
| `Crater Emission Phase2` | `20` | 阶段2每秒粒子数（阶段切换时自动更新） |
| `Crater Emission Phase3` | `40` | 阶段3每秒粒子数 |

**ParticleSystem 推荐参数**（在 Unity Inspector 中手动设置）：

| 属性 | 推荐值 |
|---|---|
| Start Lifetime | 1.5 ~ 2.5（随机） |
| Start Speed | 1.5 ~ 3.0 |
| Start Size | 0.05 ~ 0.15（小火星）|
| Start Color | HDR 橙色 `(1.0, 0.5, 0.0)` × Intensity 2 |
| Gravity Modifier | -0.2（微微上升） |
| Shape → Cone Angle | 15°（集中向上喷射） |
| Emission → Rate Over Time | 初始设为 8，代码运行时会覆盖 |
| Renderer → Sorting Layer | 与 Boss Body 同层，Order = Body03 Order + 1 |

---

## 七、VFX 预留接口

以下字段均为**可选（null = 跳过，不报错）**，等美术制作完对应特效后再拖入。

| Inspector 字段 | 类型 | 触发时机 | 推荐特效内容 |
|---|---|---|---|
| `Vfx Phase2 Transition` | `GameObject` | 进入阶段2时，在 Boss 位置播放 | 外壳碎片飞散 + 岩浆喷射 |
| `Vfx Phase3 Transition` | `GameObject` | 进入阶段3时，在 Boss 位置播放 | 大型岩浆爆发 + 震裂光效 |
| `Vfx Absorb Slime` | `GameObject` | 汲取融合：每个 Slime 被吸收时 | 绿色粘液被吸入的粒子弧线 |
| `Vfx Meteor Burst` | `GameObject` | 陨石砸中目标时，在目标位置播放 | 火球爆炸 + 岩浆溅射 |
| `Vfx Desperate Press Start` | `GameObject` | 绝境碾压技能启动时 | 下压冲击波 + 地面裂纹 |

**VFX 制作规范**：
- 使用对象池中的 `VFXPoolManager`，特效预制件的 `ParticleSystem` 需设置 `Stop Action = Disable`（而非 Destroy），以便对象池回收。
- 若特效只播放一次（非循环），确认 `Play On Awake = true`，生命周期内自动停止。

---

## 八、SFX 预留接口

以下字段均为**可选**，等音效制作完成后直接拖入 `AudioClip` 资源。

| Inspector 字段 | 类型 | 触发时机 | 推荐音效风格 |
|---|---|---|---|
| `Sfx Meteor Launch` | `AudioClip` | 陨石发射技能启动时 | 低沉"轰"的喷射声，带混响 |
| `Sfx Absorb Slime` | `AudioClip` | 每个 Slime 被吸收时 | 湿润的吸食声 |
| `Sfx Phase Transition` | `AudioClip` | 阶段2或3切换时（共用一个） | 震裂、岩石崩碎声 |
| `Sfx Desperate Press Start` | `AudioClip` | 绝境碾压启动时 | 重压地面冲击声 |

**播放机制**：代码通过 `AudioManager.Instance?.PlayOneShot(clip)` 播放，受全局 SFX 音量控制。

---

## 九、眼睛颜色阶段变化（BossEyeController）

代码在进入阶段3时，会自动调用 `eyeController.SetTintColor()` 将眼睛颜色渐变为炽热红色。

**如需调整颜色**，修改 `VolcanoBossController.cs` 中 `OnEnterPhase3()` 里的参数：

```csharp
// 当前默认：阶段3眼睛变为深红/亮红
eyeController.SetTintColor(
    new Color(0.8f, 0.0f, 0.0f),   // 闭眼颜色：暗红
    new Color(1.0f, 0.2f, 0.0f),   // 睁眼颜色：亮橙红
    duration: 1.0f
);
```

---

## 十、BossHealth 血量配置

| 组件字段 | 推荐值 | 说明 |
|---|---|---|
| `Max HP` | 根据数值平衡文档 | 注意：阶段2在50% HP触发，阶段3在25% HP触发 |

阶段切换阈值在 `VolcanoBossController.cs` 中用常量定义：
```csharp
private const float PHASE2_HP_RATIO = 0.5f;   // 50% 进入阶段2
private const float PHASE3_HP_RATIO = 0.25f;  // 25% 进入阶段3
```

---

## 十一、Inspector 字段速查表

| 分类 | 字段名 | 类型 | 必填 |
|---|---|---|---|
| **外壳** | Body 02 Renderer | SpriteRenderer | ★ |
| **外壳** | Body 02 Phase Sprites [0~2] | Sprite | ★ |
| **顶部** | Body 03 Renderer | SpriteRenderer | ★ |
| **顶部** | Body 03 Phase Sprites [0~2] | Sprite | ★ |
| **颜色** | Body 03 Idle Color（HDR） | Color | ★ |
| **颜色** | Body 03 Charge Telegraph Color（HDR） | Color | ★ |
| **颜色** | Body 03 Charge Active Color（HDR） | Color | ★ |
| **颜色** | Body 03 Press Color（HDR） | Color | ★ |
| **颜色** | Body 03 Summon Color（HDR） | Color | ★ |
| **颜色** | Color Lerp Speed | float | ○ 默认3 |
| **呼吸** | Breathing Amplitude | float | ○ 默认0.015 |
| **呼吸** | Breathing Period | float | ○ 默认4 |
| **震动** | Visual Root | Transform | ★ |
| **震动** | Move Shake Amplitude | float | ○ 默认0.025 |
| **震动** | Move Shake Speed | float | ○ 默认18 |
| **粒子** | Crater Particles | ParticleSystem | ○ 推荐配置 |
| **粒子** | Crater Emission Phase 1/2/3 | float | ○ 默认8/20/40 |
| **VFX** | Vfx Phase2/3 Transition | GameObject | ○ 可留空 |
| **VFX** | Vfx Absorb Slime | GameObject | ○ 可留空 |
| **VFX** | Vfx Meteor Burst | GameObject | ○ 可留空 |
| **VFX** | Vfx Desperate Press Start | GameObject | ○ 可留空 |
| **SFX** | Sfx Meteor Launch | AudioClip | ○ 可留空 |
| **SFX** | Sfx Absorb Slime | AudioClip | ○ 可留空 |
| **SFX** | Sfx Phase Transition | AudioClip | ○ 可留空 |
| **SFX** | Sfx Desperate Press Start | AudioClip | ○ 可留空 |

★ = 必须配置，否则运行时报错或功能失效
○ = 可选，有默认值或容错处理

---

## 十二、测试检查清单

### 基础功能
- [ ] Boss 生成后不报 NullReferenceException
- [ ] 待机状态下可以看到轻微缩放呼吸
- [ ] Body03 顶部有橙色 Bloom 发光效果（需场景启用 Post Processing + Bloom）

### 阶段切换
- [ ] HP 降至 50% 时，Body02/Body03 贴图切换为阶段2图片
- [ ] HP 降至 25% 时，切换为阶段3图片
- [ ] 阶段切换时眼睛颜色在阶段3变为红色（渐变过渡）
- [ ] 火山口粒子在阶段3明显比阶段1密集

### 技能状态颜色
- [ ] 冲撞前摇时 Body03 变暗红（telegraph 颜色）
- [ ] 冲撞中 Body03 变亮橙
- [ ] 绝境碾压时 Body03 变深红
- [ ] 汲取融合召唤时 Body03 变深橙
- [ ] 技能结束后自动回到 Idle 橙红色

### 移动动画
- [ ] Boss 移动时 VisualRoot 有高频微小震动
- [ ] Boss 停止移动时屏幕有短暂冲击震动

### 汲取融合（修复验证）
- [ ] 召唤出的是 LavaSlime（粘液怪），非 LavaExploder
- [ ] LavaSlime 生成后朝向 Boss 移动（而非朝向玩家光棱塔）
- [ ] LavaSlime 到达 Boss 位置后被吸收，Boss HP 回复

---

## 十三、常见问题

**Q：Body03 没有 Bloom 效果？**
A：确认 URP/Built-in Render Pipeline 已启用 Post Processing，且 Camera 上勾选了 Post Processing，Bloom 阈值低于 HDR Intensity 设置值。

**Q：VisualRoot 震动了但有偏移没有归零？**
A：确认 `Visual Root` 字段已拖入正确节点（不是 Root GameObject 本身）。停止移动后代码会调用 `visualRoot.localPosition = Vector3.zero` 归零。

**Q：HDR Color 字段在 Inspector 看不到 Intensity 滑条？**
A：需要在 Color Picker 弹窗中选择 `HDR` 模式（部分 Unity 版本需在颜色字段旁的下拉菜单中启用）。确认字段使用了 `[ColorUsage(true, true)]` 特性。

**Q：材质颜色修改影响了其他使用同一材质的对象？**
A：代码在 `OnBossInitialized()` 中调用 `body03Renderer.material`（注意不是 `sharedMaterial`）创建运行时实例，不会影响共享材质。若仍有影响，检查是否有其他脚本意外使用了 `sharedMaterial`。
