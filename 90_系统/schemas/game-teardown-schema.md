# Game Teardown Schema

> 游戏拆解资料的 frontmatter 规范。位置：`40_知识/07_游戏拆解库/<YYYY-MM>_<游戏名>.md`

```yaml
---
type: game-teardown
status: draft  # draft | refined | distilled
game_name: ""
developer: ""
publisher: ""
platform: []  # 抖音小游戏 / 微信小游戏 / Steam / 海外 / iOS / Android
release_date: ""  # YYYY-MM 或 YYYY-MM-DD
genre: ""  # 主品类：三消 / 塔防 / Merge / 模拟经营 / Roguelike / 弹球塔防 等
sub_genre: ""  # 子分支：堆叠消除 / 物理弹幕塔防 / 装修治愈 等
core_mechanic_keywords: []  # 弹球 / 物理 / 三选一 / 卡片摆放 / 灵魂经济
art_style: ""  # 风格化 / 恐怖暗黑 / 明亮卡通 / 像素 / 写实
monetization: ""  # IAA / IAP / 混合
torn_down_at: ""  # 拆解日期 YYYY-MM-DD
torn_down_by: ""  # claude-code / codex / user
sources: []  # 调研用到的链接 / 视频 / 文章
related_projects: []  # 跟用户已有项目的相邻度（美妆叠叠乐 / 光与朽 / 概念池方向）
distilled_ref: ""  # 蒸馏到方法论后的引用 wikilink
tags: []
---
```

## 字段含义

- **status 三态**：
  - `draft` — 初版拆解（200-1000 字，不要求每段完整）
  - `refined` — 深度补充（用户/AI 二次完善某些段落）
  - `distilled` — 已被 `/distill` 蒸馏到 `40_知识/01_游戏设计/` 方法论文件
- **torn_down_by**：标注是哪个 AI 或 user 写的，便于后续质量审计
- **related_projects**：跟用户当前项目的关系——这是拆解的"为我所用"价值
- **distilled_ref**：从拆解到方法论的双向引用，避免重复蒸馏

## 防重复机制

写新拆解前必须 grep `40_知识/07_游戏拆解库/_INDEX.md` 的 `game_name` 字段，已存在的不重复拆解（除非游戏有重大版本更新需 `refined`）。
