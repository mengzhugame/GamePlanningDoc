# 周度评审

> 这里存放“休闲小游戏创意评审 Agent”的周度输出。它不是创意仓库本体, 只负责把本周候选收束成可执行动作。

## 输出目标

每周只回答三件事：

1. 哪 3 个方向最值得做概念视频。
2. 哪 1 个方向值得进入 7 天原型。
3. 哪些方向应该暂缓、否决或合并。

如果没有方向通过 7 天原型门槛, 结论必须写 `本周不启动 7 天原型`。

## 使用方式

调用：

```text
/idea-review-agent weekly
```

周报文件使用：

- 模板：`90_系统/templates/idea-review-weekly-template.md`
- schema：`90_系统/schemas/idea-review-weekly-schema.md`
- skill：`90_系统/skills/skill-idea-review-agent.md`

## 纪律

- 不生成 100 个点子。
- 不把市场扫描当权威知识。
- 不跳过概念视频直接立项。
- 不因为“本周要有原型”就强行选一个原型。

