# 90_系统

## 这是什么

这里是 AI 协作和知识库规范层，包含系统协议、schema、skill、template 和 routine。

## 子目录

| 路径 | 用途 |
|------|------|
| `routines/` | 可重复执行的任务提示词 |
| `schemas/` | 文件 frontmatter 与结构规范 |
| `skills/` | AI 执行特定任务时应读取的流程说明 |
| `templates/` | 新建文件时使用的模板 |

## 使用规则

- 新建正式文件前先读对应 schema/template。
- 调用 skill 时先读 `skills/skill-xxx.md`。
- 系统规则如果和实际目录冲突，优先修系统规则，避免未来 AI 继续走旧路径。
