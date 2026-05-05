# 游戏代码模板库 INDEX

> **创建日期**：2026-03-15
> **来源工程**：MZ02（美妆叠叠乐）+ LightVSDecay（光与朽，2026-04 起补充）
> **维护人**：小龙儿
> **决策伴侣**：[[40_知识/02_引擎与技术/Unity通用技术栈复用指南|Unity 通用技术栈复用指南]] — 回答"什么时候抄哪个、为什么这么选、踩过什么坑"

---

## 模板清单

| 编号  | 文件                                          | 模板名称                                                                | 适用场景                          | 复用价值  |
| --- | ------------------------------------------- | ------------------------------------------------------------------- | ----------------------------- | ----- |
| 01  | `01_GameLogger.md`                          | 全局日志管理器                                                             | 所有项目                          | ⭐⭐⭐⭐⭐ |
| 02  | `02_SingletonPattern.md`                    | 单例模式基类（普通+持久化）                                                      | 所有项目                          | ⭐⭐⭐⭐⭐ |
| 03  | `03_SafeSceneLoader.md`                     | 安全异步场景加载器                                                           | WebGL/微信小游戏                   | ⭐⭐⭐⭐⭐ |
| 04  | `04_AudioManager.md`                        | 音频管理器（简化版）                                                          | 小型项目（≤5 音效）                   | ⭐⭐⭐⭐⭐ |
| 05  | `05_SaveManager.md`                         | 存档管理器（本地+服务器双保险）                                                    | 有用户系统的项目                      | ⭐⭐⭐⭐☆ |
| 06  | `06_WXAdsManager.md`                        | **微信激励视频广告管理器（成熟版）** — 6 广告位 + 频控 + 预加载 + 超时兜底                       | 微信小游戏 IAA 变现                  | ⭐⭐⭐⭐⭐ |
| 07  | `07_UIAnimationHelper.md`                   | UI 动画工具（Scale Punch + Rolling Number + 资源栏整体 Q 弹）                   | 所有有属性数值反馈的项目                  | ⭐⭐⭐⭐⭐ |
| 08  | `UGUI挖孔遮罩/README.md`                        | SDF Shader 挖孔遮罩 + TutorialDirector 接入方案                             | 有新手引导的项目                      | ⭐⭐⭐⭐⭐ |
| 09  | `09_CoinFlyAnimation.md`                    | **战斗金币掉落动画** — 散落 + 悬浮 + 贝塞尔吸附 + 视觉数压缩 + GameEvents 驱动              | 战斗类掉落奖励                       | ⭐⭐⭐⭐⭐ |
| 10  | `10_FloatingTextSystem.md`                  | 战斗飘字系统（多类型 + 对象池 + 优先级回收）                                           | 战斗 / 拾取 / 状态文字                | ⭐⭐⭐⭐⭐ |
| 11  | `11_GameEvents.md`                          | **全局事件总线** — 静态 event + Trigger 方法对、ClearAllEvents 防 leak           | 任何中型以上项目（Manager 解耦）          | ⭐⭐⭐⭐⭐ |
| 12  | `12_AnalyticsManager.md`                    | **微信场景分析埋点** — 三种语义级别 + 首战漏斗封装 + Editor 日志                          | 微信小游戏数据埋点                     | ⭐⭐⭐⭐⭐ |
| 13  | `13_AudioManagerPro.md`                     | **AudioManager Pro** — 多 AudioSource + AudioConfig SO + 自动场景 BGM + 冷却防抖 | 中等以上完整项目                      | ⭐⭐⭐⭐⭐ |
| 14  | `14_ProgressManager_CurrencyTopBar.md`      | **局外资源 + 顶部栏** — 多资源 + 体力恢复 + 离线补算 + 看广告 + 事件驱动 UI 刷新               | 有局外资源 / 体力 / 局内外区分的项目         | ⭐⭐⭐⭐⭐ |

---

## 使用说明

### 新项目启动按规模选包

| 项目规模 | 必抄模板 |
|---------|---------|
| **小 Demo（无场景切换、≤5 音效、单关卡）** | 01 + 02 + 04 |
| **中等休闲游戏（多场景、完整 UI）** | 01 + 02 + 03 + 11 + 13 + 07 |
| **微信小游戏 + IAA 变现** | 上一档 + 06 + 12 |
| **战斗 / 关卡 / Roguelite 类** | 上一档 + 09 + 10 + 14 |
| **有云存档 / 跨设备** | 上一档 + 05 |
| **有新手引导** | 上一档 + 08 |

### 标准爽感链路（休闲反馈三件套）

**07 + 09 + 10** 配合，构成"金币掉落 → 飞向资源栏 → 资源栏抖一下 → 数字滚动 → 飘字 +1"的标准爽感链路。具体接线见各文档"与其他模板的关系"小节。

### 标准上线打包（微信小游戏完整链路）

**01 GameLogger（关日志）+ 03 SafeSceneLoader（防崩）+ 06 WXAdsManager（变现）+ 12 AnalyticsManager（埋点）+ 13 AudioManagerPro（音频）+ 14 ProgressManager（资源）**——这 6 条是任何要上微信小游戏的项目都必须有的工程基础设施。

---

## 休闲小游戏共通模板候选（双工程扫描，2026-04-29 更新）

### ✅ P0 — 已完成（2026-04-29）

| 模板 | 编号 |
|------|------|
| GameEvents 全局事件总线 | 11 |
| AudioManager Pro（ScriptableObject 配置） | 13 |
| ProgressManager + 资源栏控制器 | 14 |
| AnalyticsManager（场景分析埋点） | 12 |

### P1 — 单项目已成熟，等下一款验证后再提

| 候选模板 | 来源 | 等待信号 |
|---------|------|---------|
| HapticFeedback 振动反馈 | LightVSDecay `Core/HapticFeedback.cs` + `Core/BattleHapticController.cs` | 下一款是动作 / 物理 satisfying 类型时直接抄 |
| CameraShake 屏幕震动 | LightVSDecay `Core/CameraShake.cs` | 同上 |
| Pool 通用对象池基类 | LightVSDecay `Core/Pool/` | 下一款有大量重复实例化（怪物 / 子弹 / 道具）时 |
| ButtonScaleEffect 按钮按压反馈 | BeautyStacking `Runtime/UI/ButtonScaleEffect.cs` + `ButtonSoundPlayer.cs` | 任何项目都通用，但目前两边实现不一，等下一款统一标准 |
| AutoPersistentSingleton 自动跨场景单例 | LightVSDecay `Core/AutoPersistentSingleton.cs` | 与 02 简单 Singleton 互补，下一款验证后并入 02 |
| WaveManager 波次管理器 | LightVSDecay `Logic/WaveManager.cs` | 下一款也是关卡 / 波次型时再提 |
| SkillSystem 三选一 + ScriptableObject 架构 | LightVSDecay | Roguelite 类项目才用 |
| DifficultyManager 难度管理器 | LightVSDecay `Logic/DifficultyManager.cs` | 多章节 / 多难度项目才用 |
| ScreenBoundaryManager 屏幕边界管理 | LightVSDecay `Core/ScreenBoundaryManager.cs` | 自由移动类项目（下一款是消除/收纳就用不到） |
| FpsDisplay 性能 HUD | LightVSDecay `Debug/FpsDisplay.cs` | 任何上线前调试期都需要 |

### P2 — 项目特定，不应作为通用模板

- **LightVSDecay 专属**：DroneReward / TacticalDrop / Equipment / TechTree / WaveConfig / FloatingText 的元素特化（Shatter / Chain / Explosion）——绑定 Roguelite 塔防玩法
- **BeautyStacking 专属**：Stack / TempSlot / OrderBag / Brand / Collection / SwitchToggle / OrderBagSkin——绑定叠放/收纳玩法

---

## 最近更新

- **2026-04-29（下午）**：
  - 09 CoinFlyAnimation 重写——纠正来源（实际是光与朽 `Logic/Coin/CoinPickup + Spawner` 的成熟三阶段动画，不是美妆叠叠乐的简单飞行）；旧版极简版作为附录保留
  - **新增 P0 4 条**：11 GameEvents、12 AnalyticsManager、13 AudioManagerPro、14 ProgressManager+顶部栏
  - "休闲小游戏共通模板候选"P0 段标注为已完成
- **2026-04-29（上午）**：
  - 06 WXAdsManager 升级为成熟版（光与朽 6 广告位 + 频控 + 预加载 + 超时；旧简化版作为附录保留）
  - 新增 10 FloatingTextSystem（光与朽 UI/FloatingText/ 全套）
  - 07 UIAnimationHelper 补"资源栏整体 Q 弹"用例 + 与 09 / 10 协同标准链路
- **2026-04-14**：新增 07 UIAnimationHelper、08 UGUI 挖孔遮罩。均来源于 LightVSDecay 新手引导系统重构。
