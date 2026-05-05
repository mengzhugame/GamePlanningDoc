# /start — Morning Planning Skill (v2)

## Trigger
User invokes `/start` at the beginning of the day.

If today's `10_流水/<today>.md` already exists with `/start` 早会段落，先问：「今日早会已生成，是否要重新跑 `/start` 覆盖？」

## Procedure

### Step 1 — Read context
按顺序读：
1. 最近 1 份 `10_流水/<YYYY-MM-DD>.md`（昨天的流水，提取「未完成 / 待办」项）
2. `90_系统/skills/_INDEX.md`（找 **Auto-trigger Detection Matrix** 段）
3. `70_计划/<current YYYY-MM>_行动清单.md`（如果存在 — 找今天日期 / 本周对应的 `[ ]` 项）

### Step 2 — Scan auto-trigger signals

按 `_INDEX.md` 的 Auto-trigger Detection Matrix 检测：

| 检测 | 工具 |
|------|------|
| `00_草稿/创意库/` 是否有 .md 不在 `00_概念池.md` | Glob + Grep |
| `30_openclaw_inbox/` 最新 `monthly_radar_*.md` 的「下月观察」段 | Read 最新月度雷达 |
| `30_openclaw_inbox/**` 有几个 status=inbox 文件 | Grep frontmatter |
| `AI对话记录/` 有几个 .txt 不在 `60_索引/AI对话/全景图.md` | Glob + Grep |
| 距上次 `/distill` 多少天 | Glob `40_知识/**` mtime |
| 90 天以上未动的 inbox/draft | Glob mtime |
| 知识 `review_count` 最新更新 > 30 天的条目 | Grep frontmatter |

### Step 3 — Generate today's 流水

写入 `10_流水/<today>.md`（**不是** `10_日记/`，v1 老 bug 已修）：

```markdown
---
type: daily
date: <today>
status: active
---

# {today} 早会

## 📅 今日里程碑
{从 70_计划/<month>_行动清单.md 提取本周对应 [ ] 项 + 今天日期对应项}
{若 70_计划 不存在或本月无文件，标注「本月无行动清单，建议创建」}

## 🎯 建议触发的 Skill
{Step 2 扫描结果，每条形如：「`/skill-name` —— 因为 [触发条件描述]」}
{若无触发，写「✅ 今日无 skill 待触发」}

## ⚠️ 待处理
- 待蒸馏 inbox: {count} 个
- 待索引 AI 对话: {count} 个
- 待立项拷问的草稿: {list}
- Active 项目: {list}
- 未完成 [ ] (来自昨日流水): {list}

## 📝 昨日关键回顾
{从昨日流水提取标记为「重要」「待办」「[x]」的 1-3 条}

## 今日笔记
（append-only，开始记录今日想法）
```

### Step 4 — READ BACK
Read 写入的文件确认 schema + 结构正确

## Constraints

- **不修改** 已有 `10_流水/`、`70_计划/`、`_INDEX.md`、`SYSTEM_PROMPT.md` 等文件
- **不替用户做决定**——只 surface 信号，不说"你今天必须做 X"
- **不强制触发任何 skill**——「建议触发」≠ 立刻触发
- **路径正确**：`10_流水/`（NOT `10_日记/`，v1 的 bug，v2 已修）
- 如果今日流水已存在，先确认是否覆盖（不要默认覆盖）

## 输出风格

- 项目符号简洁，每条 ≤ 1 行
- 整体 ≤ 30 行（早会要快读完）
- 中文 + 英文 skill 名混排可以
- 不要鸡汤式开头（"早安！新的一天" 之类）
