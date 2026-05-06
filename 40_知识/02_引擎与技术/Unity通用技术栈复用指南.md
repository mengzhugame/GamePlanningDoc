---
type: knowledge
status: review
created: 2026-04-29
source_book: 代码模板库 14 个模板（MZ02 + LightVSDecay 双项目提取）
source_page: 40_知识/02_引擎与技术/代码模板库/00_INDEX.md; 01_GameLogger.md; 02_SingletonPattern.md; 03_SafeSceneLoader.md; 04_AudioManager.md; 05_SaveManager.md; 06_WXAdsManager.md; 07_UIAnimationHelper.md; UGUI挖孔遮罩/README.md; 09_CoinFlyAnimation.md; 10_FloatingTextSystem.md; 11_GameEvents.md; 12_AnalyticsManager.md; 13_AudioManagerPro.md; 14_ProgressManager_CurrencyTopBar.md
domain: 02_引擎与技术
tags: [Unity, 微信小游戏, 单例, 场景加载, 音频, 存档, 广告, UI动效, 飘字, 事件总线, 埋点, 资源管理, 工程化, 决策指南]
last_reviewed: 2026-04-29
review_count: 2
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
