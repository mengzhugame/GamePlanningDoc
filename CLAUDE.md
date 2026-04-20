# GamePlanningDoc — AI 协作入口（Claude Code）

> 你正进入一个已运行的 LifeOS Obsidian vault，不是空白项目。本文件是**地图**，不是内容——具体内容按需 Read。

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
| `00_MOC.md` | 全局地图，主题入口 | 用户问"有什么" / 会话启动 |
| `00_流水/YYYY-MM-DD.md` | 今日 append-only 笔记 | 记任何临时想法 |
| `20_项目/NN_项目名项目/` | 单个游戏项目的全部文档 | 在该项目上下文工作 |
| `30_openclaw_inbox/` | Mac 端 openclaw 抓的原料（inbox） | 跑 `/ingest-claw` 时 |
| `40_知识/0N_主题/` | 蒸馏后的**永久知识**（权威） | 找方法论 / 复用经验 |
| `40_知识/代码模板库/` | 生产级代码模板 | 写代码时 |
| `AI对话记录/` | 从各平台导出的原始 AI 对话 txt | 跑 `/ingest-chat` 时 |
| `AI角色设定/` | 13+ 角色的提示词库（策划/主程/美术 等） | 切换工作角色时 |
| `90_系统/SYSTEM_PROMPT.md` | **6 条铁律 + Skill Map**（必读） | 首次进入必读 |
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
| `/knowledge` `/project` `/research` `/review` `/archive` `/start` | 对应同名 `skill-xxx.md` |

---

## 跨会话记忆约定（这是关键）

| 性质 | 位置 | 处理态度 |
|------|------|---------|
| **永久记忆（权威）** | `40_知识/`、`90_系统/` | 可直接作为事实引用 |
| **半成品（待蒸馏）** | `30_openclaw_inbox/` status=triaged、`AI对话记录/` 索引 | 仅作为线索，不作为结论 |
| **临时状态** | `00_流水/`、`30_openclaw_inbox/` status=inbox、草稿 | **不要当真理**，等 `/distill` 后再用 |
| **项目专属决策** | `20_项目/NN_/` | 绑项目上下文，不跨项目类推 |

**核心纪律**：永远不要跳过 `/distill` 蒸馏，把流水条目直接当成"已知知识"喂给用户或下次会话。临时态 → 永久态必须经 schema 化。

---

## 绝对禁止

- 未经用户同意改动 `20_项目/` 下已有文件
- 未经用户同意批量转换 `.docx` / `.xlsx` → `.md`
- 把 `00_流水/` 或 inbox 的临时条目当作知识源推荐（未 distilled 的不算）
- 自作主张 `git push`（push 是对外动作，要先问用户）
- 删除 `30_openclaw_inbox/` 下的任何文件（只改 frontmatter 状态）

---

## 会话启动流程

1. 读 `00_MOC.md` 了解全局
2. 根据用户意图读对应子目录的 README 或 skill 定义
3. 不要全量载入任何目录——按需 Read，省 token

---

## 协作工具分工

- **Claude Code（本工具）**：主蒸馏、项目推进、知识组织
- **Codex**（如使用）：读 `AGENTS.md`（与本文件同源），偏代码生成任务
- **openclaw（Mac 端）**：只写 `30_openclaw_inbox/YYYY-MM-DD/*.md`，不读其他
