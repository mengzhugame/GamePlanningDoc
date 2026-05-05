# GamePlanningDoc — AI 协作入口（Codex / 通用）

> 你正进入一个已运行的 LifeOS Obsidian vault，不是空白项目。本文件与 `CLAUDE.md` 同源维护，内容同步更新。

---

## 这是什么

**wangyongcheng 的个人游戏制作知识库**。
- 已上线：《美妆叠叠乐》（女性向·叠放休闲）、《光与朽》（动作·Boss 战）
- 平台：微信小游戏 / Unity 技术栈
- 双端协作：Windows（主写作 + 蒸馏）+ Mac（openclaw 情报采集）
- git 远端：`github.com/mengzhugame/GamePlanningDoc`

---

## 目录地图（只记这张表，其余按需读）

| 路径 | 内容 | 何时读 |
|------|------|--------|
| `00_全局地图.md` | 全局地图，主题入口 | 用户问"有什么" / 会话启动 |
| `10_流水/YYYY-MM-DD.md` | 今日 append-only 笔记 | 记任何临时想法 |
| `20_项目/NN_项目名项目/` | 单个游戏项目的全部文档 | 在该项目上下文工作 |
| `20_项目/03_概念验证_YYYY-MM/` | 立项前概念验证：真实玩法视频、数据记录、是否进入切片 | 新游戏方向未验证前 |
| `30_openclaw_inbox/` | Mac 端 openclaw 抓的原料（inbox） | 跑 `/ingest-claw` 时 |
| `40_知识/0N_主题/` | 蒸馏后的**永久知识**（权威） | 找方法论 / 复用经验 |
| `40_知识/代码模板库/` | 生产级代码模板 | 写代码时 |
| `AI对话记录/` | 从各平台导出的原始 AI 对话 txt | 跑 `/ingest-chat` 时 |
| `AI角色设定/` | 13+ 角色的提示词库（策划/主程/美术 等） | 切换工作角色时 |
| `90_系统/SYSTEM_PROMPT.md` | **6 条铁律 + Skill Map**（必读） | 首次进入必读 |
| `90_系统/用户画像.md` | **用户深度档案**（谁在用这个库）| 任何需要了解用户背景、偏好、目标时 |
| `90_系统/人生维度/` | 用户画像的分维度扩展（事业、关系、身体等） | 讨论对应人生主题时 |
| `90_系统/schemas/` | 所有 frontmatter 规范 | 新建文件前读对应 schema |
| `90_系统/skills/` | 所有 skill 定义 | 用户调 `/xxx` 时读对应 skill |

---

## 6 条铁律（详情见 `90_系统/SYSTEM_PROMPT.md`）

1. **Skill-Based Execution** — 只在用户调 `/skill-xxx` 时行动，不自由发挥
2. **Read Context First** — 动作前先读相关文件，不凭假设生成
3. **Schema Compliance** — 新建文件必须符合 `90_系统/schemas/` 下对应 schema
4. **Template Compliance** — 结构不要魔改
5. **State Progression** — 文件状态机走对（pending → researched → knowledged / inbox → triaged → distilled）
6. **Read-Back Verification** — 写完文件要读回验证

---

## 可用 Skills（完整列表在 SYSTEM_PROMPT.md · Skill Map）

| 用户输入 | 读这份 skill |
|---------|-------------|
| `/new-project` | `90_系统/skills/skill-new-project.md` |
| `/ingest-chat` | `90_系统/skills/skill-ingest-chat.md` |
| `/ingest-claw` | `90_系统/skills/skill-ingest-claw.md` |
| `/distill` | `90_系统/skills/skill-distill.md` |
| `/iron-rule-check <方向名>` | `90_系统/skills/skill-iron-rule-check.md`(立项铁律拷问)|
| `/knowledge` `/project` `/research` `/review` `/archive` `/start` | 对应同名 `skill-xxx.md` |

---

## 跨会话记忆约定（这是关键）

| 性质 | 位置 | 处理态度 |
|------|------|---------|
| **永久记忆（权威）** | `40_知识/`、`90_系统/` | 可直接作为事实引用 |
| **半成品（待蒸馏）** | `30_openclaw_inbox/` status=triaged、`AI对话记录/` 索引 | 仅作为线索，不作为结论 |
| **临时状态** | `10_流水/`、`30_openclaw_inbox/` status=inbox、草稿 | **不要当真理**，等 `/distill` 后再用 |
| **项目专属决策** | `20_项目/NN_/` | 绑项目上下文，不跨项目类推 |

**核心纪律**：永远不要跳过 `/distill` 蒸馏，把流水条目直接当成"已知知识"。临时态 → 永久态必须经 schema 化。

---

## 立项前概念验证规则

任何新游戏方向在进入正式项目或 7 天切片前，先放入 `20_项目/03_概念验证_YYYY-MM/` 做轻量验证。

- 概念视频必须展示未来游戏中真实可玩的核心玩法，不做“广告骗玩法”
- 没有视频信号，不进原型
- 没有陌生人试玩信号，不进 Demo
- 没有留存 / 买量信号，不进完整项目
- 朋友、运营、发行说“新颖 / 不错 / 有潜力”，只解锁下一层验证，不解锁完整开发

验证通过后，才能升级为完整策划案、7 天垂直切片或 `/new-project` 项目目录。

---

## 立项铁律（2026-05 起强制执行 · 必读）

任何"新游戏立项"相关讨论，**必须先读 `40_知识/01_游戏设计/立项铁律_2026-05.md`**。三条铁律：

1. **差异化拷问**：和市面上最火的同类，最核心的 3 个不同点是什么？答不上来就不做。
2. **美术差异化**：必须走风格化 / 猎奇 / 反差路线，避免和大厂卷画质。
3. **速度换容错**：开发周期压缩到 1 个月内出 Demo 测试。

**完整执行流程**（创意收集 → 铁律拷问 → AI 概念视频 → 陌生玩家社群调研 → 立项决策）见 `40_知识/00_工作流/休闲小游戏创意验证流程_v1.md`。

**纪律**：三条铁律任何一条没过都不能进入"AI 概念视频制作"阶段，更不能进入"项目立项"阶段。这是用户基于《美妆叠叠乐》和《光与朽》两次立项失败教训确立的最高优先级判断标准。

---

## 绝对禁止

- 未经用户同意改动 `20_项目/` 下已有文件
- 未经用户同意批量转换 `.docx` / `.xlsx` → `.md`
- 把 `10_流水/` 或 inbox 的临时条目当作知识源推荐（未 distilled 的不算）
- 自作主张 `git push`（push 是对外动作，要先问用户）
- 删除 `30_openclaw_inbox/` 下的任何文件（只改 frontmatter 状态）

---

## 会话启动流程

1. 读 `00_全局地图.md` 了解全局
2. 根据用户意图读对应子目录的 README 或 skill 定义
3. 不要全量载入任何目录——按需读，省 token

---

## 知识库共享架构（Multi-AI 共享约定）

**本地根目录**：`/Users/joye.wang/Projects/GamePlanningDoc/`

所有 AI 工具以此目录为共同根，各司其职：

| 工具 | 可读路径 | 可写路径 |
|------|---------|---------|
| Claude Code | 全库 | 全库（需用户确认破坏性操作） |
| Codex | 全库 | 代码相关文件（`40_知识/代码模板库/`、`20_项目/`） |
| openclaw | `40_知识/`、`20_项目/`、`30_openclaw_inbox/` | `30_openclaw_inbox/YYYY-MM-DD/` |
| Obsidian | 全库（用户直接编辑） | 全库（用户直接编辑） |

**openclaw 配置要点**：
- 知识库存储根路径 → `/Users/joye.wang/Projects/GamePlanningDoc/`
- 情报写入 → `30_openclaw_inbox/YYYY-MM-DD/` （保持 inbox 状态，等待 `/ingest-claw` 蒸馏）
- 采集前先读 `40_知识/` —— 了解已蒸馏知识，避免重复采集
- 采集前先读 `20_项目/` —— 了解在制项目上下文，辅助情报过滤与打标
- **禁止直接写入 `40_知识/`**，知识沉淀必须经 Claude Code 执行 `/distill` schema 化

---

## 协作工具分工

- **Codex（本工具）**：偏代码生成、重构、技术实现任务
- **Claude Code**：读 `CLAUDE.md`（与本文件同源），主蒸馏、项目推进、知识组织；唯一有权写 `40_知识/` 的 AI
- **openclaw（Mac 端）**：情报采集写 `30_openclaw_inbox/`；可读 `40_知识/` 和 `20_项目/` 获取上下文
- **Obsidian**：用户直接编辑的主界面，全库可见
