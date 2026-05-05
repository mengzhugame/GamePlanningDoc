# /iron-rule-check — Project Pitch Iron Rules Check Skill

## Trigger
User invokes `/iron-rule-check <方向名>` (e.g. `/iron-rule-check 弹射猫咪游戏`).

If user invokes without a direction name, ask which direction to audit (offer to list all `status: active` directions in `20_项目/03_概念验证_*/`).

## Procedure
1. **Read context**(按顺序):
   - `40_知识/01_游戏设计/立项铁律_2026-05.md` — 铁律本体
   - `40_知识/00_工作流/休闲小游戏创意验证流程_v1.md` — 创意流程上下文
   - `90_系统/templates/pitch-iron-rule-audit-template.md` — 拷问表模板
   - `90_系统/schemas/pitch-iron-rule-audit-schema.md` — frontmatter schema
   - `20_项目/03_概念验证_YYYY-MM/00_概念池.md` — 当前概念池

2. **Determine target month**: scan `20_项目/03_概念验证_YYYY-MM/` directories, use latest. If not exists, create `20_项目/03_概念验证_<current YYYY-MM>/`.

3. **Create audit file** at `20_项目/03_概念验证_YYYY-MM/<方向名>_立项拷问.md` using template. Fill `direction_name` and `parent_pool`.

4. **Sequential interrogation via AskUserQuestion** (一次问一条,不要堆叠):

   **铁律 1 - 差异化拷问**(最关键):
   - 问用户:"<方向名> 和市面上最火的 3 款同类是什么?分别的核心不同点?"
   - 自由文本回答(用户必须自己列举,不要替用户填)
   - 如果用户答"这个没有同类",push back: "至少列出 1 款最接近的,否则铁律 1 自动 fail"

   **铁律 2 - 美术差异化**:
   - 用 AskUserQuestion 4 选项:风格化 / 猎奇 / 反差 / 其他
   - 选"其他"或描述含糊(如"画风可爱")→ 标 fail
   - 进一步问"一句话视觉钩子"和"参考案例"

   **铁律 3 - 1 个月 Demo 范围**:
   - 问 Week 1-4 各做什么
   - 如果用户答不出来或范围明显 > 1 个月 → 标 fail
   - 同时问"为达到 1 个月,要砍掉的功能是什么"(强制思考取舍)

5. **Fill audit file** 把答案落到对应章节。每条铁律的判断:
   - 铁律 1 pass: 用户能清晰列出 3 个对手 + 3 个不同点,且不同点不是主观描述
   - 铁律 2 pass: 选了风格化/猎奇/反差之一,且视觉钩子具体
   - 铁律 3 pass: Week 1-4 拆得出,且砍了至少 3 个功能

6. **Compute final_verdict**:
   - 三条全 pass → `passed`
   - 一条 fail 但接近 → `partial-needs-pivot`,标出哪条
   - 多条 fail 或铁律 1 严重 fail → `failed`
   - 写 verdict_date = today

7. **Update concept pool** `00_概念池.md`:
   - 在表格里加一行: 方向名 / 拷问文件链接 / 核心问题 / 当前状态(=拷问 verdict)
   - 不要触碰其他方向

8. **Verbal verdict to user**:
   - `passed`: 提示"三条全过 → 现在可以启动 AI 概念视频制作。下一步参考 `40_知识/00_工作流/休闲小游戏创意验证流程_v1.md` 阶段 3"
   - `partial-needs-pivot`: 明确指出哪条险过 + 调整建议,不要继续推进
   - `failed`: 直接说"该方向砍掉,不要继续。回到 `00_概念池.md` 找下一个方向。已归档拷问文件作为未来复盘案例"

9. **READ BACK** 拷问文件 + 概念池更新,验证 schema + 表格行正确。

## Constraints
- **不替用户做答**:用户必须自己写出 3 个对手 + 3 个不同点。AI 可以在用户答完后说"你这个不同点太弱",但不能替用户填。
- **不绕过铁律**:用户说"我觉得没有同类爆款"时,要求至少列 1 个最接近的(因为"没有同类"99% 是用户没调研到位)
- **拷问通过不等于立项**:只等于解锁下一阶段(概念视频)
- **拷问失败不删除文件**:落地到 status=failed,供未来复盘
- **不修改 40_知识/**(铁律本体只读)
- **不修改其他方向的拷问文件**(只动当前方向)
- **失败时不要安慰用户**:直接告诉"该方向砍掉",这是这个 skill 存在的意义——避免"逃避发布心理"
