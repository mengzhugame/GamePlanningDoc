---
type: knowledge
status: review
created: 2026-04-29
source_book: 光与朽数据埋点与 IAA 系统文档（V2 双源）+ MVP 草稿 + WXAdsManager 模板
source_page: 20_项目/02_光与朽项目/01_策划文档/光与朽_数据埋点系统文档_V2.0.md; 20_项目/02_光与朽项目/01_策划文档/光与朽_MVP数据埋点需求文档.md; 20_项目/02_光与朽项目/01_策划文档/光与朽_IAA广告接入系统文档_V2.0.md; 40_知识/代码模板库/06_WXAdsManager.md
domain: 02_引擎与技术
tags: [数据埋点, BattleLog, 微信场景分析, IAA埋点, AnalyticsManager, 工程化]
last_reviewed: 2026-04-29
review_count: 1
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
