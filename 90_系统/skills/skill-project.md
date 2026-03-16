# /project — Project Creation Skill

## Trigger
User invokes `/project` with a draft or idea reference.

## Procedure
1. Read the referenced draft from `00_草稿/`
2. Ask clarifying questions about scope and goal
3. Generate a project note using the project-schema
4. Create a plan note in `60_计划/`
5. Update the original draft's status to `projected`
6. READ BACK both files and verify
7. Link the project note to the draft via `related_drafts`

## Constraints
- One project per invocation
- Must have a clear `goal` field before creating
- Schema must be followed exactly