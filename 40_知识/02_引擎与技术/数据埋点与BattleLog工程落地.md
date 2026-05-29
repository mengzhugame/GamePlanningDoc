---
type: knowledge
status: review
created: 2026-04-29
updated: 2026-05-22
source_book: 光与朽数据埋点与 IAA 系统文档（V2 双源）+ MVP 草稿 + WXAdsManager 模板
source_page: 20_项目/02_光与朽项目/01_策划文档/光与朽_数据埋点系统文档_V2.0.md; 20_项目/02_光与朽项目/01_策划文档/光与朽_MVP数据埋点需求文档.md; 20_项目/02_光与朽项目/01_策划文档/光与朽_IAA广告接入系统文档_V2.0.md; 40_知识/02_引擎与技术/代码模板库/06_WXAdsManager.md; 10_流水/光与朽项目/Claude-2026-04-15.md; 10_流水/光与朽项目/Claude-2026-04-16.md
domain: 02_引擎与技术
tags: [数据埋点, BattleLog, 微信场景分析, IAA埋点, AnalyticsManager, 工程化]
last_reviewed: 2026-05-22
review_count: 9
---

# 数据埋点与 BattleLog 工程落地

> 这条知识回答的是：**新项目要上线时，埋点系统应该怎么搭，才能既不被工时吃垮，又能在上线后真的帮你做决策？**
> 来源：光与朽 V2.0 数据埋点 + IAA 双文档（已落地待实装）的工程经验。

---

## 一、两层架构（先搭一期，二期再说）

把"上线必须有"和"上线后再补"分开，是这套方案能落地的核心。

```
┌─────────────────────────────────────────────────────────┐
│                    AnalyticsManager.cs                  │
│                       （统一入口）                       │
│              ↓                          ↓               │
│     wx.reportScene()            BattleStatistics.cs     │
│   一期：场景分析（无参数）       二期：CSV / 第三方 SDK  │
└─────────────────────────────────────────────────────────┘
```

| 层 | 工具 | 能上报什么 | 何时启用 |
|---|------|-----------|---------|
| **一期** | 微信后台 → 场景分析 → `wx.reportScene({ sceneId })` | 只能记录"事件发生了几次"——**不带参数** | **上线前必须有**。免费、零成本、官方留存看板内置 |
| **二期** | 友盟 Umeng / 自建后端 | 携带参数（死在第几波、选了哪个技能、构筑类型） | **上线后看一期数据反应再决定**。多数项目根本走不到二期 |
| **开发期专用** | `BattleStatistics.cs` 本地 CSV（126+ 字段） | 深度战斗数据（用于策划手动调数值，不上线） | **正式包必须用编译宏关掉**，`#if !UNITY_EDITOR && !DEVELOPMENT_BUILD return;` |

**核心纪律**：上线 MVP 不要碰二期。一期 22 个场景就够你做新手漏斗 + 广告意愿 + 章节通关三个最关键的看板。

---

## 二、AnalyticsManager 的 4 条工程纪律

落地一期所有埋点的入口都通过这个类。它本身极简，但有 4 条不能省的纪律：

### 1. 编译期分支，不要运行期判断

```csharp
public static void LogScene(string sceneId)
{
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
    WXSDKManager.Instance?.ReportScene(sceneId);
#else
    Debug.Log($"[Analytics] Scene: {sceneId}");
#endif
}
```

- Editor 内打日志，正式包才上报——开发期能看到所有事件触发情况
- 用 `#if` 不用 `Application.platform`：前者编译期剔除，后者运行时分支

### 2. 一次性事件单独走 `LogSceneOnce`

```csharp
public static void LogSceneOnce(string sceneId)
{
    string key = $"Analytics_Done_{sceneId}";
    if (PlayerPrefs.GetInt(key, 0) == 1) return;
    PlayerPrefs.SetInt(key, 1);
    LogScene(sceneId);
}
```

- 新手引导步骤、章节首通这类"每个玩家只该上报一次"的事件，必须走 `LogSceneOnce`
- 否则同一玩家多次触发会让漏斗数据严重偏离真实流失率

### 3. user_id 在首次启动时生成 UUID 并持久化

```csharp
private void InitUserId()
{
    if (!PlayerPrefs.HasKey("Analytics_UserId"))
        PlayerPrefs.SetString("Analytics_UserId", System.Guid.NewGuid().ToString());
}
```

- 微信小游戏没有强制账号系统，必须自己造一个稳定标识符
- 用 `PlayerPrefs` 而不是文件——前者切设备会丢，后者读写更复杂；选 PlayerPrefs 是接受"切设备 = 新用户"的代价

### 4. sceneId 集中常量化

**绝不要**让 `"battle_start_ch1"` 这类字符串散落在业务代码各处。集中放在 `AnalyticsConstants.cs`：

```csharp
public static class AnalyticsConstants
{
    public const string AppLaunch = "app_launch";
    public const string FirstBattleStart = "first_battle_start";
    public const string TutorialLaserDone = "tutorial_laser_done";
    // ...
}
```

否则一个字母笔误（`first_batle_start`）就会让微信后台找不到对应场景，数据永久丢失，上线后才发现就来不及了。

---

## 三、最少必要的场景清单（22 个就够）

光与朽 V2.0 文档敲定的 22 个场景，按优先级分组。**新项目可以直接抄结构**：

### 3.1 P0 基础漏斗（4 个，最高优先级）

| 场景名 | sceneId | 触发时机 | 一次性 |
|---|---|---|---|
| 游戏启动 | `app_launch` | 主界面 `Start()` | 否 |
| 首战开始 | `first_battle_start` | 首次进战斗（`isFirstPlay == true`）| **是** |
| 首战胜利 | `first_battle_win` | 首战通关 | **是** |
| 首战失败 | `first_battle_lose` | 首战死亡 | **是** |

→ 算转化率：`first_battle_start / app_launch` 是新手漏斗第一道门，目标 ≥ 70%

### 3.2 P0 广告意愿（6 个）

> **关键设计**：把"点击意愿"和"完播奖励"拆成两个事件——前者测**意愿**，后者测**实际渗透**。

| sceneId | 含义 |
|---|---|
| `ad_click_revive` / `ad_click_double` / `ad_click_reroll` / `ad_click_energy` | 点击了广告按钮（不管广告是否成功）|
| `ad_reward_revive` / `ad_reward_double` | 看完广告并发放奖励 |

→ 复活转化率 = `ad_reward_revive / ad_click_revive`，能直接判断 SDK 健康度

### 3.3 P1 新手引导步骤（6 个，全部一次性）

`tutorial_laser_done` / `tutorial_overload_done` / `tutorial_techtree_open` / `tutorial_techtree_upgrade` / `tutorial_equip_open` / `tutorial_equip_install`

→ 算各步骤转化率，能精确定位新手引导的卡点

### 3.4 P1 战斗里程碑（6 个，混合一次性 + 反复）

`battle_start` / `battle_win` / `battle_lose` / `chapter1_clear` / `chapter2_clear` / `chapter3_clear`

→ 章节通关漏斗 + 玩家黏度（人均战斗次数）

---

## 四、IAA 埋点：意愿 vs. 实际渗透要分开

这是上线后能不能调广告策略的关键。看光与朽 IAA 文档的设计：

| 事件 | 触发位置 | 意义 |
|------|---------|------|
| **`ad_click_*`**（6 个广告位各 1 个） | 玩家点击广告按钮的瞬间 | **意愿**：玩家想看广告（不管 SDK 好不好用）|
| **`ad_reward_*`**（仅复活/双倍 2 个） | 微信 SDK 回调 `isEnded == true` 后 | **实际渗透**：广告真的播完且奖励到位 |

**为什么不全做完播事件？**

- 体力补充/金币补充这类"刚需广告"，玩家点了就一定要看完才能拿——**点击 ≈ 完播**，多埋一个事件浪费
- 复活/双倍这类"挫败/正向情绪驱动广告"，**点击与完播之间会有 30% 左右的流失**——这是必须分开测的关键指标

### 频控规则要落到 PlayerPrefs

```csharp
// 每日观看次数：key 为 ad_watch_count_{date}_{adType}
// 每日重置：通过对比本地存储日期与当日日期判断
// 场次内状态（如 HasRevived）：存于内存，随场次初始化重置
```

不要把场次内频控也放 PlayerPrefs——退游戏再进就重置，但内存场次内是"本场限 1 次"的语义。

---

## 五、BattleLog（CSV）的工程纪律

`BattleStatistics.cs` 的 126+ 字段 CSV 是**开发期专用**，正式包必须关掉。

### 5.1 编译宏开关（必须有）

```csharp
private void ExportToCSV()
{
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
    return;  // 正式包不导出 CSV
#endif
    // 原有导出逻辑保持不变
}
```

### 5.2 字段分组（设计参考）

| 分组 | 字段数 | 说明 |
|------|--------|------|
| 基础信息 | 5 | 波次、构筑类型、玩家等级、结果、用时 |
| 击杀/生成统计 | 12+ | 各怪物类型的击杀数和生成数 |
| 玩家血量 | 9 | 开始/结束血量、受伤来源拆分 |
| 输出数据 | 9 | 各伤害来源、暴击统计、过量击杀 |
| 面板快照 | 3 | 当波次的 DPS / 暴击率 / 关键属性宽度 |
| Boss 数据 | 9+ | Boss 技能使用次数、阶段、剩余血量 |
| 章节特化 | 27-40+ | 各章节专属机制数据 |

→ 跨项目复用：每加一个新章节专属机制（火焰/冰冻/毒…），就在 `BattleStatTypes.cs` 加一组字段，逐波次 dump

### 5.3 文件路径与触发时机

- 路径：`{Application.persistentDataPath}/BattleLog_{timestamp}_{result}.csv`
- 触发：`OnGameVictory` 或 `OnGameDefeat`
- **不要**用 `Application.dataPath`——后者在不同平台的可写权限不一致

---

## 六、上线前的 4 个看板（直接抄）

在微信后台「场景分析→漏斗」中拼起来即可：

### 看板 1：新手漏斗（最重要）

```
app_launch  →  first_battle_start  →  first_battle_win/lose  →  tutorial_techtree_open  →  tutorial_techtree_upgrade
   100%        目标 ≥90%               目标 ≥70%                     目标 ≥50%                    目标 ≥80%
```

### 看板 2：广告意愿分析

| 指标 | 计算式 | 目标 |
|------|---------|------|
| 复活按钮点击率 | `ad_click_revive / battle_lose` | > 60% |
| 双倍奖励点击率 | `ad_click_double / battle_win` | > 50% |
| 各广告位点击量对比 | 5 个 `ad_click_*` 直接看次数排序 | — |
| 复活完播率 | `ad_reward_revive / ad_click_revive` | > 70%（低于这个数说明 SDK 或预加载有问题）|

### 看板 3：章节通关漏斗

```
chapter1_clear  →  chapter2_clear  →  chapter3_clear
```

### 看板 4：留存

→ 微信后台内置 D1/D3/D7 留存，**无需自行实现**。以 `app_launch` 为基准查看即可。

---

## 七、实装顺序与工时（光与朽实测估算）

| 阶段 | 任务 | 工时 |
|------|------|------|
| 立即 | 新建 `AnalyticsManager.cs`，封装 `LogScene`、`LogSceneOnce`，生成 user_id | 0.5 天 |
| 立即 | 在微信后台创建 22 个场景，获取 sceneId 填入常量 | 1 小时 |
| 第一周 | P0 基础漏斗（4 个）+ 广告意愿（6 个）接入代码 | 1 天 |
| 第二周 | 新手引导（6 个）+ 战斗里程碑（6 个）接入代码 | 1 天 |
| 同步 | `BattleStatistics.ExportToCSV()` 加编译开关 | 0.5 小时 |
| 上线后 | 看一期数据反应，决定是否接入二期（友盟）| — |

**总工时约 2.5 天**——可以塞进任何 MVP 的最后冲刺，不会拖延上线。

---

## 八、跨项目踩坑总结（铁律）

1. **场景必须先在后台注册**——`wx.reportScene()` 调用没注册过的 sceneId 数据会被丢弃，没有任何报错
2. **sceneId 集中常量化**——字符串散落是字母笔误的温床，上线后才发现一个事件没数据
3. **一次性 vs 反复 用错就废**——新手引导用了 `LogScene`（反复）会让漏斗严重失真
4. **正式包关 CSV / 关 GameLogger / 关 Debug 日志**——不关就在用户设备上无意义写文件 + 拖性能
5. **意愿和完播要分开埋**——只埋一个就永远不知道是用户不愿看广告还是 SDK 有问题
6. **微信后台数据有 ~10 分钟延迟**——验证时不要急着说"事件没上报"
7. **二期不要提前做**——多数项目走不到二期，提前做就是预付的复杂度

---

## 九、相关条目

- [[06_WXAdsManager]] — IAA 广告管理器代码模板（与本文 IAA 埋点一一对应）
- [[Unity通用技术栈复用指南]] — 包括 AnalyticsManager 应该继承 PersistentSingleton 的理由
- [[微信小游戏运营教训]] — 上线后的运营策略层（本文是工程层，那篇是运营层）
- [[数值迭代方法论]] — BattleLog CSV 是数值迭代的数据源，那篇讲怎么用

---

## 来源: `10_流水/光与朽项目/分析-Ch2数据采集扩展_202604.md` · 提取日期 2026-05-18

## 章节专属机制必须扩展 BattleLog，不能复用旧字段糊过去

Ch2 的火山章节暴露了一个典型问题：第一章的 `SpawnCounter` / `KillCounter` 只覆盖 Ch1 怪物类型，Ch2 新 EnemyType 没注册时不会报错，而是静默丢数据。结果是策划以为“没有问题”，实际是 BattleLog 根本没看见问题。

新章节接入时，BattleLog 要按“机制问题”扩展，而不是只把怪物塞进旧字段：

| 机制 | 应单独记录的字段 |
| --- | --- |
| 炮手 | 炮手生成 / 击杀、射击次数、子弹命中、子弹伤害来源 |
| 自爆怪 | 自爆死亡次数、AoE 命中、AoE 伤害来源 |
| 熔浆液 | 生成次数、峰值数量、存活 / 清除影响 |
| 分裂怪 | 父体死亡、子体生成、子体击杀 |
| Boss 吸收 | 吸收次数、回血总量、回血触发阶段 |
| Boss 技能 | 陨石、熔岩弹、吸收、其他章节技能分别统计 |

伤害来源也要拆细。比如炮手子弹如果记成 `BossBullet`，就会误导为 Boss 伤害偏高；自爆 AoE 如果记成 `MobCollision`，就会看不出爆炸机制的真实压力。至少要把 `MobBullet`、`MobExplosionAoE`、`BossMeteor` 这类来源拆出来。

工程上优先采用“尾部追加字段”的方式扩展 CSV，保留旧字段顺序，避免历史脚本和表格分析模板失效。新增字段可以集中放在 `BattleStatTypes.cs`，由 `BattleStatistics.cs` 统一导出。

关键插桩点要贴近行为源头：

- 怪物死亡 / 分裂：`EnemyBlob`
- 炮手射击：`LavaGunnerAI`
- 子弹命中：`LavaProjectile`
- Boss 陨石 / 熔岩弹：Boss 控制器或技能脚本
- Boss 吸收 / 回血：`BossHealth` 与 Boss 控制器
- 动态障碍峰值：生成与回收处同步增减计数

一句话规则：**新章节出现了新机制，就给它一个能回答策划问题的字段；不要把它塞进“普通怪物伤害”这种大桶里。**

## 来源: `10_流水/光与朽项目/分析-Ch3数据采集扩展_202604.md` · 提取日期 2026-05-18

## 新章节埋点从“要回答的问题”反推字段

Ch3 的冰雪章节不是简单多几个怪，而是多了一整组“冰盾、施法、冰墙、冻结、拦截、Boss 技能”的机制。因此 BattleLog 扩展要先列问题，再定字段：

| 要回答的问题 | 字段方向 |
| --- | --- |
| 哪类怪物真正造成压力？ | FrostSlime / Tank / Catalyst / Frostcaster / Guard / Elite / IceWall 的生成与击杀 |
| 冰盾机制是否有效？ | 护盾破坏次数、护盾吸收伤害 |
| 施法怪是否发挥作用？ | Catalyst 爆发次数、Frostcaster 施法次数 |
| 冰墙是否过量阻断？ | IceWall 峰值数量、生成 / 击杀 |
| 冻结是否影响炮塔输出？ | 炮塔被冻结次数、冻结总时长 |
| 冰刺是否被玩家处理？ | 冰刺拦截次数、命中次数 |
| Boss 技能强度是否可控？ | 冰墙、冰冻射线、冲锋、绝对零度、打断次数 |

这种字段设计比“总伤害 / 总击杀”更贵一点，但价值高很多：它能直接回答“是冰墙太多、冻结太久，还是 Boss 技能太频繁”。没有机制级字段时，后续所有数值讨论都会退化成猜测。

可复用流程：

1. 为每个新章节列出 5-8 个核心机制问题。
2. 每个问题至少对应 1 个计数字段或时长字段。
3. 动态对象必须有峰值字段，不能只统计总生成。
4. Boss 技能必须按技能拆开，不要只记 BossDamage。
5. 新字段统一尾部追加，保持 CSV 向后兼容。

## 来源: `10_流水/光与朽项目/分析-第三章对战数据分析_202604.md` · 提取日期 2026-05-19

## 动态召唤对象要从行为源头补统计

Ch3 对战日志里 `Spawn_IceWall` 长期为 0，但 `Frostcaster_Cast_Count` 有数值，说明施法者确实在生成冰墙。根因是：波次 Spawn 计数只统计 `WaveManager.SpawnEnemy()` 直接生成的敌人，而 Frostcaster 在 AI 层动态召唤的 IceWall 没经过这条入口。

这类问题在召唤、孵化、分裂、Boss 技能里很常见。BattleLog 不应该只依赖 WaveManager，任何动态生成对象都要在行为源头补一条统计：

| 动态来源 | 统计位置 |
| --- | --- |
| 施法者生成冰墙 | `FrostcasterAI.SpawnIceWalls()` |
| 冰墙孵化史莱姆 | 冰墙孵化逻辑 |
| 分裂怪死亡生成子体 | 怪物死亡分裂逻辑 |
| Boss 召唤额外波次 | Boss 技能执行逻辑 |
| 自爆怪死亡留坑 | 死亡留坑逻辑 |

否则 CSV 会出现“机制明明存在，但生成数永远是 0”的假象，后续数值判断会被误导。

## 不要用 Total 反推核心敌人

Ch3 缺少 `Kill_FrostGunner` / `Spawn_FrostGunner` 字段，只能从 `Kill_Total` 减去已知类型推算炮手贡献。这种反推在调数值时很危险：一旦还有隐藏生成物、动态召唤物或未注册类型，差值就会混进多个来源。

可复用规则：

1. 每个会影响玩家决策的敌人类型，都应有独立 kill / spawn 字段。
2. `Total` 只做总量校验，不做核心单位分析。
3. 新增 EnemyType 时同步补 BattleLog 字段，不等数据出问题再补。
4. 动态对象既要统计总生成，也要统计峰值数量，避免只知道“总量很多”却不知道同屏压力。

## 来源: `10_流水/光与朽项目/Claude-2026-04-07.md` · 提取日期 2026-05-19

## Boss 技能重命名后要同步埋点语义

Ch2 Boss 技能从 Meteor / LavaProjectile 调整为火球等新表现后，BattleLog 字段不能只沿用旧名字。比如 `Boss_Meteor_Count` 如果实际记录的是火球命中，就会让后续分析误判技能来源。

字段维护规则：

1. 技能表现或玩法语义变化时，同步检查字段名、导出列名和分析表头。
2. 新增“发射次数 / 被拦截次数 / 命中次数”三类字段时，优先尾部追加，保持历史 CSV 兼容。
3. 子弹脚本和 Boss 控制器都要插桩：控制器记录发射，Projectile 记录命中/拦截。
4. 字段名宁可长一点，也要表达真实机制，例如 `bossFireballBurstCount`、`bossFireballInterceptedCount`。

埋点字段是策划语言的一部分。字段名过期，数据就会开始撒谎。

## 来源: `10_流水/光与朽项目/Claude-2026-04-11.md` · 提取日期 2026-05-20

## 建模前先修 P0 采集缺陷

Ch3 三局测试里，两个采集缺陷会直接阻断数值建模：没有 `Kill_FrostGunner / Spawn_FrostGunner` 字段，且 `Spawn_IceWall` 恒为 0。前者让核心炮手无法单独分析，后者让动态召唤的冰墙从数据里消失。

这类缺陷应先修再跑样本：

1. 新增核心单位字段，不从 Total 反推。
2. 动态召唤对象在行为源头插桩，例如 `FrostcasterAI.SpawnIceWalls()`。
3. 召唤物生成统计可以传 0 HP，避免污染 Enemy_Total_HP。
4. 修复后至少重跑 3-5 局，再进入正式锚点建模。

如果 P0 字段缺失，样本越多只会越精确地误导结论。

## 来源: `10_流水/光与朽项目/Claude-2026-04-12.md` · 提取日期 2026-05-20

## CSV 导出要校验“字段定义数 = 实际写出数”

Ch3 15 局数据里，Boss 技能后 9 列始终为空，根因不是技能没触发，而是 `BuildCSVDataLine` 的格式字符串在 `{123}` 后截断，新增字段定义了但没有写进 CSV。

BattleLog 每次扩字段都要做完整性校验：

1. Header 列数、数据列数、格式占位符数量必须一致。
2. 新字段尾部追加后，要用一局最小战斗确认字段非空或明确为 0。
3. 对 Boss 技能、章节机制这类关键字段，不要只看代码里有 `RecordXxx()`，还要看最终 CSV。
4. 建议开发期导出时检测列数不一致并打 Error。

“字段存在”不等于“数据已经进入分析表”。CSV 是最后一公里，必须验证。

## 来源: `10_流水/光与朽项目/Claude-2026-04-13.md` · 提取日期 2026-05-20

## 微信小游戏一期埋点用场景分析，不要照搬小程序自定义分析

微信小游戏后台的可落地路径是「统计 → 基础数据 → 场景分析 → 新建场景」，代码侧调用 `wx.reportScene()` 或小游戏可用的场景上报接口。它更像“漏斗节点统计”，只能记录某个场景发生次数，不能携带自定义参数。

因此一期 / 二期要分清：

| 阶段 | 工具 | 适合记录 |
| --- | --- | --- |
| 一期 | 场景分析 | 启动、首战开始/结束、引导完成、章节通关、广告按钮点击 |
| 二期 | 友盟 / 自建后端 / 其它 SDK | 死亡波次、构筑类型、技能选择、Boss 剩余血量等参数 |
| 开发期 | BattleStatistics CSV | 深度数值调试，不进入正式包 |

不要在微信小游戏里假设一定有小程序后台的“自定义分析”模块。先用场景分析把上线漏斗跑通。

## 没开通流量主时，先埋广告意愿事件

未开通流量主时，不需要接 `ad_video_start / complete / skip` 这类真实 SDK 事件，但仍应记录玩家是否点击了“免费复活 / 双倍奖励 / 补资源”等按钮。

最小事件：

| 事件 | 用途 |
| --- | --- |
| `ad_btn_click` | 记录玩家想要哪个广告奖励，判断广告位意愿 |
| `ad_reward_grant` | 验证公测福利或占位奖励是否正常发放 |

等广告能力开通后，再在两者之间插入 SDK 拉起、完播、跳过和失败回调。这样不会因为暂时没有广告 SDK，就错过早期意愿数据。

## 来源: `10_流水/光与朽项目/Claude-2026-04-15.md` · 提取日期 2026-05-20

## 文档里的 SDK 类名必须和工程真实 API 对齐

《光与朽》数据埋点文档里曾写 `WXSDKManager.Instance?.ReportScene(sceneId)`，但工程里的 `WX-WASM-SDK-V2` 并没有对应 C# 封装，也没有 `wx.reportScene` 的现成桥接。这个阶段不能直接照文档写代码。

工程落地前必须确认三件事：

| 检查项 | 处理 |
| --- | --- |
| 是否已有 C# SDK 封装 | 有则按现有 API 接，不重造 |
| 是否只有 WebGL / 小游戏静态资源 | 新建 `.jslib` 桥接，由 C# `DllImport("__Internal")` 调 JS |
| Editor / Dev 包如何验证 | 提供 stub，只打日志不真实上报 |

埋点文档里的接口名只是产品意图，不是工程事实。上线前最危险的是“文档写了一个类，于是业务代码引用了一个不存在或不工作的类”。

## 首局埋点不要依赖会被业务提前改写的 isFirstPlay

`isFirstPlay` 是业务状态，不是埋点状态。如果主界面为了自动进入战斗，在加载战斗前就把 `isFirstPlay=false` 存档，那么战斗开始时再判断它就永远拿不到首局。

更稳的做法是给埋点单独一套一次性标记：

```csharp
LogSceneOnce("first_battle_start");
// 内部使用 PlayerPrefs key: Analytics_Done_first_battle_start
```

埋点状态要回答“这个事件是否已经上报过”，不要借用业务状态回答。业务为了流程会变，埋点需要更稳定。

## app_launch 要先定义统计语义

`app_launch` 看似简单，但有两种完全不同的语义：

| 语义 | 触发 | 用途 |
| --- | --- | --- |
| 主界面加载次数 | 每次回到 MainScene 都报 | 衡量玩家查看主界面的次数 |
| App 会话启动 | 每次打开小游戏会话只报一次 | 作为 DAU / 留存漏斗基准 |

如果目标是新手漏斗和留存基准，应优先按“会话启动一次”定义；如果放在主界面 `Start()` 且不做 session 标记，战斗结束返回主界面也会再次上报，漏斗分母会膨胀。

## 广告事件表要消除“点击数”和“奖励数”的口径矛盾

IAA 文档曾出现“5 个 ad_click”文字，但表格实际是 4 个点击事件 + 2 个奖励事件。实现前要以表格为准重新核口径。

核对清单：

1. `ad_click_*` 统计点击意愿，不管广告是否成功。
2. `ad_reward_*` 统计看完并发奖，只给真正需要完播验证的广告位。
3. TopBar 如果同一个按钮承载体力 / 金币 / 图纸三种模式，要明确是否分别埋点。
4. 文档中的“总数”必须和表格逐项一致，否则看板会一开始就口径混乱。

埋点系统不是“多打几个事件”。它首先是统计口径工程。

## 来源: `10_流水/光与朽项目/Claude-2026-04-16.md` · 提取日期 2026-05-20

## 局外经济可以先用预算表建模，实装后再补采集

局外经济初稿不一定需要先改采集工具。只要工程里已经能读取怪物金币、波次配置、难度倍率、Boss 奖励，就可以先做一版预算模型，判断科技树和装备升级的消耗是否同量级。

但实装后必须补验证字段：

| 字段 | 用途 | 优先级 |
| --- | --- | --- |
| `totalCoinsEarned` | 验证单局金币收入是否符合预算 | P1 |
| `bossKilled` | 区分通关奖励是否发放 | P1 |
| `techTreeTotalSpent` | 验证科技树金币消耗速度 | P2 |
| `equipmentGoldSpent` / `blueprintSpent` | 验证装备升级金图是否同速耗尽 | P2 |

经济数值先靠预算表能快速起步，但预算表不能替代上线后的收支流水。尤其是金币有科技树和装备两大出口时，必须看到真实消耗分布。
## 来源: `10_流水/光与朽项目/Codex-2026-04-13.md` · 提取日期 2026-05-21

## 未开通流量主时，也要先做广告语义层

广告系统不应等到真实广告位 ID、流量主权限、SDK 联调全部齐备才开始写。对《光与朽》这类微信小游戏，早期更重要的是先把“玩家在哪些情境下愿意点广告”这层语义搭出来，再把真实 SDK 接到同一套入口上。

可复用做法：

| 阶段 | 工程目标 | 数据价值 |
| --- | --- | --- |
| 未开通广告 / 无广告 ID | `AdManager` 先提供统一入口，点击广告按钮直接发奖，记录广告意愿事件 | 验证按钮位置、奖励语义、玩家是否愿意点 |
| 已有 SDK / 无完整数据 | 保留 `AdType`、`placement`、`onReward`、`onFail` 等接口，逐步替换为真实激励视频 | 避免 UI 层散落真实 SDK 调用 |
| 正式上线 | 接入 `CreateRewardedVideoAd` / Banner，并把展示、点击、完播、失败、发奖分开记录 | 能区分“没人点”“广告加载失败”“完播但奖励不值” |

这类语义层的关键不是“假装已经有广告收入”，而是提前固定业务边界：复活、结算翻倍、资源补给、技能刷新、Banner 展示都从同一个广告入口走。等真实广告 ID 到位时，只替换底层适配，不重写玩法 UI。

## 微信 SDK 选择要以 Package Manifest 为准

微信小游戏工程里可能同时存在 `Packages/manifest.json` 的 UPM 包、`Library/PackageCache` 里的运行时代码，以及旧的 `Assets/WX-WASM-SDK-V2` 资源目录。判断真实 SDK 入口时，应优先看 Unity 当前解析出来的包依赖和 PackageCache，而不是只看 Assets 目录。

《光与朽》这次排查的结论是：

- `com.qq.weixin.minigame` 是真实 UPM SDK 来源；
- `WXBase.cs` 中能确认 `WX.CreateRewardedVideoAd`、`WX.CreateBannerAd`、`WX.CreateFixedBottomMiddleBannerAd` 等可用接口；
- `Assets/WX-WASM-SDK-V2` 更像资源 / 配置遗留，不应被当作主要代码源；
- `Assembly-CSharp.csproj` 可能滞后于 Unity 刷新，新增脚本缺失时不要先怀疑代码结构，要先让 Unity 重新生成工程文件。

可复用原则：接 SDK 前先确认“编译期真正引用的是哪一套包”。否则很容易按旧目录写适配，最后在构建时才发现命名空间、接口或 asmdef 根本不一致。

## 广告按钮点击意愿早于真实广告完播数据

在没有真实广告完播数据之前，广告按钮仍然值得埋点。点击行为本身已经能回答三个早期问题：

1. 玩家是否看懂这是一个补救机会。
2. 当前奖励是否正好解决失败或卡点。
3. 广告点是否出现在玩家仍有希望继续的时刻。

早期可先记录：

| 事件 | 含义 |
| --- | --- |
| `ad_offer_show` | 广告机会出现，说明系统判断此处可以给补救 |
| `ad_button_click` | 玩家愿意付出一次广告成本 |
| `ad_reward_granted_mock` | 未接真实广告时的占位发奖 |
| `ad_reward_used_result` | 奖励后是否继续、通关或再次失败 |

等真实 SDK 接入后，再补充 `ad_loaded`、`ad_show_failed`、`ad_completed`、`ad_skipped`。这样广告系统从一开始就是可观测的，而不是上线后才临时补洞。
## 来源: `10_流水/光与朽项目/Codex-2026-04-15.md` · 提取日期 2026-05-21

## 微信场景分析先收 P0 口径，不要一次性接满文档表格

《光与朽》的数据埋点文档列出了基础漏斗、新手引导、战斗里程碑和广告行为等 24 个事件，但真实落地时先收敛到 `AnalyticsManager + P0 4 个 + 广告/资源点击 6 个 + CSV 开关`。这比一口气接完所有事件更稳。

首批 10 个事件：

| 类别 | 事件 |
| --- | --- |
| 基础漏斗 | `app_launch`、`first_battle_start`、`first_battle_win`、`first_battle_lose` |
| 广告/资源点击 | `ad_click_revive`、`ad_click_double`、`ad_click_reroll`、`ad_click_energy`、`ad_click_gold`、`ad_click_blueprint` |

工程原则：

1. `app_launch` 用 Session 级去重，每次打开小游戏只报一次，作为 DAU / 留存分母。
2. 首战开始、首战胜利、首战失败用独立 `PlayerPrefs` 标记，不依赖 `isFirstPlay`，避免教程流程提前改状态导致漏报。
3. 没开通流量主时，广告入口先记录“点击意愿”，不记录“完播奖励”。
4. 章节通关、教程完成、Boss 里程碑可以第二阶段再接，等核心漏斗稳定后补。

埋点不是越多越好。第一批事件要保证口径清晰、触发稳定、后台能建完并验证，而不是把文档表格全铺到代码里。

## 微信后台场景 ID 要做集中映射，并区分 eventType

微信场景分析后台创建后，拿到的不一定是最初设想的数字 `sceneId`。这次实际后台给出的是一组 `branchId` 字符串，并需要通过 `WX.ReportUserBehaviorBranchAnalytics(...)` 上报，同时用 `eventType` 区分“发生/曝光”和“点击”。

推荐结构：

| 字段 | 用法 |
| --- | --- |
| 逻辑事件名 | 代码内部稳定使用，如 `ad_click_reroll` |
| 后台 `branchId` | 由微信后台创建场景后返回，集中填表 |
| `eventType = 1` | 启动、首战开始、首战结果等发生类事件 |
| `eventType = 2` | 复活、翻倍、重掷、资源领取等点击类事件 |
| `branchDim` | 首批不勾选参数时传空，后续做章节/难度/流派再启用 |

不要把后台 ID 散落在业务 UI 脚本里。`AnalyticsManager` 应该只暴露逻辑事件入口，真实微信 ID 和 eventType 放在一张集中映射表中。这样后台重建场景或改 ID 时，不需要翻遍 `RevivePanel`、`SettlementPanel`、`SkillChooseOnePanel`、`TopBarTipsPanel`。

## 来源: `10_流水/光与朽项目/Codex-2026-04-16.md` · 提取日期 2026-05-21

## BattleLog 真机采集开关不要被构建宏误伤

早期为了避免正式包写 CSV，曾把 `BattleStatistics.ExportToCSV` 在 `!UNITY_EDITOR && !DEVELOPMENT_BUILD` 下直接 `return`。这对线上包是安全的，但会误伤“普通真机包也要采集战斗数据”的调试需求。

更稳的分层：

| 需求 | 控制方式 |
| --- | --- |
| 上线正式包默认不采集 | Inspector / 配置里的 `enableDataCollection` 默认关闭 |
| 真机普通包临时采集 | 手动勾选 `enableDataCollection`，保存到 `Application.persistentDataPath` |
| 开发包 Profiler 调试 | Development Build + Autoconnect Profiler |
| 只看帧率 | 独立 `FpsDisplay`，不写 CSV、不采样内存 |

构建宏适合防止编辑器 API 进入正式包，不适合替代业务开关。战斗数据采集这种“有时需要在普通真机包里打开”的能力，应由显式开关控制，而不是被平台宏一刀切。

## 来源: `10_流水/历史聊天/Claude_光与朽程序_2025-11-01.md` · 提取日期 2026-05-22

## BattleStatistics 新增字段要同步改 Header、DataLine 和字段数

战斗日志新增字段时，最容易出错的不是计算公式，而是 CSV 导出结构不同步。《光与朽》早期讨论里补充过几类对平衡判断很有用的字段：

| 字段 | 计算/含义 | 用途 |
| --- | --- | --- |
| `Effective_DPS` | `Dmg_Dealt_Total / Time_To_Clear` | 观察真实清怪效率，而不是只看面板 DPS |
| `Player_Hit_Count` | 玩家受击次数 | 区分“血量压力”和“操作压力” |
| `Exp_Gained` | 局内获得经验 | 判断等级断层和 XP 掉落是否足够 |
| `Gold_Gained` | 局内获得金币 | 连接局内奖励和局外经济 |
| `Time_In_Danger` | 低血量区间累计时长 | 判断濒死压力，阈值应与低血量提示一致 |

新增字段时必须一起检查：

1. CSV Header 是否新增列名。
2. DataLine 是否按相同顺序写入。
3. `CSV_FIELD_COUNT` 或等价校验是否同步更新。
4. 字符串格式化索引是否是实际索引，不要写成占位的 `{N}`。

BattleLog 是平衡决策工具，不只是日志文件。只要列错位，后面的 Excel、脚本分析和经验判断都会被污染。
