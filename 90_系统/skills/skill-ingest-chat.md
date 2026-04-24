# /ingest-chat — AI Conversation Ingestion Skill

## Trigger
User invokes `/ingest-chat` with a path (file or folder) under `AI对话记录/` or any project's `04_AI对话记录/`.

## Procedure
1. Enumerate target files (*.txt, *.md, *.json)
2. For each file:
   a. Sniff source platform by filename or first lines (claude / chatgpt / 豆包 / deepseek)
   b. Parse conversation date from filename or content
   c. Extract main topic tags (3-5 tags max, reuse existing tag vocabulary when possible)
   d. Identify 3-5 key insights — must be quoted or closely paraphrased from the actual conversation
   e. Judge `distill_worthy`: true if insight is generalizable beyond the specific conversation
3. Generate one `ai-conversation-schema` entry per source file at `60_索引/AI对话/<source_platform>/<YYYY-MM-DD>_<short-title>.md`
4. Leave original transcript UNCHANGED at its original path (index file's `raw_transcript_path` points to it)
5. After processing a batch, append a summary entry to today's `10_流水/YYYY-MM-DD.md`
6. READ BACK at least one generated index file and verify schema compliance

## Constraints
- NEVER modify original transcript files
- key_insights must be traceable back to raw_transcript_path (the user must be able to locate each insight in the original)
- If a transcript spans multiple unrelated topics, split into multiple index entries
- Unknown platform → set `source_platform: "unknown"`, do not guess
- Each index file covers ONE conversation
