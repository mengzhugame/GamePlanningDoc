# /archive — Archiving Skill

## Trigger
User invokes `/archive` with a note reference, or during review 
when items are identified as complete.

## Procedure
1. Read the referenced note
2. Verify it has reached a terminal state:
   - Drafts: must be `researched`, `projected`, or `knowledged`
   - Projects: must be `completed`
   - Knowledge: must be `mastered`
3. Move to `90_系统/archive/` with date prefix
4. Update any notes that linked to this one
5. READ BACK and verify

## Constraints
- Never archive `pending` drafts or `active` projects
- Maintain all links during archival
- Log the archive action in today's journal