# 小龙儿的 Claude Code 专家进阶路线图 (Claude Code Expert Roadmap)

## 0. 基础入门篇 (Beginner)
**目标：掌握 Claude Code 基本操作，能在终端流畅对话。**
1. **基础命令与安装：**
   - 学习如何启动、停止 Claude Code，基本的 CLI 标志 (如 `--print`, `--permission-mode bypassPermissions`)。
   - 学会常用的基础命令：`/help`, `/clear`（频繁清理上下文以节省 token），`/compact`。
2. **对话与上下文管理：**
   - 理解 Claude Code 如何读取当前目录文件。
   - 学习如何使用 `/add` 或直接拖拽文件给 Claude Code。
   - 认识到上下文溢出的风险，学习“单一职责”的提问方式。
3. **初级 Prompt 技巧：**
   - 怎么让 Claude 帮忙写简单的 Python/Node.js 脚本。
   - 学会让它写 README 或者简单的单元测试。

## 1. 进阶应用篇 (Intermediate)
**目标：能够用 Claude Code 完成中小型功能模块的开发，并掌握 Vibe Coding 初级理念。**
1. **系统预设：掌握 `CLAUDE.md` / `.claude`：**
   - 学习编写项目的 `CLAUDE.md`：设定代码规范、启动命令、测试指令。
   - 学习模块化拆分任务，通过 `CLAUDE.md` 教会 Claude 项目的专属上下文。
2. **调试与重构：**
   - 让 Claude Code 运行测试，报错后自动修复。
   - 给定一份旧代码，让 Claude Code 按照 SOLID 原则重构，并附带注释。
3. **模型选择策略：**
   - 了解 Opus, Sonnet, Haiku 的差别与成本。
   - 掌握何时用 Opus (处理极其复杂的长文本重构)，何时用 Sonnet (日常快速迭代)，如何在 Claude Code 中切换模型。

## 2. 高级开发篇 (Advanced)
**目标：主导大型重构，实现跨文件复杂业务逻辑。**
1. **Orchestrator-Workers (包工头-打工人) 模式：**
   - 学习用主 Agent 拆解需求 (Spec 先行)。
   - 使用多个独立的 Claude Code session (并行或串行) 去各自完成子任务。
2. **复杂环境控制与开源生态吸收 (gstack 拆解)：**
   - 深入研究 GitHub 开源项目 `gstack` 的架构设计。
   - 提取并改造其 `/browse`（极速无头浏览器端到端测试）和 `/qa`（代码质检）自动化脚本，融入我们的项目。
   - 让 Claude Code 自动运行 Lint、Format、Test，直到所有流水线通过。
   - 使用终端输出管道传递数据给 Claude Code。
3. **Git 与版本控制协作：**
   - 教会 Claude 编写高质量的 commit message。
   - 让 Claude 阅读 diff，自动生成 Review 意见。

## 3. 资深架构篇 (Senior)
**目标：将 Claude Code 整合进全自动/半自动 DevOps 工作流。**
1. **测试驱动开发 (TDD) 的 Agent 实践：**
   - 要求 Claude 先写失败的测试用例，再写业务代码，最后通过测试。
2. **性能与安全审查：**
   - 用 Claude 扫描全栈代码的安全漏洞 (SQL注入、XSS、提权)。
   - 进行内存泄漏排查、性能瓶颈分析。
3. **跨语言/框架迁移：**
   - 比如把大型 Python 项目自动迁移到 Go，或把 Vue 迁移到 React。
   - 建立一套严密的迁移验证流水线，让 Claude Code 自动跑完。

## 4. 终极专家篇 (Expert)
**目标：超越代码，成为 AI 时代的超级个体/架构师。**
1. **Zero-Shot 复杂应用生成：**
   - 提供一份千字以上的 PRD (产品需求文档) 和 UI 截图。
   - 让 Claude Code 从零开始初始化脚手架、配置数据库、搭建前后端、部署上线。
2. **多 Agent 协同生态构建 (如 OpenClaw 集成)：**
   - 将 Claude Code 封装进类似 OpenClaw 的 `sessions_spawn` 自动化流水线。
   - 动态调整 `CLAUDE.md` 以适应不同时刻的开发上下文。
3. **突破上下文极限的工程学 (Context Engineering)：**
   - 设计“记忆压缩”机制，对于 20 万行以上的代码库，如何教 Claude 精准定位修改点，避免检索幻觉。
   - 自主构建并维护项目知识图谱，作为 Claude 的长期外挂大脑。
