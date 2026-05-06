# /idea-score — 创意打分卡 Skill

## Trigger

User invokes `/idea-score <创意名>` to score a new game idea using component×theme matrix + 4-axis evaluation.

If invoked without 创意名, ask which idea to score or offer to list draft files in `00_草稿/游戏创意库/`.

## When to Use

Use when:

- A new game idea has been written down.
- A component×theme matrix produced 5-10 candidates.
- A concept video signal looks promising and needs a pre-project score.

Do not use when:

- The idea only exists verbally and has no written core.
- The same idea has already been scored without major changes.

## Procedure

### Step 1 — Read context

- `40_知识/01_游戏设计/立项铁律_2026-05.md`
- `40_知识/07_游戏拆解库/_INDEX.md` if it exists
- `90_系统/templates/idea-score-template.md`
- `90_系统/schemas/idea-score-schema.md`
- Relevant `30_市场分析/` user-pain scans or market radar notes if present

### Step 2 — Create file

Path: `00_草稿/游戏创意库/<创意名>_打分.md`

Use `idea-score-template.md`. Search first to avoid duplicate files.

### Step 3 — Sequential interrogation

Ask the user one item at a time:

1. Core idea: what does the player do, and what feels satisfying?
2. Component dimensions.
3. Theme dimensions and micro-innovation.
4. Three closest competitors and three real differences.
5. Market demand support from teardown notes or market signals.
6. Capability fit: can the user produce assets, code, and video clearly?
7. Lasting play: why will repeated play not feel stale?

### Step 4 — Compute scores

Use 4 dimensions, 1-5 points each:

- Differentiation
- Market demand
- Self capability
- Lasting play

Rules:

- Differentiation ≤2 -> fail regardless of total score.
- Total ≥15 -> `passed-to-video`.
- Total 12-14 -> `needs-iteration`.
- Total <12 -> `rejected`.

### Step 5 — Next step

If passed, suggest `/iron-rule-check <方向名>` and adding it to `00_草稿/游戏创意库/00B_已通过创意表.md`.

### Step 6 — Read back

Read the written file and verify frontmatter, total score, and verdict.

## Constraints

- Do not answer the scoring questions for the user.
- Do not inflate differentiation.
- Do not modify `40_知识/`.
