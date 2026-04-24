# HEARTBEAT.md - 心跳检查

> **注意**：大部分定期任务已迁移至 Cron (Isolated Mode) 独立运行。
> 此文件仅用于紧急状态检查。

## 💓 默认行为

如果没有任何紧急异常，请直接回复：`HEARTBEAT_OK`

## 🔍 紧急检查 (可选)

仅在确实需要时执行：
- 检查是否有 Cron 任务执行失败的残留日志 (如果能看到)
- 检查是否有极其紧急的未读消息 (WhatsApp 等)

---
*任务已托管给 System Cron，不再依赖 Heartbeat 轮询。*
