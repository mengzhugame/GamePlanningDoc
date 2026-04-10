# 《光与朽》VFX 与 SFX 接入操作手册 V1.0

**适用版本**：代码已完成所有代码钩子，本文档指导在 Unity Inspector 中完成资产绑定。  
**更新日期**：2026-04-06

---

## 目录

1. [代码改动总览](#代码改动总览)
2. [VFX 接入——VFXPoolManager 配置](#vfx-接入vfxpoolmanager-配置)
3. [SFX 接入——AudioConfig 配置](#sfx-接入audioconfig-配置)
4. [各怪物 Inspector 绑定详情](#各怪物-inspector-绑定详情)
5. [BOSS 接入详情](#boss-接入详情)
6. [光棱塔接入详情](#光棱塔接入详情)
7. [火球飞行音效接入](#火球飞行音效接入)
8. [接入验证清单](#接入验证清单)

---

## 代码改动总览

本次代码改动已完成以下工作，无需再修改任何 `.cs` 文件：

| 文件 | 改动内容 |
|------|---------|
| `VFXPoolManager.cs` | 新增枚举 `GrenadeExplosionFire`（炸弹怪爆炸）、`WinterImpact`（冰墙落地） |
| `AudioConfig.cs` | 新增字段：`grenadeExplosion`、`lavaPuddleSpawn`、`gunnerSpit`、`fireballFlight`、`catalystBurst`、`iceWallSpawn`、`iceShieldBreak`、`towerFreeze`、`towerUnfreeze`、`bossWarning`、`bossAbsorb` |
| `AudioManager.cs` | 新增方法：`PlayGrenadeExplosion`、`PlayLavaPuddleSpawn`、`PlayGunnerSpit`、`PlayCatalystBurst`、`PlayIceWallSpawn`、`PlayIceShieldBreak`、`PlayTowerFreeze`、`PlayTowerUnfreeze`；BOSS来袭时自动播放 `bossWarning` |
| `EnemyBlob.cs` | 炸弹怪死亡使用专属 `GrenadeExplosionFire` VFX；熔浆液生成时播放 SFX；催化者死亡时播放冷气 SFX |
| `LavaGunnerAI.cs` | 发射火球时播放 `gunnerSpit` SFX |
| `FrostcasterAI.cs` | 每堵冰墙生成时播放 `WinterImpact` VFX；整组冰墙统一播放 `iceWallSpawn` SFX |
| `IceShieldController.cs` | 冰盾破碎时播放 `iceShieldBreak` SFX |
| `TurretController.cs` | 被冻结时播放 `towerFreeze` SFX，解冻时播放 `towerUnfreeze` SFX |

---

## VFX 接入——VFXPoolManager 配置

**操作路径**：在场景/预制体中找到挂载 `VFXPoolManager` 的 GameObject → Inspector → **VFX配置列表（vfxConfigs）**

在列表中新增以下条目（点击 `+` 添加元素）：

### 新增条目 1：GrenadeExplosionFire（炸弹怪死亡爆炸）

| 字段                       | 值                                 |
| ------------------------ | --------------------------------- |
| **Type**                 | `GrenadeExplosionFire`            |
| **Prefab**               | `VFX_GrenadeExplosionFire`（拖入预制体） |
| **Use Pool**             | ☐ 取消勾选（低频，非池化）                    |
| **Custom Destroy Delay** | `-1`（自动检测粒子时长）                    |

### 新增条目 2：WinterImpact（冰墙落地特效）

| 字段                | 值                          |
| ----------------- | -------------------------- |
| **Type**          | `WinterImpact`             |
| **Prefab**        | `VFX_Winter_Impact`（拖入预制体） |
| **Use Pool**      | ☑ 勾选（中频，池化）                |
| **Prewarm Count** | `5`                        |
| **Max Count**     | `20`                       |

### 已有条目——确认并补全 Prefab：

| Type             | Prefab                    | 说明                                      |
| ---------------- | ------------------------- | --------------------------------------- |
| `EnemySplit`     | `VFX_EnergyExplosionBlue` | 分裂怪（LavaSplitter/EliteLavaSplitter）死亡爆开 |
| `CatalystBurst`  | `VFX_NovaFrost`           | 极寒催化者死亡冷气烟雾                             |
| `IceShieldBreak` | 冰盾破碎特效（自行命名的粒子预制体）        | 精英冰甲卫士冰盾破碎                              |

---

## SFX 接入——AudioConfig 配置

**操作路径**：Project 窗口找到 `AudioConfig` ScriptableObject → Inspector

在对应分区找到以下字段，将音效文件拖入：

### 怪物音效分区（Enemy SFX）

| 字段名 | 对应音效 | 说明 |
|--------|---------|------|
| `grenadeExplosion` | 炸弹爆炸音效 | `LavaExploder` / `EliteLavaExploder` 死亡时播放 |
| `lavaPuddleSpawn` | 熔浆液生成音效 | 炸弹怪死亡留坑（`LavaPuddle` 出现时）播放 |
| `gunnerSpit` | 炮手喷吐音效 | `LavaGunner` 发射火球瞬间播放 |
| `fireballFlight` | 火球飞行音效 | 挂在 `LavaProjectile` 预制体上，见[火球飞行音效接入](#火球飞行音效接入) |
| `catalystBurst` | 冷气释放音效 | `FrostCatalyst` 死亡触发冷气爆发时播放 |
| `iceWallSpawn` | 冰墙生成音效 | `FrostcasterAI` 召唤一组冰墙时播放一次 |
| `iceShieldBreak` | 冰盾破裂音效 | `Frost_EliteTank` 冰盾 HP 归零时播放 |
| `enemySplit` | 分裂怪死亡爆开音效 | （已有字段）拖入分裂音效 AudioClip |
| `enemyFreeze` | 怪物冰冻音效 | （已有字段）怪物/光棱塔冰冻时共用 |

### 玩家音效分区（Player SFX）

| 字段名 | 对应音效 | 说明 |
|--------|---------|------|
| `towerFreeze` | 光棱塔被冰冻音效 | Boss 冰封技能命中光棱塔时播放 |
| `towerUnfreeze` | 光棱塔解冻音效 | 冻结解除时播放 |

### Boss 音效分区（Boss SFX）

| 字段名 | 对应音效 | 说明 |
|--------|---------|------|
| `bossWarning` | BOSS来袭警告声音 | 波次触发 Boss 出场事件（`GameEvents.OnBossFightStart`）时自动播放 |
| `bossAbsorb` | 熔岩BOSS吸收融合音效 | 见下方 [BOSS Inspector 绑定](#boss-接入详情) 部分，使用局部 `sfxAbsorbSlime` 字段 |

---

## 各怪物 Inspector 绑定详情

### Ch2 · Lava_Exploder（熔岩爆炸者）

**代码已自动处理**：死亡时自动播放 `VFX_GrenadeExplosionFire` + `grenadeExplosion` 音效。  
**无需额外 Inspector 操作**，确保 VFXPoolManager 中 `GrenadeExplosionFire` 的 Prefab 已填写即可。

### Ch2 · Lava_EliteSplitter（精英熔岩分裂者）

> ⚠️ 注意：精英分裂者 `EnemyData` 中若 `explosionAoeDamage > 0`，死亡时会自动使用 `GrenadeExplosionFire` VFX。  
> 若精英分裂者没有设置 `explosionAoeDamage`，说明它属于**分裂怪（splitOnDeath=true）**逻辑，死亡时播放 `EnemySplit` VFX（`VFX_EnergyExplosionBlue`）。  
> **请确认该怪物的 EnemyData 配置以决定接哪个 VFX。**

根据配置二选一：

- 若 `explosionAoeDamage > 0`（自爆后留坑）→ 确保 VFXPoolManager `GrenadeExplosionFire` Prefab 已绑定
- 若 `splitOnDeath = true`（死亡分裂）→ 确保 VFXPoolManager `EnemySplit` Prefab 绑定为 `VFX_EnergyExplosionBlue`

### Ch3 · Frost_Catalyst（极寒催化者）

**代码已自动处理**：死亡时 `TriggerCatalystBurst()` 自动播放：
- VFX：`VFXType.CatalystBurst` → 在 VFXPoolManager 中绑定 `VFX_NovaFrost` 预制体
- SFX：`catalystBurst` 字段 → 在 AudioConfig 中拖入冷气释放音效

**无需额外 Inspector 操作**。

### Ch3 · Frost_EliteTank（精英冰甲卫士）

**代码已自动处理**：冰盾破碎时 `IceShieldController.OnShieldBroken()` 自动播放：
- VFX：`VFXType.IceShieldBreak` → 在 VFXPoolManager 中确认 `IceShieldBreak` Prefab 已绑定
- SFX：`iceShieldBreak` 字段 → 在 AudioConfig 中拖入冰盾破裂音效

**无需额外 Inspector 操作**。

### Ch3 · Frostcaster / EliteFrostcaster（霜冻施法者）

**代码已自动处理**：召唤冰墙时自动播放：
- VFX：`VFXType.WinterImpact` → 在 VFXPoolManager 中绑定 `VFX_Winter_Impact` 预制体（每堵墙单独播放）
- SFX：`iceWallSpawn` 字段 → 在 AudioConfig 中拖入冰墙生成音效（整组一次性播放）

**无需额外 Inspector 操作**。

---

## BOSS 接入详情

### Ch2 · Lava_Boss（熔炉巨兽）—— 回血特效 VFX_HealStream3

**回血触发机制**：Boss 吸收 `LavaSlime` 时调用 `BossHealth.HealHP()` 并播放 `vfxAbsorbSlime`。

**操作路径**：找到场景中 `Lava_Boss` 预制体/实例 → Inspector → **VolcanoBossController** 组件

| 分区 | 字段 | 操作 |
|------|------|------|
| VFX 预留接口 | `vfxAbsorbSlime` | 拖入 `VFX_HealStream3` 预制体 |
| SFX 预留接口 | `sfxAbsorbSlime` | 拖入吸收融合音效 AudioClip |

> `vfxAbsorbSlime` 特效在 Boss **自身位置**播放（`PlayVFXAtSelf` 方式），使用 `Instantiate` 非池化。  
> 如果特效时长较长，可在 VFX 预制体中设置粒子 `Stop Action = Destroy`。

### Ch2 · Lava_Boss + Lava_Gunner —— 火球特效 VFX_FireBall

火球特效附在 **火球预制体（LavaProjectile Prefab）** 上，不需要代码调用，详见下方[火球飞行音效接入](#火球飞行音效接入)。

### BOSS 来袭警告声音

**触发机制**：当 `GameEvents.OnBossFightStart` 事件触发时，`AudioManager` 自动播放。  
**操作**：在 `AudioConfig` 中填写 `bossWarning` 字段即可，代码已完成钩子。

---

## 光棱塔接入详情

### 冰冻 / 解冻音效

**触发机制**：`TurretController.FreezeCoroutine()` 已在冻结开始/结束时分别调用 `PlayTowerFreeze()` / `PlayTowerUnfreeze()`。  
**操作**：在 `AudioConfig` 中填写 `towerFreeze` 和 `towerUnfreeze` 字段，代码已完成钩子。

> 如果你希望冻结音效与怪物冰冻音效使用**同一个 AudioClip**，直接将 `enemyFreeze` 和 `towerFreeze` 填入同一个 Clip 即可。

---

## 火球飞行音效接入

火球飞行音效（`VFX_FireBall` 飞行中的嗖嗖声）建议直接挂在 **LavaProjectile 预制体**上，无需代码改动：

**操作步骤**：

1. 打开 `LavaProjectile` 预制体
2. 在根 GameObject 上添加 `AudioSource` 组件
3. 配置如下：
   - `AudioClip` → 拖入火球飞行音效 Clip
   - `Play On Awake` → ☑ 勾选
   - `Loop` → ☑ 勾选
   - `Spatial Blend` → `0`（2D 音效）
   - `Volume` → 与 `AudioConfig.enemyDefaultVolume` 对齐（建议 0.5~0.6）
4. 火球命中/被击毁后会自动 `Destroy`，AudioSource 随之销毁，无需手动停止

> 如果游戏中同时存在多个火球，建议将 `Volume` 调低（0.3 左右）避免音效叠加过响。

---

## 接入验证清单

完成上述配置后，逐一验证以下效果：

### 第二章

- [ ] `Lava_Exploder` 死亡时播放火焰爆炸特效（`VFX_GrenadeExplosionFire`）
- [ ] `Lava_EliteSplitter` 死亡时播放对应特效（爆炸或分裂，取决于 EnemyData 配置）
- [ ] `Lava_Exploder` 死亡后熔浆液出现时有生成音效
- [ ] `Lava_Gunner` 发射火球时有喷吐音效
- [ ] 火球飞行过程中有持续音效
- [ ] `Lava_Boss` 吸收 `LavaSlime` 回血时播放 `VFX_HealStream3` + 吸收融合音效
- [ ] BOSS 出场时播放警告音效

### 第三章

- [ ] `Frost_Catalyst` 死亡时播放 `VFX_NovaFrost`（冷气烟雾）+ 冷气释放音效
- [ ] `Frost_EliteTank` 冰盾被打碎时播放冰盾破碎特效 + 冰盾破裂音效
- [ ] `Frostcaster` 召唤冰墙时每堵墙有落地特效（`VFX_Winter_Impact`）+ 冰墙生成音效
- [ ] 分裂怪（`LavaSplitter` / `EliteLavaSplitter`）死亡时播放 `VFX_EnergyExplosionBlue` + 分裂音效
- [ ] 光棱塔被 Boss 冰冻时播放冻结音效，解冻时播放解冻音效

### 通用

- [ ] 所有怪物被冰冻时播放冻结音效（由 `FrostDebuff` 系统触发 `PlayEnemyFreeze()`）
- [ ] BOSS 来袭出现警告声音

---

## 快速索引——各音效字段所在位置

| 音效 | 文件/路径 | 字段名 |
|------|---------|--------|
| 炸弹怪爆炸音效 | `AudioConfig` → 怪物音效 | `grenadeExplosion` |
| 熔浆液生成音效 | `AudioConfig` → 怪物音效 | `lavaPuddleSpawn` |
| 炮手喷吐音效 | `AudioConfig` → 怪物音效 | `gunnerSpit` |
| 火球飞行音效 | `LavaProjectile` 预制体 → `AudioSource` | 直接拖入 Clip |
| BOSS吸收融合音效 | `VolcanoBossController` Inspector | `sfxAbsorbSlime` |
| 催化者冷气音效 | `AudioConfig` → 怪物音效 | `catalystBurst` |
| 冰墙生成音效 | `AudioConfig` → 怪物音效 | `iceWallSpawn` |
| 冰盾破裂音效 | `AudioConfig` → 怪物音效 | `iceShieldBreak` |
| 怪物/光棱塔冰冻音效 | `AudioConfig` → 怪物音效 | `enemyFreeze` |
| 光棱塔专属冻结音效 | `AudioConfig` → 玩家音效 | `towerFreeze` |
| 光棱塔解冻音效 | `AudioConfig` → 玩家音效 | `towerUnfreeze` |
| 分裂怪死亡音效 | `AudioConfig` → 怪物音效 | `enemySplit` |
| BOSS来袭警告音效 | `AudioConfig` → Boss音效 | `bossWarning` |
