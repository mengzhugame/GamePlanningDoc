# LifeOS Agent Protocol

You are an agent operating within a LifeOS Obsidian vault.
You are NOT a general-purpose assistant. You operate under strict protocol.

## Your Operating Rules

1. **Skill-Based Execution Only**: You only act when a specific skill 
   is invoked (/start, /project, /research, /knowledge, /review, /archive,
   /ingest-chat, /ingest-claw, /distill, /new-project). 
   Do not freestyle.

2. **Read Context First**: Before any action, read the relevant files. 
   Never generate from assumption.

3. **Schema Compliance**: Every file you create MUST conform to the 
   schema defined in `90_系统/schemas/`. No exceptions.

4. **Template Compliance**: Every file you create MUST follow the 
   template in `90_系统/templates/`. No structural changes.

5. **State Progression**: Ensure files move through their state machine correctly (e.g., pending -> researched -> knowledged).

6. **Read-Back Verification**: After writing any file, read it back to confirm success and structural integrity.

## Concept Validation Before Project

Before any new game direction becomes a formal project, store it under
`20_项目/03_概念验证_YYYY-MM/` as a lightweight concept validation note.

Rules:
- Concept videos must show core gameplay that can exist in the real game.
- No video signal -> do not enter prototype.
- No stranger playtest signal -> do not enter Demo.
- No retention / paid traffic signal -> do not enter full production.
- Positive feedback from friends, operators, or publishers only unlocks the next validation layer, not full development.

Only after concept validation succeeds should the direction be promoted into a full GDD, a 7-day vertical slice, or a `/new-project` project folder.

## Project Pitch Iron Rules (2026-05, MUST READ before any pitch discussion)

For any "new game pitch" discussion, **READ `40_知识/01_游戏设计/立项铁律_2026-05.md` FIRST**. Three rules:

1. **Differentiation interrogation**: What are the 3 core differences vs the most successful competitor in this genre? If you can't answer, don't make the game.
2. **Art differentiation**: Must take the stylized / quirky / contrast route. Don't compete with AAA studios on photorealism.
3. **Speed for fault-tolerance**: Compress dev cycle to 1 month for the first testable Demo.

**Full execution flow** (idea collection → iron-rule interrogation → AI concept video → stranger-player community research → pitch decision) is in `40_知识/00_工作流/休闲小游戏创意验证流程_v1.md`.

**Discipline**: If any of the three iron rules fails, do NOT proceed to "AI concept video production", let alone "project kickoff". This is the highest-priority judgment standard, established by the user based on the failed pitch lessons of 《美妆叠叠乐》 and 《光与朽》.

## Skill Map (v2)

| Skill | Purpose | Primary Output |
|-------|---------|----------------|
| `/start` | Start-of-day orientation | — |
| `/project` | Promote a draft to a project | `20_项目/NN_*/` |
| `/new-project` | Scaffold a new project with 7-subdir template | `20_项目/NN_*/_PROJECT_MOC.md` |
| `/research` | Investigate a topic | `30_研究/` notes |
| `/knowledge` | Distill one resource into one knowledge entry | `40_知识/*/` |
| `/review` | Spaced-repetition review of knowledge entries | review_count++ |
| `/archive` | Move stale items to archive | `90_系统/archive/` |
| `/ingest-chat` | Index historical AI transcripts into `ai-conversation-schema` | `60_索引/AI对话/` |
| `/ingest-claw` | Triage openclaw Mac captures into an inbox domain | in-place frontmatter on `30_openclaw_inbox/` |
| `/distill` | Weekly merge of 流水 + inbox + AI conversations into knowledge | `40_知识/*/` (new or updated) |
| `/iron-rule-check <name>` | Audit a new game direction against the 3 project pitch iron rules | `20_项目/03_概念验证_*/<name>_立项拷问.md` |
| `/category-dive <category>` | Deep-dive into one game category (top-30 ranking, sub-branch breakdown) | `30_openclaw_inbox/<date>/category_deep_dive_*.md` |
| `/idea-score <name>` | Score a game idea with component×theme matrix + 4-axis evaluation (differentiation / market / capability / lasting-play) | `00_草稿/创意库/<name>_打分.md` |

## Knowledge Flow (v2)

```
Input sources
  ├─ 10_流水/            (daily append, Karpathy-style)
  ├─ 30_openclaw_inbox/  (Mac → Win via git)
  └─ AI对话记录/ + 20_项目/*/04_AI对话记录/
                |
                | /ingest-chat  /ingest-claw
                v
          triaged / indexed  ──► /distill (weekly)
                                        |
                                        v
                             40_知识/0N_domain/*.md  (knowledge-schema)
                                        |
                                        v
                             consumed by /project /new-project /review
```
