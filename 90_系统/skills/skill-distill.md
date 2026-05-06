# /distill — Weekly / Monthly Distillation Skill

## Trigger

User invokes `/distill` with an optional time range. Default range: last 7 days.

## Procedure

1. Collect candidate sources:
   - `10_流水/YYYY-MM-DD.md` files within range
   - `30_市场分析/` notes within range
   - `00_草稿/游戏创意库/` concept cards and concept-video data records with clear validation results
   - AI conversation index entries with `distill_worthy: true` and no `distilled_ref`
   - project review notes from `20_项目/`
2. Cluster candidates by topic using existing tag vocabulary and target domain.
3. For each cluster with enough evidence:
   - Decide whether to create a new `knowledge-schema` entry or update an existing entry.
   - If new, create it under `40_知识/<domain>/`.
   - If update, append a dated section to the existing file and update review metadata.
   - Cross-link every consumed source with Obsidian wikilinks.
4. Update consumed source status where the source has frontmatter.
5. Write a summary report to `10_流水/YYYY-MM-DD.md`.
6. Read back every new or updated knowledge file and verify schema, links, and source traceability.

## Constraints

- Preserve original phrasing for definitions, formulas, and exact data.
- Every knowledge entry should cite at least one source path or source note.
- Single-source weak signals can remain in source areas until more evidence appears.
- Never delete or overwrite source materials during distillation.
