# Idea Review Weekly Schema

> 休闲小游戏创意评审 Agent 的周报规范。位置：`00_草稿/游戏创意库/周度评审/<YYYY-Www>_休闲小游戏创意评审.md`

```yaml
---
type: idea-review-weekly
status: draft          # draft | scored | decided | archived
cycle_start: ""        # YYYY-MM-DD
cycle_end: ""          # YYYY-MM-DD
week_id: ""            # YYYY-Www, e.g. 2026-W22
created: ""            # YYYY-MM-DD
updated: ""            # YYYY-MM-DD

candidate_count: 0
scored_count: 0
selected_concept_videos: []  # exactly 0-3 idea names
selected_prototype: ""       # idea name or empty
prototype_decision: ""       # selected | no-qualified-candidate | deferred

source_files: []        # idea tables, concept cards, video flow files
market_signal_files: [] # recent 30_市场分析 files used as signals
notes: ""
---
```

## Required Sections

1. `本周结论`
2. `输入来源`
3. `候选池`
4. `评分表`
5. `本周最值得做的 3 个概念视频`
6. `本周 7 天原型选择`
7. `暂缓 / 否决 / 合并`
8. `文件动作`
9. `Read-Back Verification`

## Scoring Fields

Each candidate must be scored from 1-5 on:

- `3 秒钩子`
- `差异化`
- `美术成本`
- `1 周原型`
- `IAA 变现点`
- `素材传播性`

`美术成本` uses higher score for lower cost and easier execution.

## Validation Rules

- `candidate_count` must be greater than or equal to `scored_count`.
- `selected_concept_videos` may contain 0-3 items. Do not exceed 3.
- `selected_prototype` may contain 0-1 item.
- `prototype_decision = selected` requires `selected_prototype` to be non-empty.
- `prototype_decision = no-qualified-candidate` requires the prototype section to explain which gate failed.
- A candidate with `3 秒钩子 < 3` or `差异化 < 3` cannot appear in `selected_concept_videos`.
- A candidate with `1 周原型 < 3` cannot appear in `selected_prototype`.
- A weekly review must not create or modify files under `20_项目/`.

