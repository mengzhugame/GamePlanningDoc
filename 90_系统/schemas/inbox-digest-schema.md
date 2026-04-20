# Inbox Digest Schema
---
type: inbox-digest
status: inbox  # inbox | triaged | distilled | archived
source: ""  # openclaw | 手抓 | rss | 其他
source_url: ""  # 原始网址，若为本地采集留空
captured_at: {{date}}  # Mac 端抓取日期
ingested_at: ""  # Windows 端 /ingest-claw 处理日期
raw_path: ""  # 30_openclaw_inbox/ 下的相对路径
topic_hint: ""  # Mac 端可选预填的主题提示
assigned_domain: ""  # triaged 后分配到 40_知识/ 的哪个一级分区
distilled_ref: ""  # distilled 后指向 40_知识/ 里的知识条目 wikilink
tags: []
---
