# 游戏代码模板库 INDEX

> **创建日期**：2026-03-15
> **来源工程**：MZ02（美妆叠叠乐）
> **维护人**：小龙儿
> **下一步**：光与朽上线后，从 LightVSDecay 工程提取第二批模板

---

## 模板清单

| 编号 | 文件 | 模板名称 | 适用场景 | 复用价值 |
|------|------|---------|---------|---------|
| 01 | `01_GameLogger.md` | 全局日志管理器 | 所有项目 | ⭐⭐⭐⭐⭐ |
| 02 | `02_SingletonPattern.md` | 单例模式基类（普通+持久化） | 所有项目 | ⭐⭐⭐⭐⭐ |
| 03 | `03_SafeSceneLoader.md` | 安全异步场景加载器 | WebGL/微信小游戏 | ⭐⭐⭐⭐⭐ |
| 04 | `04_AudioManager.md` | 音频管理器 | 所有项目 | ⭐⭐⭐⭐⭐ |
| 05 | `05_SaveManager.md` | 存档管理器（本地+服务器双保险） | 有用户系统的项目 | ⭐⭐⭐⭐☆ |
| 06 | `06_WXAdsManager.md` | 微信激励视频广告管理器 | 微信小游戏 IAA 变现 | ⭐⭐⭐⭐⭐ |
| 07 | `07_UIAnimationHelper.md` | UI 动画工具（Scale Punch + Rolling Number） | 所有有属性数值反馈的项目 | ⭐⭐⭐⭐⭐ |
| 08 | `UGUI挖孔遮罩/README.md` | SDF Shader 挖孔遮罩 + TutorialDirector 接入方案 | 有新手引导的项目 | ⭐⭐⭐⭐⭐ |

---

## 使用说明

1. **新项目启动时**：直接复制 01/02/04 三个模板，5分钟内搭好项目基础骨架
2. **微信小游戏项目**：额外加入 03/06 两个模板，解决微信平台特有的坑
3. **有云存档需求的项目**：加入 05 模板，替换 `IServerDataManager` 的实现即可接入不同后端

---

## 待补充（光与朽上线后）

| 模板名称 | 来源 | 优先级 |
|---------|------|-------|
| WaveManager 波次管理器 | LightVSDecay | P0 |
| SkillSystem 技能系统（三选一+ScriptableObject架构） | LightVSDecay | P0 |
| DroneReward 无人机随机奖励系统 | LightVSDecay | P1 |
| DifficultyManager 难度管理器 | LightVSDecay | P1 |
| ScriptableObject数据驱动架构范例 | LightVSDecay | P0 |
| UIManager 状态机面板管理 | MZ02/LVD | P1 |

## 最近更新

- **2026-04-14**：新增 07 UIAnimationHelper（Scale Punch + Rolling Number），新增 08 UGUI挖孔遮罩（含 TutorialDirector 完整接入方案）。均来源于 LightVSDecay 新手引导系统重构。
