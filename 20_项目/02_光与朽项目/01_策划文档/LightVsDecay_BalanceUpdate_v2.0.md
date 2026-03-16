# 《光与朽 Light vs Decay》平衡性优化方案 v2.0

> **文档版本**: 2.0  
> **创建日期**: 2025-01-26  
> **状态**: 已确认，待实施

---

## 📋 修改概览

| 模块 | 优先级 | 复杂度 | 预计工时 |
|------|--------|--------|----------|
| 模块1：数值膨胀 | ⭐⭐⭐ 高 | 🔧 低 | 1小时 |
| 模块2：暴击系统重做 | ⭐⭐⭐ 高 | 🔧 低 | 1小时 |
| 模块3：Gacha机制优化 | ⭐⭐ 中 | 🔧🔧 中 | 2小时 |
| 模块4：Focus技能重做 | ⭐⭐ 中 | 🔧 低 | 1小时 |
| 模块5：Chain技能（新建） | ⭐⭐ 中 | 🔧🔧🔧 高 | 4小时 |
| 模块6：波次调整 | ⭐⭐ 中 | 🔧 低 | 1小时 |
| 模块7：大招系统（新建） | ⭐ 低 | 🔧🔧🔧 高 | 6小时 |

**建议实施顺序**: 模块1 → 模块2 → 模块4 → 模块6 → 模块3 → 模块5 → 模块7

---

## 模块1：数值膨胀 (Big Numbers)

### 1.1 修改目标

将基础伤害从10提升至100，让玩家能明显感受到数值成长。

### 1.2 数值变更表

| 项目 | 旧值 | 新值 | 倍率 |
|------|------|------|------|
| 基础 DPS | 100 | **1000** | ×10 |
| TickRate | 0.1s | 0.1s | 不变 |
| 每跳伤害 | 10 | **100** | ×10 |

### 1.3 怪物血量同步膨胀

| 怪物类型 | 旧血量 | 新血量 | 倍率 |
|----------|--------|--------|------|
| Slime（普通史莱姆） | 50 | **500** | ×10 |
| Rusher（突进怪） | 30 | **300** | ×10 |
| Tank（坦克怪） | 200 | **2000** | ×10 |
| Drifter（漂流怪） | 40 | **400** | ×10 |
| EliteSlime（精英史莱姆） | 150 | **1500** | ×10 |
| EliteTank（精英坦克） | 500 | **5000** | ×10 |
| Boss | 5000 | **50000** | ×10 |

### 1.4 涉及文件

```
Assets/Scripts/Core/GameConstants.cs
Assets/Scripts/Data/SO/GameSettings.cs
Assets/Scripts/Data/SO/EnemyConfig.cs (每种怪物的配置)
Assets/Scripts/Data/SO/BossConfig.cs
```

### 1.5 具体修改点

#### GameConstants.cs
```csharp
// 修改前
public const float BASE_DPS = 100f;
public const float DAMAGE_PER_TICK = BASE_DPS * DAMAGE_TICK_RATE; // = 10

// 修改后
public const float BASE_DPS = 1000f;
public const float DAMAGE_PER_TICK = BASE_DPS * DAMAGE_TICK_RATE; // = 100
```

#### GameSettings.cs (ScriptableObject)
```csharp
// 修改 Inspector 中的值
[Header("激光伤害")]
public float baseDPS = 1000f;  // 旧值: 100f
```

#### 各 EnemyConfig.cs (ScriptableObject)
```csharp
// 在 Unity Inspector 中修改每个敌人配置的 maxHealth 字段
// 全部乘以 10
```

### 1.6 测试要点

- [ ] 激光命中敌人时，飘字显示100+
- [ ] 暴击时飘字显示200+
- [ ] 普通怪物击杀时间与修改前保持一致（约3-5秒）
- [ ] Boss击杀时间与修改前保持一致

---

## 模块2：暴击系统重做

### 2.1 修改目标

- 基础暴击率从10%降至5%
- 设置暴击上限50%
- Crit技能每级+8%（满级+40%）
- 无人机奖励：Normal +3%，Epic +8%

### 2.2 数值变更表

| 项目 | 旧值 | 新值 |
|------|------|------|
| 基础暴击率 | 10% | **5%** |
| 暴击上限 | 无 | **50%** |
| 暴击伤害倍率 | 200% | 200%（不变） |

### 2.3 Crit技能等级数据

| 等级 | 暴击率加成 | 累计暴击率（含基础5%） |
|------|-----------|----------------------|
| LV1 | +8% | 13% |
| LV2 | +8% | 21% |
| LV3 | +8% | 29% |
| LV4 | +8% | 37% |
| LV5 | +8% | 45% |

### 2.4 涉及文件

```
Assets/Scripts/Core/GameConstants.cs
Assets/Scripts/Data/SO/GameSettings.cs
Assets/Scripts/Data/SO/SkillData.cs (Crit技能配置)
Assets/Scripts/Logic/Player/LaserController.cs
Assets/Scripts/Logic/Player/SkillEffectManager.cs
```

### 2.5 具体修改点

#### GameConstants.cs
```csharp
// 新增
public const float BASE_CRIT_RATE = 0.05f;      // 基础暴击率 5%
public const float MAX_CRIT_RATE = 0.50f;       // 暴击上限 50%
public const float CRIT_DAMAGE_MULTIPLIER = 2.0f; // 暴击伤害倍率
```

#### GameSettings.cs
```csharp
[Header("暴击系统")]
public float baseCritRate = 0.05f;    // 旧值: 0.1f
public float maxCritRate = 0.50f;     // 新增：暴击上限
```

#### LaserController.cs
```csharp
// 修改 CurrentCritRate 属性，添加上限
public float CurrentCritRate => Mathf.Min(baseCritRate + critRateBonus, gameSettings.maxCritRate);
```

#### Skill_Crit.asset (ScriptableObject 配置)
```
// 在 Unity Inspector 中修改 Crit 技能的 SkillLevelData
LV1: critRateBonus = 0.08
LV2: critRateBonus = 0.08
LV3: critRateBonus = 0.08
LV4: critRateBonus = 0.08
LV5: critRateBonus = 0.08

// 修改技能描述
LV1: "暴击率 +8%"
LV2: "暴击率 +16%"
LV3: "暴击率 +24%"
LV4: "暴击率 +32%"
LV5: "暴击率 +40%"
```

### 2.6 测试要点

- [ ] 游戏开始时，面板显示暴击率5%
- [ ] 选择Crit技能后，暴击率正确增加
- [ ] 暴击率不会超过50%（即使来源总和超过50%）
- [ ] 暴击时有明显的视觉反馈（不同颜色飘字）

---

## 模块3：Gacha机制优化

### 3.1 修改目标

- 删除"Nothing（空）"结果
- 添加负面保护（不连续2次负面）
- 负面效果改为"增强怪物"或"永久轻微削弱玩家"
- 智能奖励池（暴击≥45%时不再出暴击奖励）

### 3.2 概率分布调整

| 结果类型 | 旧概率 | 新概率 |
|----------|--------|--------|
| Nothing（空） | 10% | **删除** |
| Negative（负面） | 10% | **15%** |
| Normal（正常） | 70% | **70%** |
| Epic（大奖） | 10% | **15%** |

### 3.3 负面效果池

#### 增强怪物类（下一波生效）

| 效果名称 | 效果描述 | 权重 |
|----------|----------|------|
| 狂暴协议 | 下一波怪物移速 +30% | 25 |
| 强化外壳 | 下一波怪物血量 +40% | 25 |
| 过载核心 | 下一波怪物伤害 +25% | 25 |
| 急速脉冲 | 下一波怪物攻击速度 +20% | 10 |

#### 削弱玩家类（永久生效）

| 效果名称 | 效果描述 | 权重 |
|----------|----------|------|
| 信号干扰 | 攻击力 -5%（永久） | 5 |
| 数据衰减 | 暴击率 -1%（永久） | 5 |
| 透镜污染 | 激光宽度 -5%（永久） | 3 |
| 能量泄漏 | 激光长度 -5%（永久） | 2 |

### 3.4 正面奖励调整

| 奖励类型 | Normal数值 | Epic数值 |
|----------|-----------|----------|
| 攻击力 | +10% | +25% |
| 暴击率 | +3% | +8% |
| 激光宽度 | +0.1 | +0.3 |
| 激光长度 | +1 | +3 |
| 生命恢复 | +100 | +300 |
| 护盾恢复 | +50 | +150 |

### 3.5 智能奖励池规则

```
当玩家暴击率 ≥ 45% 时：
  - 从 Normal 奖励池移除 "暴击率+3%"
  - 从 Epic 奖励池移除 "暴击率+8%"
```

### 3.6 涉及文件

```
Assets/Scripts/Data/SO/DroneRewardConfig.cs
Assets/Scripts/Logic/TacticalDrop/TacticalDropManager.cs
Assets/Scripts/Logic/WaveManager.cs (新增：怪物增强效果)
```

### 3.7 具体修改点

#### DroneRewardConfig.cs

```csharp
// 1. 删除 GachaResultType.Nothing 的处理
// 2. 新增负面效果枚举
public enum NegativeEffectType
{
    // 增强怪物
    EnemySpeedBoost,      // 移速+30%
    EnemyHealthBoost,     // 血量+40%
    EnemyDamageBoost,     // 伤害+25%
    EnemyAttackSpeedBoost,// 攻击速度+20%
    
    // 削弱玩家（永久）
    PlayerDamageReduce,   // 攻击力-5%
    PlayerCritReduce,     // 暴击率-1%
    PlayerWidthReduce,    // 宽度-5%
    PlayerLengthReduce,   // 长度-5%
}

// 3. 新增负面效果配置类
[Serializable]
public class NegativeEffectEntry
{
    public NegativeEffectType type;
    public float value;
    public string displayText;
    public int weight;
    public bool affectsEnemy; // true=增强怪物, false=削弱玩家
}
```

#### TacticalDropManager.cs

```csharp
// 1. 添加上次结果记录
private GachaResultType lastGachaResult;

// 2. 修改 ProcessGachaReward 方法
// - 如果上次是 Negative，本次强制跳过 Negative
// - 添加智能奖励池逻辑

// 3. 新增负面效果应用方法
private void ApplyNegativeEffect(NegativeEffectEntry effect)
{
    if (effect.affectsEnemy)
    {
        // 通知 WaveManager 下一波增强
        WaveManager.Instance.SetNextWaveModifier(effect.type, effect.value);
    }
    else
    {
        // 永久削弱玩家
        ApplyPermanentDebuff(effect.type, effect.value);
    }
}
```

#### WaveManager.cs

```csharp
// 新增：下一波怪物修正器
private NegativeEffectType? nextWaveModifier;
private float nextWaveModifierValue;

public void SetNextWaveModifier(NegativeEffectType type, float value)
{
    nextWaveModifier = type;
    nextWaveModifierValue = value;
}

// 在生成怪物时应用修正器
private void ApplyWaveModifiers(EnemyBlob enemy)
{
    if (nextWaveModifier == null) return;
    
    switch (nextWaveModifier.Value)
    {
        case NegativeEffectType.EnemySpeedBoost:
            enemy.ApplySpeedModifier(1f + nextWaveModifierValue);
            break;
        case NegativeEffectType.EnemyHealthBoost:
            enemy.ApplyHealthModifier(1f + nextWaveModifierValue);
            break;
        // ... 其他效果
    }
}
```

### 3.8 测试要点

- [ ] Gacha不再出现"谢谢惠顾"（空）
- [ ] 连续2次负面结果不会发生
- [ ] 负面效果正确应用（怪物增强/玩家削弱）
- [ ] 玩家暴击率≥45%时，不再出暴击奖励
- [ ] 正面奖励数值正确

---

## 模块4：Focus技能重做

### 4.1 修改目标

- 技能重命名：Focus → 高能透镜 (Hyper Lens)
- 删除"激光变细"效果
- 改为纯伤害加成 + 对Boss额外伤害

### 4.2 技能等级数据

| 等级 | 伤害加成 | 对Boss额外伤害 | 描述 |
|------|---------|---------------|------|
| LV1 | +30% | +10% | "能量聚焦，伤害+30%，对Boss额外+10%" |
| LV2 | +50% | +20% | "能量聚焦，伤害+50%，对Boss额外+20%" |
| LV3 | +70% | +30% | "能量聚焦，伤害+70%，对Boss额外+30%" |
| LV4 | +100% | +40% | "能量聚焦，伤害+100%，对Boss额外+40%" |
| LV5 | +150% | +50% | "能量聚焦，伤害+150%，对Boss额外+50%" |

穿透伤害计算示例
场景： LV3聚能透镜，面板伤害100，命中3个排成一排的敌人
敌人A（第1个）：100 × 1.8（+80%加成）= 180 伤害
敌人B（第2个）：180 × 0.9（-10%衰减）= 162 伤害
敌人C（第3个）：162 × 0.9（-10%衰减）= 146 伤害
敌人D（第4个）：无法命中（LV3最多穿透3个，共命中4个）
LV5无限穿透示例：
敌人A：180 伤害
敌人B：162 伤害
敌人C：146 伤害
敌人D：131 伤害
敌人E：118 伤害
... 持续衰减直到没有敌人

方案A：激光视觉不穿透，伤害判定穿透
┌────────────────────────────────────┐
│  视觉效果：激光射到第一个敌人停止    │
│  伤害判定：射线继续检测后方敌人      │
│  飘字显示：后方敌人也显示受伤飘字    │
└────────────────────────────────────┘
这样玩家能感受到"穿透"效果（后方敌人掉血），但视觉上不会显得奇怪。

真实伤害"的具体规则
无视Boss护甲 + 无视连体Buff减伤
这意味着：

普通攻击打Boss身体：100伤害 × 20%（80%减伤）= 20实际伤害
真实伤害打Boss身体：100伤害 × 100% = 100实际伤害




### 4.3 涉及文件

```
Assets/Scripts/Data/SO/SkillData.cs
Assets/Scripts/Logic/Player/SkillEffectManager.cs
Assets/Scripts/Logic/Player/LaserController.cs
Assets/Resources/Skills/Skill_Focus.asset
```

### 4.4 具体修改点

#### SkillData.cs

```csharp
// SkillLevelData 类中，确保有以下字段
[Header("伤害相关")]
public float damageMultiplier = 1.0f;      // 伤害倍率
public float bossDamageBonus = 0f;         // 对Boss额外伤害（已存在）

// 删除或忽略 widthMultiplier 在 Focus 中的使用
```

#### SkillEffectManager.cs

```csharp
// 修改 ApplyFocusEffect 方法
private void ApplyFocusEffect(int level, SkillData skillData)
{
    focusLevel = level;
    
    var levelData = GetLevelData(skillData, level);
    if (levelData == null) return;
    
    // 【修改】不再修改宽度，只修改伤害
    // 删除: totalWidthBonus = ... 
    // 删除: UpdateWidthMultiplier();
    
    // 应用伤害加成
    float damageBonus = levelData.damageMultiplier - 1f;
    float bossBonus = levelData.bossDamageBonus;
    
    if (laserController != null)
    {
        laserController.SetFocusDamageBonus(damageBonus, bossBonus);
    }
}
```

#### LaserController.cs

```csharp
// 新增 Focus 伤害加成字段
private float focusDamageBonus = 0f;
private float focusBossDamageBonus = 0f;

public void SetFocusDamageBonus(float damageBonus, float bossBonus)
{
    focusDamageBonus = damageBonus;
    focusBossDamageBonus = bossBonus;
}

// 修改伤害计算，添加 Focus 和 Boss 加成
private float CalculateDamage(bool isBoss)
{
    float baseDamage = CurrentDamagePerTick;
    
    // Focus 伤害加成
    baseDamage *= (1f + focusDamageBonus);
    
    // 对 Boss 额外伤害
    if (isBoss)
    {
        baseDamage *= (1f + focusBossDamageBonus);
    }
    
    return baseDamage;
}
```

#### Skill_Focus.asset (Unity Inspector)

```
displayName: "高能透镜"
description: "给激光充能，大幅提高伤害，对Boss造成额外伤害"

levels[0]: damageMultiplier=1.3, bossDamageBonus=0.1, widthMultiplier=1.0
levels[1]: damageMultiplier=1.5, bossDamageBonus=0.2, widthMultiplier=1.0
levels[2]: damageMultiplier=1.7, bossDamageBonus=0.3, widthMultiplier=1.0
levels[3]: damageMultiplier=2.0, bossDamageBonus=0.4, widthMultiplier=1.0
levels[4]: damageMultiplier=2.5, bossDamageBonus=0.5, widthMultiplier=1.0
```

### 4.5 测试要点

- [ ] 选择Focus技能后，激光宽度不再变化
- [ ] 伤害正确增加（+30%~+150%）
- [ ] 对Boss伤害正确增加（额外+10%~+50%）
- [ ] Wide技能不再被Focus影响

---

## 模块5：Chain技能（新建）

### 5.1 修改目标

- 新建技能：连锁反应 (Chain Reaction)
- 替代原 Reflex（反射透镜）技能
- 激光击中敌人后传导到附近敌人

### 5.2 技能等级数据

| 等级 | 传导次数 | 传导距离 | 伤害衰减 | 描述 |
|------|---------|---------|---------|------|
| LV1 | 1次 | 3米 | 20% | "激光传导至1个敌人，伤害衰减20%" |
| LV2 | 2次 | 3米 | 15% | "激光传导至2个敌人，伤害衰减15%" |
| LV3 | 3次 | 3米 | 10% | "激光传导至3个敌人，伤害衰减10%" |
| LV4 | 4次 | 3米 | 5% | "激光传导至4个敌人，伤害衰减5%" |
| LV5 | 5次 | 3米 | 0% | "激光传导至5个敌人，伤害不衰减" |

### 5.3 涉及文件

```
Assets/Scripts/Data/SO/SkillData.cs (修改)
Assets/Scripts/Logic/Player/SkillEffectManager.cs (修改)
Assets/Scripts/Logic/Player/LaserController.cs (修改)
Assets/Scripts/Logic/Player/ChainLightningController.cs (新建)
Assets/Scripts/VFX/ChainLightningVFX.cs (新建)
Assets/Resources/Skills/Skill_Chain.asset (新建)
Assets/Prefabs/VFX/ChainLightning.prefab (新建)
Assets/Materials/ChainLightning_Mat.mat (新建)
Assets/Textures/ChainLightning_Sequence.png (需要美术资源)
```

### 5.4 具体修改点

#### SkillData.cs - 新增字段

```csharp
// 在 SkillLevelData 类中新增
[Header("连锁反应相关（Chain）")]
[Tooltip("传导次数")]
public int chainCount = 0;

[Tooltip("传导距离（米）")]
public float chainRange = 3f;

[Tooltip("每次传导伤害衰减 (0.2 = 20%)")]
[Range(0f, 0.5f)]
public float chainDamageDecay = 0.2f;
```

#### SkillType 枚举修改

```csharp
public enum SkillType
{
    // 主动技能
    Prism,
    Focus,
    Impact,
    Chain,      // 【新增】替代 Reflex
    Frost,
    
    // 被动技能
    Power,
    Wide,
    Crit,
    Shatter,
}
```

#### ChainLightningController.cs (新建)

```csharp
// ============================================================
// ChainLightningController.cs
// 文件位置: Assets/Scripts/Logic/Player/ChainLightningController.cs
// 用途：连锁反应技能控制器
// ============================================================

using UnityEngine;
using System.Collections.Generic;
using LightVsDecay.Logic.Enemy;

namespace LightVsDecay.Logic.Player
{
    public class ChainLightningController : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private float chainRange = 3f;
        [SerializeField] private int maxChainCount = 1;
        [SerializeField] private float damageDecay = 0.2f;
        [SerializeField] private LayerMask enemyLayer;
        
        [Header("视觉效果")]
        [SerializeField] private GameObject chainVFXPrefab;
        
        // 运行时状态
        private int chainLevel = 0;
        private List<ChainLightningVFX> activeChains = new List<ChainLightningVFX>();
        
        /// <summary>
        /// 设置连锁等级
        /// </summary>
        public void SetChainLevel(int level, int chainCount, float range, float decay)
        {
            chainLevel = level;
            maxChainCount = chainCount;
            chainRange = range;
            damageDecay = decay;
        }
        
        /// <summary>
        /// 当主激光命中敌人时调用
        /// </summary>
        public void OnLaserHitEnemy(EnemyBlob sourceEnemy, float baseDamage, bool isCrit)
        {
            if (chainLevel <= 0 || maxChainCount <= 0) return;
            
            // 执行连锁传导
            ProcessChain(sourceEnemy, baseDamage, isCrit, 0, new HashSet<EnemyBlob> { sourceEnemy });
        }
        
        /// <summary>
        /// 递归处理连锁传导
        /// </summary>
        private void ProcessChain(EnemyBlob source, float damage, bool isCrit, int depth, HashSet<EnemyBlob> hitEnemies)
        {
            if (depth >= maxChainCount) return;
            
            // 计算衰减后的伤害
            float chainDamage = damage * (1f - damageDecay);
            
            // 查找范围内最近的未命中敌人
            EnemyBlob target = FindNearestEnemy(source.transform.position, hitEnemies);
            if (target == null) return;
            
            // 标记已命中
            hitEnemies.Add(target);
            
            // 造成伤害
            target.TakeDamage(chainDamage, isCrit);
            
            // 显示传导特效
            ShowChainVFX(source.transform.position, target.transform.position);
            
            // 继续传导
            ProcessChain(target, chainDamage, isCrit, depth + 1, hitEnemies);
        }
        
        /// <summary>
        /// 查找范围内最近的敌人
        /// </summary>
        private EnemyBlob FindNearestEnemy(Vector3 position, HashSet<EnemyBlob> excludeList)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, chainRange, enemyLayer);
            
            EnemyBlob nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (var col in colliders)
            {
                EnemyBlob enemy = col.GetComponentInParent<EnemyBlob>();
                if (enemy == null || excludeList.Contains(enemy)) continue;
                
                float dist = Vector3.Distance(position, enemy.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// 显示连锁特效
        /// </summary>
        private void ShowChainVFX(Vector3 from, Vector3 to)
        {
            // 从对象池获取或实例化
            // 使用面片+序列帧材质
        }
    }
}
```

#### ChainLightningVFX.cs (新建)

```csharp
// ============================================================
// ChainLightningVFX.cs
// 文件位置: Assets/Scripts/VFX/ChainLightningVFX.cs
// 用途：连锁闪电视觉效果（面片+序列帧）
// ============================================================

using UnityEngine;

namespace LightVsDecay.VFX
{
    public class ChainLightningVFX : MonoBehaviour
    {
        [Header("组件")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter meshFilter;
        
        [Header("序列帧配置")]
        [SerializeField] private int frameCount = 8;
        [SerializeField] private float frameRate = 24f;
        [SerializeField] private float lifetime = 0.2f;
        
        // 运行时
        private Material material;
        private float timer;
        private int currentFrame;
        private bool isPlaying;
        
        /// <summary>
        /// 播放连锁特效
        /// </summary>
        public void Play(Vector3 from, Vector3 to)
        {
            // 计算方向和长度
            Vector3 direction = to - from;
            float length = direction.magnitude;
            
            // 设置位置和旋转
            transform.position = (from + to) / 2f;
            transform.right = direction.normalized;
            
            // 设置缩放（X方向拉伸）
            transform.localScale = new Vector3(length, 0.3f, 1f);
            
            // 开始播放
            timer = 0f;
            currentFrame = 0;
            isPlaying = true;
            gameObject.SetActive(true);
        }
        
        private void Update()
        {
            if (!isPlaying) return;
            
            timer += Time.deltaTime;
            
            // 更新序列帧
            int newFrame = Mathf.FloorToInt(timer * frameRate) % frameCount;
            if (newFrame != currentFrame)
            {
                currentFrame = newFrame;
                UpdateUV();
            }
            
            // 生命周期结束
            if (timer >= lifetime)
            {
                isPlaying = false;
                gameObject.SetActive(false);
                // 返回对象池
            }
        }
        
        private void UpdateUV()
        {
            // 更新材质UV偏移以显示不同帧
            float uvOffset = (float)currentFrame / frameCount;
            material.SetTextureOffset("_MainTex", new Vector2(uvOffset, 0f));
        }
    }
}
```

### 5.5 美术资源需求

| 资源 | 规格 | 说明 |
|------|------|------|
| ChainLightning_Sequence.png | 1024×128 或 2048×256 | 8帧序列帧，水平排列 |
| ChainLightning_Mat.mat | Unlit/Transparent | 支持UV动画的材质 |

### 5.6 测试要点

- [ ] Chain技能选择后，激光命中敌人会传导
- [ ] 传导次数正确（LV1=1次，LV5=5次）
- [ ] 传导距离正确（3米内）
- [ ] 伤害衰减正确（LV1=20%，LV5=0%）
- [ ] 传导特效正确显示
- [ ] 不会传导到已命中的敌人（避免循环）

---

## 模块6：波次调整

### 6.1 修改目标

- 总波数从12波减少至8波
- Boss出现在第8波
- 目标玩家等级18级
- 经验倍率×2.0

### 6.2 数值变更表

| 项目 | 旧值 | 新值 |
|------|------|------|
| 总波数 | 12 | **8** |
| 普通怪物波数 | 11 | **7** |
| Boss波次 | 12 | **8** |
| 目标等级 | 16 | **18** |
| 经验倍率 | ×1.0 | **×2.0** |

### 6.3 涉及文件

```
Assets/Scripts/Data/SO/GameSettings.cs
Assets/Scripts/Data/SO/WaveConfig.cs (如果有)
Assets/Scripts/Logic/WaveManager.cs
Assets/Scripts/Logic/ProgressManager.cs
```

### 6.4 具体修改点

#### GameSettings.cs

```csharp
[Header("波次设置")]
public int totalWaves = 8;           // 旧值: 12
public int bossWave = 8;             // 旧值: 12

[Header("经验系统")]
public float expMultiplier = 2.0f;   // 新增：经验倍率
```

#### ProgressManager.cs

```csharp
// 修改经验计算方法
public void AddExperience(int baseExp)
{
    float multiplier = gameSettings != null ? gameSettings.expMultiplier : 1f;
    int actualExp = Mathf.RoundToInt(baseExp * multiplier);
    
    currentExp += actualExp;
    CheckLevelUp();
}
```

#### WaveManager.cs

```csharp
// 确保 Boss 波次正确
private void StartWave(int waveNumber)
{
    if (waveNumber >= gameSettings.bossWave)
    {
        StartBossWave();
    }
    else
    {
        StartNormalWave(waveNumber);
    }
}
```

### 6.5 测试要点

- [ ] 游戏只有8波
- [ ] 第8波是Boss
- [ ] 打完7波后玩家约18级
- [ ] 升级频率比原来高（约每30秒升一级）

---

## 模块7：大招系统（新建）

### 7.1 修改目标

新建"超载模式 (Overload)"大招系统：
- 游戏开始默认拥有
- 击杀50只怪或60秒自动充满
- 持续5秒：无敌 + 伤害×2 + 宽度×2 + 自动吸附
- 8秒强制冷却

### 7.2 技能参数

| 参数 | 数值 |
|------|------|
| 充能条件 - 击杀 | 50只怪 |
| 充能条件 - 时间 | 60秒 |
| 持续时间 | 5秒 |
| 强制冷却 | 8秒 |
| 伤害倍率 | ×2 |
| 宽度倍率 | ×2 |
| 自动吸附 | ✅ |
| 回血 | ❌ |

### 7.3 涉及文件

```
Assets/Scripts/Logic/Player/OverloadController.cs (新建)
Assets/Scripts/Logic/Player/LaserController.cs (修改)
Assets/Scripts/Logic/Player/TurretHealth.cs (修改)
Assets/Scripts/UI/OverloadButton.cs (新建)
Assets/Prefabs/UI/OverloadButton.prefab (新建)
```

### 7.4 具体修改点

#### OverloadController.cs (新建)

```csharp
// ============================================================
// OverloadController.cs
// 文件位置: Assets/Scripts/Logic/Player/OverloadController.cs
// 用途：超载模式大招控制器
// ============================================================

using UnityEngine;
using System;
using LightVsDecay.Core;

namespace LightVsDecay.Logic.Player
{
    public class OverloadController : MonoBehaviour
    {
        public static OverloadController Instance { get; private set; }
        
        [Header("充能配置")]
        [SerializeField] private int killsToCharge = 50;
        [SerializeField] private float timeToCharge = 60f;
        
        [Header("效果配置")]
        [SerializeField] private float duration = 5f;
        [SerializeField] private float cooldown = 8f;
        [SerializeField] private float damageMultiplier = 2f;
        [SerializeField] private float widthMultiplier = 2f;
        
        [Header("自动瞄准")]
        [SerializeField] private float autoAimSpeed = 180f; // 度/秒
        [SerializeField] private LayerMask enemyLayer;
        
        // 状态
        public enum OverloadState { Charging, Ready, Active, Cooldown }
        public OverloadState CurrentState { get; private set; } = OverloadState.Charging;
        
        // 充能进度
        private int killCount = 0;
        private float chargeTimer = 0f;
        public float ChargeProgress => Mathf.Max(
            (float)killCount / killsToCharge,
            chargeTimer / timeToCharge
        );
        
        // 持续/冷却计时
        private float activeTimer = 0f;
        private float cooldownTimer = 0f;
        
        // 事件
        public event Action<OverloadState> OnStateChanged;
        public event Action<float> OnChargeProgressChanged;
        
        // 引用
        private LaserController laserController;
        private TurretHealth turretHealth;
        
        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            laserController = FindObjectOfType<LaserController>();
            turretHealth = FindObjectOfType<TurretHealth>();
            
            // 监听击杀事件
            GameEvents.OnEnemyKilled += OnEnemyKilled;
        }
        
        private void OnDestroy()
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }
        
        private void Update()
        {
            switch (CurrentState)
            {
                case OverloadState.Charging:
                    UpdateCharging();
                    break;
                    
                case OverloadState.Active:
                    UpdateActive();
                    break;
                    
                case OverloadState.Cooldown:
                    UpdateCooldown();
                    break;
            }
        }
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 状态更新
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        
        private void UpdateCharging()
        {
            chargeTimer += Time.deltaTime;
            OnChargeProgressChanged?.Invoke(ChargeProgress);
            
            if (ChargeProgress >= 1f)
            {
                SetState(OverloadState.Ready);
            }
        }
        
        private void UpdateActive()
        {
            activeTimer -= Time.deltaTime;
            
            // 自动瞄准逻辑
            if (!Input.GetMouseButton(0))
            {
                AutoAimToNearestEnemy();
            }
            
            if (activeTimer <= 0f)
            {
                EndOverload();
            }
        }
        
        private void UpdateCooldown()
        {
            cooldownTimer -= Time.deltaTime;
            
            if (cooldownTimer <= 0f)
            {
                ResetCharge();
                SetState(OverloadState.Charging);
            }
        }
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共接口
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        
        /// <summary>
        /// 激活超载模式（由UI按钮调用）
        /// </summary>
        public void ActivateOverload()
        {
            if (CurrentState != OverloadState.Ready) return;
            
            activeTimer = duration;
            
            // 应用效果
            if (laserController != null)
            {
                laserController.SetOverloadMode(true, damageMultiplier, widthMultiplier);
            }
            
            if (turretHealth != null)
            {
                turretHealth.SetInvincible(true);
            }
            
            SetState(OverloadState.Active);
        }
        
        /// <summary>
        /// 结束超载模式
        /// </summary>
        private void EndOverload()
        {
            // 移除效果
            if (laserController != null)
            {
                laserController.SetOverloadMode(false, 1f, 1f);
            }
            
            if (turretHealth != null)
            {
                turretHealth.SetInvincible(false);
            }
            
            // 进入冷却
            cooldownTimer = cooldown;
            SetState(OverloadState.Cooldown);
        }
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 自动瞄准
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        
        private void AutoAimToNearestEnemy()
        {
            if (laserController == null) return;
            
            // 查找最近敌人
            Collider2D[] enemies = Physics2D.OverlapCircleAll(
                laserController.transform.position, 
                20f, 
                enemyLayer
            );
            
            if (enemies.Length == 0) return;
            
            // 找最近的
            Transform nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (var col in enemies)
            {
                float dist = Vector2.Distance(
                    laserController.transform.position, 
                    col.transform.position
                );
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = col.transform;
                }
            }
            
            if (nearest != null)
            {
                // 平滑转向
                laserController.AutoAimTowards(nearest.position, autoAimSpeed * Time.deltaTime);
            }
        }
        
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 内部方法
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        
        private void OnEnemyKilled(EnemyType type)
        {
            if (CurrentState == OverloadState.Charging)
            {
                killCount++;
                OnChargeProgressChanged?.Invoke(ChargeProgress);
            }
        }
        
        private void ResetCharge()
        {
            killCount = 0;
            chargeTimer = 0f;
        }
        
        private void SetState(OverloadState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
```

#### LaserController.cs 修改

```csharp
// 新增超载模式字段
private bool isOverloadActive = false;
private float overloadDamageMultiplier = 1f;
private float overloadWidthMultiplier = 1f;

/// <summary>
/// 设置超载模式
/// </summary>
public void SetOverloadMode(bool active, float damageMult, float widthMult)
{
    isOverloadActive = active;
    overloadDamageMultiplier = damageMult;
    overloadWidthMultiplier = widthMult;
    
    // 更新激光宽度
    UpdateLaserWidth();
}

/// <summary>
/// 自动瞄准（超载模式用）
/// </summary>
public void AutoAimTowards(Vector3 targetPosition, float maxAngle)
{
    Vector3 direction = (targetPosition - transform.position).normalized;
    float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
    float currentAngle = transform.eulerAngles.z;
    float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxAngle);
    
    transform.rotation = Quaternion.Euler(0, 0, newAngle);
}

// 修改伤害计算
public float CurrentDamagePerTick => baseDPS * tickRate * skillDamageMultiplier * overloadDamageMultiplier;

// 修改宽度计算
public float CurrentLaserWidth => baseLaserWidth * skillWidthMultiplier * overloadWidthMultiplier;
```

#### TurretHealth.cs 修改

```csharp
// 新增无敌状态
private bool isInvincible = false;

public void SetInvincible(bool invincible)
{
    isInvincible = invincible;
}

// 修改受伤方法
public void TakeDamage(int damage)
{
    if (isInvincible) return;  // 无敌状态不受伤
    
    // ... 原有逻辑
}
```

#### OverloadButton.cs (新建)

```csharp
// ============================================================
// OverloadButton.cs
// 文件位置: Assets/Scripts/UI/OverloadButton.cs
// 用途：超载模式UI按钮
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LightVsDecay.Logic.Player;

namespace LightVsDecay.UI
{
    public class OverloadButton : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private Button button;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("状态颜色")]
        [SerializeField] private Color chargingColor = Color.gray;
        [SerializeField] private Color readyColor = Color.yellow;
        [SerializeField] private Color activeColor = Color.cyan;
        [SerializeField] private Color cooldownColor = Color.red;
        
        private OverloadController controller;
        
        private void Start()
        {
            controller = OverloadController.Instance;
            
            if (controller != null)
            {
                controller.OnStateChanged += UpdateVisual;
                controller.OnChargeProgressChanged += UpdateProgress;
            }
            
            button.onClick.AddListener(OnButtonClick);
            
            UpdateVisual(controller?.CurrentState ?? OverloadController.OverloadState.Charging);
        }
        
        private void OnButtonClick()
        {
            controller?.ActivateOverload();
        }
        
        private void UpdateVisual(OverloadController.OverloadState state)
        {
            switch (state)
            {
                case OverloadController.OverloadState.Charging:
                    iconImage.color = chargingColor;
                    statusText.text = "";
                    button.interactable = false;
                    break;
                    
                case OverloadController.OverloadState.Ready:
                    iconImage.color = readyColor;
                    statusText.text = "READY";
                    button.interactable = true;
                    break;
                    
                case OverloadController.OverloadState.Active:
                    iconImage.color = activeColor;
                    statusText.text = "ACTIVE";
                    button.interactable = false;
                    break;
                    
                case OverloadController.OverloadState.Cooldown:
                    iconImage.color = cooldownColor;
                    statusText.text = "";
                    button.interactable = false;
                    break;
            }
        }
        
        private void UpdateProgress(float progress)
        {
            fillImage.fillAmount = progress;
        }
    }
}
```

### 7.5 UI布局

```
屏幕右下角：
┌─────────────────┐
│                 │
│                 │
│                 │
│                 │
│           [🔘]  │  ← 超载按钮（圆形，带充能进度条）
└─────────────────┘
```

### 7.6 测试要点

- [ ] 游戏开始时，大招按钮显示充能状态
- [ ] 击杀50只怪或60秒后，按钮变为可点击
- [ ] 点击后进入超载模式，持续5秒
- [ ] 超载期间：无敌、伤害×2、宽度×2
- [ ] 玩家不触摸时，自动瞄准最近敌人
- [ ] 玩家触摸时，手动控制优先
- [ ] 超载结束后，8秒冷却期无法充能
- [ ] 冷却结束后，重新开始充能

---

## 📋 实施检查清单

### 阶段1：数值基础（模块1+2）
- [ ] 修改 GameConstants.cs 中的 BASE_DPS
- [ ] 修改 GameSettings.cs 中的 baseDPS
- [ ] 修改所有 EnemyConfig 的血量（×10）
- [ ] 修改 BossConfig 的血量（×10）
- [ ] 修改 GameSettings.cs 中的 baseCritRate 为 5%
- [ ] 新增暴击上限 maxCritRate = 50%
- [ ] 测试：确认伤害飘字显示100+

### 阶段2：技能重做（模块4）
- [ ] 修改 Skill_Focus.asset 的等级数据
- [ ] 修改 SkillEffectManager.cs 的 ApplyFocusEffect
- [ ] 修改 LaserController.cs 添加 Focus 伤害加成
- [ ] 测试：确认 Focus 不再影响宽度

### 阶段3：波次调整（模块6）
- [ ] 修改 GameSettings.cs 的 totalWaves = 8
- [ ] 修改 GameSettings.cs 的 bossWave = 8
- [ ] 修改 ProgressManager.cs 添加经验倍率
- [ ] 测试：确认8波后出Boss，7波后约18级

### 阶段4：Gacha优化（模块3）
- [ ] 修改 DroneRewardConfig.cs 删除 Nothing
- [ ] 新增负面效果类型和配置
- [ ] 修改 TacticalDropManager.cs 添加负面保护
- [ ] 添加智能奖励池逻辑
- [ ] 修改 WaveManager.cs 支持怪物增强
- [ ] 测试：确认无空奖励，不连续负面

### 阶段5：Chain技能（模块5）
- [ ] 新建 ChainLightningController.cs
- [ ] 新建 ChainLightningVFX.cs
- [ ] 新建 Skill_Chain.asset
- [ ] 准备闪电链序列帧资源
- [ ] 修改 SkillType 枚举
- [ ] 修改 SkillEffectManager.cs 支持 Chain
- [ ] 测试：确认传导次数和伤害衰减正确

### 阶段6：大招系统（模块7）
- [ ] 新建 OverloadController.cs
- [ ] 新建 OverloadButton.cs
- [ ] 修改 LaserController.cs 支持超载模式
- [ ] 修改 TurretHealth.cs 支持无敌状态
- [ ] 创建 UI 按钮预制体
- [ ] 测试：确认充能、激活、效果、冷却全流程

---

## 📝 备注

1. **美术资源需求**：
   - 闪电链序列帧贴图（模块5）
   - 超载按钮UI图标（模块7）
   - 超载激活特效（可选）

2. **音效需求**：
   - 闪电链传导音效
   - 超载激活音效
   - 超载结束音效
   - 充能完成提示音

3. **后续扩展预留**：
   - 天赋系统接口（超载回血、清屏等）
   - 无尽模式波次配置
   - 更多负面效果类型

---

**文档完成，等待确认后按步骤实施。**
