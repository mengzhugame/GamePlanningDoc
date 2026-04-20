# /ingest-claw — openclaw Inbox Ingestion Skill

## Trigger
User invokes `/ingest-claw` (typically after `git pull` fetches new Mac-side captures).

## Procedure
1. Scan `30_openclaw_inbox/` recursively for `.md` files where status is missing or `status: inbox`
2. For each file:
   a. Read content
   b. Add or update `inbox-digest-schema` frontmatter at the top of the file in-place
   c. Determine `assigned_domain` — one of: 01_游戏设计 / 02_引擎与技术 / 03_美术与表现 / 04_音频与节奏 / 05_数值与经济 / 06_发行与运营
   d. Set `status: triaged`, fill `ingested_at` with today's date
   e. Extract 3-5 tags
3. Append a digest line to today's `00_流水/YYYY-MM-DD.md`:
   `- [[relative/path/to/inbox/file]] → 40_知识/<assigned_domain>/ (待蒸馏)`
4. Do NOT create knowledge entries yet — that is `/distill`'s job
5. READ BACK at least one modified inbox file and verify frontmatter integrity

## Constraints
- Never move or rename inbox files. They live in `30_openclaw_inbox/` forever.
- Only modify the frontmatter; the body stays as openclaw wrote it
- If content is ambiguous across domains, pick the most specific one and note alternates in `tags`
- If content is clearly spam / off-topic, set `status: archived` instead of triaged
