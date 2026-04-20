# AI Conversation Schema
---
type: ai-conversation
status: raw  # raw | indexed | distilled
source_platform: ""  # claude | chatgpt | 豆包 | deepseek | gemini | 其他
conversation_id: ""  # 平台内的会话 id 或导出文件名
conversation_date: ""  # 对话发生日期 YYYY-MM-DD
captured_at: {{date}}  # 录入知识库的日期
raw_transcript_path: ""  # 原始 txt/md 的相对路径
topic_tags: []  # 主题标签，例如 [游戏设计, 留存, 休闲游戏]
role: ""  # 当时 AI 扮演的角色，例如 "游戏策划" "主程" "美术"；无可留空
key_insights: []  # 3-5 条 bullet：从这次对话中提炼的、值得留存的要点
distill_worthy: false  # 是否值得进一步蒸馏到 knowledge-schema
distilled_ref: ""  # 如果已蒸馏，指向 40_知识/ 的 wikilink
---
