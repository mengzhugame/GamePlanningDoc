# /review — Knowledge Review & Output Skill

## Trigger
User invokes `/review` (can specify weekly/monthly/knowledge)

## Procedure for Knowledge Review:
1. Query `40_知识/` for notes where `status: draft` or 
   `last_reviewed` > 7 days ago
2. Present knowledge notes for active recall testing
3. Update `review_count` and `last_reviewed`
4. Advance status: draft → review → mastered (based on count/quality)
5. READ BACK and verify

## Procedure for Weekly Review:
1. Read all journal entries from the past 7 days
2. Read all project updates
3. Generate review note using review-schema
4. Write to `80_回顾/`
5. Identify outputs ready for `50_成果/`
6. READ BACK and verify

## Constraints
- Reviews must reference specific notes, not generalities
- Weekly review must include both wins and lessons