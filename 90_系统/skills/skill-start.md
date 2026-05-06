# /start — Morning Planning Skill

## Trigger

User invokes `/start` at the beginning of the day.

If today's `10_流水/<today>.md` already exists with a `/start` section, ask whether to append a new section or skip.

## Procedure

### Step 1 — Read context

Read in order:

1. Latest `10_流水/<YYYY-MM-DD>.md` or latest file in `10_流水/`.
2. `90_系统/skills/_INDEX.md`.
3. Current month file in `60_计划/` if it exists.
4. `00_全局地图.md`.

### Step 2 — Scan auto-trigger signals

Use the Auto-trigger Detection Matrix in `_INDEX.md`:

| 检测 | 工具 |
|------|------|
| `00_草稿/游戏创意库/` 是否有未评分创意 | List + read frontmatter |
| `00_草稿/游戏创意库/00A_待审核创意表.md` 是否有待拷问方向 | List + read table |
| `30_市场分析/` 是否有新行业周报/月报/痛点扫描 | List recent files |
| AI 对话是否有未索引文件 | List relevant directory if present |
| 距上次 `/distill` 多少天 | Inspect `40_知识/**` mtimes or latest summary |
| 90 天以上未动的 draft / market note / 待整理文件 | File mtime scan |
| 知识 review_count 最新更新 > 30 天的条目 | Search frontmatter |

### Step 3 — Generate today's 流水

Append or create `10_流水/<today>.md`:

```markdown
---
type: daily
date: <today>
status: active
---

# <today> 早会

## 今日里程碑
<从 60_计划/ 当前月文件提取本周或今日相关项>

## 建议触发的 Skill
<按扫描结果列 0-5 条>

## 待处理
- 待蒸馏材料：
- 待索引 AI 对话：
- 待立项拷问创意：
- 活跃项目：

## 昨日关键回顾
<提取 1-3 条>

## 今日笔记
```

### Step 4 — Read back

Read the written file and verify frontmatter and section structure.

## Constraints

- Do not overwrite existing daily notes by default.
- Do not force-trigger any skill.
- Keep the morning section concise.
