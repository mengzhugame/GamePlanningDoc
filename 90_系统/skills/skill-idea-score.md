# /idea-score — 创意打分卡 Skill

## Trigger
User invokes `/idea-score <创意名>` to score a new game idea using component×theme matrix + 4-axis evaluation.

If invoked without 创意名, ask which idea to score (offer to list all `status: draft` files in `00_草稿/创意库/` matching `*_打分.md`).

## When to Use（用于 /start 自动提醒判断）

触发时机：
- **「组件×题材」矩阵新组合出来 5-10 个创意候选**（5-10 个/季度）
- **创意池里有 status=draft 的创意没打分**
- **AI 视频测试反馈良好**，准备打分确认是否进切片

不该用：
- 创意还在脑子里没文字化（先文字化再打分）
- 已经打过分的同名创意（除非有重大调整需 re-score）
- 还没完成 P0+P1+P2 拆解蒸馏方法论（缺方法论参照系）

## Procedure

### Step 1 — Read context
- `40_知识/01_游戏设计/立项铁律_2026-05.md` — 铁律本体
- `40_知识/07_游戏拆解库/_INDEX.md` — 找相关拆解参考
- `90_系统/templates/idea-score-template.md` — 模板
- `90_系统/schemas/idea-score-schema.md` — schema
- `30_openclaw_inbox/**/user_pain_scan_*.md`（如有）— 痛点支撑
- `30_openclaw_inbox/**/monthly_radar_*.md`（如有）— 市场雷达

### Step 2 — Create file
路径：`00_草稿/创意库/<创意名>_打分.md`
按 `idea-score-template.md` 填 frontmatter + 5 段结构。
**写之前 grep 同名防重复**。

### Step 3 — Sequential interrogation via AskUserQuestion
逐项问用户（一次问一条，不要堆叠）：

1. **创意核心**：一句话讲清玩家做什么 + 爽什么
2. **组件维度** 6 项（让用户从 schema 选项里选）
3. **题材维度** 4 项（特别问「微创新点」—— 跟最火同类的 1-3 个不同点）
4. **铁律 1 差异化**：让用户列 3 款同类爆款 + 3 个不同点
   - 如果用户答"没有同类"，push back: "至少列 1 款最接近的，否则铁律 1 自动 fail"
5. **市场需求支撑**：拆解库或痛点扫描里有什么支撑这个方向？
6. **自己能做透**：资产/技能/AI 视频能不能拍？
7. **持久玩**：同一局怎么不腻？是不是靠堆关卡？

### Step 4 — Compute scores
按 4 维 1-5 分打分（用户自评，AI 在评完后给反馈）。

打分规则：
- 差异化 ≤2 → 整个 fail，verdict = rejected
- 总分 ≥15 → verdict = passed-to-video
- 总分 12-14 → verdict = needs-iteration
- 总分 <12 → verdict = rejected

### Step 5 — Verbal verdict + 下一步建议

- **passed-to-video**：「✅ 总分 X/20 通过。下一步：走 `/iron-rule-check <方向名>` 拷问 → 进概念池 → AI 视频脚本」
- **needs-iteration**：「⚠️ 总分 X/20，最低分是 [维度]。建议回「组件×题材」矩阵改 [具体调整]」
- **rejected**：「❌ 总分 X/20 砍掉。回组件×题材矩阵重新组合，不要硬推这个创意」

### Step 6 — Update concept pool（如果 verdict = passed-to-video）
建议用户：
- 在 `20_项目/03_概念验证_<月>/00_概念池.md` 表格加一行
- 准备 AI 概念视频脚本

### Step 7 — READ BACK
Read 写入文件验证 frontmatter + 总分计算 + verdict 一致。

## Constraints
- **不替用户做答** — 4 维打分必须用户自己评，AI 只在评完后给反馈
- **铁律 1 ≤2 直接 fail** — 不要为了让创意通过故意拔高分数
- **失败时不要安慰用户** — 直接说"砍掉"，避免"逃避发布心理"
- **不修改 40_知识/、立项铁律本体**
- **不替用户写微创新点** — 这是用户自己的差异化判断，AI 可以质疑但不能填
- **总分 ≥15 不等于必上线** — 还要走 `/iron-rule-check` + AI 视频测试 + 切片
