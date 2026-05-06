# LifeOS Agent Protocol

You are an agent operating within a LifeOS Obsidian vault.
This vault is not a blank project. Read local context before acting.

## Operating Rules

1. **Skill-Based Execution**: when the user invokes a `/skill`, read and follow the corresponding file in `90_系统/skills/`.
2. **Read Context First**: before writing or deciding, read the relevant README, schema, project entry, or knowledge file.
3. **Schema Compliance**: every formal file you create should conform to the relevant schema in `90_系统/schemas/`.
4. **Template Compliance**: when a template exists in `90_系统/templates/`, use it.
5. **State Progression**: respect file states such as draft, inbox, triaged, distilled, archived.
6. **Read-Back Verification**: after writing a file, read it back to verify structure and links.

## Concept Validation Before Project

Before any new game direction becomes a formal project, store it under `00_草稿/游戏创意库/` as a lightweight concept validation note, using `创意名_概念卡.md` and `创意名_概念视频数据记录.md`.

Rules:

- Concept videos must show core gameplay that can exist in the real game.
- No video signal -> do not enter prototype.
- No stranger playtest signal -> do not enter Demo.
- No retention / paid traffic signal -> do not enter full production.
- Positive feedback from friends, operators, or publishers only unlocks the next validation layer, not full development.

Only after concept validation succeeds should the direction be promoted into a full GDD, a 7-day vertical slice, or a `/new-project` project folder.

## Project Pitch Iron Rules

For any new game pitch discussion, read `40_知识/01_游戏设计/立项铁律_2026-05.md` first.

1. **Differentiation interrogation**: what are the 3 core differences vs the most successful competitor in this genre?
2. **Art differentiation**: take a stylized / quirky / contrast route. Do not compete with larger studios on raw production value.
3. **Speed for fault-tolerance**: compress the dev cycle to 1 month for the first testable Demo.

Full execution flow before project creation: `40_知识/00_工作流/休闲小游戏创意验证流程_v1.md`.
Formal project flow after validation: `40_知识/00_工作流/休闲小游戏正式立项流程_v1.md`.

## Skill Map

| Skill | Purpose | Primary Output |
|-------|---------|----------------|
| `/start` | Start-of-day orientation | `10_流水/YYYY-MM-DD.md` |
| `/project` | Promote a validated draft to a project | `20_项目/NN_*/` |
| `/new-project` | Scaffold a new project | `20_项目/NN_*/` |
| `/research` | Investigate a topic | research note or market note |
| `/knowledge` | Distill one resource into one knowledge entry | `40_知识/*/` |
| `/review` | Spaced-repetition review of knowledge entries | updated review metadata |
| `/archive` | Move stale items to archive | `90_系统/archive/` |
| `/ingest-chat` | Index historical AI transcripts | AI conversation index |
| `/distill` | Merge 流水 + 市场分析 + AI conversations into knowledge | `40_知识/*/` |
| `/iron-rule-check <name>` | Audit a new game direction against the 3 iron rules | `00_草稿/游戏创意库/<name>_立项拷问.md` or merged into the concept card |
| `/category-dive <category>` | Deep-dive into one game category | `30_市场分析/<date>/category_deep_dive_*.md` |
| `/idea-score <name>` | Score a game idea with component×theme matrix + 4-axis evaluation | `00_草稿/游戏创意库/<name>_打分.md` |

## Knowledge Flow

```text
Input sources
  ├─ 10_流水/              daily append-only notes
  ├─ 30_市场分析/           market notes, user-pain scans, category dives
  ├─ 00_草稿/              ideas and concept validation notes
  └─ AI conversations + project records
                |
                | /ingest-chat and /distill
                v
          indexed / triaged / clustered
                |
                v
          40_知识/0N_domain/*.md
                |
                v
          consumed by /project, /new-project, /review
```
