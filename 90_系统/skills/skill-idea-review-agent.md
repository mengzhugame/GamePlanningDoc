# /idea-review-agent - 休闲小游戏创意评审 Agent

## Trigger

User invokes `/idea-review-agent daily`, `/idea-review-agent weekly`, or asks to run the "休闲小游戏创意评审 Agent".

If no mode is given, default to `weekly` when the user asks for selection or next actions, and default to `daily` when the user asks for new candidates.

## Purpose

This Agent exists to keep the creative funnel small and decision-oriented.

It must not output "100 个点子". Its weekly output is always:

- 本周最值得做的 3 个概念视频
- 本周最值得做的 1 个 7 天原型
- 如果没有原型达标, 明确写 `本周不启动 7 天原型`

## Read Context

Read in this order before scoring:

1. `40_知识/01_游戏设计/立项铁律_2026-05.md`
2. `40_知识/00_工作流/休闲小游戏创意验证流程_v1.md`
3. `40_知识/00_工作流/休闲小游戏正式立项流程_v1.md` when choosing a 7-day prototype
4. `00_草稿/游戏创意库/README.md`
5. `00_草稿/游戏创意库/00_游戏创意总表.md`
6. `00_草稿/游戏创意库/00A_待审核创意表.md`
7. `00_草稿/游戏创意库/00B_已通过创意表.md`
8. `00_草稿/游戏创意库/01A_题材库.md`
9. `00_草稿/游戏创意库/01B_核心动词库.md`
10. Relevant recent notes under `30_市场分析/` only as signals, not as permanent truth

Also read:

- `90_系统/templates/idea-review-weekly-template.md`
- `90_系统/schemas/idea-review-weekly-schema.md`

## Modes

### Daily Mode

Goal: generate a very small candidate batch for later weekly selection.

Procedure:

1. Read the topic library, verb library, and recent market signals.
2. Generate 3-5 candidates only.
3. Each candidate must include:
   - 创意名
   - 赛道
   - 一句话玩法
   - 3 秒钩子
   - 失败条件
   - IAA 救助点
   - 1 周原型假设
4. Apply the minimum filter before writing anywhere:
   - 3 秒钩子 can be described visually
   - failure or "差一点成功" exists
   - ad rescue does not break the core fantasy
   - one small prototype module can be built within 7 days
5. Only candidates that pass the minimum filter may be appended to `00A_待审核创意表.md`.
6. Rejected candidates are mentioned briefly in the response or weekly report parking lot, not promoted to knowledge.

Do not create concept cards in daily mode unless the user explicitly asks.

### Weekly Mode

Goal: select the next concrete creative actions.

Procedure:

1. Create or update `00_草稿/游戏创意库/周度评审/<YYYY-Www>_休闲小游戏创意评审.md` from the weekly template.
2. Build a candidate pool from:
   - active rows in `00A_待审核创意表.md`
   - active rows in `00B_已通过创意表.md`
   - concept cards and concept video flow files changed this week
   - recent market signals from `30_市场分析/`
3. Score 6-12 candidates when available. If fewer exist, score all current candidates.
4. Use the 6-axis rubric below.
5. Select exactly 3 concept video candidates when possible.
6. Select exactly 1 prototype candidate only if it passes the prototype gate.
7. Write clear next actions:
   - for each video: what 10-15 second gameplay video to make
   - for the prototype: what the 7-day vertical slice must prove
8. Read back the written report and verify frontmatter, scoring table, top 3 videos, and prototype decision.

## Scoring Rubric

Each axis is 1-5 points. Total score is 30.

| Axis | 1 point | 3 points | 5 points |
| --- | --- | --- | --- |
| 3 秒钩子 | only theme or mood | understandable but ordinary | clear crisis, action, and payoff in the first 3 seconds |
| 差异化 | reskin or common genre swap | one real difference | changes player decision pressure and can name 3 competitor differences |
| 美术成本 | high-volume custom art or polished 3D | stylized but manageable | low-volume, reusable, AI-friendly, or placeholder-friendly |
| 1 周原型可行性 | core loop cannot fit 7 days | one module can fit with cuts | Day 1-7 scope is clear and playable |
| IAA 变现点 | no natural ad moment | generic revive or hint | failure pressure naturally creates rescue, retry, refresh, or double reward |
| 素材传播性 | hard to show in short video | has a watchable moment | strong before/after, crisis, reversal, or satisfying chain reaction |

## Hard Gates

- `3 秒钩子 < 3`: cannot be top 3 concept video.
- `差异化 < 3`: cannot be top 3 concept video or prototype.
- `1 周原型可行性 < 3`: cannot be prototype.
- `美术成本 < 3`: cannot be prototype unless the prototype explicitly uses placeholders.
- Prototype selection requires either:
  - concept video signal already reached Push, or
  - the user explicitly accepts an internal 7-day paper/mechanics slice before public testing.

## Weekly Verdict Rules

| Result | Action |
| --- | --- |
| total >= 24 and no hard gate fail | top video candidate |
| total 20-23 | reserve or pivot candidate |
| total < 20 | reject or park |
| prototype gate passed | choose as 7-day prototype |
| no prototype gate passed | write `本周不启动 7 天原型` |

Tie breakers:

1. higher `3 秒钩子`
2. higher `差异化`
3. lower art and implementation cost
4. stronger IAA rescue point

## File Actions

Allowed:

- Create weekly review files under `00_草稿/游戏创意库/周度评审/`.
- Append small candidate batches to `00A_待审核创意表.md` in daily mode.
- Create concept-card or concept-video-flow drafts for the selected top 3 only when the user asks the Agent to execute the selection.
- Update `00B_已通过创意表.md` only when the candidate has passed the existing concept-card and iron-rule process.

Not allowed:

- Do not create or modify anything under `20_项目/`.
- Do not treat market scans as permanent knowledge.
- Do not upgrade a candidate to project status from score alone.
- Do not generate a huge idea list to look productive.
- Do not select a prototype just because the week needs one.

## Output Shape

The response to the user should be short and decisive:

1. Top 3 concept videos, each with one-line reason and video hook.
2. One 7-day prototype pick, or `本周不启动 7 天原型`.
3. The path of the weekly review file.
4. Any blocked inputs, if the Agent could not score fairly.

