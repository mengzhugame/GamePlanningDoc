# Idea Score Schema

> 创意评分卡的 frontmatter 规范。位置：`00_草稿/创意库/<idea_name>_打分.md`

```yaml
---
type: idea-score
status: draft  # draft | scored | passed-to-video | needs-iteration | rejected
idea_name: ""
created: ""  # YYYY-MM-DD
last_scored: ""

# === 组件维度（基于 8 款拆解抽出的可复用机制） ===
component_retention: ""        # 运气型 / 操作型 / 合成进化型 / 混合
component_feedback: ""         # 连击爆发 / 心流积累 / 装备开箱 / 高光时刻
component_session: ""          # 短局 / 中局 / 长局
component_decision: ""         # 单层 / 多层 / 自由组合
component_social: ""           # 纯单机 / 排行榜 / 裂变 / MMO
component_monetization: ""     # IAA / IAP / 混合

# === 题材维度 ===
theme_art: ""                  # 明亮卡通 / 中式恐怖 / 像素 / 极简 / 写实
theme_genre: ""                # 女性向治愈 / 男性向爽感 / 猎奇魔性 / 复古怀旧 / 田园经营 / 全龄
theme_culture: ""              # 中式 / 西式 / 日式 / 无文化指向
theme_micro_innovation: ""     # 「组合订单包」对应位 — 你的微创新点（差异化护城河）

# === 4 维打分（每项 1-5 分） ===
score_differentiation: 0       # 铁律 1 差异化（≤2 直接 fail，无视其他分数）
score_market_demand: 0         # 市场需求（拆解库或痛点扫描有支撑？）
score_self_capability: 0       # 自己能做透（资产/技能/AI视频能拍出来？）
score_lasting_play: 0          # 持久玩（同一局会不会腻？用户血泪铁律）

# === 总分 + 判断 ===
total_score: 0                 # 4 项相加，≥15 才进 AI 视频测试
verdict: ""                    # passed-to-video / needs-iteration / rejected

# === 关联 ===
related_teardowns: []          # 拆解库里相关的游戏 wikilink
related_competitors: []        # 市场上最火的 3 款同类
related_pain_signals: []       # 来自 user-pain-scan 的痛点支撑

notes: ""
---
```

## 字段含义

- **status 五态**：
  - `draft` — 创意刚提出，还没打分
  - `scored` — 已打分，等待决策
  - `passed-to-video` — 总分 ≥15，进 AI 视频测试
  - `needs-iteration` — 12-14 分，需要迭代某项
  - `rejected` — <12 分 或 铁律 1 ≤2 fail
- **score 4 维**：
  - **差异化**（≤2 直接 fail，无视其他分数）—— 这是铁律 1
  - **市场需求**（来自拆解库或痛点扫描）
  - **自己能做透**（资产/技能/AI 视频）
  - **持久玩**（用户血泪铁律——同一局不腻）
- **theme_micro_innovation**：你的"组合订单包"对应字段——这是跟最火同类的 1-3 个真正不同点，是创意的差异化护城河

## 验证规则

- 创建后立即 grep `00_草稿/创意库/` 防同名重复
- `score_differentiation ≤ 2` 时 verdict 必须 = `rejected`（不能 passed-to-video）
- `total_score ≥ 15` 才 verdict = `passed-to-video`
- `verdict = passed-to-video` 后下一步走 `/iron-rule-check` 进概念池
