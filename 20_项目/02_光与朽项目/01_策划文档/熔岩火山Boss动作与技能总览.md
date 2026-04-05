# 《光与朽》· 第二章 Boss
# 熔岩火山（The Magma Furnace）· 动作与技能总览

**版本**：V1.2
**日期**：2026-04-01
**修订**：V1.2 — 汲取融合全链路修复（吸收点架构、动画死亡、物理推力修复、血条回血缓冲）；明确眼睛不拆分不旋转；新增冰冻蓝色不污染Body03方案
**脚本对应**：`VolcanoBossController.cs`、`EnemyBlob.cs`、`HUDPanel.cs`

---

## 一、Boss 身份定位

### 1.1 设计主题
> **"不可撼动的大地意志，被激光点燃的最终爆发"**

熔岩火山是第二章"地形改变"主题的终极体现。与第一章的污染之核（圆球滚动，全方向无差别威胁）不同，火山的形状决定了它是一个**有方向感、有重心感的Boss**：

- 底部宽大稳重 → 难以推动，移动迟缓但压迫性极强
- 顶部是爆发口 → 所有弹道攻击从顶部喷出，视觉焦点向上聚集
- 侧面布满裂缝 → 角力状态下裂缝喷射火球，侧向骚扰
- 整体轮廓如山 → 玩家本能感受到"这不是可以推走的东西"

### 1.2 核心挑战设计（对比Ch1）

| 对比维度 | 第一章·污染之核（圆球） | 第二章·熔岩火山 |
|---|---|---|
| **形状** | 圆球，全方向对称 | 火山，上窄下宽，方向性强 |
| **移动感** | 滚动感，弹性冲撞 | 缓慢研磨，冲撞路径留特效拖尾 |
| **主要威胁** | 喷射追踪弹 + 冲撞 | 陨石砸点 + 地形封锁 + 吸收增强 |
| **激光角力** | 1轮，教学性质 | 3轮递增，角力中持续被骚扰 |
| **分心考验** | 需要拦截追踪弹 | 需要截杀被召唤的小怪（不截就让Boss回血变强）|
| **战场控制** | 污染斑短暂减伤 | 陨石岩浆水坑持续封锁激光路径 |

### 1.3 基础数值

| 属性 | 数值 |
|---|---|
| HP | 600,000 |
| 接触伤害 | 800 / 秒 |
| 移速 | 极慢（0.2x） |
| 击退抗性 | 几乎免疫 |

---

## 二、预制体层级与视觉状态

### 2.1 预制体结构

```
Lava_Boss（根节点）
├── VisualRoot              ← 移动震动动画根节点（纯视觉，无物理）
│   ├── Body                ← 液体RT渲染内核（被外壳覆盖，无需视觉状态变化）
│   ├── Body02              ← 火山外壳（岩石纹理图，随阶段硬切换破裂版本）
│   ├── Body03              ← 顶部喷发层（材质球HDR亮度/颜色驱动所有状态变化）
│   ├── Eyes                ← Boss眼睛（金黄→阶段三变红）
│   ├── CraterParticles     ← 火山口持续喷射粒子
│   └── AbsorptionPoint     ← 【V1.2新增】汲取融合吸收点（椭圆Trigger，无Rigidbody）
```

> **Body（内核）**：被 Body02 外壳和 Body03 喷发层遮挡，不做状态变化。
>
> **AbsorptionPoint**：Inspector 拖入 `VolcanoBossController → Absorption Point` 字段。粘液怪以此节点为移动目标，进入范围后触发吸收动画。

### 2.2 Body02（外壳）·阶段硬切换

| 阶段 | 外壳图片 | 触发时机 |
|---|---|---|
| 阶段一（HP > 70%） | 完整岩石版本（细裂缝）| 初始 |
| 阶段二（30% < HP ≤ 70%） | 中度破裂版本（宽裂缝，岩浆渗出）| HP 降至70%，硬切换 |
| 阶段三（HP ≤ 30%） | 重度破裂版本（大范围剥落）| HP 降至30%，硬切换 |

> **注意**：无法实现裂缝动态扩张动画，也无法实现碎片飞散，各阶段之间为瞬间切换图片。如需过渡感，可配合全屏震动掩盖切换瞬间。

### 2.3 Body03（顶部喷发层）· 状态指示器

**这一层是所有状态变化的主要视觉承载者，通过材质球 HDR 颜色 + 亮度驱动：**

| 游戏状态 | Body03 表现 | 实现方式 |
|---|---|---|
| 待机 Idle | 低强度喷发，基础橙色，缓慢呼吸闪烁 | DOTween 循环 Tween 材质 HDR 亮度 |
| 陨石蓄力前摇（1.5s）| 亮度骤增至白炽，颜色偏白 | DOTween 在1.5s内插值HDR到峰值 |
| 陨石喷发中 | 每颗陨石发射时同步闪一次 | 与陨石 Launch 时机对齐触发一次脉冲 |
| 火山冲撞前摇 | 顶部由橙变红，高亮闪烁（替代之前设计的"边缘闪红"）| DOTween 颜色由橙→红 |
| 冲撞中 | 红色持续高亮（霸体期间保持）| 维持红色直到冲撞结束 |
| 冲撞结束/硬直 | 颜色回归橙色，亮度降低 | DOTween 淡回正常 |
| 汲取融合召唤中 | 喷发减弱（能量外放）| DOTween 降低亮度 |
| 每次吸收小怪 | 一次短促爆闪（橙色高亮）| 脉冲 Tween |
| 绝境碾压角力中 | 白炽化，每2秒与火球同步脉冲一次 | 与 OnPressTick 的火球射出同步 |
| 阶段三激活（HP≤30%）| 持续高亮，颜色偏红白 | 参数调整 |

### 2.4 Eyes（眼睛）

| 状态 | 表现 |
|---|---|
| 阶段一/二 | 金黄色，缓慢眨眼 |
| 识别威胁（技能前摇中）| 眼睛停止眨眼，瞳孔收缩 |
| 吸收成功 | 短暂白色闪光 |
| 阶段三 | BossEyeController 切换为红色发光，停止眨眼 |
| 死亡 | 双眼慢慢熄灭 |

> **【V1.2 决策】关于眼睛是否旋转 / 左右眼是否拆分**：
> - **结论：不拆分，不旋转。** 左右眼保持在同一张图上。
> - 理由：火山是无机岩石结构，眼睛没有"追踪方向"的生物需求；双眼同步眨眼反而体现庞然大物的对称压迫感。
> - 现有 `BossEyeController` 的 squint/open 眨眼动画完全适用于单张合图。
> - 若将来需要双眼独立动画（如受击单眼受损），届时再拆分贴图和代码。

---

## 三、待机与移动动画方案（新增完整设计）

> 这是让火山"活起来"的关键。静止图片 + 合理动画 = 有生命感的Boss。

### 3.1 待机动画（Idle Animation）

#### A. 岩浆呼吸脉冲（Magma Pulse）
**目标感受**：身体在"心跳"，内部高压时刻准备爆发。

**实现方法（代码，DOTween）**：
```csharp
// 在 Boss 初始化时启动，全程循环
void StartMagmaPulse()
{
    // Body03 顶部材质呼吸（亮度 0.8 ↔ 1.2，周期3秒）
    DOTween.To(() => body03Material.GetFloat("_EmissionIntensity"),
               x => body03Material.SetFloat("_EmissionIntensity", x),
               1.2f, 1.5f)
           .SetLoops(-1, LoopType.Yoyo)
           .SetEase(Ease.InOutSine);

    // 眼睛同步呼吸（略滞后0.3秒，错开感）
    DOTween.To(() => eyeRenderer.color.a, ...)
           .SetDelay(0.3f)
           .SetLoops(-1, LoopType.Yoyo);
}
```

> 如无 DOTween，在 `Update()` 中用 `Mathf.Sin(Time.time * cycleSpeed)` 驱动 `material.SetFloat("_EmissionIntensity", ...)` 同样可行。

#### B. 沉重呼吸缩放（Heavy Breathing）
**目标感受**：不是弹跳Q弹，是"内部气压把岩石撑开"的紧绷感。

**关键原则**：
- ❌ 错误做法：Squash & Stretch（Y轴0.8↔1.3的Q弹拉伸），会破坏岩石坚硬感
- ✅ 正确做法：极小幅度、极慢速度的缩放

**参数推荐**：
```
Scale X: 1.00 ↔ 1.01（幅度仅1%）
Scale Y: 1.00 ↔ 1.02（幅度仅2%）
周期: 4秒（非常缓慢）
曲线: InOutSine（匀缓进出，避免机械感）
```

**实现**：
```csharp
DOTween.To(() => transform.localScale,
           x => transform.localScale = x,
           new Vector3(1.01f, 1.02f, 1f), 2f)
       .SetLoops(-1, LoopType.Yoyo)
       .SetEase(Ease.InOutSine);
```

#### C. 火山口粒子（Crater Particles）
**目标感受**：顶部有持续的动态点缀，使静态图片立刻"活"起来。

**挂载位置**：在 Body03 顶部区域新建子节点 `CraterParticles`，挂载 `ParticleSystem`。

**粒子参数参考**：
| 参数 | 值 |
|---|---|
| Emission Rate | 8–12 颗/秒 |
| 粒子寿命 | 1.5–2.5 秒 |
| 起始速度 | 0.5–1.5 m/s（向上） |
| 起始大小 | 0.03–0.08 |
| 颜色梯度 | 橙色（起点，不透明）→ 黑色（终点，全透明）|
| 阶段三 | Emission Rate 提高至 25–35，颜色更红 |

> **组合效果**：黑色火山灰 + 橙色火星（两种 ParticleSystem 叠加），参考火山喷发的视觉层次。

### 3.2 移动动画（Movement Logic）

**核心理念**：它是一座山，不是在溜冰，移动必须有破坏感和地壳运动的质感。

#### A. 高频位移震动（Body Shake During Movement）
**原理**：移动时每帧在基础移动位置叠加微小随机偏移，模拟"地基在振动"的感觉。

```csharp
// 在 FixedUpdate 或 Update 中，当 Boss 正在移动时叠加
if (isMoving)
{
    float shakeX = Mathf.PerlinNoise(Time.time * 18f, 0f) * 0.04f - 0.02f;
    float shakeY = Mathf.PerlinNoise(0f, Time.time * 18f) * 0.03f - 0.015f;
    // 叠加到 Rigidbody2D 的位置（或使用视觉子节点偏移）
    visualRoot.localPosition = new Vector3(shakeX, shakeY, 0f);
}
else
{
    visualRoot.localPosition = Vector3.zero;
}
```
> `visualRoot` 是一个包裹所有视觉子节点的空父节点，物理根节点 `Lava_Boss` 本身不做偏移。

#### B. 屏幕震动（Camera Shake）
**这是最重要的单一反馈效果！**

| 时机 | 震动类型 | 参数参考 |
|---|---|---|
| Boss 移动中（持续）| 低频持续微震 | 幅度0.02，频率8Hz |
| Boss 停下瞬间 | 重击震动，一次性 | 幅度0.15，衰减0.4s |
| 陨石落地 | 中等冲击震动 | 幅度0.1，衰减0.3s |
| 冲撞着地 | 强烈冲击震动 | 幅度0.25，衰减0.5s |
| 阶段切换 | 全场重震 | 幅度0.3，衰减0.8s |

> 调用方式取决于项目的 CameraShake 实现（如 Cinemachine Impulse，或自定义 CameraShaker）。

#### C. 移动拖尾/印记
**目标**：Boss走过的地方，地形被破坏，增强真实感与场地压迫。

- **方案A（建议）**：生成比 LavaPuddle 更小的"焦黑痕迹"精灵（静态贴图，2秒内淡出，不阻断激光，仅视觉）
- **方案B**：保留现有 LavaPuddle 但缩小尺寸（0.4x）、减少阻断时间至3秒，仅出现在移动路径（非冲撞路径）
- **暂不实现**：等待后期美术资产到位后再定

---

## 四、视觉补充建议：熔岩炙烤底座（增加再图片上）

### 设计动机
火山的底部在原图中是截断的（宽大的岩石底部），如果在底部加一层"底座"，可以：
1. 遮挡底部的硬截断感，让Boss看起来"生长"在地面上
2. 提供额外动态变化的空间（底座流动岩浆）
3. 进一步强化"火山插在地上"的稳固感

### 实现方案
预制体层级：

```
Lava_Boss（根节点）
├── Body
├── Body02
├── Body03
└── Eyes
```

**Base 层效果建议**：
- Sprite：扁椭圆形，比 Body02 宽约20%，像熔岩流漫过地面
- 材质球：使用与 Body 相同的液体流动着色器，但颜色更深（暗红→深橙）
- 动态效果：缓慢左右流动（Shader UV 动画），仿佛底座持续向外渗流
- 阶段三：底座亮度升高，流动速度加快
- 冲撞时：底座短暂被压缩（`Scale Y *= 0.85f`，DOTween），停止冲撞后弹回

---

## 五、阶段总览

```
HP 100%                HP 70%                HP 30%               HP 0%
   │                     │                     │                    │
   ├────── 阶段一 ────────┼────── 阶段二 ────────┼────── 阶段三 ───────┤
   │ 被动：陨石喷发        │ 被动：陨石喷发        │ 技能4：绝境碾压×3轮  │
   │ 技能1：汲取融合       │ 技能1：汲取融合（强化）│ + 裂缝持续喷射火球   │
   │ 技能2：火山冲撞       │ 技能3：火山冲撞（加速）│                    │
   │ 召唤：6只LavaSlime   │ 召唤：8只LavaSlime    │                    │
```

### 阶段切换表现（可实现范围内）

**阶段一 → 阶段二（HP=70%）**：
- Body02 外壳硬切换为中度破裂版本
- 全屏中等震动（遮盖切换瞬间）
- Body03 亮度提升10%，颜色略偏红
- 技能CD轻微缩短（约20%）

**阶段二 → 阶段三（HP=30%）**：
- Body02 外壳硬切换为重度破裂版本
- 全屏强烈震动 + 画面边缘短暂泛红
- Body03 白炽化，粒子发射速率大幅提升
- 强制中断当前行为 → 短暂硬直 → 触发绝境碾压

---

## 六、被动技能：陨石喷发

### 技能概述

| 项目 | 值 |
|---|---|
| 类型 | 被动·Idle 期间周期触发 |
| 触发间隔 | 每 12 秒 |
| 陨石数量 | 3 颗 |
| 陨石 HP | 15（可被激光击落）|
| 单颗命中伤害 | 60（对光棱塔）|
| 前摇 | 1.5 秒（Body03 白炽化蓄力）|
| 落点 | 以光棱塔为中心，半径3格散布 |
| 预警红圈 | **无**（光棱塔固定位置，玩家已知落点区域）|

### 时间轴
```
0s       1.5s    1.9s    2.3s    2.7s
│         │       │       │       │
│ Body03  │ 第1颗 │ 第2颗 │ 第3颗 │
│ 白炽化  │ 发射  │ 发射  │ 发射  │
│ （蓄力）│               陨石间隔 0.4s
```

### 玩家交互
- **无预警红圈**，玩家凭借光棱塔固定位置判断大致落点
- **激光拦截**：陨石 HP=15，激光集火可击落
- **落地效果**：范围60伤害 + 生成小型熔岩水坑（持续5秒阻断激光）

### 设计意图
> 持续蚕食战场，不拦截则激光通道越来越窄。与火山冲撞形成双重地形封锁。

---

## 七、技能1：汲取融合

### 【V1.2 完整重设计】全链路修复

V1.1 中的 `SetOverrideTarget(this.transform)` 方案存在两个新问题：
1. 粘液怪目标是 Boss 根节点 Transform，Rigidbody2D 之间发生物理碰撞 → Boss 被推飞
2. `AbsorbedByBoss()` 立即回收池，无动画/VFX/SFX，吸收表现为"消失"

#### 吸收流程设计（V1.2）

```
Boss 召唤 LavaSlime
    ↓ 设置 overrideTarget = AbsorptionPoint（椭圆 Trigger 节点）
    ↓ 召唤时 Physics2D.IgnoreCollision（Boss ↔ 粘液怪），消除物理推力
粘液怪移动向 AbsorptionPoint（火山口椭圆区域）
    ↓ 进入 AbsorptionPoint 碰撞检测范围
粘液怪播放吸收动画
    ↓ 0.4s 内 Scale 从当前大小线性缩到 0
    ↓ 缩小峰值时：播放蒸汽特效（同激光击杀）+ 死亡音效
    ↓ 动画结束：ReturnToPool（无奖励）
Boss 血条回血动画
    ↓ bossBloodBuffer（红色底）立即跳到新目标值
    ↓ bossBloodFill（实体 HP 层）慢追上去（0.5s 缓动）
    ↓ 视觉效果：先亮红底，再填实 ← 与掉血方向相反
```

#### 关键技术实现

**AbsorptionPoint 节点要求（用户操作）**：
- 在 `VisualRoot` 下新建 GameObject，命名 `AbsorptionPoint`
- 挂 `EllipseCollider2D`（或 PolygonCollider2D 近似椭圆），**勾选 Is Trigger**
- **不添加 Rigidbody2D**，它只是一个检测区域
- 放在 Boss 顶部火山口位置
- 将此节点拖入 `VolcanoBossController → Absorption Point` 字段

**VolcanoBossController.cs 变化**：
- `[SerializeField] private Transform absorptionPoint` 新增字段
- `ExecuteSummonBehavior()` 内：`enemy.SetOverrideTarget(absorptionPoint ?? transform)`
- 召唤后：`Physics2D.IgnoreCollision(GetComponent<Collider2D>(), enemy.GetComponent<Collider2D>())`
- 距离检测目标改为 `absorptionPoint.position`

**EnemyBlob.AbsorbedByBoss() 变化（V1.2）**：
- 启动 `ShrinkAndAbsorbCoroutine()` 协程
- 协程：关闭碰撞体 → 0.4s 内 Scale → 0 → 播放蒸汽 VFX/SFX → ReturnToPool

**HUDPanel.cs 血条变化**：
- 检测 `normalizedHP > bossCurrentHP`（回血判断）
- 回血时：buffer 立即跳 → solid 慢追；掉血时保持原有逻辑（solid 先降，buffer 慢追）

### 技能配置

| 项目 | 阶段一 | 阶段二 |
|---|---|---|
| **召唤类型** | LavaSlime（熔岩粘液） | LavaSlime（熔岩粘液） |
| 召唤数量 | 6 只 | 8 只 |
| 小怪行为 | 向 AbsorptionPoint 移动 | 同左 |
| 吸收动画 | 缩小0.4s + 蒸汽VFX + 死亡SFX | 同左 |
| 每次吸收效果 | 回血 2000 HP + 叠1层攻击力（3%减伤/层）| 同左 |
| 最大叠层 | 6 层（最高 18% 减伤）| 同左，累计上限 |

### 反制策略
- **正确做法**：激光优先扫描靠近Boss的小怪，在进入 AbsorptionPoint 范围前击杀
- **越拖越难**：未被截杀的小怪持续给Boss减伤叠层，最终到6层时Boss受到伤害降低18%

---

## 八、技能2 / 3：火山冲撞

### 技能概述

| 项目 | 阶段一 | 阶段二 |
|---|---|---|
| 前摇 | 1.2 秒 | 1.0 秒 |
| 前摇表现 | **Body03 顶部由橙变红、高亮闪烁**（非边缘闪红）| 同左 |
| 霸体状态 | 冲撞全程无法被推动 | 同左 |
| 冲撞伤害 | 100（接触光棱塔额外100）| 同左 |
| 冲撞速度 | 标准 × 1.0 | 标准 × 1.2 |
| 硬直（冲撞后）| 2 秒（输出窗口）| 2 秒 |
| 冲撞路径地形 | **无岩浆水坑**（已移除）| **无**（等待粒子特效资产）|
| 屏幕震动 | 强烈冲击，幅度0.25，衰减0.5s | 同左 |

### 冲撞路径拖尾（待实现）
> 当前版本已**移除**冲撞路径上的 LavaPuddle 生成（椭圆水坑形状与冲撞路径不匹配）。
> 后期制作专属粒子特效后，在 `OnChargeDashComplete()` 中接入熔浆拖尾效果。

### 前摇动画说明
```
0s → 1.2s（前摇阶段）
  Body03：颜色 橙色 → 红色，HDR 亮度 × 2.0
  粒子系统：发射速率骤升

1.2s（冲撞开始）
  切换霸体
  Body03：维持高亮红色

冲撞结束
  Body03：颜色回归橙色（DOTween 0.5s 淡回）
  硬直 2s → 输出窗口
```

---

## 九、技能4：绝境碾压（激光角力·终幕技能）

### 触发条件
- HP ≤ 30%，仅触发一次
- 强制中断当前行为 → 短暂硬直 → 进入角力

### 技能参数

| 项目 | 值 |
|---|---|
| 总轮次 | 3 轮 |
| 轮次难度递增 | Boss 推力每轮 +20% / +40% |
| 裂缝火球 | 每 2 秒向两侧各喷 1 发（共 2 发/次）|
| 每发火球伤害 | 25 |
| 火球可否击落 | 否（HP=999，穿越场地）|
| 角力失败惩罚 | 光棱塔损失35% HP |
| 角力3轮全胜 | Boss眩晕5秒（终极输出窗口）|

### 轮次难度

| 轮次 | Boss推力倍率 | 感受 |
|---|---|---|
| 第1轮 | 1.0x | 稳定，可压住 |
| 第2轮 | 1.2x（+20%）| 开始吃力 |
| 第3轮 | 1.4x（+40%）| 极度紧张，一旦分心即失败 |

### 形状带来的独特体验
裂缝火球从**侧面向左右水平喷出**（非从顶部），方向与激光角力形成对角线干扰：
- 专心角力 → 被侧面火球打到
- 躲火球偏移激光 → 角力推力不足，失败

---

## 十、代码修改清单（需实现）

### 🔴 必须修复（影响核心机制）

#### Fix 1 · EnemyBlob.cs — 添加移动目标覆盖

**修改位置**：私有字段区 + `MoveChase()` 方法

```csharp
// 新增字段
private Transform overrideTarget = null;

// 新增方法（公开）
public void SetOverrideTarget(Transform target) { overrideTarget = target; }

// 修改 MoveChase() 第一行：
Transform chaseTarget = (overrideTarget != null) ? overrideTarget : targetTower;
if (chaseTarget == null) return;
Vector2 direction = (chaseTarget.position - transform.position).normalized;
```

#### Fix 2 · VolcanoBossController.cs — 召唤目标 + 移动覆盖

**修改1**：`ExecuteSummonBehavior()` 中改为 `LavaSlime` + 设置覆盖目标：
```csharp
var enemy = EnemyPoolManager.Instance.Spawn(EnemyType.LavaSlime, spawnPos);
if (enemy != null)
{
    enemy.SetOverrideTarget(this.transform);
    summonedSlimes.Add(enemy);
}
```

**修改2**：`OnChargeDashComplete()` 注释掉/删除 LavaPuddle 生成代码（等待粒子特效）：
```csharp
protected override void OnChargeDashComplete(Vector3 startPos, Vector3 endPos)
{
    // 暂时移除岩浆水坑拖尾，待粒子特效资产完成后接入
    // TODO: 接入熔浆路径粒子特效
}
```

### 🟡 建议实现（表现优化）

| 项目 | 实现方式 |
|---|---|
| 待机 Magma Pulse | DOTween 循环 Body03 材质 HDR 亮度（参见第三节代码示例）|
| 待机 Heavy Breathing | DOTween Scale 极小幅度（Y:1.0→1.02，周期4秒）|
| 火山口粒子 | CraterParticles 子节点，两套ParticleSystem叠加（灰烬+火星）|
| 移动震动 | Perlin Noise 驱动 visualRoot.localPosition 偏移 |
| 屏幕震动 | 调用项目 CameraShaker，区分持续震/冲击震 |
| Body03冲撞前摇 | DOTween 颜色橙→红，配合HDR亮度提升 |
| 阶段切换震动 | 切换时机调用 CameraShaker 强震一次 |
| 火山底座 Base 层 | 新增扁椭圆精灵，流动着色器，位于 Body 之下 |
| 阶段三粒子加强 | 通过 ParticleSystem.emission.rateOverTime 代码调整 |

---

## 十一、Inspector 配置速查

| 字段组 | 字段名 | 推荐值 |
|---|---|---|
| **汲取融合** | summonCountPhase1 | `6` |
| | summonCountPhase2 | `8` |
| | absorptionRadius | `1.2` |
| | absorptionHealPerSlime | `2000` |
| | absorptionMaxStacks | `6` |
| **陨石喷发** | meteorPrefab | VolcanoMeteor.prefab（必须拖入）|
| | meteorCount | `3` |
| | meteorInterval | `12` |
| | meteorSpreadRadius | `3` |
| **火山冲撞** | chargeTrailSpacing | （暂时无效，等待粒子）|
| **绝境碾压** | desperatePressThreshold | `0.3` |
| | desperatePressRounds | `3` |
| | pressFireballInterval | `2` |
| | pressFireballPrefab | 火球预制体（必须拖入）|
| | pressFireballDamage | `25` |
| | pressFireballSpeed | `6` |

---

## 十二、冰冻流技能与 Body03 色彩冲突处理（V1.2 新增）

### 问题描述

玩家选择冰冻流技能后，`FrostDebuff` 系统会将所有目标的 `SpriteRenderer.color` 染成蓝色。
Boss 的 `Body03`（顶部喷发层）使用 `WobblyLiquidSprite` Shader，其 HDR `_Color` 属性控制橙色发光强度。
`SpriteRenderer.color`（顶点色）与 HDR 橙色相乘 → 蓝 × 橙 = 灰绿色，在 Bloom 放大下效果极差。

### 解决方案

**不修改 FrostDebuff.cs 或 BaseBossController.cs**（最小化改动）。

在 `VolcanoBossController.OnBossInitialized()` 末尾，重新设置 FrostDebuff 的目标渲染器，**排除 body03Renderer**：

```csharp
// 在 OnBossInitialized() 末尾
if (frostDebuff != null && body03Renderer != null)
{
    var filtered = System.Array.FindAll(
        bodyRenderers,
        r => r != null && r != body03Renderer
    );
    frostDebuff.SetTargetRenderers(filtered);
}
```

**效果**：
- Body01（内核）+ Body02（外壳）正常显示冰冻蓝色，保留冰冻视觉反馈
- Body03（顶部喷发层）不受蓝色染色影响，HDR 橙色材质动画保持原貌

---

*文档版本：V1.2 | 修订日期：2026-04-01 | 对应代码：VolcanoBossController.cs + EnemyBlob.cs + HUDPanel.cs*
