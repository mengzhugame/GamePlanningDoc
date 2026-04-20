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
| `/ingest-chat` | Index historical AI transcripts into `ai-conversation-schema` | `50_索引/AI对话/` |
| `/ingest-claw` | Triage openclaw Mac captures into an inbox domain | in-place frontmatter on `30_openclaw_inbox/` |
| `/distill` | Weekly merge of 流水 + inbox + AI conversations into knowledge | `40_知识/*/` (new or updated) |

## Knowledge Flow (v2)

```
Input sources
  ├─ 00_流水/            (daily append, Karpathy-style)
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