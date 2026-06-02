---
type: knowledge
status: review
created: 2026-04-29
source_book: 代码模板库 14 个模板（MZ02 + LightVSDecay 双项目提取）
source_page: 40_知识/02_引擎与技术/代码模板库/00_INDEX.md; 01_GameLogger.md; 02_SingletonPattern.md; 03_SafeSceneLoader.md; 04_AudioManager.md; 05_SaveManager.md; 06_WXAdsManager.md; 07_UIAnimationHelper.md; UGUI挖孔遮罩/README.md; 09_CoinFlyAnimation.md; 10_FloatingTextSystem.md; 11_GameEvents.md; 12_AnalyticsManager.md; 13_AudioManagerPro.md; 14_ProgressManager_CurrencyTopBar.md; 10_流水/光与朽项目/Claude-2026-04-16.md; 10_流水/光与朽项目/Claude-2026-04-17.md; 10_流水/光与朽项目/Claude-2026-04-18.md
domain: 02_引擎与技术
tags: [Unity, 微信小游戏, 单例, 场景加载, 音频, 存档, 广告, UI动效, 飘字, 事件总线, 埋点, 资源管理, 工程化, 决策指南]
updated: 2026-05-30
last_reviewed: 2026-05-30
review_count: 22
---

# Unity 通用技术栈复用指南

> 这不是模板说明书。模板说明书在 [[00_INDEX]] 与各 `0X_*.md`。
> 本文回答的是：**新项目启动时，先抄哪几个？什么场景下抄、什么场景下不抄？为什么选这个方案？踩过什么坑？**

---

## 一、新项目按规模选包

模板库现有 14 条。**不是"无脑全抄"，按项目规模分档**：

| 项目规模 | 必抄模板 | 累计文件数 |
|---------|---------|-----------|
| **小 Demo**（无场景切换、≤5 音效、单关卡） | 01 + 02 + 04 | 3 |
| **中等休闲游戏**（多场景、完整 UI、解耦良好） | 上面 + 03 + 11 + 13 + 07 | 7 |
| **微信小游戏 + IAA 变现** | 上面 + 06 + 12 | 9 |
| **战斗 / 关卡 / Roguelite 类** | 上面 + 09 + 10 + 14 | 12 |
| **有云存档 / 跨设备** | 上面 + 05 | 13 |
| **有新手引导** | 上面 + 08 | 14 |

### 1.1 小 Demo 启动 3 件套（无脑抄）

| 顺序 | 模板 | 必抄理由 |
|------|------|---------|
| 1 | [[02_SingletonPattern\|02 Singleton / PersistentSingleton]] | 所有 Manager 都依赖。先建好基类，避免后期返工换基类 |
| 2 | [[01_GameLogger\|01 GameLogger]] | 全局 `Debug.Log` 在微信小游戏会拖性能。`const bool ENABLE_LOG` 在编译期剔除调用，零开销 |
| 3 | [[04_AudioManager\|04 AudioManager]] | 简化版，5 音效以内够用。中等以上项目升级到 13 |

### 1.2 中等以上项目升级到的"工程基础设施"

到了"中等以上"档，需要再加 4 条来撑起松耦合架构 + 长项目维护性：

| 模板 | 升级理由 |
|------|---------|
| [[03_SafeSceneLoader\|03 SafeSceneLoader]] | 多场景必备，不抄就是切场景崩 |
| [[11_GameEvents\|11 GameEvents]] | Manager 间解耦的核心。没这条，加个新 Manager 要改 5 处 `using` |
| [[13_AudioManagerPro\|13 AudioManager Pro]] | 取代 04，多 AudioSource + AudioConfig SO + 自动场景切 BGM + 冷却防抖 |
| [[07_UIAnimationHelper\|07 UIAnimationHelper]] | 任何属性数值反馈都要用——金币 +1、等级提升、装备强化 |

> **关键纪律**：上面所有模板的命名空间（`LightVsDecay.Core` 之类）要在导入第一份代码时一次性 rename 成新项目的命名空间——晚了会变成全局替换噩梦。

---

## 二、微信小游戏专属补丁（必抄 3 个）

微信小游戏 = WebGL + 内存敏感 + 异步加载坑 + 流量主变现。下面 3 个是踩过坑后总结的"必须有"：

| 模板 | 不抄会怎样 | 关键机制 |
|------|-----------|---------|
| [[03_SafeSceneLoader\|03 SafeSceneLoader]] | 切场景时 `NullReferenceException` / `CS1626` / 黑屏 | 6 阶段异步加载 + 主动 GC + 帧间隔等待 + 超时保护 |
| [[06_WXAdsManager\|06 WXAdsManager]] | IAA 接入散落在各业务，换广告位要全局搜；缺频控会被微信限流 | 三层架构（业务/AdManager/WeChatAdsPlugin）+ 6 广告位 + 共享每日总上限 + 180s 超时兜底 |
| [[12_AnalyticsManager\|12 AnalyticsManager]] | 上线后看不到任何漏斗数据，决策全靠拍脑袋 | 三种语义级别（永久一次性/本次启动一次/反复）+ 首战漏斗封装 + Editor 日志 |

### SafeSceneLoader 的关键决策

- **不要**用 Unity 自带 `SceneManager.LoadSceneAsync` 直接切场景。微信端会因为 GC 时序问题随机崩溃
- **必须**在 `allowSceneActivation = true` 前后留 ≥3 帧的等待——Unity 物体销毁不是同步的，缺这几帧就是 `NullReferenceException`
- **建议**在切场景前主动 `System.GC.Collect() + Resources.UnloadUnusedAssets()`，再异步加载——能消掉 80% 的卡顿与崩溃

### WXAdsManager 的关键决策

- **业务/决策/桥接三层切分**：业务调 `AdManager` 永远不接触微信 SDK；`WeChatAdsPlugin` 用 `#if !UNITY_EDITOR && UNITY_WEBGL` 包裹，Editor 内走 `usePlaceholderAds` 占位
- 广告位 ID **不要**散在业务里，集中放 `AdUnitIds[]` 数组，按 `AdType` 枚举索引——后台换 ID 时只改一处
- **必须有 180s 超时兜底**——微信偶尔会丢 `OnClose` 回调，没兜底就让 `isShowing` 永远为 true，按钮永远点不动
- **场次内"每场限 1 次"用内存变量，不入 PlayerPrefs**——退游再进就该重置，这是产品语义不是 bug
- **不要**自己实现"看完才发奖"逻辑：用微信 SDK 的 `onClose(res.isEnded)` 判定，官方都帮你兜底

### AnalyticsManager 的关键决策

- **三种语义级别**（`LogScene` / `LogSceneOnce` / `LogSceneOncePerSession`）必须分清——用错了漏斗数据严重失真
- **首战漏斗用专门的 `TryLog*` 方法**——保证"开始过才能报结果"+"全局一次"，纯 `Once` 表达不出依赖
- **`branchId` 集中放 `Dictionary`** + **`sceneId` 集中放 `AnalyticsSceneIds`**——字符串笔误是这套系统的头号杀手
- **`#if !UNITY_EDITOR && !DEVELOPMENT_BUILD && (WEIXINMINIGAME || UNITY_WEBGL)` 编译期剔除**——Editor / Dev 包内只打日志，不真实上报

---

## 三、按需抄（看场景）

| 模板 | 抄的场景 | 不抄的场景 |
|------|---------|-----------|
| [[05_SaveManager\|05 SaveManager]] | 有用户系统、有云存档、需要本地+服务器双写 | 纯单机数据用 `PlayerPrefs` 直接搞，别套这个 |
| [[07_UIAnimationHelper\|07 UIAnimationHelper]] | 任何属性数值反馈（金币 +1、等级提升、装备强化） | 没有数值反馈的纯展示 UI |
| [[代码模板库/UGUI挖孔遮罩/README\|08 UGUI 挖孔遮罩]] | 有新手引导聚光灯、高亮某个按钮的需求 | 没有引导需求 |
| [[09_CoinFlyAnimation\|09 CoinFlyAnimation]] | 战斗 / 关卡类，怪物死亡掉金币 / 宝箱开奖 | 纯解谜 / 收纳类（用旧 09 极简版即可） |
| [[10_FloatingTextSystem\|10 FloatingTextSystem]] | 战斗类、有暴击 / 弱点 / 元素伤害 / 拾取奖励数值 | 单条文字提示用 `Text + Animator` 就够了 |
| [[14_ProgressManager_CurrencyTopBar\|14 ProgressManager + 顶部栏]] | 有局外资源（金币/钻石/体力/蓝图）+ 顶部 HUD 实时刷新 | 无体力概念的休闲 / 解谜（用 BeautyStacking 简单版即可） |

### SaveManager 的设计哲学

> **本地优先 + 时间戳冲突解决 + 接口替换后端**

- 写操作先落 `PlayerPrefs`，再异步同步服务器——任何时候断网都能存
- `IServerDataManager` 接口：开发期用 `MockServerDataManager`，上线前换 `RealServerDataManager`，业务代码不改一行
- **不要**在 `OnApplicationQuit` 里同步存档——微信小游戏的退出不一定走完整生命周期，最稳是定时（5 分钟）+ 关键节点（结算/胜负）双触发

### UIAnimationHelper 的设计哲学

> **静态 + IEnumerator + 无框架依赖**

- 全部方法返回 `IEnumerator`，调用方 `StartCoroutine(...)` 驱动——不绑定 DOTween/LeanTween，避免引入第三方依赖
- 三个组合：`PlayScalePunch`（Q 弹）/ `RollInt`（数字滚动）/ `PunchThenRollInt`（先弹再滚）——基本覆盖装备/属性/金币的所有反馈场景

### UGUI 挖孔遮罩的关键选择

- **不要**用四块 Image 几何切割来做镂空——4 DrawCall + 硬边 + 不支持圆角
- 用 SDF Shader 单图层方案：1 DrawCall + 软边 + 圆角参数化 + 归一化坐标自适应任意分辨率
- 配套 `HoleMaskClickBlocker` 解决点击穿透——孔内点击穿透到底层 UI，孔外点击被拦截

### CoinFlyAnimation 的关键选择

- **三阶段优于单段直飞**——散落（让玩家看到掉了多少）→ 悬浮（视觉记忆 anticipation）→ 贝塞尔吸附（被吸入感）
- **视觉数压缩**（50 金币 → 7 个动画每个代表 7 块）是性能与爽感的关键平衡——真生成 50 个金币动画 = GC 灾难
- **`targetPositionGetter` 用 `Func<Vector3>` 不是 `Vector3`** —— 资源栏在 Canvas 缩放或新手引导时会移动，每帧重取才能正确跟踪
- 与 [[14_ProgressManager_CurrencyTopBar]] 的 `GoldBarWorldPos` 配合，业务零代码即可获得完整爽感链路

### FloatingText 的关键选择

- **多 Prefab 不是单 Prefab + 运行时换样式**——TMP 字体/描边切换在 WebGL 上 GC 大
- **Update 自驱动不用协程**——同屏 50+ 飘字时协程的状态机开销比 Update 高
- **优先级回收公式 `priority * 100 + remainingPercent * 100`**——保护暴击/Boss 飘字不被普通飘字挤掉
- **`ReturnAll()` 关卡切换必调**——否则旧飘字会带着失效的 worldCamera 引用进新场景

---

## 四、单例模式：什么时候用 Singleton vs PersistentSingleton

这是新手最容易选错的决策。原则：

| 选项 | 用在 | 例子 |
|------|------|------|
| **Singleton**（场景内单例） | 只在某个场景活的 Manager；切场景就该被销毁 | `BattleManager`、`CoinPickupSpawner`、`FloatingTextManager` |
| **PersistentSingleton**（跨场景单例） | 整个游戏生命周期只该有一份的 | `AudioManager`、`SaveManager`、`AdManager`、`AnalyticsManager`、`ProgressManager` |

**不要做的事**：
- 不要让 PersistentSingleton 在第一个场景就 `FindObjectOfType<T>()` 自动创建——把它放在 Bootstrap 场景的 GameObject 上预先存在，避免运行时隐式生成对象
- 不要在 `OnApplicationQuit` 后访问 Instance——基类已经标记了 `applicationIsQuitting`，访问会返回 null（这是为了防 NullReferenceException 故意的）
- 不要在没继承 Singleton 基类的情况下手写 `static T Instance` ——基类的 `OnDestroy` 清理静态引用 + 重复实例销毁是踩坑后才补全的

---

## 五、Manager 解耦：GameEvents vs 直接引用

中等以上项目第一天必须铺好 [[11_GameEvents]]。决策原则：

| 方式 | 何时用 | 缺点 |
|------|--------|------|
| **GameEvents 静态事件** | 跨 Manager 通信、UI 监听数据变化、跨场景事件 | 静态状态、不支持多游戏实例（一般游戏不需要） |
| **直接 `Singleton.Instance.Method()`** | 强相关 Manager 之间（如 `BattleManager.Instance.SpawnEnemy()`）| 制造硬依赖，单测难 |
| **C# Action 字段（实例事件）** | 单个组件的局部事件（按钮点击、动画完成）| 跨 Manager 通信会让代码到处传引用 |

**经验法则**：跨 Manager / 跨场景 → GameEvents；同一系统内的局部事件 → 实例 Action。

**铁律**：
- `OnEnable` 订阅、`OnDisable` 取消，**永远成对**
- 用具名方法不用匿名 lambda——lambda 无法 `-=` 取消订阅，会留下幽灵订阅
- 切场景时调 `GameEvents.ClearAllEvents()` 防 leak
- DontDestroyOnLoad 的 Manager 在场景切换后必须 `ResubscribeToGameEvents()`（参考 [[13_AudioManagerPro]]）

---

## 六、休闲反馈三件套（标准爽感链路）

[[07_UIAnimationHelper]] + [[09_CoinFlyAnimation]] + [[10_FloatingTextSystem]] 配合，构成战斗类游戏的标准爽感链路：

```
怪物死亡 → GameEvents.OnEnemyDied 触发
   ↓
[[09_CoinFlyAnimation|09]] CoinPickupSpawner 监听事件，生成 N 个金币（视觉数压缩）
   ↓ 三阶段动画：散落 → 悬浮 → 贝塞尔吸附
[[14_ProgressManager_CurrencyTopBar|14]] TopAreaController.GoldBarWorldPos 作为吸附目标
   ↓ 到达 80%
[[09_CoinFlyAnimation|09]] coinArriveNotifier 回调
   ├── ProgressManager.AddCoins(visualValue)         ← 资源累加
   ├── AudioManager.PlayCoinCollect()                ← 带冷却音效
    ├── [[07_UIAnimationHelper|07]] UIAnimationHelper.PlayScalePunch       ← 资源栏整体抖一下
    └── [[10_FloatingTextSystem|10]] FloatingTextManager.Show "+1"          ← 飘字浮起
```

这套链路任何一环都不可少——少一个就感觉"不够爽"。

---

## 七、未来要补的模板（光与朽上线后）

来自 [[00_INDEX|代码模板库 00_INDEX]] 的待补清单，对应 `LightVSDecay` 工程：

| 优先级 | 模板 | 价值 |
|-------|------|------|
| P1 | HapticFeedback 振动反馈 + CameraShake | 动作 / 物理 satisfying 类型必备 |
| P1 | Pool 通用对象池基类 | 大量重复实例化场景（怪物 / 子弹 / 道具） |
| P1 | AutoPersistentSingleton 自动跨场景单例 | 与 02 简单 Singleton 互补 |
| P1 | WaveManager / SkillSystem / DifficultyManager | Roguelite 类游戏核心 |
| P0 | ScriptableObject 数据驱动架构范例 | 一旦上手就回不去 Inspector 配数据 |
| P1 | UIManager 状态机面板管理 | 复杂 UI 切换 |

**触发提取的时机**：光与朽真实买量数据回流 + 决定继续运营后，或下一款游戏立项时同步提取——避免提取一份"还要改"的代码。

---

## 八、跨项目踩坑总结（铁律）

1. **命名空间在第一天 rename**——晚了就是全局替换地狱
2. **微信小游戏的切场景必走 SafeSceneLoader**——别想着省事用 Unity 自带的
3. **广告位 ID、音频枚举、场景常量、sceneId 都集中放一处**——散在业务里就是字符串笔误的温床
4. **Editor / 真机分支用 `#if !UNITY_EDITOR`**——别用运行时 `Application.platform` 判断，前者是编译期剔除，后者是运行时分支，性能与确定性都差
5. **数据驱动优先于硬编码**——能用 ScriptableObject 配的就别写 if-else，能配在 Inspector 上的就别写 const
6. **日志总开关在编译期**（`const bool ENABLE_LOG`），不是运行时——前者零开销，后者每次调用都会进函数
7. **跨 Manager 通信走 GameEvents 不走相互引用**——DontDestroyOnLoad 的 Manager 切场景后必须 ResubscribeToGameEvents
8. **资源修改只走 Manager 公共方法**——`AddGoldCoins(100)` 不直接 `meta.goldCoins += 100`；后者不触发事件不刷 UI 不存档
9. **高频 SFX 必须有冷却防抖**——同屏 N 个怪同时死亡时一起播音效会炸耳
10. **PlayerPrefs.Save() 显式调用**——不要靠 Unity 自动 Save（不可靠）；每次写完 `Set*` 都立刻 Save 一次

---

## 九、与 [[代码模板库/00_INDEX|代码模板库]] 的关系

| 放哪 | 内容 |
|------|------|
| `40_知识/02_引擎与技术/代码模板库/0X_*.md` | **可直接复制的代码 + 配套接入步骤** |
| 本文（`02_引擎与技术/Unity通用技术栈复用指南.md`） | **决策与经验**：什么时候选哪个、为什么这么选、踩过什么坑 |

新项目立项时的工作流：
1. 读本文 → 决定要抄哪几个模板
2. 跳到 `40_知识/02_引擎与技术/代码模板库/00_INDEX.md` → 按列表复制对应文件
3. 按本文的"铁律"做命名空间 rename + 集中常量化

---

## 来源: `10_流水/光与朽项目/分析-Ch2怪物数据检查与注册流程_202603.md` · 提取日期 2026-05-17

## ScriptableObject 数据驱动要检查“双注册”

Unity 数据驱动项目里，`EnemyData.asset` 存在不等于怪物能生成。《光与朽》Ch2 无怪物出现的根因是两处注册缺失：

| 注册层 | 作用 | 漏掉后表现 |
| --- | --- | --- |
| 数据库注册 | `EnemyDatabase.asset` 负责从 `enemyType` 找到 EnemyData | Wave 引用 type，但 `GetData(type)` 返回 null |
| 对象池注册 | `EnemyPoolManager.enemyConfigs` 负责从 type 找到预制体池 | 数据能查到，但 Spawn 返回 null |

可复用检查清单：

1. 新增任何怪物类型时，同时更新枚举、EnemyData、EnemyDatabase、Pool 配置、波次配置。
2. 精英怪如果不走 Pool，要确认独立 Instantiate 路径真的覆盖该 type。
3. 静态障碍、炮手、分裂怪等特殊行为要检查 `behaviorType`，不要沿用 Chase 默认值。
4. 子体生成字段要检查章节一致性，避免 Ch2 怪分裂出 Ch1 怪。
5. 对“无怪物出现”这类问题，先查数据注册和 Pool 注册，再查 AI 逻辑。

## 来源: `10_流水/Openclaw知识库文件/code_analysis_report.md` · 提取日期 2026-05-26

## 双项目扫描说明：模板库优先提 Core，不急着提玩法专属

早期对《光与朽》和《美妆叠叠乐》的代码扫描确认了一个提取顺序：先提跨项目必用的 Core 层，再提管理器和表现系统，最后才考虑玩法绑定模块。

第一批高复用模块：

| 模块 | 复用价值 | 已沉淀位置 |
| --- | --- | --- |
| Singleton / PersistentSingleton | 所有 Manager 依赖 | [[代码模板库/02_SingletonPattern]] |
| GameEvents | Manager 解耦和 UI 监听 | [[代码模板库/11_GameEvents]] |
| ObjectPool / IPoolable | 怪物、子弹、飘字、金币 | 代码模板库候选 / 后续抽取 |
| GameLogger | Debug 日志开关和格式统一 | [[代码模板库/01_GameLogger]] |

不要一开始就把 `WaveManager`、`SkillSystem`、装备、订单包、堆叠逻辑这类玩法专属系统抽成“通用模板”。它们复用前要等下一款同类项目验证，否则会把旧项目假设带进新项目。

## 巨型控制器要拆职责，但不要为了拆而拆

源报告指出 `LaserController` 这类 1000+ 行控制器容易承担过多职责：伤害、击退、暴击、扩散、穿透、音效、VFX 等都塞在一个类里。拆分方向可以是：

| 职责 | 候选组件 |
| --- | --- |
| 伤害与倍率 | `LaserDamageHandler` |
| 击退 / 控制 | `LaserKnockback` |
| 暴击 / 弱点 | `LaserCritSystem` |
| 元素扩散 | `LaserFrostSpread` |
| 音效与反馈 | `LaserAudio` / `LaserFeedback` |

但拆分要服务验证和稳定，不要在上线前做大规模“好看式重构”。更稳的顺序是先为巨型类写接入清单和回归点，再从最独立、最少耦合的职责开始拔出。

## 魔法数字和调试日志是最容易拖垮复用的两类小债

- 高频调试日志应通过 `GameLogger` 或条件编译控制，正式包不能留下大量 `Debug.Log`。
- 半径、倍率、间隔、冷却等配置如果会被策划调，优先进 ScriptableObject，不要散在 const 里。
- 两个项目的基础设施如果实现不一致，优先统一到更完整、更安全的版本，再进入模板库。

这类 bug 的危险点是“不一定报错”：系统可能静默跳过生成，导致开发者误判为波次逻辑或场景问题。

## 来源: `10_流水/光与朽项目/分析-Ch2怪物音效特效接入_202603.md` · 提取日期 2026-05-17

## VFX / SFX 系统优先扩展，不轻易重构

当现有 `VFXPoolManager` 和 `AudioManager` 已经具备枚举、配置表、语义化方法和对象池能力时，新章节怪物接入不要急着重构。更稳的扩展模式：

| 层 | 操作 |
| --- | --- |
| VFX 枚举 | 新增语义化类型，例如 `EnemySplit` |
| VFX 管理器 | 新增 `PlayEnemySplit(position)` 这类业务可读方法 |
| AudioConfig | 新增可空字段，例如 `enemySplit` |
| AudioManager | 新增 `PlayEnemySplit()`，字段为空则静默跳过 |
| 业务调用点 | 在 `splitOnDeath` 等特定逻辑末尾调用 VFX/SFX |

低频特效可以 `usePool: false`，高频命中、击杀、爆炸才走对象池。可选音效 / 特效允许 Inspector 留空，这样美术资源没准备好时不会阻塞功能开发。

未来新章节接入表现层时，优先按“枚举 + 配置 + 语义化方法 + 业务调用点”的 4 步扩展。只有当接口变得重复、概念混乱或性能不够时，再考虑重构管理器。

## 来源: `10_流水/光与朽项目/分析-VolcanoBoss三项修复_202604.md` · 提取日期 2026-05-18

## Boss / 复杂敌人不要把根节点当所有行为目标

复杂 Boss 常见结构是 Root 挂 Rigidbody / Collider / 控制器，VisualRoot 下挂身体、特效、弱点、吸收点等视觉对象。如果所有行为都默认瞄准 Root，会出现很多“看起来像物理 bug”的问题：小怪撞到 Boss 刚体、推动 Boss、吸收点不对、特效位置漂移。

更稳的做法是给关键行为建立显式目标点：

| 行为 | 目标点建议 |
| --- | --- |
| 小怪被 Boss 吸收 | `AbsorptionPoint` 子物体，使用 Trigger |
| 弱点命中 | 独立 WeakPoint Collider |
| 技能发射 | Muzzle / CastPoint |
| 特效挂点 | VFXAnchor / BodyAnchor |

吸收类行为尤其要避免物理碰撞干扰：目标点使用 Trigger，吸收过程中禁用被吸收单位 Collider，移动到目标点后再播放缩放、VFX、SFX 和回收逻辑。这样既减少 Rigidbody 推挤，也让表现和数值结算绑定在同一个明确位置。

## 多 Renderer 特效要做白名单或黑名单

全身染色 / 冰冻 / 中毒这类效果如果自动抓取所有 `SpriteRenderer`，很容易污染特殊材质。VolcanoBoss 的 Body03 使用 WobblyLiquid HDR 橙色材质，被 FrostDebuff 蓝色 tint 乘上后变成灰绿色，这不是数值 bug，而是 Renderer 选择范围太粗。

可复用规则：

1. 复杂对象初始化时缓存关键 Renderer。
2. 通用 Debuff 不要无脑作用于所有子 Renderer。
3. 对特殊材质、液体、发光核心、UI 血条等对象建立排除列表。
4. 只有身体主轮廓、可读受击部位进入染色目标。

这类修复优先在初始化阶段过滤目标集合，少改运行期逻辑。能用白名单 / 黑名单解决的表现污染，不要升级成 Shader 重写。

## 来源: `10_流水/光与朽项目/分析-真机性能优化_202604.md` · 提取日期 2026-05-19

## 移动端帧率要显式设置，不要相信默认值

Unity Android 默认可能锁在 30fps。如果项目里没有任何 `Application.targetFrameRate` 或 `QualitySettings.vSyncCount` 设置，真机“始终 30fps”往往不是性能不够，而是配置没打开。

移动端启动阶段建议显式设置：

```csharp
Application.targetFrameRate = 60;
QualitySettings.vSyncCount = 0;
```

排查顺序：

1. 先确认是不是被默认帧率或 vSync 锁住。
2. 再看 CPU 峰值、GC、Instantiate、Shader 首帧编译。
3. 最后才判断机型性能不够。

## 对象池预热不能集中在同一帧

第二、三章进入战斗瞬间掉到 15fps 的核心原因，是 `EnemyPoolManager.InitializeForChapter()` 在同一帧同步预热 50-80 个怪物实例。对象池方向是对的，但“预热时机”错了。

可复用规则：

| 做法 | 结果 |
| --- | --- |
| 进入战斗前同帧 Instantiate 50+ 个对象 | 首帧 60-100ms，明显卡顿 |
| 协程每帧预热 1-3 个实例 | 总等待稍长，但帧时间稳定 |
| 先建空池，再由 WarmupCoroutine 接管 | 初始化流程可控，可显示加载/准备状态 |

对象池不是性能万灵药。运行时 Spawn/Despawn 快，不代表初始化可以把所有 Instantiate 堆到一帧。

## 高频流程里避免全场景搜索

`FindObjectsOfType<T>()` 放在场景初始化阶段通常可接受，放在每波结算、每帧检测、战斗循环里就会变成隐性卡点。比如波次保护掉落阶段只需要知道 `TacticalDropManager.Instance.IsDropPhase`，就不该每波遍历场景里的所有 `TacticalCrate`。

工程判断：

1. 初始化一次：可以临时搜索。
2. 每波 / 每秒 / 每帧：改为 Manager 状态、事件缓存或注册表。
3. 找到对象后长期使用：缓存引用，场景切换时清理。

移动端优化先抓“同帧峰值”和“循环搜索”，通常比微调小函数收益更大。

## 来源: `10_流水/光与朽项目/实现-Ch2数据层_202603.md` · 提取日期 2026-05-19

## 新章节怪物数据层要一次性打通“枚举到行为”

Ch2 数据层实现说明：新增章节怪物不是只加几个 Prefab，而是要从类型枚举、SO 字段、运行时缓存、死亡行为、移动行为、表现开关全部打通。

最小链路：

| 层 | 要补的内容 |
| --- | --- |
| 类型层 | `EnemyType` 新增章节怪物，例如 Splitter / Tank / Exploder / Gunner / Puddle |
| 配置层 | `EnemyData` 新增行为类型和特殊字段 |
| 运行时层 | `EnemyBlob.LoadDataFromConfig()` 缓存字段 |
| 行为层 | `Die()`、移动逻辑、远程 AI、静止障碍分支 |
| 表现层 | 是否禁用白闪、是否禁用击退、是否接 VFX/SFX |

特殊行为最好数据化：

- 分裂死亡：`splitOnDeath`、`splitEnemyType`、`splitCount`
- 死亡留坑：`spawnPuddleOnDeath`、`puddleEnemyType`
- 静止障碍：`Stationary` 行为类型，强制速度为 0
- 远程炮手：`RangedGunner` 行为类型，交给专属 AI
- 障碍表现：`disableHitFlash`、`disableKnockback`

这套模式能让后续 Ch3 / Ch4 继续扩展，而不是每章都在 `EnemyBlob` 里硬写一堆特判。

## 来源: `10_流水/光与朽项目/分析-VolcanoBoss闪白效果系统_202604.md` · 提取日期 2026-05-19

## 持续命中类 Shader 反馈要拆材质组并限频

Boss 白闪问题的工程根因是 `CacheBodyMaterials()` 自动抓了所有 `SpriteRenderer`，导致 body01 / body02 / body03 都进入 `bodyMaterials[]`。持续激光每帧触发 `TriggerHitEffect()`，又不断把协程重置到最高强度，最终形成持续白光。

技术层可复用解法：

1. 不把 `GetComponentsInChildren<SpriteRenderer>()` 的结果直接当最终特效目标。
2. 按语义拆材质组，例如 `lavaOnlyMaterials`、`allFlashMaterials`、`coreFlashMaterials`。
3. `TriggerHitEffect()` 接受命中类型参数，决定作用哪组 Renderer。
4. 高频命中加最小间隔，例如 0.1-0.12 秒。
5. 峰值强度参数化，不要把 `1.0` 写死。

持续武器的视觉反馈要按“可读”设计，而不是按“每次命中都触发最强反馈”实现。

## 来源: `10_流水/光与朽项目/实现-Ch3Boss极寒之核Unity接入文档_202604.md` · 提取日期 2026-05-19

## Unity 接入文档要记录隐藏依赖

复杂技能的失败经常不在 C#，而在 Unity 编辑器侧：Prefab 层级、Layer、Tag、Inspector 引用、Database 注册少一个，代码就会表现成“偶发失效”。

GlacialBoss 接入暴露的关键隐藏依赖：

| 依赖 | 工程含义 |
| --- | --- |
| `BossPollutionBall` Layer | 让激光检测冰刺投射体，形成可反制对象 |
| Tower / Shield Tag | 区分命中塔本体还是护盾 |
| LineRenderer `Use World Space` | 脚本用世界坐标设置射线端点时必须开启 |
| EnemyDatabase 注册 | `EnemyData.asset` 存在不等于 `GetData()` 能找到 |
| Inspector 数组引用 | BingCi 位置和 Renderer 数组必须一一对应 |

可复用习惯：每个复杂技能都配一份“接入清单 + 测试清单”，把代码外依赖写到文档里。尤其是 Layer / Tag / Database / Inspector 引用，这些最容易在迁移 Prefab 或新建场景时漏掉。

## 来源: `10_流水/光与朽项目/实现-Ch3数据层_202603.md` · 提取日期 2026-05-19

## 新章节行为系统要用“组件专责 + EnemyData 配置”扩展

Ch3 数据层比 Ch2 更复杂：它不是多几个怪物，而是多了催化暴走、霜冻施法者、前置冰盾、静止单位自动消失四套行为。更稳的结构是：通用字段放 `EnemyData`，专属行为放独立组件，`EnemyBlob` 只负责串联生命周期。

| 行为系统 | 推荐承载 |
| --- | --- |
| 催化暴走 | `EnemyBlob.ApplyBerserk()` + 死亡触发 |
| 施法者召唤冰墙 | `FrostcasterAI` 独立状态机 |
| 前置冰盾 | `IceShieldController` 管理护盾 HP 与显隐 |
| 静止单位自动消失 | `EnemyData.autoDestroyTime` + Stationary 分支 |

`EnemyBlob` 可以负责 `OnSpawn` 重置、`OnDespawn` 清理、`TakeDamage` 转发，但不要把所有机制都堆成一个巨大类。否则后续每章都会在同一个文件里加新分支，难以维护。

## 来源: `10_流水/光与朽项目/实现-FrostcasterAI重构_202604.md` · 提取日期 2026-05-19

## 相近 AI 行为要复用通用字段，保留专属字段

FrostcasterAI 重构把 `frostcasterStopYPercent`、`frostcasterCastInterval` 删除，改用远程怪通用字段 `gunnerStopYPercent`、`gunnerShootInterval`，同时保留施法者专属的 `frostcasterIceWallCount`、`frostcasterRandomWallCount`、`frostcasterIceWallType`。

这是一个很好的折中：

| 字段类型 | 处理方式 |
| --- | --- |
| 入场停驻位置、行为间隔、离塔距离、换位范围 | 远程怪共用 |
| 召唤冰墙数量、随机数量、冰墙类型 | Frostcaster 专属 |

共用字段减少配置重复，专属字段保留机制表达力。不要为了复用把所有远程怪揉成同一种行为，也不要每个怪都复制一套几乎一样的参数。

## 频繁施法敌人要用平滑换位制造预判

Frostcaster 的 `castInterval = 2s` 很短，如果每次施法后瞬移，会让画面变乱，玩家也难以形成策略。改成 `Entering → Charging → Casting → Repositioning → Charging` 的平滑状态机后，移动本身变成了两次施法之间的可读间隙。

AI 表现规则：

1. 高频行为不要瞬移，优先平滑移动。
2. 施法前要有蓄力或发光，让玩家能预判。
3. 施法后允许短暂复位或换位，制造节奏。
4. 精英版可以降低换位范围、拉长间隔，用更强效果替代更频繁骚扰。

## 来源: `10_流水/光与朽项目/实现-精英冰甲卫士冰盾系统_202604.md` · 提取日期 2026-05-19

## 护盾类敌人要把伤害结算和穿透路径分开

EliteIceShieldGuard 的冰盾机制不是简单“多一条血”，而是同时改变三件事：伤害折扣、击退衰减、激光穿透阻断。工程上要把这三层分别接好。

| 层 | 做法 |
| --- | --- |
| 伤害结算 | 冰盾存在时伤害先打 `IceShieldController`，可配置 50% 折扣 |
| 物理反馈 | 冰盾存在时仍可轻微击退，但推力衰减，例如只保留 20% |
| 激光路径 | 命中冰盾后 `break` 穿透循环，不再击中后方目标 |

关键接口是给敌人暴露 `HasActiveIceShield` 这类只读语义属性，让 `LaserController` 能判断“这次命中是否应该截断射线”。不要让激光系统去猜某个 prefab 是否有盾，也不要把护盾判断硬编码到 enemy type。

## 来源: `10_流水/光与朽项目/Project_Diary.md` · 提取日期 2026-05-19

## Inactive 起步的 UI 组件要防 Awake / Show 竞态

`TutorialSpotlightOverlay` 的真机/编辑器问题说明：Unity 中 inactive 的 GameObject 在 `SetActive(true)` 时会同步触发 `Awake()`。如果 `Awake()` 末尾又有自隐藏逻辑，`Show()` 刚激活它，它就会把自己关掉。

可复用处理：

1. `Show()` 在 `SetActive(true)` 前设置 `_showRequested` 之类的标志。
2. `Awake()` 检查这个标志，跳过初始化期自隐藏。
3. `SetActive(true)` 返回后再复位标志。

所有“场景中默认 inactive，但运行时由代码 Show”的 UI 组件都要防这个竞态，尤其是新手引导、遮罩、弹窗和教程聚光灯。

## Shader 缺失时不要让交互判定一起失效

真机上 `UI/HoleMask` Shader 如果未加入 Always Included Shaders，`Shader.Find()` 可能返回 null。错误做法是因为 Material 为 null 就提前 return，导致孔洞坐标也不更新，最终点击穿透判定失效，遮罩拦截所有触摸。

更稳的结构是把“交互几何计算”和“Shader 属性写入”解耦：

| 职责 | 即使 Shader 缺失也应执行？ |
| --- | --- |
| 目标 Rect 转屏幕坐标 | 是 |
| `_holeScreenCenter` / `_holeScreenHalfSize` 更新 | 是 |
| `IsRaycastLocationValid` 点击判定 | 是 |
| Material 属性赋值 | 否，Material 存在时才写 |

这样即使视觉降级，点击逻辑仍然可用，不会把玩家锁死在黑色遮罩上。

## 来源: `10_流水/光与朽项目/Claude-2026-03-30.md` · 提取日期 2026-05-19

## 单 GameScene 多章节要按 ChapterConfig 动态绑定

《光与朽》第二章背景、Boss、波次和对象池多次错用第一章配置，根因不是“需要多个 GameScene”，而是单场景内章节配置绑定不完整。

更稳的架构是保留一个 `GameScene`，在进入战斗时从 `ChapterConfig` 动态绑定：

| 内容 | 动态来源 |
| --- | --- |
| 背景图 | `ChapterConfig.battleBackgroundImage` |
| 波次表 | `ChapterConfig.waveConfig` |
| Boss Prefab | `ChapterConfig.bossPrefab` |
| 关卡标题 | 当前章节号 / 难度 |
| 对象池 | `EnemyPoolManager.InitializeForChapter(chapterIndex)` |

不要在场景 Inspector 里长期硬绑某一章的 Boss 或波次，把它们当作 fallback 即可。多章节项目复制多个战斗场景会带来同步成本：UI、相机、管理器、修复补丁都要改多份。

## 对象生成要统一入口，用配置区分池化策略

早期 `WaveManager` 同时维护 Boss Prefab、精英怪 Prefab、普通怪对象池，导致新增 Ch2 精英怪时漏注册。更稳的做法是：`WaveManager` 只调用统一生成接口，`EnemyPoolManager` 根据配置决定是否池化。

| 类型 | usePool | 理由 |
| --- | --- | --- |
| 普通怪 | true | 高频生成 / 回收，需要预热和复用 |
| 精英怪 | false | 数量少，Instantiate / Destroy 成本可接受 |
| Boss | false | 通常一只，语义上不需要对象池 |

统一入口的价值不是“所有东西都进对象池”，而是“所有生成配置都在一个地方”。以后新增章节，只补 `EnemyPoolConfig`，不要再改 `WaveManager` 字段。

## 来源: `10_流水/光与朽项目/Claude-2026-03-31.md` · 提取日期 2026-05-19

## Stationary 障碍物要彻底拆开物理、战斗和波次语义

LavaPuddle / IceWall 这类 Stationary 对象的定义是“场景障碍”，不是普通敌人。它要阻挡激光，但不能阻挡怪物，不能吃状态，不能掉经验，也不能卡波次。

可复用语义表：

| 系统 | Stationary 规则 |
| --- | --- |
| 物理碰撞 | Collider 设为 Trigger，让怪物可穿过 |
| 激光 Raycast | `queriesHitTriggers = true` 时仍能被射线命中，继续阻挡激光 |
| 伤害结算 | `TakeDamage()` 直接 return，不抖动、不缩小、不掉经验 |
| 状态效果 | 冰冻、减速、变色、中毒等入口直接豁免 |
| 波次完成 | 不计入 `TotalActiveEnemies`，不占 global enemy limit |
| 连锁 / 自动索敌 | 不作为起点、跳点或目标 |

实现时不要只处理 `CircleCollider2D`，要遍历所有 `Collider2D`。LavaPuddle 使用 PolygonCollider2D 时，只改 CircleCollider 会完全不生效。

## 死亡流程不要用死因早返回跳过副作用

自爆怪被其他爆炸炸到时，曾因为 `killedByExplosion` 标记过早和 `Die()` 早返回，导致特效、水坑、AoE 全部被跳过。正确做法是只在“爆炸伤害致死”时标记死因，并且死因只影响某些表现，不应该直接跳过所有死亡副作用。

死亡流程设计规则：

1. 死因标记要在确认致死后再写。
2. 自爆怪无论被什么杀死，都应执行自己的爆炸 / 留坑逻辑，除非明确设计禁止连锁。
3. 普通怪被爆炸杀死可以跳过重复蒸汽 VFX，但不能跳过回收、掉落、统计等必要流程。
4. `return` 放在死亡流程中段很危险，最好把“是否播放某个表现”做成局部条件。

## 来源: `10_流水/光与朽项目/Claude-2026-04-02.md` · 提取日期 2026-05-19

## 数据配置和组件契约必须一致

Frost_Catalyst 的实际问题不是移动代码坏了，而是 `behaviorType` 配成了 `FrostCaster`，但 Prefab 上没有 `FrostcasterAI`。运行时 EnemyBlob 把行为交给专属 AI 后，刚体无人驱动，怪物就静止不动。

新怪物配置要把“数据枚举”和“组件存在”当成一条契约检查：

| 配置 | 必须对应 |
| --- | --- |
| `Chase` | EnemyBlob 自身能驱动移动和基础攻击 |
| `RangedGunner` / `FrostCaster` | Prefab 上必须挂对应 AI 组件 |
| `Stationary` | 伤害、波次、碰撞和奖励语义都要按障碍物处理 |
| 特殊死亡效果 | `isCatalyst`、`splitOnDeath`、`spawnPuddleOnDeath` 等开关必须同步 |

特殊数值效果和特殊视觉效果要分开。Catalyst 暴走可以由数值逻辑统一生效，但冰刺覆盖这类表现组件应手动挂载，不要运行时自动给所有怪注入组件；否则会失去 Prefab 级美术控制。

## 非池化 Instantiate 也必须走生成生命周期

对象池路径通常会调用 `OnSpawn()`，但精英怪 / Boss 常常走 `usePool = false` 的 Instantiate 路径。EliteIceShieldGuard 的冰盾未初始化，根因就是非池化生成后没有补调用 `enemy.OnSpawn()`。

统一规则：

1. 不管对象来自 Pool 还是 Instantiate，都必须进入同一套 `OnSpawn()` 初始化。
2. `OnSpawn()` 负责重置护盾、显隐、状态、计时器、材质、AI 状态和动态注册。
3. `OnDespawn()` / Destroy 前负责注销 tracker、清理状态和停止协程。
4. 低频对象可以不进池，但不能绕开生命周期。

这类 bug 表面像某个机制没挂好，实际是生成路径分叉导致初始化遗漏。

## 来源: `10_流水/光与朽项目/Claude-2026-04-06.md` · 提取日期 2026-05-19

## 通用爆炸反馈要语义化，但保留 Prefab 内置特效优先级

Projectile 爆炸反馈适合统一到 `VFXPoolManager` / `AudioManager` 的语义接口，例如 `PlayProjectileExplosion()`，让 Ch1 污染球、Ch2 熔岩弹、陨石、Ch3 冰刺都能复用同一套表现入口。

但统一不等于强行覆盖。BossPollutionProjectile 已经自带 `explosionParticle` 时，通用 VFX 只应在“没有内置特效”的情况下兜底播放，避免同一颗子弹死亡时双重爆炸。

可复用接入顺序：

1. 给 VFX / SFX 新增语义枚举和 `PlayXxx()` 方法。
2. 高频爆炸走池化，低频一次性 UI / Boss 警告可直接 Instantiate 或 Animator。
3. 业务脚本只调用语义方法，不关心具体 Prefab。
4. 如果旧 Prefab 已有局部特效，先保留局部优先级，再逐步迁移。

## 一次性 UI 警告交给 Animator，代码只触发状态

Boss 警告 UI 这类“一次性、强表现、设计师想调时间”的序列，不适合把所有缩放、透明度、停留时长都写进 Coroutine。更稳的方式是：

| 职责 | 放哪 |
| --- | --- |
| 何时出现 | 代码，例如第 10 波开始 |
| 动画节奏 | Animator |
| 音效触发 | 代码或 Animation Event，按项目习惯统一 |
| 播完隐藏 | Animator 末尾事件或协程等待动画时长 |

代码只负责触发语义状态，Animator 负责可编辑的表现时间线。这样后续改警告节奏不用反复改脚本。

## 来源: `10_流水/光与朽项目/Claude-2026-04-09.md` · 提取日期 2026-05-19

## 可视化充能要用 Inspector 列表，并在 OnSpawn 复位

Frostcaster 从“无限施法怪”改成“身体水晶代表剩余施法次数”后，工程上最稳的做法不是按子物体名字查找，而是暴露 `crystals` 列表由 Inspector 明确配置。

实现规则：

1. 每次施法隐藏一个水晶，视觉状态和剩余次数一一对应。
2. `OnSpawn()` 恢复所有水晶，避免对象复用后残留上一轮状态。
3. 预留 `OnCrystalExpended()` 这类虚方法或事件钩子，方便后续加水晶爆裂 VFX。
4. 精英版只是更多 charge / crystal，不要复制一套 AI。

当机制有“剩余次数”时，视觉资源本身就是 UI。让玩家看到还剩几颗水晶，比让他数施法次数可靠得多。

## 来源: `10_流水/光与朽项目/Claude-2026-04-10.md` · 提取日期 2026-05-20

## 状态增益要作用到真实威胁源

Catalyst 暴走最初只提高怪物移动速度，但 Frostcaster 和 FrostGunner 的主要威胁不在移动，而在施法 / 射击频率。如果暴走不影响 AI 计时器，玩家会觉得“它暴走了，但威胁没变”。

实现状态增益时要按敌人职责分层：

| 敌人类型 | 暴走应影响 |
| --- | --- |
| 近战 / 冲塔怪 | 移动速度、碰撞伤害 |
| 施法怪 | 蓄力时间、施法间隔、召唤频率 |
| 炮手 | 射击间隔、弹速或命中压力 |
| 障碍物 | 不套普通暴走，改为专属效果，例如孵化加速 |

通用 buff 不应只改一组基础属性。它要命中这个单位真正制造压力的那条链路。

## 来源: `10_流水/光与朽项目/Claude-2026-04-14.md` · 提取日期 2026-05-20

## UI 引导定位要等布局完成，并持续跟踪目标

科技树升级按钮的挖孔位置曾经错误，根因是 DetailPanel `SetActive(true)` 后同帧触发事件，Canvas Layout 还没 rebuild，`GetWorldCorners()` 读到旧坐标。另一个问题是手指只定位一次，后续目标修正后手指不跟随。

UI 引导定位规则：

1. 面板刚激活后，至少 `yield return null` 等一帧再读目标 RectTransform。
2. 目标中心用 `GetWorldCorners()` 算视觉包围盒中心，不直接用 `target.position`，否则左上锚点按钮会偏。
3. 手指 Prefab 挂到 TutorialDirector 或统一层级，不挂到目标按钮下。
4. 正常模式也要每帧跟踪目标，debug 只是额外加偏移，不应决定是否更新。
5. 位置偏移写进配置，例如 `localPosition`，并提供运行时调参字段。

UI 引导的坐标 bug 很少是“数学错一行”，更多是布局时机、锚点、父节点和 Canvas 坐标系没有统一。

## 来源: `10_流水/光与朽项目/Claude-2026-04-16.md` · 提取日期 2026-05-20

## HapticFeedback 适合做成低频事件服务，而不是散落调用

移动端震动系统的工程结构建议拆成两层：

| 层 | 职责 |
| --- | --- |
| `HapticFeedback` | 平台封装、默认开关、Android SDK 分支、节流 |
| `BattleHapticController` | 订阅战斗事件，把护盾破碎、Boss 入场、阶段切换等转成震动 |

这样业务脚本不需要到处写 AndroidJavaObject，也不需要知道权限、SDK 版本或玩家开关。新项目可复用的接口只保留 `Trigger(HapticType.Heavy)` / `TriggerRaw(ms)` 这类语义调用。

注意两点：

1. 高频事件必须有节流 key，比如护盾受击不能每帧震。
2. 可选震动必须走 PlayerPrefs 持久化，默认开启，但设置里可关闭。

## Boss 演出 VFX 要自动销毁，并在未配置时显式警告

Boss 入场黑线特效这类低频演出，不一定需要进对象池，但必须有生命周期管理。常见风险是 Instantiate 后不销毁，场景里累积空对象；或者 Inspector 漏拖预制体时静默跳过，排查成本很高。

可复用处理：

1. 先读 Inspector 字段，允许章节 Boss 覆盖专属特效。
2. 未配置时可用 `Resources.Load` 提供默认兜底，但要打 Warning。
3. 实例化后读取 ParticleSystem 总时长，按真实时长 Destroy。
4. 粒子配置问题要给检查清单：Sorting Layer、Order、Start Size、Duration、Material。

演出特效的工程目标是“漏配能看见、播完能回收、不同章节可覆盖”。

## 来源: `10_流水/光与朽项目/Claude-2026-04-17.md` · 提取日期 2026-05-20

## Unity Android 不要用主 AndroidManifest 去“追加权限”

在团结 / Unity Android 工程里，`Assets/Plugins/Android/AndroidManifest.xml` 不是简单追加配置，它可能成为 `tuanjieLibrary` 模块的主 manifest，改变 Unity 生成图标资源的解析路径，导致桌面图标消失。

给权限的稳妥方式：

| 方式 | 适用 | 风险 |
| --- | --- | --- |
| `IPostGenerateGradleAndroidProject` 后处理脚本 | 推荐，生成 Gradle 工程后注入权限 | 不干扰 Unity 图标生成 |
| 极简 `.androidlib` 只放权限 | 可选 | 仍需确认 manifest 合并不影响图标 |
| 直接放 `Assets/Plugins/Android/AndroidManifest.xml` | 不推荐 | 容易替换主 manifest，引发图标 / 合并问题 |

权限注入的安全顺序是：Unity 先生成完整 Gradle 工程和图标资源，后处理脚本再读取最终 manifest，检查缺少的 `<uses-permission>` 并插入。

这条经验不只适用于 `VIBRATE`，后续相机、存储、通知权限也应优先走后处理脚本，而不是手写主 manifest。

## AutoSingleton 适合“可按需出现”的服务，但要懂生命周期

`HapticFeedback.Instance?.Trigger(...)` 如果继承普通 `Singleton<T>`，而场景里没有挂载组件，`Instance` 会一直是 null，空安全调用会静默跳过，真机就完全没有震动。

这类服务更适合 `AutoSingleton<T>`：

| 场景 | 推荐 |
| --- | --- |
| 必须从 Bootstrap 常驻、跨场景保存 | `PersistentSingleton` |
| 场景内必须由设计师摆放 | `Singleton` |
| 第一次调用时自动创建即可 | `AutoSingleton` |

震动这类轻服务可以自动创建，因为它没有复杂场景引用，开关状态也能从 PlayerPrefs 恢复。但 UIManager、GameManager、AudioManager 这类有 Inspector 引用或全局状态的 Manager，不要随便 AutoSingleton 化。

## VFX 回收计时必须和粒子 timeScale 保持一致

EnemySteam 死亡冒烟特效“越打越没有”的根因，是粒子 `useUnscaledTime=1` 按真实时间播完，但回收协程用 `WaitForSeconds`，暂停时协程停止计时，导致对象永远卡在 activeInstances，池逐渐耗尽。

判断规则：

| 粒子播放 | 回收等待 |
| --- | --- |
| `useUnscaledTime = true` | `WaitForSecondsRealtime` |
| 受游戏暂停影响 | `WaitForSeconds` |

对象池 bug 常常不是池大小不够，而是回收生命周期和表现时间轴不一致。暂停、设置面板、技能选择面板都会把这类问题放大。

## 来源: `10_流水/光与朽项目/Claude-2026-04-18.md` · 提取日期 2026-05-20

## 关键演出不要在暂停 UI 背后播放完

Boss 入场 VFX 的偶发消失暴露了一个通用坑：战斗暂停 UI 与 unscaled 粒子混用时，粒子会在 UI 背后继续播放，玩家关闭面板时特效已经结束。

排查路径：

1. 看触发时是否可能弹出升级 / 技能 / 设置面板。
2. 看移动 Tween 是否受 `timeScale` 控制。
3. 看粒子是否 `useUnscaledTime`。
4. 看 Destroy / 回收等待使用游戏时间还是真实时间。

修复不是一律改成 unscaled，而是让同一段演出的移动、VFX、等待、销毁使用一致的时间语义。Boss 入场 / 死亡 / 新手引导这类“玩家必须看见”的演出，优先使用真实时间并避免被暂停 UI 遮挡。

## 来源: `10_流水/光与朽项目/Codex-2026-04-07.md` · 提取日期 2026-05-21

## 出界怪物优先校正位置，不要只做超时销毁

远程怪物（如 LavaGunner、Frostcaster）出现在屏幕外继续攻击时，直接“屏幕外 N 秒销毁”只能消掉对象，不能解决玩家听到音效、看到攻击却找不到来源的困惑。

更稳处理：

| 步骤 | 作用 |
| --- | --- |
| 检测远程怪是否越界 | 只作用于远程怪，避免影响弹球怪、Boss、特殊反弹行为 |
| 校正到最近合法屏幕点 | 让威胁重新可见 |
| 越界时禁止射击 / 施法 | 防止屏幕外伤害 |
| 保留超时回收兜底 | 防止极端卡死 |

空间约束不要全局一刀切。只修出现问题的敌人类型，避免破坏依赖越界、反弹或特殊位移的机制。

## 粒子 Ready VFX 重播要同时重置状态和播放

UI 粒子如果只 `SetActive(true)`，很容易因为粒子已经播完、引用丢失或对象隐藏后状态未重置而不再出现。大招 Ready VFX 这类提示要做成显式方法：

1. Inspector 引用丢失时，从子节点按固定名兜底查找。
2. 隐藏统一走 `HideReadyVFX()`，不要散落 `SetActive(false)`。
3. 显示统一走 `ShowReadyVFX()`：先激活，再 `Clear(true)`，再 `Simulate(0f, true, true)`，最后 `Play(true)`。

提示型粒子不是背景装饰，它承担状态反馈。每次状态切换都应该能稳定从第一帧重播。

## 爆炸音效要按语义统一并做短窗口去重

多个敌人或子弹在同一帧触发爆炸时，如果每个系统各播自己的 SFX，会变成浑浊的噪音。更稳的做法是把同类爆炸统一路由到一个语义接口，例如 `PlayEnemyExplode()`，并设置 0.05 秒左右的去重窗口。

规则：

| 情况 | 处理 |
| --- | --- |
| LavaExploder 死亡爆炸 | 播敌人爆炸，不再叠通用死亡音 |
| Projectile / Grenade 爆炸 | 可路由到同一爆炸语义 |
| 同一瞬间多个爆炸 | 短窗口内只保留一次或做有限混音 |
| 进入结算面板 | 关闭战斗 SFX、激光循环、Boss 循环和场景残留 AudioSource |

音效系统的目标不是“每个事件都播放”，而是让玩家听清当前最重要的事件。

## 来源: `10_流水/光与朽项目/Claude-2026-04-22.md` · 提取日期 2026-05-21

## 配置字段必须进入运行时公式

`enemyHealthMultiplier` 和 `chapterSpeedMult` 这类字段如果只存在于配置或 Inspector，不进入 `WaveModifiers` / 运行公式，就会造成“策划已经改了，游戏完全没变”的安静失效。

检查清单：

1. 配置类里有字段。
2. 运行时加载章节时读到字段。
3. 字段被合并进本波 `waveModifiers`。
4. 怪物生成和子体继承同一套 modifiers。
5. 实机改一个极端值验证结果确实变化。

《光与朽》的具体坑是：代码读到了 `chapterHealthMult=0.6`，但最终 HP 只用了 `waveDifficulty * extraHealthMult`，漏乘章节倍率。字段定义完成，不等于机制完成。

## 来源: `10_流水/光与朽项目/Claude对话记录.md` · 提取日期 2026-05-21

## 旧功能回归要同时看历史实现和当前架构

反射透镜技能曾被删除，后来因为第三章冰墙机制重新有价值。回归旧功能时，不能直接复制旧代码，也不该完全凭记忆重写。

更稳流程：

1. 查 git 历史，找旧实现的业务逻辑和边界处理。
2. 读当前技能架构，确认枚举、SO 字段、技能效果管理、激光控制和数据库注册方式。
3. 用旧逻辑适配新接口，而不是粘贴旧类。
4. 测试技能顺序问题：例如先选 Reflex 再选 Prism 时，副激光是否继承反射状态。
5. 对“主激光生效、副激光不生效”这类问题，优先检查状态是否同步到已有子对象和新创建子对象。

旧功能回归的风险不在“逻辑写不出来”，而在它和重构后的对象生命周期、注册路径、子对象继承关系不一致。

## 来源: `10_流水/光与朽项目/Codex-2026-04-08.md` · 提取日期 2026-05-21

## UI 相机改造后，所有世界/UI 坐标换算都要统一重审

《光与朽》把 Canvas 从 Overlay 逐步改到 Screen Space - Camera 后，连续暴露了几类问题：大招按钮粒子偏移、怪物飘字消失、无人机奖励飘字消失、经验球飞向升级条位置错误。根因相同：旧代码还在用 `Camera.main.WorldToScreenPoint()` 后直接塞给 `RectTransform.position`，或者把 UI world corners 当屏幕坐标。

统一规则：

| 场景 | 推荐换算 |
| --- | --- |
| 世界对象显示 UI 飘字 | 世界相机 `WorldToScreenPoint` -> `RectTransformUtility.ScreenPointToLocalPointInRectangle` |
| UI 目标转世界吸附点 | 先用 Canvas `worldCamera` 得到 UI 屏幕点，再用战斗相机转世界点 |
| UI 按钮上的粒子 | 优先做成同一 Canvas / UI Camera 下的本地 UI 特效 |
| 复杂 3D/模型预览 | 才考虑 UI Camera + RenderTexture |

按钮充能特效这类小 UI 特效，不值得上 RenderTexture。更稳的是把特效节点挂到按钮 RectTransform 下，使用 UI Camera 和本地坐标，让 Safe Area、分辨率和长宽比变化时仍跟随 UI。

## 阻塞 UI 出现时，战斗表现要进入同一套暂停语义

技能三选一、复活、设置、结算这类面板弹出时，不只是 Time.timeScale 变化。战斗循环音、Boss 循环音、投射物飞行音、ReadyVFX 都要按同一套遮挡语义暂停或隐藏。

可复用结构：

| 系统 | 处理 |
| --- | --- |
| ReadyVFX | `ShouldShowReadyVFX()` 统一判断是否被面板遮挡，关闭后 Ready 状态仍在则恢复 |
| 音频 | `PauseBattleAudioForOverlay()` / `ResumeBattleAudioForOverlay()` 用计数器支持多层面板 |
| 投射物循环音 | 不用 prefab `PlayOnAwake + Loop` 自播，改由脚本 `Play/Pause/UnPause/Stop` 接管 |
| UI 点击音 | 不纳入战斗暂停，避免按钮反馈消失 |

凡是“自己在 prefab 上循环播放”的 AudioSource，都可能绕过 AudioManager 的暂停逻辑。移动端战斗音频最好由脚本按战斗状态统一接管。

## 数值上限要同时约束成长、选项池和子对象

激光长度过长时，不能只在最终长度上 `Clamp`。否则玩家还会继续抽到“增加长度”的无人机奖励，感觉奖励被吞，副激光也可能因为创建时没走同一上限而越界。

更完整的上限结构：

1. 主激光和副激光各有独立上限，例如主 26、副 24。
2. 技能长度加成和无人机长度奖励都按上限反推，避免几次奖励就顶出屏幕。
3. 达到上限后，三选一 / 契约奖励池过滤掉长度选项。
4. 子激光创建、重置和继承状态时也必须应用副激光上限。
5. 如果保留“溢出转伤害”，要清楚这是补偿机制，不是继续变长。

上限不是只写在 `SetMaxLength()` 里。它还要进入奖励池、技能配置和所有新建子对象的初始化路径。

## 来源: `10_流水/光与朽项目/Codex-2026-04-10.md` · 提取日期 2026-05-21

## Unity 序列化枚举必须显式编号并迁移资产

FrostGunner 和 LavaSlime 曾因 `EnemyType` 枚举值同为 21，导致 Unity Inspector 里 FrostGunner 自动变成 LavaSlime，运行时第三章还会请求未注册的 LavaSlime。这个问题不是显示 bug，而是序列化值冲突。

修复原则：

1. 资源已经序列化的枚举，一律显式编号。
2. 新增枚举不要插在中间改变旧值。
3. 冲突修复后必须同步迁移 prefab、asset、scene、wave config 里的旧数值。
4. 分章节保留同值时要确认语义，例如 LavaSlime 的 21 仍只在第二章合法。
5. 用“未注册敌人类型”日志反查配置，不要盲目把该类型注册进对象池。

枚举一旦进入 Unity 资源，就不是普通 C# enum 了，它已经变成数据协议。

## 波次完成判定要看“仍影响战斗的对象”

第三章冰墙会孵化小怪，如果波次完成判定只看可战斗敌人，可能冰墙还在、后续小怪还会生成，无人机却已经入场。

更稳的波次结束口径：

| 对象 | 是否阻止波次结束 |
| --- | --- |
| 普通敌人 | 是 |
| Boss | 是 |
| 冰墙 / 岩浆液这类仍会阻挡或生成单位的地形 | 是 |
| 纯演出粒子 / 掉落物 | 否 |
| 已禁用对象池对象 | 否 |

命名上可以避免“敌人数”歧义，例如 `GetSceneEnemyCount()` / `GetBlockingBattleObjectCount()`。波次完成判定要服务战斗流程，而不是服务某个类名。

## Layer 和 Tag 不要混用语义

`Shield` Layer 存在，不代表 Unity 里也有 `Shield` Tag。`CompareTag("Shield")` 会在 Tag 未定义时报错。反过来，Layer 适合做物理碰撞筛选，Tag 适合做少量高层分类，不应互相替代。

推荐规则：

| 目标 | 推荐判断 |
| --- | --- |
| 物理射线 / 碰撞矩阵 | LayerMask |
| 玩家护盾组件 | `GetComponent<ShieldController>()` |
| 光棱塔本体 | `GetComponent<TurretHealth>()` |
| 敌方冰盾 | 独立 `IceShield` Layer + `IceShieldController` |
| 泛 UI / 特定命名节点 | 优先引用或组件，不靠 Tag 字符串 |

当玩家护盾和敌方冰盾都叫 Shield 时，后续所有激光显示/伤害逻辑都会出现歧义。更好的结构是玩家 `Shield`、敌方 `IceShield` 分层。

## 来源: `10_流水/光与朽项目/Codex-2026-04-11.md` · 提取日期 2026-05-21

## 协程动画被 StopAllCoroutines 打断前要恢复对象状态

极寒炮手和霜冻施法者偶发“压扁”的根因，是攻击动画直接改 `transform.localScale`，中途被 `StopAllCoroutines()` 打断后没有复位。后来被玩家打一下又恢复，是因为受击逻辑重新按等比缩放写了一次。

可复用规则：

1. 协程动画不要长期直接写本体 `localScale`，能写子节点就写子节点。
2. 如果必须写本体，缓存基准缩放。
3. 在 `OnSpawned`、`OnDeactivated`、越界恢复、状态切换、对象池回收前后都调用恢复方法。
4. `StopAllCoroutines()` 之后立刻恢复关键表现状态：缩放、颜色、透明度、粒子开关、音效。

协程不是事务。中途停止时，Unity 不会帮你执行 finally，也不会自动把对象变回动画前状态。

## 通用颜色系统不要接管特殊技能零件

极寒 Boss 的 BingCi01-04 发射后本该隐藏，但它们被挂进 Boss 通用 `bodyRenderers`，通用变色/恢复逻辑把 alpha 刷回 1，于是隐藏中的冰刺以发射后的旋转姿态短暂反向显示。

处理原则：

| 渲染组 | 适合放什么 |
| --- | --- |
| `bodyRenderers` | Boss 常驻身体、受击变色、暗化统一处理 |
| 技能零件组 | 可隐藏、可旋转、可发射、可再生的部件 |
| VFX 组 | 粒子、投射物、短生命周期表现 |

技能零件如果会被隐藏、旋转或发射，就不应被通用身体颜色系统随手恢复。否则“恢复原色”会变成“破坏技能状态”。

## 来源: `10_流水/光与朽项目/Codex-2026-04-12.md` · 提取日期 2026-05-21

## 移动端卡顿排查先分 CPU 数量型热点和渲染配置

《光与朽》战斗卡顿的代码扫描显示，移动端性能问题通常不是单点，而是多个数量型系统叠加。

高优先级排查清单：

| 类别 | 风险点 |
| --- | --- |
| 激光 | 同帧重复路径计算、射线检测、`GetComponentInParent`、OverlapBox 查询 |
| 连锁闪电 | 每帧链路更新、扩链、LineRenderer 刷新、目标搜索 |
| 敌人材质 | `sr.material` 导致材质实例化，破坏批处理 |
| AOE | `Physics2D.OverlapCircleAll` 分配数组，造成 GC spike |
| 飘字 / 经验球 / 金币 | 大量对象各自 Update，且可能触发 Canvas 刷新 |
| 日志 | 真机战斗日志常开会放大 CPU/IO 压力 |
| 渲染 | HDR、阴影、Additional Lights、Metaballs RT、全屏后处理 |

Profiler 数据采集优先不要开 Deep Profile。用 Development Build + Autoconnect Profiler 录 20-30 秒，至少看 CPU Usage、Rendering、Memory、Physics 2D、UI Details。没有 profiler 时，再打一秒一次的轻量采样日志：敌人数、激光段数、连锁线段数、飘字数、经验球数、活动 VFX 数、帧时长。

## 微信小游戏打包要避免 Editor-only 调用和不兼容 ShaderGraph 分支

两个典型坑：

1. 方法定义在 `#if UNITY_EDITOR`，但调用没有包宏。编辑器正常，微信小程序构建时方法被裁掉，直接编译失败。调用和定义必须处于同一编译条件下。
2. ShaderGraph 同时挂 BuiltIn / HD / Universal target，或者粒子特效保留阴影/深度分支，微信 `gles` 构建可能在 `SAMPLE_DEPTH_TEXTURE` / ShadowCasterPass 报错。

微信小游戏 2D 特效更稳的 ShaderGraph 处理：

| 项 | 建议 |
| --- | --- |
| Active Target | 只保留项目实际使用的 Universal |
| Cast Shadows | 关闭 |
| Receive Shadows | 关闭 |
| 粒子 Mesh 特效 | 避免走深度/阴影宏 |
| 报错定位 | 看第一条 shader error，不要被后续 PPtr cast failed 误导 |

小程序构建不是编辑器播放。所有 Editor-only 调试、Shader target、平台宏都要单独过一遍。

## 金币序列帧 Shader 要匹配 Sprite 导入方式

2x2 金币序列图用 SpriteRenderer + 自定义 flipbook shader 时，如果显示成“四瓣”，通常不是帧索引错，而是 Sprite UV 不是整图 0-1。

导入规则：

1. `Texture Type = Sprite (2D and UI)`。
2. `Sprite Mode = Single`。
3. `Mesh Type = Full Rect`。
4. `Wrap Mode = Clamp`。
5. SpriteRenderer 绑定整张图，不绑定切好的子 sprite。

如果用 Multiple 子图或 Tight mesh，shader 再自己分帧就会在错误 UV 上二次切割。序列帧 shader 要么掌控整图 UV，要么就改用 Sprite Animation，不要两套切图逻辑叠在一起。
## 来源: `10_流水/光与朽项目/Codex-2026-04-15.md` · 提取日期 2026-05-21

## 激光变宽要同时处理命中体积、发光强度和倍率叠乘

《光与朽》的激光宽度由 `LineRenderer.startWidth/endWidth` 控制，不是运行时改 ShaderGraph 的 `_BeamWidth`。广域透镜提高宽度后，伤害检测也会变宽，因为伤害路径使用 `OverlapBoxNonAlloc`，盒子宽度取自 `LaserBeam.GetLaserWidth()`。

这类“变宽技能”要同时检查三条链：

| 链路 | 风险 | 处理 |
| --- | --- | --- |
| 表现宽度 | HDR/Bloom 下线宽变大，发光面积增加，玩家感到爆亮 | 按 `widthRatio` 压低 `_BaseColor` 强度，例如 `1 / sqrt(widthRatio)` |
| 命中宽度 | 只改视觉会让玩家觉得粗了但仍漏怪 | 伤害盒宽度要跟随真实激光宽度 |
| 倍率计算 | 主激光已乘大招宽度，副激光又额外乘一次，导致 `overload^2` | 副激光宽度基于当前主宽度乘固定比例，不重复乘大招 |

宽度数值也要让玩家看得出来。基础宽度 `0.5` 时，`2.0x` 只有 `1.0`，在 Bloom 下容易被亮度掩盖；广域透镜提升到 `1.3/1.6/1.9/2.2/2.5` 后，视觉和命中收益都更清楚。

## 广告按钮状态优先换图标和灰态，不在小按钮里塞长文案

技能重掷按钮从“看视频再抽一次 / 今日已达上限”改成骰子图标、摄像头图标和置灰状态后，UI 更稳。小按钮承载不了长文案时，应把语义交给图标、状态和旁边说明区域。

可复用规则：

1. 免费阶段显示原功能图标。
2. 广告阶段切到广告图标。
3. 达到次数上限时，按钮、图标、文字一起置灰。
4. 解释性文案放到面板正文，不要动态塞进按钮。
5. 新增可选引用时，允许 Inspector 绑定；未绑定时用子节点查找兜底，并缓存原始颜色用于恢复。

## 来源: `10_流水/光与朽项目/Codex-2026-04-16.md` · 提取日期 2026-05-21

## 阻挡物语义要覆盖所有攻击路径

熔浆液和冰墙作为 `Stationary` 障碍，主激光已经会在命中后 `break`，但连锁反应技能最初仍可能跳到障碍物后面的敌人。原因是连锁路径只把障碍物排除为目标，没有检查“源目标到下个目标之间是否被阻挡”。

复用原则：

| 攻击类型 | 阻挡检查 |
| --- | --- |
| 主激光 | 命中 `Stationary` 后停止穿透 |
| 副激光 / 分裂激光 | 和主激光使用同一阻挡语义 |
| 连锁闪电 | 从源目标到候选目标前先 `Linecast` 检查中间 `Stationary` |
| AOE / 溅射 | 明确定义是否绕过墙，不要默认继承连锁逻辑 |

障碍物不是“不能被选为目标”这么简单。只要它在视觉上挡住战线，所有远程传播技能都要回答：这条效果能不能穿过它？

## 调试工具的能力范围要和当前问题对齐

移动端性能排查时，曾先做了一个带 CSV、标记、GC、线程耗时的复杂采集面板，但用户真正需要的是“右上角只显示 FPS，然后 BattleStatistics 能在手机包正常保存战斗 CSV”。最终拆成两个工具更清楚：

- `FpsDisplay`：只显示 FPS，不采集、不保存、不写文件。
- `BattleStatistics`：由 `enableDataCollection` 控制是否采集战斗 CSV。

调试工具要避免“顺手做成全能面板”。当问题是“我想知道什么时候掉帧”，一个轻量 FPS 文本比复杂采样系统更低风险；当问题是“我要保存战斗数据”，就让原本的 BattleLog 管线负责。

## 来源: `10_流水/光与朽项目/Codex-2026-04-17.md` · 提取日期 2026-05-21

## UI 状态刷新必须监听真实数据事件

体力不足时主菜单开始按钮置灰，玩家在 `TopBarTipsPanel` 领取体力后，顶部体力数字变了，但开始按钮仍旧灰掉。根因是主菜单只在 `Start()` / `RefreshUI()` 刷按钮状态，没有订阅 `ProgressManager.OnEnergyChanged`。

UI 状态刷新规则：

| UI 表现 | 应监听的数据事件 |
| --- | --- |
| 开始按钮可点/置灰 | 体力变化 |
| 顶栏金币/图纸 | 对应资源变化 |
| 技能重掷次数 | 免费次数 / 广告次数变化 |
| 装备按钮 | 装备成功或背包变化 |

不要让 UI 只在面板打开时刷新。只要一个按钮的可用性依赖资源，就必须订阅资源变化事件，否则“数据已经变了，按钮还旧”的 bug 会反复出现。

## 通用按钮组件要统一置灰、Q 弹和动态按钮接入

按钮置灰如果只改 `Button.interactable`，子节点图片和文字仍然可能保持亮色；点击动画如果散落在各面板，也会产生缩放残留和风格不一致。更稳的做法是统一 `UIButtonCommon`：

- 禁用时递归处理自身和子节点的 `Image`、`TMP/Text`。
- 点击时播放统一 Q 弹动画，并允许单按钮关闭。
- `OnDisable` / 隐藏前停止动画并恢复原始缩放。
- 动态生成的按钮通过 `UIButtonCommonHelper.Ensure(button)` 接入。
- 特殊 UI 例如科技树锁定节点，可以保留专属透明度，只接入 Q 弹。

统一组件不是为了“所有按钮完全一样”，而是把常见的灰态、缩放恢复、点击反馈收口，特殊样式再显式豁免。

## 来源: `10_流水/光与朽项目/Codex-2026-04-18.md` · 提取日期 2026-05-21

## 展示数据和实际生效数据必须来自同一个 canonical 对象

技能三选一出现“卡面描述和实际技能不一致”，高概率来自同一 `SkillType` 存在重复或非规范引用：展示用列表里的 `SkillData`，实际生效时又按 `SkillType` 从数据库缓存取另一份。

解决思路：

1. 构建技能池时按 `SkillType` 去重。
2. 展示、选择、生效都收敛到数据库中的 canonical `SkillData`。
3. 同一局中升级同一技能时，卡面描述、选择回调、战斗效果必须同源。
4. 测试时重点跑“先选 A，再升级 A”的连续路径，而不是只看首次三选一。

所有“显示 A，实际吃到 B”的 bug，都优先查数据源是否分叉。

## 微信小游戏打包排查要先分清资源、图形上下文和包依赖

微信开发者工具报错时，后续错误常常是第一现场失败后的连锁反应。`Unable to create WebGL context` 后面的 `scheduler is not a function` 就更像二次报错，不应优先追业务资源。

排查顺序：

| 现象 | 优先判断 |
| --- | --- |
| 删 `Assets/Resources`、Box 场景仍失败 | 更偏图形配置 / WebGL context / 工具环境 |
| 同工具下另一个项目正常 | 对比项目配置差异 |
| 替换 ProjectSettings 后仍失败 | 查 Build Profile、MiniGameConfig、Graphics、Quality、URP 设置 |
| `Invalid WebGL template path` | 检查 `Packages/manifest.json` 是否把微信 UPM 包覆盖掉 |
| `Invalid GUILayout state in WXEditorWin` | 微信插件窗口状态或插件配置 asset 不匹配，先关窗口重启 |

跨项目对拷时最危险的是 `Packages/manifest.json` 和 `packages-lock.json`。它们不是普通配置文件，会改变实际安装的包。做 A/B 测试时，优先只改单个字段，不要整目录替换。

## DOTween 在销毁前必须 Kill，微信小游戏环境对失效 Transform 更敏感

无人机三选一在微信包里爆炸瞬间崩溃，日志指向 `DOScale -> Transform.localScale`。最终可疑链路集中在箱子和进度 UI 的 DOTween：对象被销毁后，缩放动画下一帧继续写失效 Transform。

复用规则：

1. 所有 `DOScale`、`DOFade`、脉冲循环 tween 都要保存句柄。
2. `OnDestroy()`、`Hide()`、`HideImmediate()`、对象池回收前统一 `Kill`。
3. 同一对象开始新 tween 前，先 kill 旧的同类 tween。
4. 管理器如果会 `Destroy` 一批对象，要么先通知对象清 tween，要么延迟到动画结束后销毁。
5. 微信小游戏 / WebGL 环境里，不要指望 DOTween 对已销毁对象的异常都被安全吞掉。

“编辑器里没崩”不代表生命周期安全。凡是动画和销毁在同一帧附近发生，都要按 WebGL 更严格的环境来写。

## 来源: `10_流水/光与朽项目/Codex-2026-04-19.md` · 提取日期 2026-05-21

## RemoveAllListeners 会误删通用按钮能力

背包物品、装备槽、动态按钮有 `Button` 组件却没点击音效，常见原因不是按钮坏了，而是业务代码在 `Setup()` 中调用 `RemoveAllListeners()`，把通用音效、通用动画等监听一起删掉。

推荐做法：

| 场景 | 做法 |
| --- | --- |
| 动态绑定业务点击 | 只移除自己的业务回调，再重新添加 |
| 运行时生成按钮 | `UIButtonCommonHelper.Ensure(button)` |
| 旧按钮音效脚本 | 迁移到统一按钮组件，避免双播 |
| 全局自动接入 | 场景加载后扫描 `Button`，但动态项仍要显式 Ensure |

`RemoveAllListeners()` 是很钝的刀。除非按钮确实完全私有，否则不要用它清业务逻辑。
## 来源: `10_流水/光与朽项目/Codex-2026-04-22.md` · 提取日期 2026-05-22

## 对象池里的粒子特效必须显式 Stop、Clear、Play

连锁反应替换成带粒子的 `VFX_ChainLightning` 后，开局对象池预热出的 10 个实例直接漏粒子，结束后粒子也残留。根因是对象池只管理 GameObject active，不会自动处理子 `ParticleSystem` 的生命周期。

可复用处理：

| 时机 | 粒子动作 |
| --- | --- |
| `Awake()` / 预热 | 关闭 `playOnAwake`，`Stop + Clear` |
| `Initialize()` / 启用 | `Clear + Play`，从头播放 |
| `Deactivate()` / 回池 | `StopEmittingAndClear` |
| 换 prefab | 检查根节点是否是专用 renderer，不要复用激光 prefab |

对象池预热的前提是“对象存在但不可见”。只隐藏根物体不够，粒子、Trail、LineRenderer、材质颜色都要有自己的回池清理。

## 特效颜色要以当前玩法对象为权威，不要有默认白色兜底

闪电链的颜色最初只在聚能透镜或极寒光束时改色，没选改色技能时变成白色。正确规则是：闪电链永远跟随当前激光颜色。默认激光是黄色，闪电链就黄色；聚能变红就红；极寒变蓝就蓝；恢复默认时也恢复激光材质原始色，而不是白色。

工程上要注意三层同步：

1. 从 `LaserBeam.GetCurrentColor()` 读真实当前颜色，而不是写死默认色。
2. `LaserController` 在初始化、改色、重置颜色时同步给 `ChainLightningManager`。
3. `ChainLightningRenderer` 同步根 `SpriteRenderer`、子 `ParticleSystem.main.startColor` 和材质属性。

如果 SpriteRenderer 的 shader 不吃顶点色，还要用 `MaterialPropertyBlock` 写 `_Color`、`_BaseColor`、`_EmissionColor`、`_TintColor`。不要直接改 `renderer.material`，否则会产生材质实例并破坏批处理。

## 来源: `10_流水/光与朽项目/程序AI对话.md` · 提取日期 2026-05-22

## 战斗 UI 厚血条要做缓冲条，而不是只提高伤害数字

Boss 血量很厚时，玩家打一段时间看不到血条变化，会误判“没伤害”。更好的做法是双层血条：

- 上层红条代表真实血量，受击瞬间减少。
- 下层白条代表缓冲血量，延迟 0.2 秒后追上。

这种白色 buffer bar 能让玩家看到“刚才那一下打进去了”。它不改变数值，却显著改善伤害可见性，特别适合 5 万、20 万这类高血量 Boss。

## 配置驱动技能时，展示、数值和颜色都要从同一份数据读

技能三选一、激光颜色、VFX 颜色、技能描述如果各自写硬编码，很容易出现“卡面写一套，战斗生效另一套”。后期技能配置应收敛到：

| 内容 | 推荐来源 |
| --- | --- |
| 技能数值 | `SkillData.levelData` |
| 技能是否改色 | 技能级别之外的 `changesColor` / `skillColor` |
| 聚能 + 极寒混合色 | `SkillDataBase` 或统一颜色策略表 |
| 描述文本 | 每级描述模板，重点数字用富文本标色 |
| 粒子颜色 | 复用激光当前颜色，不单独配置一套 |

颜色不要每一级重复填；数值不要在脚本里另写一份；描述不要只写 Lv1。数据源越少，后续平衡越不容易打架。

## 战斗内动态物体如果没有眼睛或不闪白，要有显式语义开关

熔浆液、冰墙这类阻挡物可以复用“高血量怪物”管线，但不能完全当普通怪处理。否则会出现不该有的飘字、抖动、白闪，甚至对 inactive 的 Eyes 节点启动协程报错。

推荐在数据或组件层显式区分：

| 对象 | 推荐语义 |
| --- | --- |
| 普通敌人 | 有眼睛、可受击反馈、掉 XP |
| 熔浆液 / 冰墙 | 阻挡激光、高血量或定时消失、无眼睛、无普通受击抖动、可不掉 XP |
| 宝箱 / 战术箱 | 可被激光打爆，但奖励逻辑和敌人死亡不同 |
| Boss 子弹 | 可被激光击落，但不走敌人经验和飘字规则 |

“复用敌人接口”不等于“继承敌人表现”。复用前先列清楚哪些反馈应关闭。

## 运行时脚本灰掉不报错时，先怀疑 Unity 编译/序列化刷新

有一次 `TurretHealth` 挂在场景物体上变成灰色且脚本名消失，Unity 没有明确报错；删除数据采集代码、重新导入也无效，重启 Unity 后恢复。后来把光棱塔做成 prefab，再加回采集代码才稳定。

经验：

- 场景对象上的脚本引用异常，不一定是业务代码逻辑错。
- Unity 编译域、程序集刷新、脚本 GUID/类名变动，都可能让 Inspector 临时丢脚本。
- 重要战斗对象尽量 prefab 化，少让关键组件只存在于场景散件上。
- 遇到“脚本灰掉但控制台无错”，先重启 Unity / 重新生成工程文件 / 检查 `.meta` 与类名，再继续追业务链路。

## 来源: `10_流水/历史聊天/Claude_光与朽程序_2025-11-01.md` · 提取日期 2026-05-22

## Metaballs 换章节色时，不要改 RT 摄像机底色

《光与朽》的怪物身体层使用 RT / Metaballs 做融合时，阈值 Shader 依赖“黑色背景 + 非黑 blob”来判断形状。如果为了换章节气氛去改 RT 摄像机背景色，很容易让阈值判断失真，导致边缘、融合和透明显示异常。

更稳的拆法：

| 层级 | 应该改什么 | 不应该改什么 |
| --- | --- | --- |
| RT / Metaballs 计算层 | 保持黑底和阈值规则稳定 | 不改相机背景来做章节色 |
| 最终显示材质 | 通过 `MetaballsManager.SetBlobColor` 或章节配置换 blob 色 | 不让表现色反向影响计算 |
| 战斗背景 | 通过 `ChapterConfig` 切换背景 Sprite / 环境层 | 不混进身体融合逻辑 |

章节视觉配置应当是数据驱动：第一章黑紫油污、第二章暗红熔岩、第三章冷蓝冰霜都可以通过显示材质和背景图完成。渲染计算层越稳定，章节表现越容易扩展。

## 来源: `10_流水/历史聊天/Claude_光与朽程序_2025-11-02.md` · 提取日期 2026-05-22

## 修旧 Unity 项目时，先读真实 API，再写补丁

长对话里多次出现一个典型风险：修复方案看似合理，但引用了项目中并不存在的字段、事件或枚举，例如 `GameEvents.OnGameOver`、错误的 Boss 配置字段、错误的投射物初始化参数、错误的伤害来源枚举。Unity 项目迭代快，聊天记录里的“应该有”不能当成事实。

落地修复前至少做三步：

1. 用 `rg` 查真实字段、事件、方法签名和调用点。
2. 对照 Inspector / ScriptableObject 数据结构，确认配置项是否存在。
3. 如果现象原因不确定，先加诊断日志和最小监控点，不要直接重写链路。

例如波间无人机不出现这类问题，优先在 `WaveManager.StartWaveInterval`、`TacticalDropManager.OnWaveComplete` 和事件触发链上加日志，确认是事件没发、订阅没接，还是条件被过滤。工程修复的第一步是让真实路径可见，而不是让想象中的架构更完整。

## 来源: `10_流水/历史聊天/Claude_光与朽程序_2025-11-03.md` · 提取日期 2026-05-22

## C# event 只能由声明类触发，业务脚本应调用 Trigger 方法

Unity 项目里把 `GameEvents.OnBossHealthChanged?.Invoke(...)` 写在 `BossHealth` 这类外部脚本中，会触发 `CS0070`：C# 的 `event` 只能在声明它的类内部直接 `Invoke`。正确做法是让事件总线提供触发方法：

| 错误写法 | 正确写法 |
| --- | --- |
| `GameEvents.OnBossHealthChanged?.Invoke(percent)` | `GameEvents.TriggerBossHealthChanged(percent)` |
| `GameEvents.OnBossDeath?.Invoke()` | `GameEvents.TriggerBossDeath()` |

事件字段负责订阅，Trigger 方法负责广播。这样可以避免外部脚本绕过事件总线，也让日志、空值保护和调试钩子集中在一个地方。

## 精英怪缩放要有“当前身份基准”，不要被受击缩放重置

如果普通怪原始缩放是 `originalScale`，精英怪出生后再乘 `1.3f`，那么受击时按血量重新设置 `transform.localScale = originalScale * newScale`，会把精英怪从 1.9 直接打回 1.5。

修复原则是：所有运行时缩放都从同一个“身份基准缩放”出发。

```csharp
float healthRatio = currentHealth / maxHealth;
float damageScale = Mathf.Lerp(minScale, 1f, healthRatio);
float identityScale = isElite ? ELITE_SCALE_MULTIPLIER : 1f;
transform.localScale = originalScale * identityScale * damageScale;
```

精英、Boss、变异、冻结、濒死变小都属于缩放层。不要让其中一层在受击时覆盖其它层。

## 低频 UI 表现可以直接 Instantiate，不必硬套对象池

对象池适合高频对象：伤害飘字、敌人死亡特效、金币飞行、子弹。波间无人机奖励飘字每波只出现 1-2 次，如果为了统一而接入复杂对象池，反而会引入类型回收、池状态、Prefab 映射错误等 Bug。

| 场景 | 推荐 |
| --- | --- |
| 敌人伤害数字 | 对象池，上限和优先级回收 |
| 金币/经验飞行 | 对象池 |
| 每波一次的无人机奖励文字 | 直接 Instantiate，动画完 Destroy |
| 结算面板动画 | 直接常驻或按需创建 |

工程化不是所有东西都池化，而是让复杂度和触发频率匹配。

## 非 EnemyBlob 也在 Enemy Layer 时，激光伤害不能只查 EnemyBlob

战术箱/无人机如果放在 `Enemy` Layer，激光物理检测能命中，但如果代码只执行 `GetComponentInParent<EnemyBlob>()`，就会因为对象不是普通怪而跳过伤害。

更稳的结构：

1. 激光检测 Layer 只负责“是否可能被激光打到”。
2. 伤害接收走 `IDamageable`、`ILaserDamageReceiver` 或明确的分支。
3. 普通怪、Boss 弱点、投射物、战术箱各自实现自己的受伤逻辑。

Layer 是粗筛，不是类型系统。战斗里凡是“可被激光打爆”的对象，都要有统一的伤害入口或清晰的分支。

## 结算、三选一和暂停界面要用同一套暂停语义

结算面板弹出时，如果怪物继续动、激光还能扫、经验球继续飞、闪红继续播，就说明只是显示了 UI，没有暂停玩法。统一规则应当是：

| 系统 | 暂停后行为 |
| --- | --- |
| WaveManager / Enemy / 物理移动 | 停止 |
| 玩家输入 / 激光旋转射击 | 停止 |
| 战斗飘字 / 经验球 / 屏幕受击特效 | 停止或冻结 |
| 结算面板、按钮、面板动画 | 使用 unscaled time 继续播放 |

不要让每个面板自己猜要不要 `Time.timeScale = 0`。更稳的是统一的游戏状态或暂停服务：技能三选一、暂停菜单、胜利/失败结算都走同一套入口。

## 来源: `10_流水/历史聊天/Claude_光与朽程序_2025-11-04.md` · 提取日期 2026-05-22

## WaveManager 的阶段推进不要被刷怪开关阻断

`isSpawning` 只应该控制“是否生成敌人”，不应该控制“是否更新当前阶段”。如果在 `Update()` 开头写 `if (!isSpawning) return;`，进入休息期后阶段切换也会停止，永远无法从 `Rest1` 走到 `Variation`。

正确拆分：

1. `GameManager.IsPlaying` 这类全局状态可以阻断整个 Update。
2. `UpdateCurrentPhase(gameTime)` 始终执行。
3. `isSpawning` 只包住 `ProcessSpawning()`。
4. 每个阶段开始时根据 `phase.enableSpawning` 重新设置 `isSpawning`。

阶段状态机和刷怪执行器是两层。休息期暂停刷怪，但时间和阶段仍然要前进。

## Screen Space Overlay 的飘字坐标要走 Canvas 坐标转换

在 `Screen Space - Overlay` 且 `Canvas Scaler = Scale With Screen Size` 时，直接把 `WorldToScreenPoint` 的像素坐标塞给 `RectTransform.position` 很容易错位。飘字系统要把世界坐标转换到目标 Canvas 的局部坐标。

同时，UI 管理器初始化要避开时序坑：

| 风险 | 处理 |
| --- | --- |
| `Awake` 时找不到 Canvas | 手动拖引用，或 `Start`/等待一帧后初始化 |
| 预热日志成功但容器为空 | 在创建实例前打印 prefab、container、parent |
| 多实例或旧脚本没编译 | 输出实例 ID / 初始化标记，必要时重开 Unity |
| Prefab 组件缺失 | 检查 `TextMeshProUGUI`、`CanvasGroup`、业务脚本 |

UI 对象池调试要先证明“容器存在、实例真的生成、坐标真的转换”，再追动画。

## 激光和粒子颜色应由技能数据驱动，并同步材质参数

如果 `SkillData.skillColor` 已经配置了颜色，就不要在 `SkillEffectManager` 中再硬编码 Focus 红、Frost 蓝。激光主材质、StartVFX、EndVFX、子粒子材质都应该从同一技能颜色策略读取。

对于通过材质自发光控制的粒子，常见做法是写一个轻量同步组件：

| 对象 | 同步字段 |
| --- | --- |
| 主激光材质 | `_Color` 或项目实际使用的颜色字段 |
| StartVFX / EndVFX 子粒子材质 | `_EmissionColor` |
| 运行时实例 | 优先用实例材质或 `MaterialPropertyBlock`，避免全局改共享材质 |

颜色规则也要有优先级：Focus 可以让主激光变红；Frost 如果不想覆盖主激光，就只把命中/喷射 VFX 改成蓝色。颜色不是装饰，它在告诉玩家当前 Build 的主要机制。

## 来源: `10_流水/历史聊天/ChatGPT_美妆叠叠乐美术_2025-10-12.md` · 提取日期 2026-05-22

## Unity 中文 UI 先建 TMP 字体资产，不要用默认字体硬顶

美妆叠叠乐这类中文休闲游戏，如果按钮、标题和结算界面仍然使用默认字体，整体 UI 会显得像临时 Demo。工程上要先把可商用中文字体接入 TextMeshPro，再谈描边、渐变和按钮质感。

推荐流程：

1. 选可商用中文字体，例如思源黑体、阿里巴巴普惠体，标题可另配更圆润的展示字体。
2. 将 `.ttf` / `.otf` 导入 Unity。
3. 打开 `Window > TextMeshPro > Font Asset Creator` 生成 TMP Font Asset。
4. 中文字库建议 `Atlas Population Mode = Dynamic`，避免一次性塞入过大字符集。
5. UI 文本、标题、数字可以拆不同字体资产，减少图集膨胀。
6. 把 TMP Font Asset 写入项目级 UI 规范，不要每个 Prefab 自己随手选字体。

字体是 UI 风格的一部分，不是最后替换的文字素材。尤其微信小游戏压缩后，过细字体和弱描边会明显损害可读性。

## 来源: `10_流水/2026-05/2026-05-21.md` · 提取日期 2026-05-23

## 轻量花园经营优先用 2D / 2.5D，不要默认上 3D

荒废花园这类治愈种植经营，如果核心不是“旋转欣赏 3D 花园、自由摆放、拍照分享”，就不应默认进入 3D。对单人开发、微信小游戏包体和 7 天灰盒来说，2D / 2.5D 更稳。

推荐技术路线：

| 阶段 | 技术选择 | 目标 |
| --- | --- | --- |
| 7 天灰盒 | Unity 2D 网格 + 占位 Sprite | 验证播种、成长、收获、卖花/复苏选择 |
| 1 个月 Demo | 2D / 2.5D 斜俯视，Tilemap 或分层 Sprite | 让花园复苏过程可读、可扩地块 |
| 表现增强 | Spine / 简单骨骼、2D 粒子、光晕、水波、开花动画 | 强化治愈反馈，不改变工程量级 |

不建议一开始做完整 3D：

- 美术和动画成本会上升。
- AI 生成 2D 资产更容易快速出风格。
- 微信小游戏包体、性能和加载压力更可控。
- 地块扩展、花朵阶段、建筑状态用 2D 更容易迭代。

只有当 3D 本身就是核心卖点时，才考虑上 3D。否则，技术路线要服务验证速度，而不是服务“看起来更像完整游戏”的心理安全感。

## 来源: `10_流水/历史聊天/Claude_美妆叠叠乐程序_2025-09-01.md` · 提取日期 2026-05-23

## 休闲关卡项目先拆清编辑器、运行时和共享数据

美妆叠叠乐这类 Unity 休闲关卡项目，编辑器工具和运行时不要混在同一层。没有 Editor 依赖的数据类可以放进运行时共享命名空间，例如 `MakeupPuzzle.Core`；编辑器窗口、生成器和关卡编辑 UI 保留在 `MakeupPuzzle.Editor`。

运行时最小 Manager 边界可以这样拆：

| Manager | 职责 |
| --- | --- |
| `GameManager` | 全局状态、主菜单/关卡/暂停/结算、进度 |
| `LevelManager` | 读取关卡数据、胜负判断、关卡流程 |
| `InputManager` | 点击检测、Raycast、输入开关 |
| `OrderManager` | 当前订单、锁定订单、完美订单、订单完成反馈 |
| `StackManager` | 堆叠物件、遮挡、点击移除、飞行动画 |
| `TempSlotManager` | 临时槽占用、满槽失败、UI 刷新 |
| `UIManager` | HUD、暂停、胜利、失败面板 |

小体量项目早期用 `Resources` 加载 ScriptableObject 可以接受，但必须缓存。不要在每个物件解析时反复 `Resources.LoadAll<CosmeticItemSO>("Cosmetics")`，200 个物件会变成 200 次同步加载。

## 世界物件飞向 UI 槽位时，先统一坐标和状态语义

如果堆叠化妆品是世界空间 GameObject，而订单包、临时槽是 UI，那么点击链路应固定为：

1. 世界物件被点击后，先确认它是当前可点击状态。
2. 将目标 UI 槽位转换为世界坐标或屏幕坐标下的可飞行终点。
3. 物件飞向对应槽位，抵达后隐藏或销毁世界物件。
4. UI 槽位显示对应 Sprite，并由订单/临时槽系统接管后续状态。

这里最容易出错的是“飞向订单包根节点”而不是“飞向具体空槽位”。订单 UI 应返回具体 slot `Transform`，否则自动匹配时会出现物件飞到包中心、槽位数据没填或视觉和数据不同步。

## 遮挡点击要用面积阈值和事件更新，不要每帧全量碰撞

叠放玩法的可点击判断不能只看中心点距离。更稳的是用 collider/bounds 重叠面积：如果某物件被更高 `sortingOrder` 的物件覆盖超过约 10%，就标为不可点击；覆盖很少则仍允许点击。

性能边界也要一开始说清楚：

| 做法 | 原因 |
| --- | --- |
| 初始生成后计算一次遮挡状态 | 静态堆叠不需要每帧判断 |
| 每次移除物件后只更新受影响区域 | 避免点击后全场 `IsTouching()` 暴涨 |
| 可点击、被遮挡、已入包用材质/饱和度状态表达 | 玩家不需要试错点击 |
| `OnMouseDown` 项目要让 z-depth 和 `sortingOrder` 对齐，或改用受控 Raycast | 避免点到下层 collider |

如果点击后 FPS 从流畅掉到个位数，优先排查两个点：是否每次点击后全量碰撞检查，是否每个物件都在同步 `Resources.LoadAll`。

## 来源: `10_流水/历史聊天/Claude_美妆叠叠乐程序_2025-09-02.md` · 提取日期 2026-05-23

## 跨场景 Manager 不要持有场景 UI 引用

把父节点或 Manager 设成 `DontDestroyOnLoad` 后，最常见的问题是它还拖着旧场景的 UI 引用。美妆叠叠乐的 MainMenu 经验可以抽成一条规则：持久 Manager 只保留数据、流程和状态；具体按钮、面板、ScrollView、奖励弹窗交给当前场景自己的 `MainMenuUI` / `GameLevelUI`。

跨场景打开面板也不要直接拿旧引用。可用流程是：

1. GameLevel 中设置 `GameManager.ShouldOpenCollectionHall = true`。
2. 切回 MainMenu。
3. MainMenu 场景 UI 初始化完成后读取 flag。
4. 打开收藏馆面板，并立刻清掉 flag。

奖励面板可以复用一个 `ReceiveAwardPanelUI`，但入口要分清：奖杯奖励、抽到化妆品、礼盒完成送道具都可以走不同 `Show(...)` 方法。复用面板的底线是每次关闭都清理 `CanvasGroup`、遮罩、raycast blocker、回调和当前奖励数据，否则下一次打开会出现“画面关闭了但按钮点不动”的幽灵状态。

## 来源: `10_流水/历史聊天/Google_美妆叠叠乐策划_2025-09-02.md` · 提取日期 2026-05-23

## GDD 转工程前先落成三层数据结构

美妆叠叠乐这类关卡益智游戏，策划案不能直接变成散落的 MonoBehaviour 字段。先把数据模型拆清，后面编辑器和运行时才能共用。

| 数据层 | 作用 | 关键字段 |
| --- | --- | --- |
| `CosmeticType` / `CosmeticItemSO` | 定义化妆品模板 | ID、名称、类型、品牌、色系、稀有度、icon、prefab |
| `CosmeticInstance` | 定义关卡内的一个物件 | 模板引用、世界坐标、层级 / sortingOrder、旋转、是否奖杯 |
| `OrderBagData` | 定义一个订单包 | 槽位列表、订单类型、是否完美订单、是否奖杯订单、对应 UI prefab |
| `LevelDataSO` | 定义完整关卡 | 关卡名、全部订单包、全部物件实例、临时槽数量、关卡类型 |

这层结构的意义是让编辑器和运行时读同一份事实。编辑器负责生成和校验，运行时负责加载和表现；不要让运行时再“猜”订单、奖杯、层级或品类。

## 来源: `10_流水/美妆叠叠乐项目/程序AI对话.md` · 提取日期 2026-05-23

## 关卡编辑器要区分“配置资产、UI 预览、世界物件”

美妆叠叠乐的关卡编辑器踩过一个典型混乱点：化妆品库是 UI 列表，堆叠区是世界空间 GameObject，订单包是 UI prefab。三者不能共用同一个 prefab 概念。

| 对象 | 推荐形态 |
| --- | --- |
| 化妆品配置 | `Resources/Cosmetics/*.asset`，数据里引用 icon 和 world prefab |
| 化妆品库预览 | UI item prefab，显示 `Image` 和 `TextMeshProUGUI` 名称 |
| 堆叠区物件 | GameObject + `SpriteRenderer` + `PolygonCollider2D` + kinematic `Rigidbody2D` |
| 订单包 | UI prefab，例如 `OrderBagX2Item` / `OrderBagX3Item`，槽位节点命名 `Slot_0`、`Slot_1` |
| 关卡数据 | `Resources/LevelData/*.asset`，记录订单、物件实例、层级和位置 |

如果预览区所有物件都显示同一张 prefab 图片，说明 UI item 没有从 `CosmeticItemSO` 写入 icon / name；如果堆叠区拖拽生成不了物件，优先检查 UI 坐标到世界坐标的转换和 world prefab 引用，而不是把 Sprite2D 改成 UI Image。

## 编辑器生成算法要先保证订单和物件严格一致

关卡编辑器生成订单和化妆品时，最重要的校验是：堆叠区物件总数必须等于所有订单包槽位总和。若组合无法整除，不要自动调整，应提示用户修改总数量或订单组合，避免生成出策划解释不了的关卡。

可复用生成流程：

1. 在订单区输入 X2/X3/X4/X5 包数量或比例，选择普通关、品牌关、颜色关、类型关。
2. 生成订单包，混合包、奖杯包作为显式选项，不靠隐藏概率。
3. 堆叠区生成前检查订单区不为空。
4. 按订单反向生成需要的化妆品实例，层级由解法顺序决定。
5. 生成完成后立即刷新遮挡状态和灰态。

算法参数不要全堆在主界面。`ClusterDensity`、局部难度、订单调整等高级参数适合放进“算法设置”弹窗；常用操作留在主面板，减少编辑器自身的认知负担。

## 透明边缘会污染遮挡判定，优先用 Sprite 形状而非 Renderer 大包围盒

如果化妆品明明没有被挡却被置灰，且 collider 显示正常，一个常见原因是 Sprite 图片有大量透明边缘，代码却用了 `renderer.bounds` 这类大矩形包围盒参与遮挡判断。叠放类物件应优先使用 `Sprite.bounds`、`PolygonCollider2D` 或自定义碰撞形状，让遮挡判断贴近可见轮廓。

运行时刷新策略：

| 场景 | 刷新方式 |
| --- | --- |
| 关卡初始化 | 所有物件生成后强制刷新一次物理状态，再统一检测 |
| 玩家移除物件 | `StackManager.RemoveItem()` 后通知剩余物件重算 |
| 编辑器拖拽 | 拖拽中可实时更新，方便设计师观察灰态 |
| 编辑器层级修改 / 加载关卡 | 立即重算，保证可视结果和保存数据一致 |

编辑器为了操作方便，可以允许拖动被遮挡物；运行时则必须禁止点击被遮挡物。这两套规则要共享检测逻辑，但不要共享交互限制。

## 来源: `10_流水/2026-05/2026-05-27.md` · 提取日期 2026-05-29

## URP 屏幕空间描边要先把深度、法线、Mask 和移动端降级讲清楚

URP 里做角色描边，较稳的结构是自定义 `ScriptableRendererFeature` + `ScriptableRenderPass`，不要只说“后处理描边”。实现前先拆五件事：

| 模块 | 推荐做法 | 易错点 |
| --- | --- | --- |
| 深度 | 开启 URP Depth Texture，Shader 中采 `_CameraDepthTexture` 并转 linear depth | 直接把深度塞普通 `ARGB32` 的 A 通道，远处精度容易丢 |
| 法线 | 使用 DepthNormals Pass，或额外渲一张 view space normals RT | 法线/深度合并时必须说明 RT 格式和精度 |
| 边缘检测 | 对深度和法线做 3x3 Sobel / 梯度检测，再合成 edge | Sobel 不是“周围 8 点平均”，而是卷积核算梯度 |
| 只描角色 | LayerMask、Rendering Layer、Stencil 或 Mask RT | 不建议靠 Tag / ShaderName 做长期维护 |
| 插入时机 | `AfterRenderingOpaques` 或 `BeforeRenderingPostProcessing`，最后叠加到 color target，UI 前 | 透明物体是否参与描边要单独定义 |

远近描边稳定性不能只靠固定采样半径。深度是非线性的，应先 linearize depth，再根据 linear depth 调整深度阈值，必要时调整采样半径或边缘阈值，避免近处细、远处粗或闪烁。

移动端优化优先级：

1. 低端机只开深度描边，关闭法线描边。
2. 3x3 采样降为 4-tap 或 cross pattern。
3. 法线 / Mask RT 使用半分辨率。
4. 只为角色层渲染 normals / mask，减少额外绘制。
5. 明确质量开关，不让 Bloom、Outline、透明特效和角色 Shader 同时抢满 GPU。

面试或技术方案里最该表达的是：描边不只是一个 Shader，而是一套 Render Pass、RT 精度、对象筛选、插入时机和移动端质量分级的系统。

## 来源: `10_流水/Openclaw知识库文件/technical_design_guide.md` · 提取日期 2026-05-30

## 技术策划交付物要让设计变成可验证工程资产

技术策划的价值不是多写一份需求，而是把抽象设计翻译成配置、原型、工具和边界条件，让程序和策划都少猜。

可复用交付物：

| 交付物 | 作用 |
| --- | --- |
| 配置表 + 校验脚本 | 自动检查字段、范围、引用、重复 ID 和导出格式 |
| Unity Editor 面板 | 让关卡、技能、敌人或奖励配置可视化 |
| 白盒原型 | 在进正式开发前验证输入、反馈、边缘情况 |
| 状态机 / 流程图 | 把动画、技能、UI 和战斗状态拆成清晰入口 |
| 表现挂点表 | 明确 VFX/SFX/震屏/卡肉出现的帧和触发条件 |

写需求时要多写“不可接受状态”：例如配置缺字段时是否报错、找不到 Prefab 是否降级、对象池满了是否丢弃、广告失败是否走等待恢复。边界比理想流程更能减少返工。

## 来源: `10_流水/Openclaw知识库文件/unity_core_modules.md` · 提取日期 2026-05-30

## 激光视觉、碰撞和物理反馈要共用同一套事实

LineRenderer 只负责视觉，BoxCollider2D / Raycast / OverlapBox 才负责命中。激光、射线或长条攻击最常见的问题，是视觉长度、碰撞长度和旋转角度各算各的。

同步规则：

| 项 | 做法 |
| --- | --- |
| 长度 | 用首尾点距离计算碰撞盒长度 |
| 中心 | 碰撞盒放在首尾点中点 |
| 旋转 | 用 `Atan2` 计算方向角 |
| 层级 | Collider 可作为子物体，避免影响 LineRenderer 世界坐标 |
| 测试 | Debug 同时画视觉线和命中盒，不只看 Inspector 数字 |

Rigidbody2D 击退也要看质量、ForceMode、Drag 和摩擦。普通击退适合 `Impulse`，持续场力适合 `Force`；如果不同敌人的质量差异很大，应明确击退是否要除以质量，避免坦克怪完全不动或小怪飞出屏幕。

对象池部分要守住生命周期：

| 风险 | 规则 |
| --- | --- |
| VFX 播完没回池 | 监听 `ParticleSystem.IsAlive()` 或按 duration 回收 |
| 父物体回池隐藏子特效 | 回池前解绑或归还子特效 |
| 旧状态残留 | 每次取出必须走 `Init()`，回收前走 `Reset()` |
| 粒子 / Trail / LineRenderer 残留 | 回池时显式 Stop、Clear、重置可见状态 |

对象池不是只管 active。它管理的是“这次使用结束后，下次拿出来像全新对象一样干净”。
