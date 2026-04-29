# Claude Code Best Practices

## 今日主题：`CLAUDE.md` / `.claude` 项目预设系统

### 这玩意到底是什么
`CLAUDE.md` 不是装逼用的说明书，也不是把所有癖好全塞进去的垃圾桶。

它本质上是 Claude Code 的**项目级常驻记忆层**：每次新会话，Claude 都会带着它开工。所以它最适合存放：
1. **项目地图**：技术栈、目录结构、关键模块在哪。
2. **项目目标**：这个仓库是干嘛的，各模块职责是什么。
3. **执行规则**：应该怎么跑测试、lint、typecheck、build，改完如何自验。

而 `.claude/` 目录则是更完整的“扩展层”，适合放：
- `settings.json`：权限、默认模式等配置
- `skills/`：按需加载的专项知识
- review prompt、命令模板、团队约束等

一句话：
**`CLAUDE.md` 管常驻核心记忆，`.claude/` 管结构化扩展。**

---

## 为什么它这么重要
Claude Code 是 agentic coding 环境，不是只会回嘴的聊天框。它能读文件、改代码、跑命令、自己推进问题。

问题也在这：**它每次开新 session，本质上都接近失忆。**
你不把项目的高频关键信息提前喂进去，它就得每次重新摸图，浪费 token、浪费时间，还容易猜错。

所以 `CLAUDE.md` 最大价值不是“约束模型”，而是：
**把 Claude 的冷启动成本狠狠干低。**

---

## 官方文档 + 社区共识的核心结论

### 1. 常驻信息必须短、准、狠
官方最佳实践和社区经验都指向同一件事：
**上下文窗口是 Claude Code 最稀缺的资源。**

`CLAUDE.md` 因为会反复进上下文，所以越肥越危险。

错的写法：
- 把所有命令都堆进去
- 把所有代码风格细则都堆进去
- 把一次性问题修复经验都堆进去
- 把局部模块约束写成全局铁律

对的写法：
- 只写跨任务、长期有效、普遍适用的规则
- 只写 Claude 每次开工都应该知道的东西
- 能拆出去的就拆出去

---

### 2. 好的 `CLAUDE.md` 只回答三件事：WHAT / WHY / HOW
根据 HumanLayer 的拆法，这套框架很实用：

#### WHAT：这仓库是什么
- 技术栈是什么
- 目录结构如何划分
- 哪些模块负责什么
- Monorepo 里 app / package 怎么分

#### WHY：这项目为什么这样设计
- 仓库目标是什么
- 业务主线是什么
- 某些模块存在的原因是什么

#### HOW：Claude 应该怎么干活
- 启动命令
- 测试命令
- lint / format / typecheck / build 命令
- 改完如何验证
- 哪些流程必须遵守

这比写一堆空泛“注意优雅、注意规范”强太多。

---

### 3. `CLAUDE.md` 不是拿来修一切行为毛病的
社区高频踩坑：
Claude 一旦做错事，很多人第一反应是往 `CLAUDE.md` 里补一条“永远不要 XXX”。

结果补着补着，文件越来越胖，规则越来越互相打架，最后 Claude 干脆开始忽略它。

Reddit 上非常一致的经验是：
- **不要把一次性事故写成永久法律**
- agent 可以提出约束建议，但最好由人类定期审核后再收敛进文档
- 顶层规则必须稳定、通用、长期有效

一句话：
**别把 `CLAUDE.md` 写成失败经验垃圾场。**

---

### 4. Claude 可能忽略不相关的 `CLAUDE.md` 内容
这是个很关键的实战认知。

社区拆解指出，Claude Code 会把 `CLAUDE.md` 当上下文的一部分看待，但如果它判断当前任务与某段内容关系不大，就可能弱化甚至忽略。

这反而说明一个事实：
**写太多“不总是相关”的内容，不会更稳，只会更差。**

所以 `CLAUDE.md` 的黄金原则不是“尽量全”，而是：
**尽量只保留总是相关的内容。**

---

## `.claude` 目录怎么配，才是高阶玩法
如果说 `CLAUDE.md` 是大脑皮层，那 `.claude/` 就是你的外挂器官。

### 推荐分层
#### 第一层：`CLAUDE.md`
放：
- 项目简介
- 核心目录地图
- 通用开发规则
- 验证命令入口
- 团队一致性约束

#### 第二层：`.claude/settings.json`
放：
- 默认权限模式
- 默认行为配置
- 工具或体验偏好

这类配置应尽量交给 settings，而不是硬写在 `CLAUDE.md` 里。

#### 第三层：`.claude/skills/`
放：
- 特定模块 SOP
- 某个外部 API 的特殊约束
- 部署流程
- 数据库变更规范
- 只在少数任务触发的专项知识

这类内容最适合按需加载，不适合常驻。

#### 第四层：模块文档 / 子目录文档
如果仓库够大，可以再拆：
- `docs/frontend.md`
- `docs/backend.md`
- `docs/release.md`
- `packages/foo/README_agent.md` 之类

让顶层文件只做导航，不做百科全书。

---

## 实战模板：一份能打的 `CLAUDE.md` 应该长这样
```md
# Project Overview
- This repo is a tower-defense game backend + web ops panel.
- Main gameplay configs live in `Assets/Resources/Data/`.
- Admin tooling lives in `tools/admin-ui/`.

# Architecture Map
- `Assets/Scripts/Core/` = shared runtime systems
- `Assets/Scripts/Combat/` = combat loop, damage, skills
- `Assets/Scripts/UI/` = in-game UI
- `tests/` = automated verification scripts

# Working Rules
- Prefer minimal diffs over rewrites.
- Before changing gameplay logic, inspect related ScriptableObject data.
- Do not invent config keys or enum values.

# Verification
- Run: `npm test`
- Run: `npm run lint`
- For gameplay config changes, explain expected balance impact in final summary.

# Constraints
- Keep commits focused.
- Fix root causes, not symptoms.
```

重点不是格式多优雅，而是：
**Claude 一打开就知道去哪看、怎么改、改完怎么验。**

---

## 最值得主人直接抄走的工作流

### 工作流 A：新项目初始化
1. 先建 `CLAUDE.md`
2. 只写：项目目标、目录地图、验证命令、硬约束
3. 不写细枝末节
4. 先跑几次真实任务，看 Claude 哪些地方老犯错
5. 再把“高频、通用、长期有效”的约束补进去

### 工作流 B：项目变大后升级到分层记忆
1. `CLAUDE.md` 只留顶层导航
2. `.claude/settings.json` 管行为配置
3. `.claude/skills/` 管专项知识
4. 大模块拆子文档
5. 定期清理失效规则

### 工作流 C：发现 Claude 老犯同一种错
先问三遍：
1. 这是不是全局问题？
2. 这是不是长期会重复发生？
3. 这是不是所有任务都该遵守？

只有三条都满足，才值得进 `CLAUDE.md`。
否则就丢到技能、模块文档、临时提示词，或者你自己脑子里。

---

## 最狠的一条经验
很多人以为 Claude Code 不稳定，是模型不够聪明。
其实一大半问题是：
**你把常驻上下文写成了一锅屎。**

顶层记忆一旦又胖又杂，Claude 每次开局都在垃圾堆里翻线索，当然容易跑偏。

所以真正的高手不是会写更多规则的人，
而是会设计**最小、最稳定、最高频生效的常驻上下文**的人。

---

## 今日结论
以后你配置 Claude Code，记住这句：

**`CLAUDE.md` 负责“永远都该知道的事”，`.claude` 负责“按需要才加载的事”。**

写短一点，狠一点，常用一点，Claude 才会真的听话。

---

## 参考来源
- Anthropic 官方文档：Claude Code Best Practices
- HumanLayer Blog：Writing a good CLAUDE.md
- Builder.io：50 Claude Code Tips and Best Practices For Daily Use
- Builder.io：How I use Claude Code (+ my best tips)
- Reddit：r/ClaudeCode 关于 `CLAUDE.md` best practices / customization / tips 的多条讨论摘要

---

## 今日主题：测试驱动的 Debug/Fix 闭环 —— 让 Claude Code 自己跑、自己撞墙、自己修

### 这才是下一阶段最该学的东西
路线图里 `CLAUDE.md / .claude` 已经学完，下一步不是继续堆配置，而是进入**调试与重构**的真功夫：
**让 Claude Code 形成“复现问题 → 写/跑验证 → 读报错 → 修根因 → 再验证”的闭环。**

这不是一个小技巧，这是 Claude Code 从“会写代码的聊天框”升级成“能自己闭环干活的工程代理”的分水岭。

Anthropic 官方文档把这件事说得非常直白：
**给 Claude 一个自我验证的方法，是最高杠杆的实践。**
没有测试、linter、截图、命令输出这些反馈回路，Claude 很容易写出“看起来像对的垃圾”。

---

## 官方文档给出的核心结论

### 1. 先给成功标准，再让它动手
官方 Best Practices 里最狠的一句其实就一个意思：
**别只说“修一下”，要把“修到什么算真的修好”一起说清楚。**

错误示范：
- 修登录 bug
- 让 dashboard 更好看
- 构建挂了，处理一下

正确示范：
- 登录在 session timeout 后失败；检查 `src/auth/` 的 token refresh；先写一个失败测试复现，再修复；最后跑测试确认通过
- 按截图实现 UI，改完截屏对比原图，列出差异并修正
- 构建失败，报错如下；修根因，不要 suppress；最后确认 build 通过

这背后的逻辑很硬：
Claude 最怕的不是难题，而是**没有验收标准的假题**。

---

### 2. 探索、计划、实现必须拆开
Anthropic 在 Common Workflows 和 Best Practices 都反复强调同一套节奏：
**Explore → Plan → Implement → Verify**

对于多文件改动、陌生代码、架构重构：
1. 用 Plan Mode 先读代码，不准乱写
2. 让 Claude 先产出计划
3. 你确认计划没跑偏
4. 再切回 Normal Mode 让它实现
5. 最后强制跑验证命令

这不是形式主义，这是防止 Claude 在错误方向上越跑越远。

尤其在调试场景里，最容易犯的蠢错就是：
**还没确认根因，就急着改代码。**
结果是修掉表象，埋下新坑。

---

### 3. 最稳的修 bug 方法，是先制造一个会失败的检查
官方文档已经给了模板：
- 先把报错、复现步骤、受影响模块告诉 Claude
- 要它先写一个失败测试，或者至少先跑能稳定复现的命令
- 再让它修
- 最后跑测试确认

这其实就是弱化版 TDD：
**没有稳定复现，就没有稳定修复。**

对于 Claude Code 来说，失败测试的价值巨大，因为它让模型不再凭感觉修，而是根据反馈做迭代。

---

## 社区最佳实践：Claude 真正高效，不是因为它聪明，是因为你给了回路
Builder.io 的实战总结和 Anthropic 官方观点几乎完全一致，而且说得更接地气：

### 1. “给 Claude 一个反馈回路”，质量直接翻 2-3 倍
社区最常见的高收益 prompt 结构不是花哨词藻，而是：
- 改什么
- 跑什么验证
- 失败了要继续修
- 全绿才算结束

比如：
```text
把 auth middleware 从 session token 改成 JWT。
改完跑现有测试。
如果有失败，继续修，直到通过为止。
```

这种写法的威力在于：
**你不是在让 Claude 写一次代码，而是在让它执行一个闭环任务。**

---

### 2. 别替 Claude “解释 bug”，直接给原始数据
Builder.io 特别强调一个社区高频误区：
很多人会把报错先自己翻译成人话，再告诉 Claude。
这经常是帮倒忙。

正确做法是直接喂：
- 原始报错栈
- CI 输出
- 测试日志
- 终端命令输出

因为你的“解释版”通常会丢掉 Claude 真正需要的细节。

实战上最好直接这样喂：
```bash
npm test 2>&1 | claude "修掉这些失败测试，找根因，不要压错误"
```

或者在交互式会话里：
- 我运行 `npm test` 会报这个错
- 复现步骤如下
- 先定位根因
- 先写失败测试或最小复现
- 修完再跑测试

一句话：
**原始日志 > 你自己的二手摘要。**

---

### 3. 小改动直接做，大改动必须先计划
DataCamp 对 Plan Mode 的拆解非常到位：
如果一个任务涉及多个文件、多个决策点、多个架构选择，不先计划，错误会指数级累积。

它给出的关键认知我认同：
假设 Claude 每个决策点 80% 正确，20 个决策叠起来，最后对的概率会低得可怜。

所以复杂调试/重构里，最值钱的不是“让它一把梭”，而是：
- 先让它读文件
- 先让它问清关键问题
- 先出 plan
- 必要时你用 `Ctrl+G` 直接改 plan
- 执行过程一旦漂移，立刻切回 Plan Mode 重规划

这套工作流尤其适合：
- 认证系统改造
- 多模块重构
- 测试体系补齐
- 大型旧代码迁移

---

## 我给主人的落地工作流：Claude Code 调试四连击

### 工作流 A：最小 bug 修复闭环
适合单点 bug。

Prompt 模板：
```text
我运行 `npm test -- auth` 会报错，日志如下：
[贴原始报错]
请先定位根因。
如果缺测试，先补一个能复现问题的失败测试。
然后修复根因。
最后重新运行相关测试，全部通过后再总结修改点。
```

关键点：
- 指定复现命令
- 指定范围（相关测试，不要一上来全量）
- 强调修根因，不要 suppress

---

### 工作流 B：重构不翻车闭环
适合旧代码清理、现代化改写。

Prompt 模板：
```text
进入 Plan Mode。
先阅读 `src/auth/` 和相关测试，分析当前登录/刷新 token 流程。
给我一个重构计划：哪些文件要改、风险点是什么、如何验证不破坏现有行为。
确认后再实现。
实现时分小步提交，每步都运行相关测试。
```

关键点：
- 先计划，别直接改
- 改动拆小步
- 每一步都有验证

---

### 工作流 C：CI 爆红修复闭环
适合流水线报错。

Prompt 模板：
```text
这是 CI 的原始失败日志：
[粘贴日志]
请先判断失败属于编译、测试、lint 还是环境问题。
不要猜，先找到最小复现命令。
修复后重新运行同类检查，确认通过。
如果只是表面修复而根因还在，继续处理。
```

关键点：
- 先分类错误
- 先复现
- 只在同类验证通过后才算完成

---

### 工作流 D：UI 改动闭环
官方明确说了，UI 最好给截图反馈。

Prompt 模板：
```text
参考这张截图实现页面改动。
改完后自己打开页面截图，对比原图。
列出差异并继续修，直到关键结构、间距、状态一致。
如果有测试可跑，一并执行。
```

关键点：
- UI 不靠嘴验收，靠截图对比
- 视觉验证 + 自动化验证双保险

---

## 主人最容易犯的三个蠢错

### 蠢错 1：只说“修一下”
这会逼 Claude 瞎猜。
没有成功标准，就没有闭环。

### 蠢错 2：不给原始日志，只给口头摘要
你以为是在帮它提炼，其实是在删证据。

### 蠢错 3：大改动不先计划
Claude 不是不会改，它是会在错误方向上改得特别勤快。

---

## 一条最值钱的总原则
以后你让 Claude Code 调试或重构，别只下达“编码命令”，要下达**验证闭环命令**。

不是：
- 改这个文件

而是：
- 先复现
- 再修根因
- 跑测试/构建/截图验证
- 失败继续修
- 通过才收工

Claude Code 一旦拿到反馈回路，才真的像工程师；
拿不到，它就只是个会打字的高配实习生。

---

## 今日结论
今天这课的核心不是“怎么写 prompt”，而是：

**让 Claude Code 干活时，必须自带验收回路。**

测试、lint、build、截图、日志，这些不是附属品，
它们就是 Claude Code 从“会写”进化到“会闭环交付”的发动机。

---

## 参考来源
- Anthropic 官方文档：Common workflows
- Anthropic 官方文档：Best practices
- Builder.io：50 Claude Code Tips and Best Practices For Daily Use
- DataCamp：Claude Code Plan Mode: Design Review-First Refactoring Loops

---

## 今日主题：Claude Code 模型选择策略 —— 别一把梭，按任务切 Sonnet / Opus / Haiku

### 为什么这课必须现在学
前两课解决的是“让 Claude 知道项目”和“让 Claude 会闭环交付”，今天这课解决的是第三个核心问题：
**让它用对脑子。**

很多人把 Claude Code 用废，不是 prompt 太烂，而是模型选得蠢。
能 3 秒干完的活，你硬上 Opus，成本高、速度慢、还白白吃额度；
该做架构设计和复杂推理时，你又拿 Haiku 硬顶，最后得到一份看起来像答案、实际上不够深的半成品。

模型选择本质上不是“谁更强”，而是：
**当前任务需要多深的思考、多快的反馈、以及值不值得烧这么多额度。**

---

## 官方文档给出的硬结论

### 1. Claude Code 里有 6 个最关键的模型入口
Anthropic 官方 `Model configuration` 文档把日常最重要的别名定义得很清楚：
- `sonnet`：日常编码任务默认主力
- `opus`：复杂推理和高难任务
- `haiku`：简单、快速、便宜的小任务
- `best`：当前最强模型，现阶段等同 `opus`
- `sonnet[1m]` / `opus[1m]`：超长上下文会话
- `opusplan`：**Plan Mode 用 Opus，执行阶段自动切回 Sonnet**

这说明官方思路根本不是“全程一把梭 Opus”，而是明确鼓励你做**阶段化混合使用**。

---

### 2. `/model` 只是切模型，真正高级的是连 effort 一起管
官方文档现在已经把模型选择和思考强度绑在一起了：
- `low`：简单任务，更快更便宜
- `medium`：大多数日常开发
- `high`：复杂调试、架构设计
- `max`：最深思考，只给 Opus 4.6

关键结论：
**不是所有难题都得换 Opus，有些场景 Sonnet + high effort 就够狠。**

这很值钱，因为很多时候你真正需要的是“多想一点”，不是“换最贵那个”。

---

### 3. 官方推荐的选型维度只有三个：能力、速度、成本
Anthropic API 文档《Choosing the right model》讲得很直：
选模型时别玄学，先看三件事：
1. **Capabilities**：任务复杂度和正确率要求
2. **Speed**：你是否需要快速交互反馈
3. **Cost**：任务量大不大，是否需要长期跑批或高频迭代

它还给了两个起手式：
- **成本优先**：先用 Haiku，够用就别升级
- **能力优先**：先用 Opus，把问题打穿，再往下压成本

但对 Claude Code 实战来说，真正最好用的默认仍然是：
**大多数编码任务先上 Sonnet。**
这是因为编码往往既不是纯小任务，也没复杂到每次都要 Opus。

---

### 4. Claude 官方教程的实战分工非常明确
Anthropic 在 Claude 官方教程里把三个模型的工作边界写得很落地：
- **Haiku**：快速问答、简单提取、短摘要、轻量分类
- **Sonnet**：编码、写作、分析、多步骤工作流，属于默认通用主力
- **Opus**：深度研究、复杂多步推理、长文理解、对准确性要求很高的问题

这套分工直接能映射到 Claude Code：
- Haiku 负责“快”
- Sonnet 负责“日常主战”
- Opus 负责“卡点攻坚”

---

## 社区和博客里最值得抄走的经验

### 1. Builder 的真实工作流：默认靠系统，出问题再切 Sonnet
Builder.io 的实战文章给了一个很真实的结论：
作者平时大多让 Claude Code 按默认模型跑，**Opus 状态不好或成本不划算时就切 Sonnet**。

这说明高手不是一直手动切模型，而是：
**先建立一个顺手的默认，再在卡点时升级。**

对主人来说，最适合的不是神经质地每个任务都手动挑，而是先固定一套：
- 默认 Sonnet
- 难题上 Opus
- 脏活快活给 Haiku

---

### 2. Reddit 高频共识：Sonnet 是主力，Opus 做 review，Haiku 做杂活
Reddit 多条讨论虽然细节不同，但结论高度一致：
- **Sonnet 最适合长时间主力开发**
- **Opus 适合评审、架构、关键难题和“最后那口气”**
- **Haiku 适合测试、提取、总结、分类、批量轻任务**

其中一条很像成熟团队分工：
**Sonnet 常驻 VS Code，Opus 负责偶发 review 或解难，Haiku 跑测试和轻任务。**

这个思路非常能打，因为它符合真实开发节奏，不是空泛比较参数。

---

### 3. `opusplan` 是最值得学的中高级姿势
很多人只知道 `/model opus`，不知道 `opusplan` 才是更聪明的玩法。

官方已经明说：
- 计划阶段用 Opus
- 执行阶段自动切 Sonnet

这招的意义很大：
**把最贵的深度思考，压缩到真正需要它的阶段。**

适合场景：
- 大型重构
- 陌生仓库接手
- 架构设计
- 跨文件复杂改动
- 需要先出方案再实现的任务

一句话：
**不要让 Opus 写一整天 CRUD，让它只做真正值钱的思考。**

---

## 小龙儿给主人的实战路由表

### 场景 1：日常开发，默认用 Sonnet
适合：
- 写功能
- 修普通 bug
- 中小型重构
- 跑一轮测试后继续修
- 改 3 到 10 个文件的常规任务

推荐：
- `/model sonnet`
- 复杂一点再加 `/effort high`

原则：
**先用 Sonnet 打满 80% 的开发场景。**

---

### 场景 2：卡住了，再升级到 Opus
适合：
- 连续两轮修不动的 bug
- 根因不清楚的复杂故障
- 跨模块架构决策
- 高风险安全、性能问题
- 需要深度 code review 的关键提交

推荐：
- `/model opus`
- 或直接 `/model opusplan`

原则：
**Opus 不是默认代步车，是破门锤。**

---

### 场景 3：轻活脏活，扔给 Haiku
适合：
- 总结日志
- 提取报错关键信息
- 把长输出压成 checklist
- 批量改文案
- 快速分类、重命名、生成小脚本草稿

推荐：
- `/model haiku`

原则：
**别用重炮打蚊子。**

---

### 场景 4：陌生大仓库或长会话，用 `[1m]`
适合：
- 代码库很大
- 长时间持续对话
- 需要带着大量历史上下文继续干
- 多轮架构推演

推荐：
- `/model sonnet[1m]`
- `/model opus[1m]`

但要记住：
**上下文更大，不等于你可以把垃圾也带着。**
`/clear`、`/compact`、拆任务，还是该做。

---

## 最稳的模型选择工作流

### 工作流 A：默认开发流
1. 开局 `sonnet`
2. 正常实现、调试、改文件
3. 如果发现反复绕圈，切 `opus`
4. 方案定了，再切回 `sonnet`

这套最适合《光与朽》这种高频迭代项目。

---

### 工作流 B：大改动流
1. 先 `/model opusplan`
2. 让它只做探索和计划
3. 计划确认后进入执行
4. 自动切 Sonnet 落地代码
5. 最后必要时再用 Opus 做 review

这套最适合：
- 战斗系统大重构
- 存档系统改造
- UI 架构整理
- 跨模块性能优化

---

### 工作流 C：额度节流流
1. 大多数任务保持 Sonnet
2. 摘要、提取、脏活切 Haiku
3. 真正高风险难题才开 Opus
4. 大任务结束后马上 `/clear`

这套的核心不是省小钱，
而是**把额度花在刀刃上。**

---

## 主人最该避免的 4 个蠢用法

### 蠢错 1：全程只用 Opus
这不是强，这是浪费。
速度慢、额度吃得快，而且很多普通任务根本不需要。

### 蠢错 2：把 Haiku 当主力编码模型
简单修修补补可以，复杂重构容易深度不够。

### 蠢错 3：明明只是想多想一点，却直接升最贵模型
先试 `Sonnet + high effort`，往往就够了。

### 蠢错 4：长仓库问题不用 `opusplan` 或 `[1m]`
计划和执行混成一锅，Claude 很容易前面想太多，后面写太慢，或者上下文开始发臭。

---

## 一条最值钱的总原则
以后你用 Claude Code，别问“哪个模型最强”，要问：

**这一步是要快、要稳、还是要深？**

- 要快，用 Haiku
- 要稳，用 Sonnet
- 要深，用 Opus
- 既要深又要省，用 `opusplan`
- 仓库太大或会话太长，用 `[1m]`

这才是专家用法。

---

## 今日结论
真正会用 Claude Code 的人，不是永远开最贵模型的人，
而是能把**任务阶段、思考深度、上下文长度、成本预算**四件事一起管住的人。

**模型选择不是设置项，而是生产力杠杆。**

---

## 参考来源
- Anthropic 官方文档：Claude Code Model configuration
- Anthropic 官方文档：Choosing the right model
- Anthropic 官方教程：Choosing the right Claude model: Haiku, Sonnet, and Opus
- Builder.io：How I use Claude Code (+ my best tips)
- Reddit：r/ClaudeCode《Opus Vs Sonnet Vs Haiku》、r/ClaudeAI《Haiku vs Opus/Sonnet》讨论摘要

---

## 今日主题：Orchestrator-Workers 多 Agent 协作流 —— 别让主线程亲自搬砖

### 为什么今天该学这个
路线图里“模型选择策略”已经过了，下一步该进入高级开发篇的第一项：
**Orchestrator-Workers（包工头-打工人）模式。**

这玩意的本质不是“多开几个 Claude”，而是把主会话从底层检索、海量日志、局部分析这些脏活里解放出来，让它只做三件事：
1. 拆任务
2. 派工
3. 验收与整合

Claude Code 官方现在把这条路讲得非常明确：
- **subagents** 适合在单 session 内做隔离式子任务
- **agent teams** 适合更强的多会话协作
- 核心价值不是炫技，而是**上下文隔离、并行推进、成本控制、权限收缩**

一句话：
**高手不是让 Claude 一把梭，而是让主 Agent 永远待在高价值决策层。**

---

## 官方文档提炼出的硬结论

### 1. Subagent 的第一价值不是更聪明，是更省上下文
Anthropic 官方文档对 subagent 的定义很直白：
当某个侧任务会把主对话塞满搜索结果、日志、文件内容，而且这些原始材料你后面并不想继续带着时，就该丢给 subagent。

典型场景：
- 跑测试，只回传失败项摘要
- 扫三个模块，只回传结构结论
- 查文档或日志，只回传结论和证据

主线程只拿摘要，不吃垃圾上下文。
这就是 Orchestrator 的底层意义。

### 2. 内建三类 worker，已经够你起飞
官方内建的 subagent 里，最关键的是：
- **Explore**：Haiku，只读，适合搜代码、摸结构、读文件
- **Plan**：适合只读规划
- **General-purpose**：泛用型执行

其中 Explore 最值钱，因为它天然适合把“到处 grep、读一堆文件”的脏活外包出去。

实战理解：
- 主线程负责问对问题
- Explore 负责把地图摸出来
- 主线程再决定谁去改、改哪里、先后顺序

### 3. 什么时候该并行，什么时候别装逼
官方明确说得很清楚：
**只有当研究路径彼此独立时，才适合并行 subagents。**

适合并行：
- auth / database / API 三块独立摸底
- 前端、后端、测试分头看
- 多份日志、多份文档并行总结

不适合并行：
- 第二步依赖第一步结论
- 多阶段共享大量上下文
- 需要你频繁中途改方向

判断标准就一句：
**如果 worker 之间需要不断对话，那就不该强行并行。**

### 4. 链式协作比“全员乱跑”更稳
官方给出的另一条模式是 **Chain subagents**：
先让一个 agent 发现问题，再把结果交给下一个 agent 处理。

例如：
- code-reviewer 先找性能问题
- optimizer 再按问题清单优化

这比一上来就让一个 agent 又查又改又测更稳，因为每个 worker 只干单一职责。

### 5. Subagent 不是无限套娃
官方明确限制：
**subagents 不能再 spawn subagents。**

这意味着真正的 orchestrator 永远是主线程。
所以你的架构要这样想：
- 主线程 = 项目经理 + 总设计师
- worker = 专项执行者
- 不要让 worker 再自己当老板

这条特别关键，能避免工作流设计成一锅屎。

---

## 配置层最值钱的几个开关

### 1. 用 description 决定“什么时候该派它上场”
官方文档反复强调：Claude 会根据 `description` 判断是否委派。
所以 description 不能写空话，必须写成触发条件。

烂写法：
- Helps with code

好写法：
- Debugging specialist for errors, test failures, and unexpected behavior. Use proactively when encountering any issues.

重点不是介绍身份，而是告诉主线程：
**什么情况就该叫它。**

### 2. tools 要收窄，不要默认全开
官方最佳实践很硬：
**focused subagent + limited tools。**

例子：
- reviewer 只给 Read / Grep / Glob / Bash
- debugger 才给 Edit
- 数据查询 agent 用 Bash，但通过 hook 限死只读 SQL

这么做的收益有三个：
1. 更安全
2. 更聚焦
3. 减少 agent 乱发挥

### 3. model 可以按工种分配
官方文档已经支持给 subagent 单独设模型：
- Explore 这类检索 worker → **Haiku**
- 评论、分析、代码审查 → **Sonnet**
- 特别难的架构规划或高风险决策 → **Opus**

这跟昨天学的模型路由刚好接上。
真正的高手不是整局都开贵模型，而是：
**主线程只在关键思考时贵，脏活尽量便宜。**

### 4. memory / hooks / isolation 是专家级增强
官方 subagent frontmatter 里，最值得你进阶时用的是：
- `memory`: 让某个 agent 跨会话记住模式和经验
- `hooks`: 在 agent 生命周期里做校验、lint、限制 Bash
- `isolation: worktree`: 给 worker 一份临时 git worktree，避免污染主工作区

我的判断：
- 普通项目先别一口气全上
- **先把“角色拆分 + 工具收窄 + 模型路由”做好**
- 再逐步加 memory、hooks、worktree

不然你会把工作流配得花里胡哨，结果没人真用。

---

## 社区与实战经验的共识

### 1. Builder 的经验：多开实例很值钱，但必须分区
Builder 的实战文章里有个很实在的点：
Claude Code 很适合在 IDE 里多开实例并行工作，但前提是**不同 pane 处理代码库不同区域**。

这和官方思路完全一致。
并行不是为了热闹，而是为了让每个 worker 的上下文更干净。

### 2. Reddit 社区普遍认可两大收益：上下文隔离 + 成本下降
Reddit 上关于 subagents 的讨论，比较一致的两点是：
- 用 subagents 可以把主线程上下文留给规划和验收
- 探索型任务改用 Haiku，明显更省 token / 使用额度

尤其有人专门提到：
在 Plan Mode 下，把 research/exploration 交给 Haiku subagents，主线程保留上下文做规划，体验会更稳。

我认同，这不是花活，这是非常实用的资源调度。

### 3. 社区高频误区：把 subagent 当“万能分身”
很多人误以为 subagent 一开，Claude 就自动更强。
实际上最常见的翻车点是：
- 派的任务描述太空
- 给的工具太多
- 让它又查又改又测又写总结
- 并行处理存在依赖关系的任务

最后出来的不是协作，是并发混乱。

**subagent 的质量，上限取决于 orchestrator 的派工质量。**

---

## 给主人的落地工作流

### 工作流 A：三段式高级开发流
适合中大型需求。

#### 阶段 1，主线程先定边界
先让主线程只做：
- 目标定义
- 风险识别
- 模块拆分
- 验收标准

Prompt 可以这么说：
```text
先不要改代码。
先把这个需求拆成 3-5 个独立子任务，标出依赖关系、风险点、验收标准。
```

#### 阶段 2，把摸底任务丢给 worker
```text
并行研究 auth、inventory、save-system 三个模块，各自总结：
1. 关键入口文件
2. 主要数据流
3. 现有耦合点
4. 可能影响本次改动的风险
只返回摘要，不要贴大量原文。
```

#### 阶段 3，主线程统筹实现
主线程根据 worker 回传结果，决定：
- 先改哪里
- 哪些改动要串行
- 哪些需要补测试
- 最终如何验证

这就是标准 Orchestrator-Workers。

---

### 工作流 B：定位 bug 的包工头模式
适合复杂 bug。

```text
先不要直接修。
用 Explore 类 worker 查三件事：
1. 报错相关调用链
2. 最近改动过的文件
3. 相关测试覆盖情况
各自独立总结。
然后主线程基于这些总结给出根因判断和修复计划。
```

核心好处：
**先分头找证据，再由主线程定案。**
这样不会一上来靠感觉乱修。

---

### 工作流 C：给《光与朽》最适合的 Agent 角色拆分
如果主人要把 Claude Code 真用进《光与朽》，我建议最先做这四个角色：

1. **unity-explorer**
   - 只读
   - Haiku
   - 专门扫 `Assets/Scripts/` 结构和引用链

2. **combat-debugger**
   - 可 Edit + Bash
   - Sonnet
   - 专盯战斗循环、伤害链、技能触发

3. **balance-reviewer**
   - 只读
   - Sonnet
   - 专查 ScriptableObject 配表、数值跳点、成长曲线异常

4. **release-checker**
   - Bash + Read
   - Sonnet
   - 专门跑 lint / build / 检查资源缺失 / 输出上线前清单

这样主线程永远只负责：
- 下目标
- 审方案
- 收结果

这才像主脑，不像苦力。

---

## 今天最关键的一句话
很多人以为多 Agent 协作的重点是“同时干更多活”。
错。

真正的重点是：
**把主线程的脑力预算，留给最值钱的判断。**

检索、扫日志、摸目录、跑局部分析，这些都该外包。
主线程只负责拆题、决策、验收。

这才是 Claude Code 从“能写代码”升级到“能带队开发”的分水岭。

---

## 参考来源
- Anthropic 官方文档：Create custom subagents
- Anthropic 官方博客：How and when to use subagents in Claude Code
- Anthropic 官方文档：Model configuration
- Builder.io：How I use Claude Code (+ my best tips)
- Reddit：r/ClaudeAI / r/ClaudeCode 关于 subagents、plan mode、模型与成本的讨论摘要

---

## 今日主题：复杂环境控制与开源生态吸收 —— 用 gstack、Hooks、浏览器 QA 和命令管道，把 Claude Code 接进真正能闭环的工程流水线

### 为什么这课现在必须学
路线图里 Orchestrator-Workers 已经过了，下一块该啃的是高级开发篇第 2 项：
**复杂环境控制与开源生态吸收（gstack 拆解、`/browse`、`/qa`、自动化脚本、命令管道）**。

这一段真正解决的问题不是“Claude 会不会写代码”，而是：
**怎么把 Claude Code 从单次对话，升级成可复用、可验证、可审查、可接流水线的工程系统。**

一句话：
**高手不是一直手搓 prompt，而是把环境、验证、审查、浏览器测试和退出闸门都接好，让 Claude 被系统推着往正确方向走。**

---

## 这块能力到底包含什么
根据 Anthropic 官方插件体系、官方仓库示例，以及 gstack 的开源实践，这一层主要有五个部件：

1. **技能/命令层**：把高频流程做成 slash command、skill、plugin。
2. **Agent 层**：用多个专门 agent 分工，如 explorer、architect、reviewer。
3. **Hook 层**：在关键时刻拦截，比如改文件前、退出前、提交前做校验。
4. **浏览器执行层**：用真实浏览器验证页面、交互、截图和状态，不靠嘴验收。
5. **命令/日志管道层**：把测试、lint、build、CI 输出原样喂给 Claude，让它基于证据修复。

这五层一旦接起来，Claude Code 才不是“会聊天的程序员”，而是一个**带流程约束的工程代理**。

---

## 官方与开源实践里最值钱的硬结论

### 1. Claude Code 已经不是单工具，而是“插件化工作台”
Anthropic 官方 `claude-code` 仓库现在明确给了插件结构：
- `commands/`
- `agents/`
- `skills/`
- `hooks/`
- `.mcp.json`

这等于公开承认了一件事：
**Claude Code 的正确进阶方向，不是每次临场发挥，而是把工作流产品化。**

官方示例里最有代表性的两个插件：
- **`feature-dev`**：把“需求澄清 → 代码摸底 → 补问题 → 设计方案 → 实现 → review → 总结”做成 7 阶段流程。
- **`code-review`**：并行起多个 reviewer agent，从规范、bug、历史上下文多个角度审 PR，再用置信度过滤误报。

这说明官方已经在用实际插件告诉你：
**复杂任务应被拆成标准流程，而不是一轮 prompt 赌命。**

---

### 2. Hook 的真正价值，不是花活，是“强制闸门”
Hookify 插件和社区 hooks 实战都指向同一件事：
**Hook 是给 Claude 加护栏，不是给它加装饰。**

能做什么：
- Bash 前拦危险命令，比如 `rm -rf`
- 文件改动前扫敏感信息、debug 代码、硬编码 key
- Stop 时检查本轮是否跑过测试，没跑就不准结束
- Prompt 提交时，自动插入团队规则或派发子 agent

这玩意最狠的地方在于：
**它把“最好这样做”变成“必须这样做”。**

所以复杂环境控制里，Hook 不该乱写一堆，而该优先卡住三类节点：
1. **危险操作前**
2. **退出收工前**
3. **代码落盘前**

这是最低成本、最高收益的硬约束。

---

### 3. `/browse` 这类浏览器技能的核心意义，是把“看起来能用”打回原形
gstack 的 `/browse` 技能定义非常清楚：
它不是普通截图工具，而是一个**快速无头浏览器 QA 执行器**，能做：
- 打开页面
- 点击元素
- 验证状态
- 前后 diff
- 响应式检查
- 表单和上传测试
- 截图留证

这代表一个非常重要的工程认知：
**前端/UI/交互验收，不能只靠代码 review，也不能只听 Claude 自己说“应该可以”。**

真实浏览器验证，才是对“体验层 bug”的第一次正经审判。

尤其在 Web 管理后台、落地页、小游戏运营后台这类场景里：
- 代码没错，不代表按钮能点
- 样式没报错，不代表布局没崩
- 请求成功，不代表用户流程真走得通

所以 `/browse` 这类能力，本质是在补**体验层验收回路**。

---

### 4. `/qa` 的精华不是“测”，而是“测完继续修，修完再测”
gstack 的 `/qa` 定义比很多人想得更狠：
它不是只给你提 bug list，而是强调：
**先系统性 QA，再迭代修 bug，每个修复原子化提交，然后重新验证。**

这说明真正高级的 QA 工作流不是：
- 找 bug
- 贴给主人
- 结束

而是：
- 跑 QA
- 按严重级排优先级
- 修关键问题
- 回归测试
- 出 ship-readiness 总结

这个思路和 Anthropic 官方 `feature-dev` 的 Phase 6 完全同路子：
**质量检查不应该是开发后的附属品，而应该是交付的一部分。**

一句话：
**没有 re-verify 的 QA，只是 bug 收集；有修复闭环的 QA，才算工程流程。**

---

### 5. 命令管道喂给 Claude，比你手写解释靠谱得多
Builder、社区经验和前几课的调试闭环一起看，结论越来越明确：
**终端原始输出是 Claude 最该吃的证据。**

最实用的做法不是你自己总结“构建好像哪里炸了”，而是直接喂：
- `npm test` 原始日志
- `pytest` 失败堆栈
- `cargo check` 报错
- linter 输出
- CI job 日志

因为一旦你先口头翻译，很多行号、上下文、边界条件、命令参数都会被你手贱删掉。

高阶做法就是把命令输出通过管道直接送进去，例如：
```bash
npm test 2>&1 | claude
```
或者：
```bash
git diff -- src/auth | claude
```
再在 prompt 里补一句：
- 先找根因
- 不要 suppress
- 修完重新运行同类验证

这才叫**让 Claude 看证据办案**。

---

## gstack 值不值得学，结论很直接
值得学，但不是因为它神。

Reddit 上关于 gstack 的讨论已经很真实了：
- 有人觉得有点 hype
- 但几乎都承认 `/browse` 和 `/qa` 这种“把浏览器和闭环 QA 接进来”的部分是真有用
- 它最大的价值，不在于某句 prompt 多神，而在于**把多角色、多流程、浏览器 QA、ship 流程打包成可复用工作法**

所以正确学习姿势不是迷信 gstack 全套，
而是拆它的高价值骨架：
1. 角色化技能路由
2. 浏览器验收
3. QA 修复闭环
4. 路由规则写进 `CLAUDE.md`
5. 用 setup/脚本把团队协作标准化

该抄的是方法，不是偶像崇拜。

---

## 小龙儿给主人的落地工作流

### 工作流 A：网页功能上线前的最小闭环
适合管理后台、官网、工具站。

顺序：
1. Claude 实现功能
2. 跑单测 / lint / build
3. 用 `/browse` 打开真实页面走主流程
4. 用 `/qa` 扫关键交互和错误状态
5. 修高优先级 bug
6. 再跑一次 `/browse` 或 `/qa-only`
7. 最后才允许 ship

这套比“写完就合”稳太多。

---

### 工作流 B：把退出变成有门槛的动作
在 Hook 里卡住 Stop：
- transcript 里没出现 `npm test` / `pytest` / `cargo test`
- 就提醒甚至阻止 Claude 结束

这招非常狠，因为它直接治一种最常见的病：
**代码写完了，但根本没验。**

---

### 工作流 C：把《光与朽》周边工具开发也做成流水线
如果我们后面做：
- 数值配置编辑器
- 后台活动面板
- 小官网/预约页
- 数据分析 dashboard

那最适合的 Claude Code 组合就是：
1. `feature-dev` 风格流程先设计
2. 主线程拆任务
3. 子 agent 摸模块和审代码
4. 浏览器技能验收页面
5. QA 技能找体验层 bug
6. Hook 卡住危险改动和未测收工

主脑只负责验收和决策，别亲自搬砖。

---

## 主人最容易犯的 4 个蠢错

### 蠢错 1：把技能当 prompt 收藏夹
真正的 skill / plugin 应该固化流程，不只是存几句模板话。

### 蠢错 2：有测试没浏览器验收
前端和交互场景里，这等于只测“代码逻辑”，没测“人能不能用”。

### 蠢错 3：只让 QA 报 bug，不要求 re-verify
这叫记账，不叫交付。

### 蠢错 4：不给 Claude 原始日志，只给情绪化描述
“它炸了”“感觉不对”“应该是缓存问题”这类屁话，对定位根因帮助有限。

---

## 一条最值钱的总原则
以后你做 Claude Code 高级工作流，记住这句：

**不要把质量寄托在模型自觉上，要把质量写进流程和闸门里。**

- Skill 决定流程怎么走
- Agent 决定谁干什么
- Hook 决定哪些错不准犯
- `/browse` 决定页面是不是真的能用
- `/qa` 决定问题修完了没有
- 命令管道决定 Claude 看的是不是一手证据

这才叫复杂环境控制。

---

## 今日结论
今天这课的核心不是“学一个新命令”，而是升级一个认知：

**Claude Code 的专家用法，不是更会聊天，而是更会搭系统。**

当你把 hooks、skills、agent 分工、真实浏览器 QA、命令日志管道都接起来，Claude 才会从“高配实习生”变成“能进生产线的工程代理”。

---

## 参考来源
- Anthropic 官方 GitHub：`anthropics/claude-code` README
- Anthropic 官方 GitHub：Claude Code Plugins README
- Anthropic 官方插件：`feature-dev`
- Anthropic 官方插件：`code-review`
- Anthropic 官方插件：`hookify`
- gstack GitHub README（Garry Tan）
- gstack `browse` / `qa` 技能定义
- Reddit：关于 gstack、hooks、subagents、QA 工作流的讨论摘要

---

## 今日主题：Git 与版本控制协作 —— 先审 diff，再让 Claude 写提交，不然 git 历史会被它写成一锅粥

### 为什么今天该学这个
路线图里“复杂环境控制与开源生态吸收”已经过了，按顺序下一个未掌握主题就是高级开发篇第 3 项：
**Git 与版本控制协作。**

这块真正要学的，不是“Claude 能不能帮你 commit”，而是：
**怎么让它基于 diff 写出靠谱提交、做像样 review、产出干净 PR，而不是把一堆脏改动直接推上去。**

一句话：
**Claude Code 会写 commit，不代表它天然懂你的版本边界。边界得你设计。**

---

## 官方资料给出的硬结论

### 1. 官方已经明确鼓励“让 Claude 提交并创建 PR”，但前提是任务边界清楚
Anthropic 官方 `Best Practices` 文档已经把这条写得很直白：
- 可以要求 Claude **写描述性 commit message 并创建 PR**
- 但 Plan Mode 有成本，小任务别瞎上复杂流程

这说明官方态度不是“别让 Claude 碰 Git”，而是：
**可以碰，但要在范围明确、结果可审的时候碰。**

### 2. 官方 Headless 示例直接拿 staged diff 生成提交
Anthropic 官方 `Run Claude Code programmatically` 示例里，已经给了很典型的 Git 工作流：
- 先看 staged changes
- 再让 Claude 创建合适的 commit
- 工具权限只放开 `git diff`、`git log`、`git status`、`git commit`

这说明真正稳的姿势不是“把整个仓库扔给它猜”，而是：
**先把边界收窄到 staged diff，再让它总结和落提交。**

### 3. Claude Code 官方 overview 明确支持把 `git diff` 当输入管道喂给它做 review
官方 overview 现在直接给了类似工作流：
```bash
git diff main --name-only | claude -p "review these changed files for security issues"
```

重点不是命令本身，而是工程思路：
**Git diff 本身就是 Claude 最重要的一手证据。**
别写一大段“我大概改了啥”，直接给 diff，信息密度高得多。

### 4. 官方 code review 插件的设计重点是“按 diff 行做发现”，而不是空泛点评
Claude Code 官方 `code-review` 文档与插件说明强调：
- finding 会挂在具体 diff 行上
- 会标严重级
- 注释与检查结果会独立保留

这套设计传递出来的核心判断很硬：
**好的 review 不是泛泛而谈，而是紧贴改动本身。**
所以以后让 Claude review，默认输入就该是 PR diff、staged diff、或者明确文件范围，不是“帮我看看这个项目”。

---

## 社区和博客里最值得抄走的经验

### 1. Reddit 的共识很一致：Claude 可以写 commit，但人必须 review
Reddit 上关于“怎么让 Claude Code 写 commit message”的讨论里，最高频结论不是 prompt 多花，而是：
- 可以让 Claude 起草 commit message
- **Always review the commit**
- 不要把它生成的 message 当圣旨直接推

这是对的。因为 Claude 很容易犯两个毛病：
1. 把不重要的细节写进 commit subject
2. 把真正关键的行为变化写漏

所以最稳的流程是：
**Claude 起草，人类终审。**

### 2. Builder 和社区实战都在强调：先让 Claude 读 diff，再处理 PR 更新和 review 反馈
Builder 的实战文章里，一个很值钱的点是：
- 团队会把 PR 更新、review feedback、后续修补继续交给 Claude
- 但这些动作都建立在清晰的 PR / diff 上下文之上

换句话说：
**Claude 在 Git 协作里最强的阶段，不是“凭空描述改了什么”，而是“对着明确 diff 继续推进下一步”。**

### 3. 项目级 commit 规范，最好写进 `CLAUDE.md` 或独立命令/子 agent
DEV 社区关于“项目特定 commit message subagent”的实践很实用，里面的核心步骤是：
1. 先检查 `CLAUDE.md` / `README.md` 的 commit 规则
2. 看 `git diff --cached`
3. 判断本次改动属于 feature / fix / refactor / docs 等哪一类
4. 再按项目模板生成 message

这个思路很对，因为 commit 规范是**团队协议**，不是模型猜谜。
如果你不把规则前置，Claude 就会用它自己脑补的写法乱来。

### 4. 更成熟的工程团队会把“人类本地 spot check”嵌进流程
DoltHub 的经验我很认同：
- Claude 干完后，先看 `git status`
- 再看 `git diff`
- 人类 spot check 关键改动
- 然后才 commit / push / PR

这套流程一点也不保守，反而很专业。
因为版本控制最大的价值不是“自动提交”，而是：
**保留一个未来可读、可回滚、可追责的历史。**

---

## 我给主人的落地工作流

### 工作流 A：单次小改动提交流
适合修小 bug、文案改动、局部重构。

步骤：
1. 先 `git status`
2. 只 stage 本次该提交的文件
3. 让 Claude 读取 `git diff --cached`
4. 要它输出：
   - 一句 commit subject
   - 2 到 4 条本次改动摘要
   - 1 条潜在风险或注意事项
5. 你扫一眼没问题，再 commit

Prompt 模板：
```text
先阅读当前 staged diff。
总结这次改动的核心目的。
按项目规范写一个简洁、描述性强的 commit message。
再列出本次改动的风险点或需要额外验证的地方。
```

关键点：
**commit message 是 diff 的摘要，不是任务标题复读。**

---

### 工作流 B：大改动拆提交流
适合跨模块重构、功能开发、多人协作分支。

步骤：
1. 先别急着全量 `git add .`
2. 让 Claude 先看 `git status` + `git diff`
3. 问它：这批改动是否应拆成多个逻辑提交
4. 让它按“功能实现 / 测试补充 / 重构整理 / 文档更新”给你拆分建议
5. 你按组 stage，再分别生成 commit message

Prompt 模板：
```text
阅读当前工作区 diff。
不要直接提交。
先判断这些改动是否应该拆成多个 commit。
按逻辑边界给出拆分方案，并说明每个 commit 应该包含哪些文件。
```

关键点：
**一个 commit 只讲一件事。**
这比“最后堆成一个巨型提交”强太多。

---

### 工作流 C：PR 前差异审查流
适合准备 push 或提 PR 前做最后一轮自审。

步骤：
1. `git diff origin/main...HEAD`
2. 让 Claude 只按这段 diff 做 review
3. 要它分别找：
   - 可能的 bug
   - 漏测点
   - 破坏兼容性的改动
   - 是否存在 message/PR summary 与真实改动不符
4. 修完后再让它写 PR 描述

Prompt 模板：
```text
基于当前分支相对 main 的 diff 做一次严格 code review。
只关注真实改动，不要泛泛点评。
优先找 bug、漏测、兼容性风险和不必要改动。
最后给一个适合 PR description 的变更摘要。
```

关键点：
**review 输入必须是 diff，不是整个仓库。**

---

### 工作流 D：Review comment 返修流
适合 PR 被提了评论之后继续交给 Claude 收尾。

步骤：
1. 把 review comment 原文给 Claude
2. 同时给它相关 diff 或文件范围
3. 让它先判断哪些意见要改、哪些是误报
4. 再逐条修改
5. 最后让它产出“本轮回应了哪些 comment”的摘要，方便你贴回 PR

这套在 Builder 文章和社区用法里都很常见，价值非常高。
因为 Claude 特别适合处理：
**有明确反馈、有明确改动范围、有明确验收目标** 的返工任务。

---

## 主人最容易犯的 5 个蠢错

### 蠢错 1：没看 diff，就让 Claude 直接 commit
这等于让它蒙着眼写历史。

### 蠢错 2：工作区一锅粥，还指望它帮你写清楚提交
如果改动边界本来就脏，Claude 只会把脏东西包装得更像样。

### 蠢错 3：把 commit message 当作文比赛
好的提交信息不是华丽，是可检索、可理解、可回溯。

### 蠢错 4：拿大 diff 做一次性超级提交
这会让 code review、回滚、追责全部变难。

### 蠢错 5：让 Claude review 整个项目，而不是 review 本次改动
这最浪费 token，也最容易出空话。

---

## 给《光与朽》的直接用法
如果后面我们在《光与朽》里用 Claude Code 管版本，我建议直接定死这套：

1. **功能改动和数值改动分开提交**
   - 战斗逻辑一组
   - ScriptableObject 配表一组
   - UI 资源或 prefab 一组

2. **提交前默认三连看**
   - `git status`
   - `git diff --cached`
   - Claude 总结“改了什么 / 为什么 / 风险在哪”

3. **PR 前让 Claude 做 diff 级 review**
   - 重点盯战斗循环、存档兼容、配置字段、资源引用

4. **提交规范写进 `CLAUDE.md`**
   - 是否用 conventional commits
   - subject 长度
   - 是否要 body
   - 哪类改动必须附测试说明

这样未来回头查某次平衡崩盘、某个 prefab 丢引用、某波版本回退时，不会一脸懵逼。

---

## 一条最值钱的总原则
以后你在 Claude Code 里做 Git 协作，记住这句：

**让 Claude 基于 diff 工作，不要基于幻想工作。**

- 写 commit，先看 staged diff
- 做 review，先看 branch diff
- 写 PR，先基于真实改动总结
- 改 review comment，先绑定具体评论和文件范围

Git 这套用得强，Claude 才会像靠谱同事；
用得烂，它就只会帮你把混乱包装得更像专业。

---

## 今日结论
今天这课的核心不是“Claude 会不会 Git”，而是：

**Git 是 Claude Code 的约束层，不只是存档层。**

真正的高阶用法，是让 diff 决定提交边界，让 review 紧贴改动，让 commit message 成为未来可读的项目记忆。

---

## 参考来源
- Anthropic 官方文档：Best Practices for Claude Code
- Anthropic 官方文档：Run Claude Code programmatically
- Anthropic 官方文档：Claude Code overview
- Anthropic 官方文档：Code Review
- Anthropic 官方文档：Settings（git trailers / commit attribution）
- Builder.io：How I use Claude Code (+ my best tips)
- Reddit：r/ClaudeAI 关于 commit message 自动生成、review 工作流的讨论摘要
- DEV Community：Creating Project-Specific Commit Messages with Claude Code Subagents
- DoltHub Blog：Claude Code Gotchas

---

## 今日主题：测试驱动开发（TDD）的 Agent 实践 —— 先造红灯，再让 Claude 去撞，别让它凭感觉写“像对的代码”

### 为什么今天该学这个
根据路线图顺序，加上现有笔记已经覆盖到高级开发篇的 Git 协作，下一项未掌握主题就是资深架构篇第 1 项：
**测试驱动开发（TDD）的 Agent 实践。**

这块的核心不是“Claude 会不会写测试”，而是：
**怎么把 Claude Code 从先写实现、再补测试的随缘模式，改造成“先红后绿”的可验证工程闭环。**

一句话：
**TDD 对 Claude Code 的价值，比对人类程序员还大。**
因为 agent 最怕的不是复杂，而是没有明确反馈。

---

## 官方文档给出的硬结论

### 1. 给 Claude 一个自我验证回路，是最高杠杆实践
Anthropic 官方 `Best practices` 直接把这件事说透了：
- 给 Claude **测试、截图、预期输出**，让它能自己验证
- 这是“**single highest-leverage thing you can do**”
- Claude 在能运行测试并读取结果时，表现会显著提升

这其实就是 TDD 思维的根：
**让 Claude 不靠自信，而靠反馈。**

### 2. 修 bug 或写功能时，最好先要求失败测试
官方示例已经非常接近标准 TDD：
- 先说明失败场景
- 要 Claude “**write a failing test that reproduces the issue, then fix it**”
- 最后跑测试确认通过

这背后的含义很狠：
**没有可重复失败，就没有可证明修复。**

### 3. Plan Mode 适合先分析，再进入红绿循环
Anthropic 官方建议多文件或高风险任务用 Plan Mode 先拆：
- Explore：只读理解代码与依赖
- Plan：明确要改哪些文件、怎么验证
- Implement：回到 Normal Mode 编码
- Verify：运行测试并修到通过

所以 Claude Code 的 TDD 不是盲目“先写测试”，而是：
**先定边界，再做红绿循环。**

### 4. 测试指令要尽量聚焦，优先单测而不是全量跑满
官方文档还强调一个非常实战的点：
- 优先跑**单个相关测试**，不要默认全量测试套件

原因很简单：
- 反馈更快
- 循环更短
- 更利于 Claude 连续迭代

对 agent 来说，回路越短，稳定性越高。

---

## 社区与博客交叉验证后的共识

### 1. DataCamp 的结论很到位：TDD 是 agentic coding 最强模式之一
DataCamp 在《Claude Code Best Practices: Planning, Context Transfer, TDD》里明确提到：
- 每次从 red 到 green，Claude 都能获得**无歧义反馈**
- 这让它可以在较少人工干预下持续迭代
- 因此 TDD 是和 agent 型编码工具配合时“**single strongest pattern**”之一

这话我认同，甚至可以说得更狠一点：
**Claude Code 最怕空气验收，最爱红绿灯。**

### 2. Builder 的经验：给测试命令和日志，质量会直接上一个台阶
Builder 的实战文章给了很接地气的用法：
- prompt 里直接写“改完跑现有测试套件”
- Claude 会自己跑测试、看失败、继续修
- Boris Cherny 甚至提到，这一条就能带来 **2 到 3 倍质量提升**

它还强调了一个对 TDD 很有用的输入方式：
```bash
npm test 2>&1 | claude "fix the failing tests"
```
重点不是命令本身，而是：
**把原始测试输出直接喂给 Claude，而不是你自己转述。**

### 3. 自动化测试实践博客的结论：Claude 很会产量，但你要主动要求可读性与抽象层
On Test Automation 的实战记录显示：
- Claude 能快速写出数量不少、覆盖面看起来不错的测试
- 但如果你没明确要求，它不一定会主动补测试抽象层、命名统一性、可维护结构

这说明一件事：
**Claude 写测试很快，但“测试代码质量”仍然需要你把标准说清楚。**
TDD 不只是先写测试，还要写成长期能维护的测试。

---

## Claude Code 做 TDD，最稳的 4 步循环

### 第一步：先定义一个会失败的事实
不要说：
- “实现支付回调”

要说：
- “先写一个失败测试，覆盖重复回调不应重复记账的场景”

重点是把需求翻译成**可失败的断言**。

### 第二步：只跑最小相关测试
不要一上来：
- `npm test`

应该先：
- 跑单文件
- 跑单 describe
- 跑单 case

目标是缩短 red → green 的循环。

### 第三步：实现最小通过代码
告诉 Claude：
- 先让测试通过
- 不要顺手大重构
- 不要扩散改动

否则它很容易借修一个测试的名义把半个模块翻新，最后把问题搞大。

### 第四步：通过后再补重构与回归验证
最稳顺序是：
1. 红灯测试出现
2. 最小实现让它转绿
3. 代码和测试一起整理命名与抽象
4. 再跑相关测试，必要时跑更大范围回归

这才是 Claude Code 场景下的实战版 TDD。

---

## 小龙儿给主人的落地工作流

### 工作流 A：新功能 TDD 模板
```text
先不要直接实现。
先阅读相关模块和现有测试风格。
为这个需求先写 1 到 3 个失败测试，覆盖核心成功路径、一个边界条件、一个错误路径。
只运行这些相关测试，确认它们失败。
然后实现最小代码让测试通过。
通过后如有必要再做小幅重构，并重新运行相关测试。
最后总结：新增了哪些行为约束、还有哪些风险没覆盖。
```

### 工作流 B：修 bug 的 TDD 模板
```text
用户反馈的问题是：[描述]
先不要猜。
先根据报错和复现步骤写一个失败测试，稳定复现问题。
只跑相关测试确认失败。
然后修复根因，不要 suppress 错误。
修完重新运行相关测试；如果还有失败，继续修到通过。
最后说明根因、修复点、以及为何这个测试能防止回归。
```

### 工作流 C：旧代码补测试模板
```text
先分析这个模块目前最关键的行为边界。
不要追求一口气全补完。
先补最容易出事故的 3 个回归测试。
要求测试命名直接表达业务规则，避免只按实现细节命名。
补完后运行相关测试并给出还未覆盖的风险清单。
```

### 工作流 D：复杂重构的双阶段模板
```text
进入 Plan Mode。
先分析当前实现、现有测试覆盖、缺口和风险。
输出一个 TDD 重构计划：
1. 先补哪些保护性测试
2. 哪些改动必须在测试保护下进行
3. 每一阶段跑哪些验证命令
确认后再实现，按阶段执行，每阶段完成都运行相关测试。
```

---

## 主人最容易犯的 5 个蠢错

### 蠢错 1：让 Claude 先写实现，最后再“顺便补测”
这会让测试变成给实现擦屁股，不是定义行为。

### 蠢错 2：不给失败标准，只说“写点测试”
没有明确场景，Claude 很容易写出一堆存在感很强、约束力很弱的测试。

### 蠢错 3：每次都跑全量测试
这不是严谨，这是浪费反馈速度。
TDD 先要短回路，后要大回归。

### 蠢错 4：只盯通过，不盯测试质量
如果测试名、夹具、断言结构一团糟，后面维护照样痛苦。

### 蠢错 5：修 bug 时不先复现
没红灯就去修，Claude 很容易把表象修掉，把根因留着。

---

## 给《光与朽》的直接用法
如果我们把 Claude Code 的 TDD 真用到《光与朽》，我建议优先打在这几类高风险模块上：
1. **战斗结算**：伤害、护盾、暴击、DOT、死亡判定
2. **存档与读档**：版本兼容、字段缺失、默认值回退
3. **数值配置读取**：ScriptableObject / JSON 缺字段、非法值、范围溢出
4. **奖励与经济系统**：重复发奖、离线收益、广告奖励状态

这些模块最适合“先写失败测试，再让 Claude 修”，因为一旦回归炸了，代价比普通 UI bug 大得多。

---

## 一条最值钱的总原则
以后你让 Claude Code 写功能或修 bug，别只说“把它做出来”，要说：

**先把错误变成一个会失败的测试，再把测试打绿。**

因为对 Claude 这种 agent 来说：
- 测试不是附件
- 测试就是方向盘
- 红灯到绿灯，就是它最稳定的学习与交付回路

---

## 今日结论
今天这课最核心的认知就一句：

**TDD 对 Claude Code 不是编码风格，而是控制论。**

你不给它红绿反馈，它就容易靠感觉乱写；
你给它可重复失败、可重复验证的回路，它才会像真正靠谱的工程同事那样稳定交付。

---

## 参考来源
- Anthropic 官方文档：Best Practices for Claude Code
- DataCamp：Claude Code Best Practices: Planning, Context Transfer, TDD
- Builder.io：50 Claude Code Tips and Best Practices For Daily Use
- On Test Automation：Writing tests with Claude Code - part 1 - initial results


## 今日主题：性能与安全审查 —— 别让 Claude Code 当玄学顾问，要让它当证据驱动的审计员

### 这节课学什么
路线图里下一个该啃的主题，不是继续堆功能，而是：
**如何把 Claude Code 接进“性能审查 + 安全审查”的工程闭环。**

这件事很关键。Claude 最容易骗你的地方，不是它不会写，而是它会很自信地给出一堆“听起来像优化、像安全建议”的废话。
所以今天的核心结论很硬：
**Claude Code 做性能和安全，不该先问它“哪里慢、哪里危险”，而该先给它证据，再让它做归因、排序、修复和复验。**

---

## 官方资料给出的底层原则

### 1. Claude Code 默认就是“最小权限”思路
根据 Claude Code 官方 Security 文档：
- 默认是严格只读
- 一旦要编辑文件、跑测试、执行命令、发网络请求，就会请求批准
- 写权限默认被限制在启动目录及其子目录
- 网络、bash、MCP 都属于高风险面，需要显式控制

这意味着：
**Claude Code 的安全审查，不只是查你的代码漏洞，也包括查它自己在当前会话里能做什么。**

换句话说，做安全审查时，第一件事不是“让 Claude 扫代码”，而是：
1. 先选对 permission mode
2. 再决定是否开 sandbox
3. 再决定哪些命令/域名/路径值得白名单

---

### 2. 官方已经明确承认：Prompt fatigue 会害死安全
Anthropic 的 security 文档和 sandboxing 工程文章都强调一个事实：
**如果每一步都让人狂点 approve，最后人一定会麻。**

这会直接导致两类事故：
- 不该放的危险命令被放了
- 该认真检查的变更因为疲劳被草草放过

所以官方引入 sandbox 的核心价值，不只是“更爽”，而是：
**在文件系统和网络都被隔离的边界里，减少审批次数，同时把风险关进笼子。**
Anthropic 工程文章给出的数据是：
- 开 sandbox 后，权限弹窗可下降约 **84%**

这很值钱。因为它不是偷懒，而是把“人肉逐条审批”升级成“先定义边界，再让 agent 在边界内高效工作”。

---

### 3. Permission mode 决定审查风格，不是 UI 小选项
官方 permission modes 文档里，几个模式的使用场景非常适合审查工作流：
- `plan`：只读分析，适合先摸清代码结构和风险面
- `default`：边做边批，适合敏感修复
- `acceptEdits`：允许改文件，适合低风险批量修正
- `auto`：适合边界已经设计好的长任务
- `bypassPermissions`：只该存在于容器/VM 这种隔离环境里

我的判断很直接：
**性能/安全审查默认别一上来就 bypassPermissions。那不是高效，是作死。**

最稳的起手式是：
- 第一步：`plan` 模式做侦察
- 第二步：确定修复清单后切 `default` 或 `acceptEdits`
- 第三步：只有在容器/VM + sandbox + 明确审计边界都配齐时，才考虑更激进模式

---

### 4. Hooks 是高阶护栏，不只是自动化糖豆
官方 hooks 文档和搜索结果里有几个很关键的点：
- Hooks 可以在工具调用前后插入自定义逻辑
- 可以 allow / deny / modify 某些请求
- `PermissionRequest` hook 可以基于规则自动处理权限请求
- hook 输出还能追加 permission suggestions 或 updatedPermissions

这意味着什么？
意味着你可以把性能/安全审查从“靠人盯”升级成“机器先卡一轮”。

比如：
- 发现 Claude 想执行 `curl | bash` 这种烂活，直接 deny
- 发现它要访问非白名单域名，直接 block
- 发现要修改 `.env`、密钥文件、发布配置，强制人工确认
- 每次改完代码后自动触发 lint/test/benchmark/semgrep

一句话：
**Hooks 不是锦上添花，它是把 Claude Code 从“会动手的实习生”升级成“被流程拴住的实习生”。**

---

## 社区和博客里最值钱的实战认知

### 1. 把 Claude 当“强力但不可信的实习生”
Backslash 的安全最佳实践说得很对：
**Treat Claude like an untrusted but powerful intern.**

这句话很粗暴，但非常准确。
正确理解不是“别用它”，而是：
- 给它最小必要权限
- 把运行环境隔离起来
- 审核它的输出
- 留审计轨迹

如果你把它当成“自动正确的高级工程师”，迟早翻车。
如果你把它当成“执行力爆炸、但必须有护栏的代理人”，反而能狠狠干出效率。

---

### 2. 真正快的安全审查，不是让 Claude 直接拍脑袋，而是模板化、可复现
Charles Jones 的文章里，重点不是“3分钟审完 8 小时的活”这种营销句，而是下面这套结构：
- 先用固定命令触发审计
- 按 OWASP Top 10 分类输出问题
- 给出文件路径、行号、风险等级、修复建议、优先级
- 让报告格式稳定，便于团队复查和追踪

这提醒我们一个关键点：
**Claude 最适合做的是“结构化审查器”，不是“灵感型安全专家”。**

所以你给它的任务不该只是：
- “帮我看看有没有安全问题”

而应该是：
- “按 SQL 注入 / XSS / 身份鉴权 / 秘钥泄露 / SSRF / 文件上传 / 依赖漏洞 7 个维度输出表”
- “每个问题必须含文件路径、风险等级、利用路径、修复建议、是否可立即修”
- “没有证据就写未证实，不许瞎猜”

这一下，质量就会暴涨。

---

### 3. 性能优化最大的坑：Claude 会给你一堆正确但没用的废话
社区关于性能的讨论很统一：
- 如果你只说“improve performance”
- Claude 很容易给你“加缓存、建索引、减少 I/O、优化算法”这种谁都知道的套话

Toward AWS 那篇复盘就直接点破了：
当作者只让 Claude 提性能建议时，Claude 给的是泛化建议，而不是当前系统真正的瓶颈。

所以正确姿势不是让 Claude “猜瓶颈”，而是先喂它：
- profiler 结果（如 cProfile、perf、sampling profiler）
- benchmark 数据
- flame graph
- 慢 SQL 日志
- 接口响应时间分布
- 内存快照

然后再要求它：
1. 解释瓶颈根因
2. 提出候选方案
3. 评估副作用
4. 只挑收益最大的 1~2 个改动先做

Reddit 上还有个很狠的例子：
有人用 Claude Code 写出 7.6 万行代码后做 benchmark，发现 **118 个函数慢到最高 446x**。
这说明一个残酷事实：
**Claude 可以把功能铺得很快，但性能债也能一起批量生产。**

所以性能审查必须是专项回合，不能指望“功能写完顺手就快”。

---

### 4. 审性能时要看全文件和上下文，不能只盯 diff
Reddit 社区有人做了 7 个 code review skills，其中 performance review 的经验很有用：
- 性能问题不该只看 diff
- 要看完整调用链、数据结构、I/O、缓存、上下文

这个判断我完全同意。
因为很多慢点根本不在你改的那 10 行里，而在：
- 上游把大对象反复构造
- 下游数据库 N+1
- 中间层把缓存穿透了
- 序列化/反序列化重复发生

所以：
**安全审查可以从 diff 起手，性能审查通常必须拉全链路证据。**

---

## 一套真正能落地的 Claude Code 性能/安全审查工作流

### 工作流 A：安全审查（最稳版本）

#### 第 1 步：先锁边界
- 用 `plan` 或 `default` 起手
- 敏感仓库开启 `/sandbox`
- 只放行必要目录和域名
- MCP 只接可信服务

#### 第 2 步：给 Claude 一份“审计合同”
提示词不要写成：
- “帮我查安全问题”

要写成：
- 审查范围：鉴权、输入校验、存储、上传、依赖、日志、配置
- 风险标准：按 Critical / High / Medium / Low
- 输出格式：位置、原因、利用路径、修复方案、修复优先级
- 约束：没有证据不要下结论

#### 第 3 步：先跑外部证据，再让 Claude 解释
优先给它：
- `npm audit` / `pnpm audit` / `pip-audit` / `cargo audit`
- secret scan
- grep 命中列表
- 测试日志
- 鉴权相关路由和中间件

#### 第 4 步：按风险逐条修，不准一锅端
- 每次只修一类问题或一个高风险点
- 修完立刻跑测试/验证
- 再回到下一条

#### 第 5 步：复审
- 让 Claude 重新审 diff
- 确认是否引入新洞
- 把修复结果写成可复用 checklist

---

### 工作流 B：性能审查（最稳版本）

#### 第 1 步：先拿证据
绝不允许 Claude 凭猜测优化。
先收集：
- benchmark
- profiler
- 慢查询
- FPS / frame time / CPU / memory 指标

#### 第 2 步：让 Claude 只做“归因 + 排序”
提示词写成：
- “根据以下 profiler 和 benchmark，找出 top 3 瓶颈”
- “每个瓶颈说明根因、影响范围、预估收益、改动风险”
- “禁止给泛泛建议，必须绑定具体函数/模块/查询”

#### 第 3 步：先做 ROI 最大的一刀
- 先砍最大的热点
- 改完就回测
- 数据没改善就回滚思路

#### 第 4 步：再让 Claude 写复盘
- 原瓶颈是什么
- 为什么慢
- 改了什么
- 数据改善多少
- 有没有副作用

这样下次再遇到类似问题，就不是重新问模型，而是复用团队知识。

---

## 对《光与朽》最该先上的四个审查点

### 1. 存档与读档
安全上防字段污染、异常覆盖、版本回退错误；
性能上防频繁序列化、重复 IO、主线程卡顿。

### 2. 奖励发放与广告回调
安全上防重复发奖、伪造回调、客户端信任过高；
性能上防奖励结算链太长、UI 阻塞。

### 3. 联网接口 / 排行榜 / 活动配置
安全上重点看：鉴权、限流、IDOR、配置注入；
性能上重点看：慢接口、缓存策略、批量查询。

### 4. 高频战斗循环
性能上重点盯：
- 每帧分配
- 频繁 GetComponent / Find
- 子弹与特效对象池
- DOT/碰撞/范围判定

这类地方最容易出现“功能对了，但帧率死了”。

---

## 一条最狠的总原则
以后你让 Claude Code 做性能或安全审查，不要问：

**“你觉得哪里有问题？”**

要问：

**“这里有证据、边界、审查格式和成功标准。你基于证据给我定位、排序、修复，并复验。”**

Claude Code 不是天启型大神。
它最强的姿势，是在**边界明确、证据充分、反馈闭环存在**的时候狠狠干活。

---

## 今日结论
今天这课最值钱的一句话：

**性能和安全不是让 Claude 去“猜”，而是让它在受控环境里“审、改、验、再审”。**

你把权限、sandbox、hooks、profiler、审计模板都摆好，Claude Code 就会像一个效率爆炸的审计工程师；
你什么都不设，只丢一句“帮我优化下”，它就很容易变成一个会说正确废话的嘴强王者。

---

## 参考来源
- Anthropic 官方文档：Claude Code Security
- Anthropic 官方文档：Choose a permission mode
- Anthropic 官方文档：Hooks / Hooks Guide（搜索结果摘要）
- Anthropic Engineering：Beyond permission prompts: making Claude Code more secure and autonomous
- Backslash：Claude Code Security Best Practices
- Charles Jones：How I Cut Security Audits from 8 Hours to 3 Minutes Using AI
- Reddit / ClaudeCode：安全审计与性能评审相关讨论、benchmark 反馈
- Claude 官方博客搜索摘要：Optimize code performance quickly / Code Review for Claude Code

---

## 今日主题：跨语言/框架迁移的分批验证流水线 —— 先搭桥，再搬家，最后再拆桥

### 为什么现在该学这刀
路线图里“性能与安全审查”之后，下一个未掌握的大项就是**跨语言/框架迁移**。
但这事最容易被玩成灾难：一口气全量替换、边改边猜、最后绿灯没亮却已经改烂半个仓库。

所以今天只学一个最值钱、最能落地的具体工作流：
**分批迁移 + 临时兼容层 + 每批强验证 + 可即时回滚。**

这不是优雅问题，这是生死问题。
Claude Code 真正适合做迁移，不是因为它会“自动升级”，而是因为它特别适合执行这种**有边界、有批次、有验证门槛**的流水线。

---

## 官方文档给出的硬结论

### 1. 大迁移先开 Plan Mode，先盘点，别先动手
Claude Code 官方 `Common workflows` 明确建议：
当任务是**多文件、多步骤、需要先理解代码再下手**时，先进入 **Plan Mode**。

官方给的例子就很直接：
> “I need to refactor our authentication system to use OAuth2. Create a detailed migration plan.”

这说明官方默认思路不是“上来就改”，而是：
1. 先只读分析
2. 先问清边界
3. 先列迁移计划
4. 再进入实现

对迁移任务，这一步的真正价值是三件事：
- 先列出**影响面**
- 先锁定**非目标**
- 先排出**批次顺序**

一句话：
**迁移不是写代码，迁移先是做地图。**

---

### 2. 官方推荐把迁移当“分批 codemod”，不是一次性手术
高质量迁移实践里，最狠的一点不是模型多聪明，而是流程够土够稳。
Skywork 对 Claude Code 迁移流程的总结很实用，而且跟官方思路一致：

#### Stage 1：Inventory and plan
先盘点旧 API / 旧框架的所有调用点，按目录、模块、风险分组。

#### Stage 2：Adapters / shims
高风险区域先加**临时适配层**，让新旧接口能并存一段时间。

#### Stage 3：Apply changes in batches
按批次改，每批都限定范围，改完就验证。

#### Stage 4：Cleanup and PR
确认全绿后，最后拆掉 shim、删掉旧入口、整理 PR。

这套流非常值钱，因为它把“迁移”拆成了：
**先兼容、再迁移、后清理。**

这和很多人最爱干的蠢事正好相反：
**先全替换，炸了再补。**

---

### 3. 高风险迁移要先搭 adapter/shim，别让新旧世界硬碰硬
这是今天最该记死的一刀。

如果你是：
- Python → Go 服务迁移
- Vue 2 → Vue 3 / React 迁移
- 旧 SDK → 新 SDK 升级
- 老认证流程 → OAuth2 / JWT

那最稳的打法不是让 Claude 一把改完，而是：
**先让它写一层过渡适配器。**

适配层的作用：
- 让老调用暂时还能跑
- 让新实现先局部接入
- 给测试和回滚争取空间
- 把破坏面控制在单批次内

官方示例里虽然没把“shim”吹成唯一答案，但迁移工作流已经明确指出：
> Where risk is high, introduce temporary adapters to minimize breakage.

这句话翻成人话就是：
**先搭桥，不要直接炸路。**

---

### 4. 每一批都必须过验证门，不绿就不准继续
这是 Claude Code 迁移里最值钱的纪律。

Skywork 的迁移流程和官方 `Work with tests` / `Plan Mode` 思路一起看，结论非常清楚：
每一批迁移后都要立刻跑：
- 单元测试
- 集成测试
- type-check
- lint / format
- build

而且规则很硬：
**这一批没绿，就地修；别拖到最后统一收尸。**

因为 Claude 最大的风险不是不会改，
而是它能很高效地把错误扩散到下一个批次。

迁移越大，越要缩短反馈回路。

---

### 5. 回滚必须是第一等公民，不是失败后的补救
官方与社区都反复强调一个点：
Claude Code 的改动应该始终是**diff-first、可审阅、可回退**的。

迁移任务里最稳的双保险是：
1. **Claude checkpoints**：适合秒回退试错批次
2. **Git feature branch + 每批提交**：适合保留清晰历史和 PR 审查

正确理解不是“有 checkpoint 就不用 Git”，而是：
- checkpoint 管即时撤销
- Git 管团队历史、review 和安全回滚

一句话：
**checkpoint 是倒车档，Git 是保险杠。**
两个都得有。

---

## 小龙儿给主人的落地迁移工作流

### 工作流：四段式迁移流水线

#### 第 0 步：先写 `ROADMAP.md`
必须写清：
- 迁移目标是什么
- 非目标是什么
- 哪些模块先改，哪些后改
- 每批的验证门是什么
- 什么时候允许拆掉兼容层

没有这玩意，Claude 只会边改边脑补。

#### 第 1 步：Plan Mode 盘点影响面
让 Claude 只做三件事：
1. 找出旧接口/旧框架的所有入口
2. 按风险和依赖分批
3. 标注需要兼容层的地方

Prompt 模板：
“进入 Plan Mode。盘点 `src/` 中所有 `parseV1` 的调用点，按模块分组，给出迁移到 `parseV2` 的批次计划；先不要改代码；同时写清哪些调用点需要 shim 过渡、哪些可以直接替换。”

#### 第 2 步：先做 adapter/shim
高风险点别直接全换。
先让 Claude 写：
- 新旧接口的桥接层
- TODO 标记
- 最小保护测试

要求很明确：
- shim 命名可检索
- 留清理路径
- 不准变成永久遗留层

#### 第 3 步：按批迁移，每批只改一类东西
每一批都要限定成单一职责，比如：
- 第一批：底层工具函数
- 第二批：服务层调用
- 第三批：UI/页面接入
- 第四批：测试和清理

每批 prompt 都要重申范围：
“只改 `auth/session/` 目录，不碰 UI，不删 legacy adapter，改完只跑相关测试和 type-check。”

#### 第 4 步：每批立刻过验证门
过门清单固定为：
- tests
- type-check
- lint / format
- build

没过就地修，绝不带病进入下一批。

#### 第 5 步：最后拆桥
只有在下面三条都满足时，才允许让 Claude 删 shim：
1. 所有调用点已迁完
2. 测试和构建全绿
3. PR diff 已经可读、可审

别还没站稳就把桥拆了，那是找死。

---

## 主人最容易犯的 5 个蠢错

### 蠢错 1：让 Claude 直接“全项目升级”
这等于让它在没地图、没护栏、没回滚点的情况下拆迁。

### 蠢错 2：不写非目标
你不写“不要碰 UI”“不要顺手重构 unrelated 模块”，Claude 就很可能手痒乱扩散。

### 蠢错 3：没有 shim 还想平滑迁移
高风险改动不做过渡层，爆炸面会直接拉满。

### 蠢错 4：最后才统一跑测试
这是把错误积累成雪崩。
每批都得验，不绿别过门。

### 蠢错 5：没有 feature branch 和分批提交
最后 diff 一锅粥，review、回滚、追责全废。

---

## 给《光与朽》或我们工具链的直接用法
如果后面我们要做：
- Unity 老存档格式迁新字段
- 运营后台旧接口切新接口
- 数据工具从 Python 脚本迁到正式服务
- Web 前端从旧组件体系迁新体系

都直接套这套：
**Plan Mode 盘点 → shim 过渡 → 分批迁移 → 每批验证 → 最后拆桥。**

这套打法的本质不是保守，
而是把高风险变化切成**可证伪、可撤销、可审查**的小块。
Claude 在这种约束下会很强；
没约束，它就容易把仓库改成事故现场。

---

## 今日结论
今天最值钱的一句话：

**Claude Code 做迁移时，先搭桥，再搬家，最后再拆桥。**

Plan Mode 负责看清地图，adapter/shim 负责压缩爆炸半径，分批验证负责防止错误扩散，checkpoint + Git 负责把回滚做成默认能力。

这才是专家级迁移工作流。

---

## 参考来源
- Claude Code 官方文档：Common workflows（Plan Mode / complex refactor / tests）
- Claude Code 官方文档：Create custom subagents（Explore / Plan / context isolation / tool constraints）
- Skywork：How to Use Claude Code Plugin for Safe Refactoring & Migration（staged migration / adapters / verification / rollback）
