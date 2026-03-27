# 《光与朽》第二章怪物 Unity 制作细节文档

**版本**：v1.0
**日期**：2026-03-27
**适用范围**：第二章全部怪物预制体制作 & ScriptableObject 配置
**编写依据**：已实现的 C# 脚本（EnemyBlob / EnemyData / LavaGunnerAI / LavaProjectile / VolcanoBossController / VolcanoMeteor）

---

## 总览：第二章新增怪物类型

| EnemyType | 中文名 | 行为类型 | 说明 |
|---|---|---|---|
| `LavaSplitter` | 熔岩分裂者 | Chase | 死亡时分裂为2只小分裂者 |
| `EliteLavaSplitter` | 精英熔岩分裂者 | Chase | 血量更高，分裂为4只小分裂者 |
| `LavaTank` | 熔岩坦克 | Chase | 高血量缓慢推进 |
| `LavaExploder` | 熔岩爆炸者 | Chase | 自爆，死亡留下熔岩水坑 |
| `EliteLavaExploder` | 精英熔岩爆炸者 | Chase | 爆炸伤害更高，留下更大水坑 |
| `LavaGunner` | 熔岩炮手 | RangedGunner | 停在屏幕上方定时射击 |
| `LavaPuddle` | 熔岩水坑 | Stationary | 静止地形障碍，阻挡激光 |

> **Boss**（熔炉巨兽）走独立 `BaseBossController` 体系，不在 EnemyPoolManager 中注册，单独见本文最后一节。

---

## 一、EnemyData 配置文件（ScriptableObject）

### 创建方法

在 Project 面板右键 → `Create → LightVsDecay → Enemy Data`，每种怪物创建一个。

建议保存路径：`Assets/Resources/Data/MonsterData/Ch2/`

---

### 1. LavaSplitter（熔岩分裂者）

**文件名**：`Enemy_LavaSplitter.asset`

| 字段 | 值 | 备注 |
|---|---|---|
| **基础信息** | | |
| Type | `LavaSplitter` | |
| Display Name | `熔岩分裂者` | |
| **战斗属性** | | |
| Max Health | `800` | 中等血量，打完就分裂 |
| Move Speed | `1.8` | 略快于一章基础怪 |
| Mass | `1.5` | |
| Contact Damage | `60` | 碰到塔的伤害 |
| Attack Interval | `1.0` | |
| Is Suicide | `false` | |
| Knockback Resistance | `0` | |
| **行为设置** | | |
| Behavior Type | `Chase` | 直冲追击 |
| **击退设置** | | |
| Can Be Knocked Back | `true` | |
| Knockback Multiplier | `1.0` | |
| **视觉设置** | | |
| Min Scale | `0.3` | |
| Death Fade Duration | `0.3` | |
| **奖励设置** | | |
| XP Reward | `25` | |
| Coin Reward | `2` | |
| **死亡分裂** | | |
| Split On Death | `true` | ✅ 开启分裂 |
| Split Enemy Type | `LavaSplitter`（小型版）或自定义小型类型 | 分裂产物的 EnemyType |
| Split Count | `2` | 分裂2只 |
| Split Impulse Speed | `4.0` | 分裂体的爆开速度 |
| **死亡留坑** | | |
| Spawn Puddle On Death | `false` | 分裂者不留坑 |
| **碰撞行为** | | |
| Collision Behavior | `Suicide` | 碰塔自爆 |

> **注意**：Split Enemy Type 若设为 `LavaSplitter` 本身，分裂体会继承相同配置再次分裂——如只想分裂一次，需另建一个 `LavaSplitter_Small` 配置（Split On Death = false）或后续版本通过 waveModifiers 控制。

---

### 2. EliteLavaSplitter（精英熔岩分裂者）

**文件名**：`Enemy_EliteLavaSplitter.asset`

与 `LavaSplitter` 相同，以下字段不同：

| 字段 | 值 |
|---|---|
| Type | `EliteLavaSplitter` |
| Display Name | `精英熔岩分裂者` |
| Max Health | `2000` |
| Move Speed | `1.5` |
| Contact Damage | `100` |
| XP Reward | `60` |
| Coin Reward | `5` |
| Split Count | `4` |
| Split Impulse Speed | `5.0` |

---

### 3. LavaTank（熔岩坦克）

**文件名**：`Enemy_LavaTank.asset`

| 字段 | 值 | 备注 |
|---|---|---|
| Type | `LavaTank` | |
| Display Name | `熔岩坦克` | |
| Max Health | `4000` | 高血量 |
| Move Speed | `0.7` | 缓慢 |
| Mass | `5.0` | 重，不易被推开 |
| Contact Damage | `200` | 高接触伤害 |
| Attack Interval | `1.5` | |
| Knockback Resistance | `0.8` | 高抗击退 |
| Behavior Type | `Chase` | |
| Can Be Knocked Back | `true` | |
| Knockback Multiplier | `0.2` | 几乎打不动 |
| XP Reward | `80` | |
| Coin Reward | `8` | |
| Split On Death | `false` | |
| Spawn Puddle On Death | `false` | |
| Collision Behavior | `Bounce` | 碰塔反弹，不自爆 |

---

### 4. LavaExploder（熔岩爆炸者）

**文件名**：`Enemy_LavaExploder.asset`

| 字段 | 值 | 备注 |
|---|---|---|
| Type | `LavaExploder` | |
| Display Name | `熔岩爆炸者` | |
| Max Health | `400` | 脆，但碰到塔就炸 |
| Move Speed | `2.2` | 比较快 |
| Mass | `1.0` | |
| Contact Damage | `300` | 高爆炸伤害 |
| Attack Interval | `0` | 只攻击一次 |
| Is Suicide | `true` | ✅ 自爆 |
| Knockback Resistance | `0` | |
| Behavior Type | `Chase` | |
| XP Reward | `20` | |
| Coin Reward | `2` | |
| **死亡留坑** | | |
| Spawn Puddle On Death | `true` | ✅ 死亡生成水坑 |
| Puddle Enemy Type | `LavaPuddle` | 水坑类型 |
| Split On Death | `false` | |
| Collision Behavior | `Suicide` | |

---

### 5. EliteLavaExploder（精英熔岩爆炸者）

**文件名**：`Enemy_EliteLavaExploder.asset`

与 `LavaExploder` 相同，以下字段不同：

| 字段 | 值 |
|---|---|
| Type | `EliteLavaExploder` |
| Display Name | `精英熔岩爆炸者` |
| Max Health | `800` |
| Contact Damage | `600` |
| Move Speed | `2.0` |
| XP Reward | `50` |
| Coin Reward | `5` |

---

### 6. LavaPuddle（熔岩水坑）

**文件名**：`Enemy_LavaPuddle.asset`

| 字段 | 值 | 备注 |
|---|---|---|
| Type | `LavaPuddle` | |
| Display Name | `熔岩水坑` | |
| Max Health | `300` | 激光可消除 |
| Move Speed | `0` | 静止，随便填 |
| Mass | `999` | 填大值防止误配置物理 |
| Contact Damage | `80` | 每秒烧伤（实际由接触逻辑决定） |
| Attack Interval | `1.0` | |
| Knockback Resistance | `1.0` | 完全免疫击退 |
| **行为设置** | | |
| Behavior Type | `Stationary` | ✅ 静止障碍 |
| **静止障碍设置** | | |
| Disable Hit Flash | `true` | ✅ 不显示受击闪烁 |
| Disable Knockback | `true` | ✅ 禁用击退 |
| Can Be Knocked Back | `false` | |
| **奖励** | | |
| XP Reward | `0` | 水坑不给经验 |
| Coin Reward | `0` | |
| Split On Death | `false` | |
| Spawn Puddle On Death | `false` | |
| **碰撞行为** | | |
| Collision Behavior | `None` | 不参与碰撞反弹 |

> **激光阻断原理**：`LavaPuddle` 挂在 `EnemyBlob` 上，层级为 `Enemy`。LaserBeam 的 `CalculateLaserPath()` 已用 `Physics2D.Raycast` 检测 enemyLayer，水坑的 Collider 会自动截断激光，**无需额外代码**。

---

### 7. LavaGunner（熔岩炮手）

**文件名**：`Enemy_LavaGunner.asset`

| 字段 | 值 | 备注 |
|---|---|---|
| Type | `LavaGunner` | |
| Display Name | `熔岩炮手` | |
| Max Health | `1200` | 需要专注打掉 |
| Move Speed | `3.0` | 入场速度由 LavaGunnerAI 覆盖（3m/s） |
| Mass | `2.0` | |
| Contact Damage | `0` | 不用碰撞伤害，靠弹道 |
| Attack Interval | `0` | AI 自行控制射击间隔 |
| Is Suicide | `false` | |
| Knockback Resistance | `0.5` | |
| **行为设置** | | |
| Behavior Type | `RangedGunner` | ✅ 远程炮手 |
| **击退设置** | | |
| Can Be Knocked Back | `true` | |
| Knockback Multiplier | `0.5` | |
| **奖励设置** | | |
| XP Reward | `50` | |
| Coin Reward | `5` | |
| **远程炮手设置** | | |
| Gunner Stop Y Percent | `0.3` | 从顶部往下30%处停驻（越小越靠上）|
| Gunner Shoot Interval | `8.0` | 每8秒射击一次 |
| Gunner Reposition Range | `2.0` | 射击后横移范围2米 |
| Gunner Projectile Prefab | `[拖入 LavaProjectile 预制体]` | ⚠️ 必须拖入 |
| Gunner Projectile Speed | `5.0` | |
| Gunner Projectile Damage | `80` | 每发弹道伤害 |
| Gunner Projectile HP | `30.0` | 激光打爆所需伤害量 |
| Gunner Projectile Lifetime | `8.0` | 超时自动销毁 |
| Split On Death | `false` | |
| Spawn Puddle On Death | `false` | |
| **碰撞行为** | | |
| Collision Behavior | `Bounce` | 被撞后反弹（但实际不会碰到塔）|

---

## 二、预制体制作步骤

### 2.1 通用流程（LavaSplitter / LavaExploder / LavaTank / LavaPuddle）

这四类怪物结构与第一章怪物相同，共用 `EnemyBlob` 组件，只需：

**步骤 1**：复制一个第一章怪物预制体（如 `Enemy_Slime.prefab`），重命名为对应名称（如 `Enemy_LavaSplitter.prefab`）。

**步骤 2**：修改 **SpriteRenderer** → 替换为对应的第二章美术 Sprite 和 Material。

**步骤 3**：选中预制体，找到 **EnemyBlob** 组件 → 将 `Enemy Data` 字段拖入对应的 `.asset` 文件（如 `Enemy_LavaSplitter.asset`）。

**步骤 4**：**LavaPuddle 专项**——
- `Rigidbody2D` → Body Type 可保持 `Dynamic`（代码运行时会自动改为 `Kinematic`）
- `CircleCollider2D` → 调整半径匹配美术大小
- 确认 GameObject 的 Layer 为 **Enemy**（这决定激光是否被截断）

**步骤 5**：保存预制体到 `Assets/Resources/Prefabs/Enemies/Ch2/`。

---

### 2.2 LavaGunner 预制体（含 LavaGunnerAI 组件）

> LavaGunner 需要额外挂载 `LavaGunnerAI` 组件。

**步骤 1**：复制一个标准怪物预制体，重命名为 `Enemy_LavaGunner.prefab`。

**步骤 2**：替换美术 Sprite。

**步骤 3**：EnemyBlob → 拖入 `Enemy_LavaGunner.asset`。

**步骤 4**：**添加 LavaGunnerAI 组件**：
- 在预制体根节点 Add Component → 搜索 `LavaGunnerAI` → 添加
- `LavaGunnerAI` 没有 Inspector 参数，所有配置从 EnemyData 读取，**无需手动填写任何字段**

**步骤 5**：检查 `Rigidbody2D` 设置：
- Gravity Scale → `0`
- Collision Detection → `Continuous`
- Freeze Rotation Z → ✅

**步骤 6**：保存。

---

### 2.3 LavaProjectile 预制体（熔岩弹道）

**步骤 1**：Project 面板右键 → Create Empty，命名为 `LavaProjectile.prefab`。

**步骤 2**：添加以下组件：
- `SpriteRenderer` → 拖入熔岩弹美术 Sprite
- `Rigidbody2D`
  - Gravity Scale → `0`
  - Collision Detection → `Continuous`
- `CircleCollider2D`（或 `CapsuleCollider2D`）
  - 调整大小匹配美术
- `LavaProjectile`（脚本组件，无 Inspector 参数，运行时由 LavaGunnerAI 注入）

**步骤 3**：**设置 Layer**：
- GameObject → Layer → `BossPollutionBall`
- ⚠️ 这是关键！该层已与 Shield / Tower 层配置碰撞，且 LaserController 会对此层的投射物调用 `TakeDamage`

**步骤 4**：保存到 `Assets/Resources/Prefabs/Projectiles/LavaProjectile.prefab`。

**步骤 5**：回到 `Enemy_LavaGunner.asset`，将此预制体拖入 `Gunner Projectile Prefab` 字段。

---

### 2.4 VolcanoMeteor 预制体（陨石）

> 由 VolcanoBossController 的 `meteorPrefab` 字段引用，需提前制作。

**步骤 1**：新建 `VolcanoMeteor.prefab`。

**步骤 2**：添加以下组件：
- `SpriteRenderer` → 陨石美术 Sprite（随时间旋转可加 `Rotator` 脚本）
- `VolcanoMeteor`（脚本组件）
  - **Fall Speed**：`8`（下落速度 m/s）
  - **Landing Damage**：`600`（着陆范围伤害）
  - **Damage Radius**：`1.2`（着陆伤害半径）
  - **Warning Circle**：拖入子节点的 `SpriteRenderer`（见下一步）
  - **Spawn Puddle On Land**：`true` ✅
  - **Show Debug Info**：开发期可打开

**步骤 3**：在 `VolcanoMeteor.prefab` 下新建子节点 `WarningCircle`：
- 添加 `SpriteRenderer` → Sprite 选一个红色圆圈（或 UI Circle Sprite）
- 调整大小匹配 `Damage Radius`（约直径 2.4 米）
- 默认 **SetActive = false**（代码会在 Launch 时激活）
- 将此子节点的 `SpriteRenderer` 拖入 `VolcanoMeteor` 的 `Warning Circle` 字段

**步骤 4**：保存到 `Assets/Resources/Prefabs/Boss/VolcanoMeteor.prefab`。

---

## 三、EnemyPoolManager 配置

在场景中找到挂载 `EnemyPoolManager` 的 GameObject（通常在 `_Managers` 下）。

展开 `Enemy Configs` 列表，为**每个第二章类型**添加一条配置：

| Type | Prefab | Prewarm Count | Max Count |
|---|---|---|---|
| `LavaSplitter` | Enemy_LavaSplitter.prefab | `10` | `30` |
| `LavaTank` | Enemy_LavaTank.prefab | `5` | `10` |
| `LavaExploder` | Enemy_LavaExploder.prefab | `10` | `20` |
| `LavaGunner` | Enemy_LavaGunner.prefab | `3` | `5` |
| `EliteLavaSplitter` | Enemy_EliteLavaSplitter.prefab | `3` | `5` |
| `EliteLavaExploder` | Enemy_EliteLavaExploder.prefab | `3` | `5` |
| `LavaPuddle` | Enemy_LavaPuddle.prefab | `10` | `20` |

> Max Count 填 `0` 表示使用全局上限，建议水坑和分裂体单独设限避免场面爆炸。

---

## 四、Boss：熔炉巨兽（VolcanoBoss）制作

### 4.1 Boss 预制体

Boss 不经过 EnemyPoolManager，使用独立预制体直接放在 Boss 场景中。

**步骤 1**：复制或参考第一章 Boss 预制体结构，新建 `VolcanoBoss.prefab`。

**步骤 2**：根节点组件：
- `SpriteRenderer` → Boss 美术
- `Rigidbody2D`
  - Body Type：`Dynamic`
  - Gravity Scale：`0`
  - Freeze Rotation Z：✅
- `Collider2D`（CapsuleCollider2D 或 CircleCollider2D，匹配美术）
- `BossHealth`（脚本）
- `VolcanoBossController`（脚本）

**步骤 3**：配置 `BossHealth` 组件（参考第一章 Boss）：
- Max Health：`80000`（策划案数值，可调）
- 拖入血条 UI 引用

**步骤 4**：配置 `VolcanoBossController` 组件（Inspector 全部字段）：

**汲取融合配置：**

| 字段 | 推荐值 | 说明 |
|---|---|---|
| Summon Count Phase1 | `6` | 阶段一召唤6只 LavaSplitter |
| Summon Count Phase2 | `8` | 阶段二召唤8只（含爆炸者）|
| Exploder Count Phase2 | `2` | 其中2只 LavaExploder |
| Absorption Radius | `1.2` | 小怪进入此范围被吸收（米）|
| Absorption Heal Per Slime | `2000` | 每次吸收回复 HP |
| Absorption ATK Per Stack | `0.05` | 每层5%攻击加成（目前为设计占位）|
| Absorption Max Stacks | `6` | 最多6层 |

**陨石喷发配置：**

| 字段 | 推荐值 | 说明 |
|---|---|---|
| Meteor Prefab | `[拖入 VolcanoMeteor.prefab]` | ⚠️ 必须拖入 |
| Meteor Count | `3` | 每次喷发3颗 |
| Meteor Interval | `12` | 每12秒触发一次（Idle中计时）|
| Meteor Spread Radius | `3.0` | 落点散布半径（以玩家塔为中心）|

**绝境碾压配置：**

| 字段 | 推荐值 | 说明 |
|---|---|---|
| Desperate Press Threshold | `0.3` | HP≤30% 触发 |
| Desperate Press Rounds | `3` | 触发后连续3轮激光角力 |

---

### 4.2 Boss 行为逻辑说明（供调试参考）

**阶段划分：**

| 阶段 | 触发条件 | 技能池 |
|---|---|---|
| 阶段一 | HP > 70% | 50% Charge + 50% Summon（6只分裂者）|
| 阶段二 | 30% < HP ≤ 70% | 50% Charge + 50% Summon（6分裂者+2爆炸者）|
| 阶段三 | HP ≤ 30%，仅触发一次 | 连续3轮 Press（激光角力），完成后恢复阶段一/二逻辑 |

**被动技能（Idle 期间持续）：**
- 每 `meteorInterval` 秒在玩家塔周围砸3颗陨石，陨石落地造成范围伤害并生成熔岩水坑

**汲取融合（Summon 后每帧检测）：**
- 召唤的小怪向玩家塔移动，路径上经过 Boss
- Boss 每帧检测 `absorptionRadius` 内的小怪，触发后：小怪静默消亡（无经验/金币）→ Boss 回血 2000 → 获得叠层（最多6层）
- 每层使 Boss 受到的伤害降低 3%（最高减免18%）

---

## 五、验证 Checklist

完成预制体制作后，按以下清单检查：

### 基础
- [ ] 所有 Ch2 EnemyData .asset 文件已创建，Type 字段填写正确
- [ ] EnemyPoolManager 中已为每种 Ch2 类型配置 prefab

### LavaSplitter
- [ ] 死亡时能正确分裂为2只子单位（Play Mode 测试，打死一只观察）
- [ ] 分裂子体有向外飞散的冲量效果

### LavaExploder
- [ ] 死亡时原地生成 LavaPuddle（Play Mode 测试）
- [ ] LavaPuddle 生成后静止不动

### LavaPuddle
- [ ] Layer 为 Enemy
- [ ] 放在激光路径上能截断激光（Play Mode 测试）
- [ ] 受击不闪烁，无击退位移

### LavaGunner
- [ ] LavaGunnerAI 组件已挂载
- [ ] 出生后从屏幕上方下移至 stopYPercent 位置后停止
- [ ] 每 8 秒发射一颗熔岩弹朝玩家塔方向
- [ ] 射击后横移换位
- [ ] LavaProjectile 层级为 BossPollutionBall
- [ ] 激光命中 LavaProjectile 后能打爆（需 LaserController 已更新）

### VolcanoBoss
- [ ] Meteor Prefab 已拖入，Idle 期间定时出现陨石
- [ ] 陨石着陆有预警红圈
- [ ] HP≤70% 进入阶段二（召唤中含2只爆炸者）
- [ ] HP≤30% 触发绝境碾压（激光角力×3轮）
- [ ] 小怪被吸收后 BossHealth 血量上涨
- [ ] 吸收层数在 Debug GUI 中正确显示（需开启 Show Debug Info）

---

*文档生成时间：2026-03-27 | 对应代码 Commit：75383f4*
