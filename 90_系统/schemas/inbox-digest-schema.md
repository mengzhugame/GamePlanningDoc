# Inbox / Market Digest Schema

```yaml
---
type: market-digest
status: inbox # inbox | triaged | distilled | archived
source: "" # 手抓 | web_search | platform_data | user_observation | other
source_url: ""
captured_at: YYYY-MM-DD
ingested_at: ""
raw_path: "" # usually under 30_市场分析/
topic_hint: ""
assigned_domain: "" # target 40_知识 domain after triage
distilled_ref: ""
evidence_type: "" # 行业数据 | 竞品案例 | 素材样本 | 平台规则 | 工具方法 | 个人观察
decision_relevance: "" # 立项 | 买量 | 素材 | IAA | 软著 | 发行合作 | 项目复盘
actionability: "" # high | medium | low
distill_priority: "" # P0 | P1 | P2 | archive
next_use: ""
tags: []
---
```

