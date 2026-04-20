# /distill — Weekly Distillation Skill

## Trigger
User invokes `/distill` (typically weekly) with an optional time range (default: last 7 days).

## Procedure
1. Collect candidate sources:
   - `00_流水/YYYY-MM-DD.md` files within range
   - `30_openclaw_inbox/` files with `status: triaged`
   - `50_索引/AI对话/` entries with `distill_worthy: true` and no `distilled_ref`
2. Cluster candidates by topic using existing tag vocabulary + `assigned_domain`
3. For each cluster with ≥2 related pieces:
   a. Decide if a new `knowledge-schema` entry is needed, OR an existing entry should be updated
   b. If new: create at `40_知识/<domain>/<topic-slug>.md` using knowledge-schema, status=draft
   c. If update: append to existing, increment `review_count`, update `last_reviewed`
   d. Cross-link with Obsidian `[[]]` wikilinks between the new knowledge entry and every source it consumed
4. Update all consumed sources:
   - Inbox files → `status: distilled`, fill `distilled_ref`
   - AI conversation index → `status: distilled`, fill `distilled_ref`
5. Write a summary report to `00_流水/YYYY-MM-DD.md` listing all new/updated knowledge entries
6. READ BACK every new knowledge file and verify schema + wikilink integrity

## Constraints
- Preserve original phrasing for definitions, formulas, data (see `/knowledge` constraint #1)
- Every knowledge entry must cite at least one source via `source_book` / `source_page` / `raw_path`
- Single-source items (clusters of 1) → skip this round, wait for more material
- Never delete or overwrite `00_流水/` or inbox source files
