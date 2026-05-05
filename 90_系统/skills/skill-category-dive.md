# /category-dive — 品类深度调研 Skill

## Trigger
User invokes `/category-dive <品类名>` (e.g. `/category-dive Stickman动作` / `/category-dive Merge` / `/category-dive 收纳整理`).

If invoked without 品类名, ask which 品类 to dive into.

## When to Use（用于 /start 自动提醒判断）
触发时机（任一满足即可建议触发）：
- **月度雷达**（`monthly_radar_*.md`）已 flag 某方向为「下月观察」候选
- **AI 概念视频测试**反馈良好（评论 ≥ 50 / 收藏 ≥ 100），准备进入 7 天垂直切片之前
- **概念池**（`00_概念池.md`）某方向状态从"AI 视频观察期"准备升级到"立项"前
- 找合伙人 / 谈合作时需要锁定具体品类作为合作起点

**不该用的时机**：
- 还在创意收集阶段（应该先 `/iron-rule-check`）
- 没有候选品类时（瞎扫一通 = 浪费 token）
- 距上次 `/category-dive` 同品类调研 < 2 个月（数据没明显变化，建议直接看上次报告）

**频率**：每 2-3 月一次同品类，全部品类合计每月 ≤ 2 次。

## Procedure
1. **接收品类名** `<品类名>`
2. **读上下文**：
   - `20_项目/03_概念验证_*/00_概念池.md` — 当前候选方向
   - `30_openclaw_inbox/` 最近 1 份 monthly_radar（如果有）— 看是否已 flag 此品类
3. **跑 Agent + WebSearch 调研**：
   - 七麦数据 / 微信小游戏排行榜 / data.ai / sensortower
   - 该品类在中国大陆 + 海外的下载榜 / 流水榜前 30
   - 必须真实可查，不允许编造
4. **拆子分支**（按"核心机制差异"分，不按"题材"分）。例如：
   - Stickman 动作 → 闯关 / 肉鸽 / 塔防 / 横版动作
   - Merge → 数字 / 物品 / 角色 / 装修
   - 收纳整理 → 物理整理 / 三消 / 绿洲叙事
5. **统计每个子分支在 TOP 30 的占比**（拍数据，不靠主观）
6. **TOP 3 子分支各挑 2 款代表作**做拆解：
   - 核心机制 / 数值循环 / 美术风格 / 变现路径（IAA / IAP / 混合）
7. **跟用户已有项目做相邻度判断**（哪些子分支跟用户经验最近）
8. **输出建议**：
   - 子分支 A：值得深耕（理由 3 条）
   - 子分支 B：观察（理由）
   - 子分支 C：红海，避开（理由）

## Output
- **路径**：`30_openclaw_inbox/YYYY-MM-DD/category_deep_dive_<品类名>_YYYY-MM-DD.md`
- **frontmatter** 按 `inbox-digest-schema`：
  - `status: inbox`
  - `source: manual-category-dive`
  - `evidence_type: 行业数据`
  - `decision_relevance: 立项/品类深耕`
  - `actionability: high`
  - `distill_priority: P0`
  - `topic_hint: 品类深度调研_<品类名>`
- **报告结构**：
  1. 调研对象品类 + 调研日期 + 数据源清单
  2. TOP 30 拍片结果（榜单截图 / 链接）
  3. 子分支拆解 + 占比
  4. TOP 3 子分支代表作机制拆解
  5. 跟用户已有项目相邻度判断
  6. 子分支建议（深耕 / 观察 / 避开）

## Constraints
- **必须真实可查**的榜单数据，**不允许凭空生成**
- 不写 `40_知识/`（不蒸馏，等 `/distill` 处理）
- 不改 `20_项目/`（不立项，只给子分支判断）
- 不直接给"做哪款游戏"建议——只给"哪个子分支值得深耕"判断
- 数据如果拿不全（某些榜单需付费），如实报告"数据不全"，不强行总结
- READ BACK 报告文件验证 frontmatter + 路径正确

## Post-trigger 提示
跑完后告知用户：
- "报告已写到 `30_openclaw_inbox/YYYY-MM-DD/`，状态 inbox"
- "下一步建议：等周末跑 `/distill` 把它蒸馏到 `40_知识/01_游戏设计/`，或者现在挑 1 个推荐子分支跑 `/iron-rule-check <方向名>`"
