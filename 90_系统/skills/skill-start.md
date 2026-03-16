# /start — Morning Planning Skill

## Trigger
User invokes `/start` at the beginning of the day.

## Procedure
1. Read the most recent file in `10_日记/`
2. Read all notes in `00_草稿/` with `status: pending`
3. Check `20_项目/` for any `status: active` projects
4. Generate today's journal entry using the daily-journal template
5. List:
   - Top 3 priorities (from active projects)
   - Pending drafts that need processing
   - Any knowledge notes due for review
6. Write the file to `10_日记/{{date}}.md`
7. READ BACK the file and confirm it was written correctly

## Constraints
- Do not modify any existing files during /start
- Only create the new journal entry
- Surface information, don't make decisions