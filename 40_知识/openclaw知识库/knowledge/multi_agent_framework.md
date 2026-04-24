# Multi-Agent Framework in Game Development

## 核心框架对比
在多 Agent 协同（如主控Agent、策划Agent、程序Agent协作）开发游戏的场景中，有以下主流框架：

1. **CrewAI**:
   - 核心优势：强调 **角色分配(Role Assignment)** 和结构化团队合作。
   - 适用场景：非常适合明确分工的游戏开发流程（如：策划写设定 -> 程序写代码 -> 测试测Bug）。
   - 特点：易于上手，支持复杂工作流管理。

2. **AutoGen** (Microsoft):
   - 核心优势：强调 **对话式协作(Conversation)**。
   - 适用场景：适合需要大量反复沟通、头脑风暴的设计阶段。
   - 特点：具有 AutoGen Studio 无代码/低代码 UI 界面，方便进行 Agent 工作流的快速原型构建和测试。

3. **LangGraph**:
   - 核心优势：强调 **工作流结构(Workflow Structure)**。
   - 适用场景：适合底层逻辑极其复杂、需要精确追踪每一步状态机的多分支代码开发流程。

## 游戏开发协同工作流 (Prompt & Automation)
- **Prompt 边界划分**：
  必须给每个 Agent 设定清晰的“System Prompt”边界。例如，策划Agent只负责输出 JSON 格式的数值表；程序Agent只负责解析 JSON 并生成 C# 脚本。避免职责重叠导致死循环。
- **自动化测试验证闭环**：
  Agent 生成的代码需要沙盒环境。闭环设计为：程序Agent产出代码 -> 自动化测试Agent（或真实 Unity 测试环境）运行 -> 报错输出 Log -> Log 传回程序Agent -> 自动修改。

## 结论与后续应用
对于《光与朽》后续迭代和新项目，推荐使用 **CrewAI** 进行模块化流水线搭建，或者结合 **LangGraph** 实现精准的状态机控制，打造“自动化游戏开发车间”。
