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
   f. Fill optional decision fields when the source provides enough signal:
      - `evidence_type`
      - `decision_relevance`
      - `actionability`
      - `distill_priority`
      - `next_use`
3. Append a digest line to today's `10_流水/YYYY-MM-DD.md`:
   `- [[relative/path/to/inbox/file]] → 40_知识/<assigned_domain>/ (待蒸馏/P0-P2；用途：xxx)`
4. Append a short "openclaw 消费队列" summary to today's `10_流水/YYYY-MM-DD.md`:
   - P0: must review/distill this week
   - P1: useful but can wait
   - P2: background only
   - archived: noise/off-topic
5. Do NOT create knowledge entries yet — that is `/distill`'s job
6. READ BACK at least one modified inbox file and verify frontmatter integrity

## Constraints
- Never move or rename inbox files. They live in `30_openclaw_inbox/` forever.
- Only modify the frontmatter; the body stays as openclaw wrote it
- If content is ambiguous across domains, pick the most specific one and note alternates in `tags`
- If content is clearly spam / off-topic, set `status: archived` instead of triaged
- Do not mark everything as P0. P0 is reserved for information that can change the current 90-day plan, a project decision, or a live experiment.
- If an inbox file only repeats already-distilled knowledge, set `distill_priority: P2` or `status: archived` and explain in `next_use`.
- If a file is a legacy daily memory rather than external intelligence, keep the body unchanged and classify it as either `triaged` with a clear domain or `archived` if it has no current use.
