# 30_openclaw_inbox/ — Mac→Win 情报入站管道

## 这是什么

**Mac 端的 openclaw 抓到的资料，统一放进这里。** Windows 端的 Claude Code 定期跑 `/ingest-claw` 把它们登记为 `inbox-digest-schema` 条目，分类到 `40_知识/` 的对应主题分区。

## 工作流

```
Mac 端 openclaw 抓资料
   ↓ 输出 .md 到
30_openclaw_inbox/YYYY-MM-DD/xxx.md
   ↓ git push
Windows 端 git pull
   ↓ Claude Code 跑 /ingest-claw
  · 补 YAML frontmatter（source, captured_at, status=triaged）
  · 粗分到 40_知识/0N_主题/
  · 原文保留在 inbox，标记 status=triaged
   ↓ 每周 /distill 把 triaged 条目蒸馏为 knowledge-schema
40_知识/0N_主题/xxx.md (status=distilled)
```

## 采集质量要求

openclaw 不再以“抓了多少资料”为成功标准，而以“是否能被使用”为标准。

每份新资料建议在正文或 frontmatter 中提供：

- 原始链接或来源说明
- 主题提示 `topic_hint`
- 证据类型 `evidence_type`
- 决策相关性 `decision_relevance`
- 行动价值 `actionability`
- 蒸馏优先级 `distill_priority`
- 下一步用途 `next_use`

如果一条资料无法说明将如何帮助立项、素材测试、买量、发行合作、项目复盘或工作流优化，它应被降级为背景资料，不占用 P0/P1 队列。

## 文件组织

- 按日期分子目录：`2026-04-20/`、`2026-04-21/`...（方便按天批量处理）
- 文件名用英文或拼音，避免跨平台编码问题
- openclaw 可以自由写 Markdown 正文，不需要先加 frontmatter（`/ingest-claw` 会补）
- 新任务优先使用日期目录，不再继续写入 legacy `openclaw-daily/`，除非是在迁移旧记忆

## 消费规则

**inbox 是只进不出的**：已经被 `/ingest-claw` 处理过的文件在原地保留，只修改 frontmatter 的 `status` 字段。**永不从 inbox 删除任何东西**，这样任何蒸馏结果都可溯源回原始抓取。

状态含义：

- `inbox`：刚抓取，尚未登记。
- `triaged`：已由 `/ingest-claw` 分类，可作为线索等待蒸馏。
- `distilled`：已被 `/distill` 或正式知识条目吸收，必须填写 `distilled_ref`。
- `archived`：无当前用途、重复或过期，只保留溯源。

不要把 `inbox` 或 `triaged` 内容直接当成决策依据。

## 清理策略

- 三个月以上的 inbox 文件由 `/archive` skill 自动归档到 `90_系统/archive/inbox_YYYY-QN/`
- 归档时不删数据，只移动路径
