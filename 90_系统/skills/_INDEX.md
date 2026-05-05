# Skill 索引 — When to Use & Why

> 这是 skill 速查表，按"使用场景"分类（不是字母）。
> `/start` 早会会自动扫描每个 skill 的「Auto-trigger 信号」并提示你。
> 你**不用记命令**——`/start` 每天会告诉你今天可能需要触发哪些。

---

## 🌅 每日 / 早会

| Skill | 一句话目的 | 何时触发 | 不该用 |
|-------|----------|---------|-------|
| `/start` | 早会：生成今日流水 + surface 待办 + 建议触发的 skill | **每天工作开始时**（首条消息） | 已经跑过一次了，不要重复 |

---

## 💡 创意 → 立项前

| Skill | 一句话目的 | 何时触发 | 不该用 |
|-------|----------|---------|-------|
| `/idea-score <创意名>` | 创意打分卡 — 组件×题材 + 4 维 1-5 分（差异化/市场需求/可做性/持久玩） | 「组件×题材矩阵」组合出新创意时 / 创意池有 status=draft 没打分时 | 创意还在脑子里没文字化 / 已经打过分 |
| `/iron-rule-check <方向名>` | 用 3 条立项铁律拷问一个新游戏方向 | `/idea-score` 通过后准备进概念池前 | 还没想清楚 1-2 句话核心机制时 |
| `/category-dive <品类名>` | 对一个品类做下载榜深度调研 + 子分支拆解 | 月度雷达 flag 了某方向 OR 概念视频信号良好准备深耕前 | 没有候选品类时（瞎扫一通 = 浪费 token） |

---

## 📥 资料蒸馏（数据流入）

| Skill | 一句话目的 | 何时触发 | 不该用 |
|-------|----------|---------|-------|
| `/ingest-chat` | 把 AI 对话原文索引化到 `60_索引/AI对话/` | `AI对话记录/` 有未索引 .txt | 短对话（< 10 轮），不值得索引 |
| `/ingest-claw` | 把 openclaw 抓的 inbox 文件分诊打 frontmatter | `30_openclaw_inbox/` 有 status=inbox 的文件 | 还没读懂内容就乱打 status |
| `/distill` | 周度蒸馏 — 合并流水/inbox/AI对话到知识库 | **距上次 distill ≥ 7 天**，或 triaged inbox > 5 个 | 一周内已经跑过，再跑没新增 |

---

## 🎯 项目推进

| Skill | 一句话目的 | 何时触发 | 不该用 |
|-------|----------|---------|-------|
| `/new-project` | 用 7 子目录模板新建一个游戏项目 | 概念视频 + 陌生玩家试玩信号都有，准备进入完整开发 | 概念还没验证（应该先 `/iron-rule-check`） |
| `/project` | 把一份草稿提升为正式项目 | 草稿 status=research_done 时 | 草稿还在 pending |

---

## 📚 知识管理

| Skill | 一句话目的 | 何时触发 | 不该用 |
|-------|----------|---------|-------|
| `/research` | 对一个主题做结构化研究 | 用户主动想搞清楚某主题（产业/技术/玩法机制） | 临时好奇，没打算沉淀知识 |
| `/knowledge` | 把一份研究/资源蒸馏为单一知识条目 | research 完成后想沉淀成永久知识 | 内容太散，没法成"单一条目" |
| `/review` | 间隔复习知识或做周/月回顾 | 知识条目 review_count 长期没更新 | 上周刚 review 过 |
| `/archive` | 把已完成或被否决的笔记移到归档区 | 90 天以上没动的 inbox / draft / 已否决方向 | 还在活跃使用 |

---

## 🔄 自动触发（云端 routine，你不用管）

这些是云端 routine 自动跑的，不需要你手动触发：

| Routine | 频率 | 产出 | 管理链接 |
|---------|------|------|---------|
| 周度市场扫描 | 每周一 9am | `30_openclaw_inbox/<date>/web_market_scan_*.md` | `trig_013Qw2hkPHq6dVg1MkhGFHS7` |
| **用户痛点扫描** | 每周一 9:30am | `30_openclaw_inbox/<date>/user_pain_scan_*.md` | `trig_013aCyMXnot33ztBSnY9VtBX` |
| 立项审计 | 每月 15 号 | `20_项目/03_概念验证_*/` 下 | `trig_017ufKJ4NRvaCx7GQkSCh2Rp` |
| 月度雷达 | 每月 1 号 | `30_openclaw_inbox/<date>/monthly_radar_*.md` | `trig_01UzrRhdzJJbQ2RUjdVTCod2` |
| 季度归档 | 每季度 | `90_系统/archive/` | `trig_01GdoAsXYWNgJp5ZWPBr6KRY` |

管理总入口：https://claude.ai/code/routines

---

## Auto-trigger Detection Matrix（给 `/start` 用）

`/start` 早会扫描以下条件，发现命中就在「🎯 建议触发的 Skill」段提示用户：

| 检测条件 | 建议触发 |
|---------|---------|
| `00_草稿/创意库/` 有 .md 但不在 `00_概念池.md` 列表 | `/iron-rule-check <方向名>` |
| 最新 `monthly_radar_*.md` 有「下月观察方向」段未深扫 | `/category-dive <方向名>` |
| `30_openclaw_inbox/**` 有 status=inbox 文件 ≥ 1 | `/ingest-claw` |
| `AI对话记录/` 有 .txt 不在 `60_索引/AI对话/全景图.md` | `/ingest-chat` |
| 距上次 `/distill` ≥ 7 天 OR triaged inbox > 5 | `/distill` |
| `00_草稿/创意库/` 有 `*_打分.md` 文件 status=draft | `/idea-score` |
| 90 天以上未动的 inbox/draft | `/archive` |
| 知识 review_count 最新更新 > 30 天的条目 ≥ 3 | `/review` |

**不在矩阵里的 skill**（`/new-project`、`/project`、`/research`、`/knowledge`）= 用户主动决策，不做自动提醒。

---

## 速查表使用方法

- **用户找命令**：直接 Ctrl+F 搜关键词（"立项"、"调研"、"蒸馏"等）
- **AI 用 `/start`**：读这份 Auto-trigger Detection Matrix，按条件扫描 → 写到今日流水
- **新增 skill**：加到对应"使用场景"分类下 + 加 Auto-trigger 条目（如果该 skill 该被自动提醒）
