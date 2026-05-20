---
type: knowledge
status: review
created: 2026-04-29
source_book: 代码模板库 14 个模板（MZ02 + LightVSDecay 双项目提取）
source_page: 40_知识/02_引擎与技术/代码模板库/00_INDEX.md; 01_GameLogger.md; 02_SingletonPattern.md; 03_SafeSceneLoader.md; 04_AudioManager.md; 05_SaveManager.md; 06_WXAdsManager.md; 07_UIAnimationHelper.md; UGUI挖孔遮罩/README.md; 09_CoinFlyAnimation.md; 10_FloatingTextSystem.md; 11_GameEvents.md; 12_AnalyticsManager.md; 13_AudioManagerPro.md; 14_ProgressManager_CurrencyTopBar.md
domain: 02_引擎与技术
tags: [Unity, 微信小游戏, 单例, 场景加载, 音频, 存档, 广告, UI动效, 飘字, 事件总线, 埋点, 资源管理, 工程化, 决策指南]
updated: 2026-05-19
last_reviewed: 2026-05-19
review_count: 8
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
