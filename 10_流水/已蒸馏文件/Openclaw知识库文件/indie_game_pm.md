# 独立游戏项目管理与敏捷工作流 (Indie Game PM)

## 1. 敏捷开发简化版 (Indie Agile/Scrum)
对于独立游戏开发者和小型团队，标准的Scrum通常过于繁重。可以采用简化的敏捷工作流：
- **迭代规划 (Sprint Planning)：** 通常以1-2周为一个冲刺周期。经验表明，在规划会议上，团队一般只能准确估计出60%的实际工时。
- **里程碑驱动 (Milestone Driven)：** 每个大阶段（如垂直切片、Alpha、Beta）包含多个Sprint。重点是每个Sprint结束后都要产出一个“可运行、可展示”的版本，方便随时应对突发的展示需求或发行商路演。
- **工具推荐：** Trello（适合极简任务板）、Codecks（专为游戏开发设计，采用独特的卡牌机制管理Hand/Decks/Milestones）、ClickUp（适合需要资产追踪和自定义工作流的团队）。

## 2. Bug 追踪与分类方法论 (Bug Tracking)
Bug的分类应直接与“是否阻碍游戏推进”挂钩：
- **P0 (Blocker/Critical)：** 游戏崩溃、主流程卡死、存档丢失、严重的数据异常。必须立即停下手中的功能开发进行修复，否则测试无法继续。
- **P1 (High/Major)：** 核心玩法体验受损、明显的表现错误（如材质丢失、UI错位严重）、必现的功能失效。应当在当前Sprint内或里程碑交付前解决。
- **P2 (Normal/Minor)：** 偶现的小瑕疵、边缘UI错误、轻微的音效缺失。可以在专门的“修虫周 (Bug Bash)”或有空余时间时处理。
- **P3 (Low/Trivial/Enhancement)：** 错别字、像素对齐问题、体验优化的建议。优先级最低。

## 3. 发行倒推排期法 (Reverse Milestone Planning)
从预计的发行日期（例如《光与朽》的4月上线）开始往回倒推规划：
- **T-0 (Launch)：** 游戏发售日。
- **T-2周：** 锁定版本 (Code Freeze)，只修复P0/P1级别的Bug，严禁加入任何新功能，准备营销物料和Steam/平台过审。
- **T-1月 (Release Candidate)：** 内容开发完成 (Content Complete)，开始全量测试、性能优化和本地化实装。
- **T-2月 (Beta)：** 核心循环和所有主要关卡可玩，开始招募外部玩家进行Playtest，重点收集“爽感”和数值平衡的反馈。
- **T-3月 (Alpha)：** 游戏骨架完成，美术资产大批量铺开。

这种方法的核心在于：**Scope evolves intentionally, not accidentally.** (范围的扩展应该是故意的，而不是意外的)。在逼近上线时，学会砍需求(Cut features)比增加新点子更重要。