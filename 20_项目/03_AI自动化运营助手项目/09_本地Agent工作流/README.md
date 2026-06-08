---
type: local-agent-workflow
status: active
created: 2026-06-03
updated: 2026-06-03
owner: wangyongcheng
project: AI自动化运营助手
tags: [本地Agent, Skills, 内容运营, 小红书, 自用版]
---

# 本地 Agent 工作流

## 这是什么

这里是 AI 自动化运营助手的 v0 自用版。

它不是 SaaS，不接 API，不依赖服务器。它是一套放在知识库里的本地 Agent 工作流，用 Codex / Claude Code / ChatGPT 等高能力模型执行。

核心目标：

> 围绕长期账号档案，持续完成选题、内容包生成、发布前审核、发布数据记录和 AI 复盘。

## 为什么放在知识库里

运营内容需要读取用户的真实经历、项目资料、个人 IP 方法论和商业方向。

如果把工作流放到独立目录，会出现两个问题：

1. Agent 读不到知识库里的上下文。
2. 复制个人信息后会产生两份真相，后续难以同步。

所以 v0 放在 `20_项目/03_AI自动化运营助手项目/09_本地Agent工作流/`，只做项目内隔离，不脱离知识库。

## 目录结构

```text
09_本地Agent工作流/
  AGENTS.md
  README.md
  skills/
  profiles/
  content/
    ideas/
    drafts/
    ready_to_publish/
    published/
    reviewed/
    archived/
  data/
  templates/
```

## 使用方式

在 Codex 或 Claude Code 中，可以这样调用：

```text
使用 AI 运营工作流，读取 `09_本地Agent工作流/AGENTS.md`，基于“独立开发者获客线”生成本周 5 个小红书选题。
```

或者：

```text
使用 `skills/skill-content-package.md`，把我选中的选题生成小红书内容包。
```

## 当前运营档案

| 档案 | 用途 |
| --- | --- |
| [[profiles/独立开发者获客线]] | 小游戏、AI 工具、失败复盘、开发者 IP |
| [[profiles/技术美术接单线]] | TA、Unity、Shader、小游戏 Demo、AI 应用接单 |
| [[profiles/旅行IP线]] | 旅行内容、旅行英语、海外生活方式实验 |

## 工作流入口

| Skill | 用途 |
| --- | --- |
| [[skills/skill-ops-start]] | 每次开始运营工作前读取上下文 |
| [[skills/skill-topic-generate]] | 生成选题 |
| [[skills/skill-competitor-insight]] | 手动竞品参考分析 |
| [[skills/skill-content-package]] | 生成内容包 |
| [[skills/skill-prepublish-review]] | 发布前审核 |
| [[skills/skill-data-review]] | 发布数据复盘 |
| [[skills/skill-weekly-review]] | 周复盘 |
| [[skills/skill-sync-profile]] | 同步运营档案与知识库来源 |

## 状态规则

内容文件按状态进入不同文件夹：

| 状态 | 目录 |
| --- | --- |
| idea | `content/ideas/` |
| draft | `content/drafts/` |
| ready | `content/ready_to_publish/` |
| published | `content/published/` |
| reviewed | `content/reviewed/` |
| archived | `content/archived/` |

## 同步规则

- 运营档案只保存当前运营摘要，不复制完整知识库。
- 档案内必须保留 `source_of_truth`。
- 内容生成前，如果涉及具体项目、失败经历、旅行经历或用户画像，先读来源链接。
- 新事实先进入 `data/` 或 `10_流水/`，确认后再蒸馏到长期知识。
