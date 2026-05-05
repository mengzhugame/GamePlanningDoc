# 知识库目录索引

**最后更新**: 2026-05-01
**用途**: 快速查找已积累的知识,支持智能问答

---

## 🎮 游戏开发知识

### 项目分析
- **guangyuxiu_analysis.md** - 《光与朽》完整项目分析(核心玩法、技术架构、开发进度)
- **guangyuxiu_complete.md** - 《光与朽》详细设计文档(Boss设计、技能系统、数值规划)
- **meizhuang_analysis.md** - 《美妆叠叠乐》项目复盘(失败经验、发行被拒分析)
- **game_projects_summary.md** - 所有游戏项目总览和状态

### 游戏设计
- **game_design.md** - 游戏设计基础理论和方法论
- **game_ideas_treasure.md** - 50+游戏创意宝库(分类整理)
- **game_balance_numerical_design.md** - 游戏数值平衡设计指南

### 游戏拆解与关键词
- **game_daily_analysis.md** - 每日游戏拆解记录（旋转刀刀刀、羊了个羊、打个螺丝）
- **game_keywords_library.md** - 游戏玩法关键词库（动词库、类型标签、组合公式）
- **game_breakdowns/rogue_tower.md** - 《Rogue Tower》拆解（动态扩路、卡牌构筑、三段防御克制、多路压力）
- **game_breakdowns/blue_prince.md** - 《Blue Prince》拆解（房间起草、路径塑形、知识型Roguelite、多线程解谜）
- **game_breakdowns/isle_of_arrows.md** - 《Isle of Arrows》拆解（图块放置塔防、扩岛布局、跳牌经济、邻接联动）
- **game_breakdowns/tower_dominion.md** - 《Tower Dominion》拆解（英雄化塔防、地形造卡口、阵营差异、Doctrine构筑）
- **game_breakdowns/lucky_defense.md** - 《Lucky Defense》拆解（合作塔防、随机召唤、三合一升阶、盘面回收）
- **game_breakdowns/thronefall.md** - 《Thronefall》拆解（昼夜守城、固定点位取舍、英雄参战、强广告可读性）
- **game_breakdowns/mob_control.md** - 《Mob Control》拆解（倍增门人海爽感、超休闲→混合休闲进化、广告替代货币）
- **game_breakdowns/all_in_hole.md** - 《All in Hole》拆解（吞噬成长解压、体积断点谜题、轻Meta长线化）

### 代码模板
- **code_templates/** - Unity代码模板库
  - Core/(单例、对象池、事件系统)
  - Managers/(游戏管理器、音频、UI、存档)
  - Data/(配置模板)
  - UI/(面板模板、结算、飘字)
  - VFX/(相机震动、屏幕特效)
  - Utils/(日志模板)
- **code_standards.md** - 代码规范与最佳实践
- **code_analysis_report.md** - 《光与朽》代码分析报告

---

## 🚀 游戏运营与发行

### 发行与运营
- **wechat_minigame_policy_ads.md** - 微信小游戏政策与买量策略
- **user_acquisition_guide.md** - 用户增长与买量实战指南
- **wechat_minigame_market.md** - 微信小游戏市场分析
- **data_analysis_guide.md** - 游戏数据分析指南(留存/LTV/ARPU)
- **marketing.md** - 营销策略与投放技巧
- **post_launch_liveops_guide.md** - 独立游戏上线后长线运营与留存策略(LiveOps框架、活动设计、留存优化)

### 商业化
- **game_balance_numerical_design.md** - IAA/IAP变现设计
- **investment_guide.md** - 投资理财知识(辅线学习)

---

## 👤 个人IP与内容

### 小红书运营
- **personal_ip_roadmap.md** - 小红书个人IP成长规划(发布日历、标题库)
- **learning_tasks.md** - 学习任务清单(优先级排序)

---

## 🤖 AI与工具

### 模型与成本
- **api_cost_optimization.md** - API成本优化方案(SiliconFlow双模型配置)
- **dual_model_config.md** - 双模型配置说明(DeepSeek免费+Kimi付费)

### 问题记录
- **open_issues.md** - 待解决问题清单(Cron消息投递等)

---

## 🏢 公司战略

- **company_roadmap.md** - 公司发展路线图(从个人到工作室到游戏公司)
- **project_analysis.md** - 项目分析方法论

---

## 🔍 使用规则

**当用户提问时,先搜索以下关键词匹配的知识库文件**:

| 问题关键词 | 优先搜索文件 |
|-----------|-------------|
| 发行/上线/买量/投放 | `wechat_minigame_policy_ads.md`, `user_acquisition_guide.md`, `data_analysis_guide.md` |
| 运营/数据/留存/LTV | `data_analysis_guide.md`, `user_acquisition_guide.md`, `post_launch_liveops_guide.md` |
| 上线后/LiveOps/活动/留存 | `post_launch_liveops_guide.md` |
| 游戏设计/玩法/创意 | `game_design.md`, `game_ideas_treasure.md`, `game_keywords_library.md` |
| 美术风格/题材调研/买量美术/视觉趋势 | `art_research/weekly_art_trend_archive.md`, `game_art_learning_roadmap.md` |
| 光与朽/代码/技术 | `guangyuxiu_analysis.md`, `guangyuxiu_complete.md`, `code_templates/` |
| 小红书/IP/内容 | `personal_ip_roadmap.md`, `learning_tasks.md` |
| 微信小游戏/政策 | `wechat_minigame_policy_ads.md`, `wechat_minigame_market.md` |
| 失败/复盘/经验 | `meizhuang_analysis.md`, `game_projects_summary.md` |

**流程**:
1. 识别问题关键词
2. 搜索知识库匹配文件
3. 如知识库内容不足 → 再执行网络搜索
4. 整合回答,标注信息来源

---

*此文件用于快速索引知识库内容,支持智能问答系统*
