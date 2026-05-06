# /category-dive — 品类深度调研 Skill

## Trigger

User invokes `/category-dive <品类名>`.

If invoked without 品类名, ask which category to research.

## When to Use

Use when:

- A market note flags a category as worth watching.
- A concept video receives promising feedback and the user wants to understand the category before slicing.
- The user is preparing to talk to partners, publishers, or collaborators about a specific category.

Do not use when:

- There is no candidate category.
- The user only has a vague idea and has not passed iron-rule questioning.
- The same category was researched within the last 2 months and no major market change occurred.

## Procedure

1. Receive category name.
2. Read context:
   - `00_草稿/游戏创意库/00_游戏创意总表.md` and `00A_待审核创意表.md` if present
   - recent `30_市场分析/` market notes if present
   - relevant `40_知识/07_游戏拆解库/` entries
3. Research real ranking, store, platform, and competitor data.
4. Split sub-branches by core mechanic, not only by theme.
5. Count each sub-branch's share among the sampled top games.
6. Pick representative games from top sub-branches and analyze:
   - core mechanic
   - progression and economy
   - art style
   - monetization path
   - video material angle
7. Compare with the user's existing capabilities and project history.
8. Output:
   - worth deepening
   - observe
   - avoid

## Output

Path: `30_市场分析/YYYY-MM-DD/category_deep_dive_<品类名>_YYYY-MM-DD.md`

Suggested frontmatter:

```yaml
---
type: market-analysis
status: inbox
source: manual-category-dive
captured_at: YYYY-MM-DD
topic_hint: 品类深度调研_<品类名>
evidence_type: 行业数据
decision_relevance: 立项/品类深耕
actionability: high
distill_priority: P0
---
```

Report structure:

1. Category and date.
2. Data source list.
3. Top sample table.
4. Sub-branch breakdown.
5. Representative game teardown.
6. Fit with user's project history and capability.
7. Recommendation.

## Constraints

- Must use verifiable data. Do not fabricate rankings.
- Do not write directly to `40_知识/`; let `/distill` handle long-term knowledge.
- Do not modify `20_项目/`.
- If data is incomplete or paywalled, say so clearly.
