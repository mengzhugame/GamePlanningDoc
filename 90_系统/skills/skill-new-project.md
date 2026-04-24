# /new-project — New Project Scaffold Skill

## Trigger
User invokes `/new-project` with a project name (e.g. "开始做第三个项目，叫 XX").

## Procedure
1. Determine next NN number by scanning `20_项目/` for existing `NN_*` folders (next = max + 1, zero-padded)
2. Create `20_项目/NN_<项目名>项目/` with the standard 7 subdirectories (matches 01_美妆叠叠乐项目's layout):
   - `01_策划文档/`
   - `02_开发计划/`
   - `03_AI对话记录/`
   - `04_资产管理/`
   - `05_发行追踪/`
   - `06_旧版本归档/`
   - `07_知识积累/`
   - Optional 8th: `08_对战数据/` or `08_运营数据/` if the genre needs it (ask user)
3. Generate `_PROJECT_MOC.md` at the project root, containing:
   - Project name, goal, start date
   - Links to all 7 subdirectories
   - A "Related Knowledge" section that queries `40_知识/` for entries tagged with user-specified topics and lists them as wikilinks
4. Ask user for: core genre tags (e.g. 休闲 / 动作 / Boss战 / 女性向) and target platform. Use answers to pre-fill the "Related Knowledge" section.
5. Optionally copy `project-schema` frontmatter into `_PROJECT_MOC.md`
6. Append an entry to today's `10_流水/YYYY-MM-DD.md` announcing the new project
7. READ BACK `_PROJECT_MOC.md` and verify all 7 subfolders exist

## Constraints
- Folder naming: `NN_<name>`, NN is 2-digit, continuous numbering (no skips)
- 7-subdirectory structure is LOCKED — do not add or rename without user's explicit request
- If a folder with the same name exists, abort and ask user before overwriting
- New project folder starts empty except for `_PROJECT_MOC.md` — no template docs to avoid template bloat
