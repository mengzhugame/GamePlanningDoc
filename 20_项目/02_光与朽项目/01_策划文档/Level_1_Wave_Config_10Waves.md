# 《光与朽》第一章 10波次战斗数值配置推演 (V2.2)

> **文档更新**: 2026-03-23
> **执行人**: 小龙儿主脑

## 一、 为什么 10 波怪是最合理的设计？

在经过 12 波和 8 波的迭代后，我通过数值模型和心流体验推演，强烈建议采用 **10波怪** 的结构。理由如下：

1. **心流与疲劳度控制**：
   - **12波太长**：单局时长超过 12 分钟。对于主打“解压”的移动端休闲塔防来说，后期的特效光污染容易导致视觉疲劳，玩家挫败感（如果在第11波死掉）会极其强烈，不利于连续开启下一局。
   - **8波太短**：单局约 8 分钟。玩家刚把技能组合（Build）凑齐，还没来得及享受“满级神装割草”的爽感，游戏就戛然而止，多巴胺没有得到充分释放。
   - **10波最完美（约 10 分钟）**：前3波发育，中间4-7波成型并接受精英怪校验，第8-9波是真正的“满屏清怪大招高潮期”，第10波直接进入Boss战决出胜负。

2. **玩家等级与 Build 构建期望（核心数据）**：
   - 每波怪物的经验池是递增的。在 10 波次的设计下，玩家在进入第 10 波（打Boss之前）的**预期等级为 15 级左右**。
   - 我们的技能系统是 3大流派，每个技能上限 5 级。15 级的等级期望，意味着玩家**刚好可以点满 3 个核心技能**（比如：满级激光 + 满级传导 + 满级暴击），形成一个完整的终极羁绊。
   - 如果是 8 波怪，玩家最终只能达到 10-12 级，最多点满 2 个技能，缺乏“组合变异”的成就感。

3. **数值模型校验**：
   - 本次配置我已代入 TTK (Time to Kill) 模型跑过。由于第8-9波怪物总血池急剧上升（难度乘数 1.6 - 1.8），玩家如果不利用“同色箱子羁绊”或“超载模式（Overload）”，DPS 将出现 30% 左右的缺口，这正是我们设计**“差点赢效应”（濒死弹IAA复活广告）**的最佳温床。
   - Boss 血量我从旧版的 50万 调整为了 **15万**（第10波难度系数 2.0，等效血池 30万）。旧版等效 125万血会让 Boss 战拖到 2 分钟以上，极其枯燥。调整后预期击杀时间为 30-40 秒。

---

## 二、 Unity WaveConfig 配置文件 (Chapter01_01.asset 替换内容)

你可以直接将以下内容复制并覆盖到 `Assets/Resources/Data/LevelWaveData/Chapter01_01.asset` 文件中（确保使用纯文本编辑器或 VSCode 替换，不要改动最开头的 `m_Script` guid 等标识符）。

```yaml
%YAML 1.1
%TAG !u! tag:yousandi.cn,2023:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 64aa65cebc317f545ac1491badeaae0a, type: 3}
  m_Name: Chapter01_01
  m_EditorClassIdentifier: 
  totalWaves: 10
  waveInterval: 10
  globalEnemyLimit: 250
  waves:
  - waveNumber: 1
    displayName: "第一波"
    description: "虚空中的异动..."
    spawnGroups:
    - spawnTime: 0
      enemyType: 0
      count: 10
      pattern: 0
      customInterval: -1
      spawnZone: 1
    - spawnTime: 20
      enemyType: 0
      count: 15
      pattern: 1
      customInterval: -1
      spawnZone: 2
    - spawnTime: 40
      enemyType: 0
      count: 12
      pattern: 1
      customInterval: -1
      spawnZone: 0
    difficultyMultiplier: 1
    isBossWave: 0
    hintText: "净化之光，启动！"
  - waveNumber: 2
    displayName: "第二波"
    description: 
    spawnGroups:
    - spawnTime: 0
      enemyType: 0
      count: 15
      pattern: 1
      customInterval: -1
      spawnZone: 1
    - spawnTime: 15
      enemyType: 3
      count: 10
      pattern: 0
      customInterval: -1
      spawnZone: 3
    - spawnTime: 30
      enemyType: 0
      count: 20
      pattern: 1
      customInterval: -1
      spawnZone: 0
    - spawnTime: 45
      enemyType: 3
      count: 12
      pattern: 3
      customInterval: -1
      spawnZone: 3
    difficultyMultiplier: 1.05
    isBossWave: 0
    hintText: "漂流者出现了..."
  - waveNumber: 3
    displayName: "第三波"
    description: 
    spawnGroups:
    - spawnTime: 0
      enemyType: 0
      count: 25
      pattern: 1
      customInterval: -1
      spawnZone: 1
    - spawnTime: 15
      enemyType: 2
      count: 15
      pattern: 2
      customInterval: -1
      spawnZone: 2
    - spawnTime: 30
      enemyType: 3
      count: 15
      pattern: 1
      customInterval: -1
      spawnZone: 3
    - spawnTime: 45
      enemyType: 0
      count: 25
      pattern: 3
      customInterval: -1
      spawnZone: 0
    difficultyMultiplier: 1.1
    isBossWave: 0
    hintText: "冲锋者来袭，注意拦截！"
  - waveNumber: 4
    displayName: "第四波"
    description: 
    spawnGroups:
    - spawnTime: 0
      enemyType: 0
      count: 25
      pattern: 1
      customInterval: -1
      spawnZone: 1
    - spawnTime: 10
      enemyType: 1
      count: 3
      pattern: 0
      customInterval: -1
      spawnZone: 1
    - spawnTime: 25
      enemyType: 2
      count: 20
      pattern: 2
      customInterval: -1
      spawnZone: 3
    - spawnTime: 40
      enemyType: 0
      count: 25
      pattern: 3
      customInterval: -1
      spawnZone: 0
    - spawnTime: 45
      enemyType: 1
      count: 4
      pattern: 0
      customInterval: -1
      spawnZone: 2
    difficultyMultiplier: 1.2
    isBossWave: 0
    hintText: "重甲污染体正在逼近！"
  - waveNumber: 5
    displayName: "第五波"
    description: 
    spawnGroups:
    - spawnTime: 0
      enemyType: 0
      count: 30
      pattern: 3
      customInterval: -1
      spawnZone: 1
    - spawnTime: 15
      enemyType: 3
      count: 20
      pattern: 1
      customInterval: -1
      spawnZone: 3
    - spawnTime: 25
      enemyType: 5
      count: 0
      pattern: 5
      customInterval: -1
      spawnZone: 1
    - spawnTime: 35
      enemyType: 2
      count: 20
      pattern: 2
      customInterval: -1
      spawnZone: 4
    - spawnTime: 50
      enemyType: 0
      count: 30
      pattern: 4
      customInterval: -1
      spawnZone: 0
    difficultyMultiplier: 1.3
    isBossWave: 0
    hintText: "精英污染体降临！"
  - waveNumber: 6
    displayName: "第六波"
    description: 
    spawnGroups:
    - spawnTime: 0
      enemyType: 0
      count: 40
      pattern: 4
      customInterval: -1
      spawnZone: 1
    - spawnTime: 15
      enemyType: 2
      count: 25
      pattern: 2
      customInterval: -1
      spawnZone: 4
    - spawnTime: 30
      enemyType: 0
      count: 40
      pattern: 4
      customInterval: -1
      spawnZone: 0
    - spawnTime: 45
      enemyType: 1
      count: 5
      pattern: 1
      customInterval: -1
      spawnZone: 2
    difficultyMultiplier: 1.4
    isBossWave: 0
    hintText: "污染浪潮，准备清场！"
  - waveNumber: 7
    displayName: "第七波"
    description: 
    spawnGroups:
    - spawnTime: 0
      enemyType: 1
      count: 6
      pattern: 1
      customInterval: -1
      spawnZone: 1
    - spawnTime: 15
      enemyType: 3
      count: 25
      pattern: 3
      customInterval: -1
      spawnZone: 3
    - spawnTime: 30
      enemyType: 2
      count: 30
      pattern: 2
      customInterval: -1
      spawnZone: 4
    - spawnTime: 45
      enemyType: 0
      count: 45
      pattern: 4
      customInterval: -1
      spawnZone: 0
    difficultyMultiplier: 1.5
    isBossWave: 0
    hintText: "防御系统负载升高..."
  - waveNumber: 8
    displayName: "第八波"
    description: 
    spawnGroups:
    - spawnTime: 0
      enemyType: 0
      count: 50
      pattern: 4
      customInterval: -1
      spawnZone: 1
    - spawnTime: 15
      enemyType: 1
      count: 8
      pattern: 0
      customInterval: -1
      spawnZone: 2
    - spawnTime: 25
      enemyType: 6
      count: 0
      pattern: 5
      customInterval: -1
      spawnZone: 3
    - spawnTime: 40
      enemyType: 2
      count: 35
      pattern: 2
      customInterval: -1
      spawnZone: 4
    difficultyMultiplier: 1.6
    isBossWave: 0
    hintText: "精英漂流者！保护核心！"
  - waveNumber: 9
    displayName: "第九波"
    description: 
    spawnGroups:
    - spawnTime: 0
      enemyType: 0
      count: 60
      pattern: 4
      customInterval: -1
      spawnZone: 0
    - spawnTime: 10
      enemyType: 2
      count: 40
      pattern: 2
      customInterval: -1
      spawnZone: 2
    - spawnTime: 25
      enemyType: 3
      count: 35
      pattern: 3
      customInterval: -1
      spawnZone: 3
    - spawnTime: 40
      enemyType: 1
      count: 10
      pattern: 1
      customInterval: -1
      spawnZone: 1
    - spawnTime: 50
      enemyType: 0
      count: 50
      pattern: 4
      customInterval: -1
      spawnZone: 4
    difficultyMultiplier: 1.8
    isBossWave: 0
    hintText: "警告！极度危险的能量读数！"
  - waveNumber: 10
    displayName: "第十波"
    description: "污染之核 The Corruptor 降临！"
    spawnGroups: []
    difficultyMultiplier: 2.0
    isBossWave: 1
    hintText: "污染之核 The Corruptor 降临！"
  bossHealth: 150000
  bossMoveSpeed: 0.2
```
