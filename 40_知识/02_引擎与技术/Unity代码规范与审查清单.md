---
type: knowledge
status: review
created: 2026-05-26
updated: 2026-05-26
domain: 02_引擎与技术
tags: [Unity, CSharp, 代码规范, 工程规范, 审查清单]
source: Openclaw code_standards
last_reviewed: 2026-05-26
review_count: 1
---

# Unity 代码规范与审查清单

## 来源: `10_流水/Openclaw知识库文件/code_standards.md` · 提取日期 2026-05-26

## 1. 使用定位

这份规范适合放在每个 Unity 项目的 `AI_CONTEXT.md` 或项目 README 里，作为人和 AI 共同遵守的代码边界。它的价值不是格式洁癖，而是让后续迁移、模板复用、AI 改代码和多人协作都更稳定。

最小原则：

- 类名与文件名一致。
- 公共类、方法、属性使用 `PascalCase`；私有字段和 `[SerializeField]` 字段使用 `camelCase`。
- 布尔变量优先使用 `is`、`has`、`can`、`should` 前缀。
- 常量使用 `UPPER_SNAKE_CASE`，PlayerPrefs key、事件名、Layer/Tag 名称不要散落在业务代码中。
- 调试代码必须有开关或条件编译，不能把临时 `Debug.Log` 带进发布版本。

## 2. MonoBehaviour 推荐组织顺序

单个类内部保持稳定顺序，方便 AI 和人快速定位：

1. 常量。
2. 静态字段。
3. `[SerializeField]` 配置字段。
4. 私有运行时字段。
5. 公共属性。
6. Unity 生命周期：`Awake`、`OnEnable`、`Start`、`Update`、`OnDisable`、`OnDestroy`。
7. 公共方法。
8. 私有方法。
9. 事件回调。
10. 编辑器调试方法。

如果某个类已经大到必须靠大量分隔符才能读懂，优先考虑拆职责，而不是继续加标题。

## 3. Inspector 配置规范

Inspector 是策划、美术、AI 生成代码和未来自己共同使用的接口，应尽量自解释：

- 重要配置用 `[Header]` 分组。
- 不明显的字段必须写 `[Tooltip]`。
- 数值范围明确时使用 `[Range]`。
- LayerMask、Prefab、ScriptableObject 引用要写清语义，避免 `target`、`obj`、`config` 这类含混命名。

对休闲小游戏尤其重要的是：广告、存档、关卡、货币、奖励、引导和调试开关都应在 Inspector 中有明确边界。

## 4. 事件和生命周期纪律

事件字段可以用 `OnGameStart`、`OnLevelUp` 这类 `On` 前缀；触发方法用 `Trigger` 前缀，避免外部直接调用委托。

订阅规则：

- `OnEnable` 订阅。
- `OnDisable` 取消订阅。
- 跨场景持久对象要特别检查重复订阅和残留引用。

这条规则应和 [[Unity通用技术栈复用指南]] 里的 `GameEvents`、`Singleton`、`PersistentSingleton` 配合使用。事件系统的目标是减少 Manager 之间直接互相找对象，而不是制造新的全局隐式依赖。

## 5. 调试与性能红线

发布前必须检查：

- 是否还有裸 `Debug.Log`。
- 是否还有打开的 `showDebugInfo`。
- 是否还有调试用 `[ContextMenu]` 暴露在不该暴露的对象上。
- `Update` 中是否每帧 `FindObjectOfType`、`GetComponent`、LINQ 或创建临时集合。
- 高频生成销毁对象是否已经改成对象池。
- 组件引用是否在 `Awake` / `Start` 缓存。

经验判断：小项目可以先快，但一旦进入 Demo 外部测试，调试代码和性能债会直接污染判断。卡顿、发热、日志刷屏、重复事件触发，都会让你误判玩法本身不好。

## 6. ScriptableObject 配置规范

ScriptableObject 不只是数据容器，也应该承担配置自检：

- 使用 `CreateAssetMenu` 固定创建入口。
- 提供少量语义化便捷方法，例如根据等级计算经验，而不是让调用方到处复制公式。
- 编辑器专用验证方法必须包在 `#if UNITY_EDITOR` 中。
- 复杂配置进入项目后，至少提供一个 `ValidateSettings` 或同等检查入口。

这对关卡、敌人、技能、奖励、广告点和经济数值尤其重要。配置如果没有自检，错误通常会拖到运行时或买量测试时才暴露。

## 7. PR / 自检清单

每次让 AI 或自己改完 Unity 代码，至少跑一遍这张清单：

- 文件名、类名、命名空间是否正确。
- 字段命名和访问级别是否符合用途。
- Inspector 字段是否有 Header / Tooltip。
- 事件是否成对订阅和取消。
- 是否有未清理的调试代码。
- 是否有每帧分配、重复查找、重复 GetComponent。
- 高频对象是否走对象池。
- PlayerPrefs、Layer、Tag、事件名是否集中管理。
- ScriptableObject 是否可验证。
- 改动是否影响模板复用或 AI 后续读代码的可理解性。

如果一个改动无法通过这张清单，不一定不能合入，但必须明确它是临时债，并写下何时清理。
