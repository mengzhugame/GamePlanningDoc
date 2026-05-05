# 2026-04-29 每周 OpenClaw 与技能更新评估

- 评估时间：2026-04-29 21:23 Asia/Shanghai
- 当前运行版本：OpenClaw 2026.4.23（`session_status`）
- 官方最新稳定版：OpenClaw 2026.4.26（GitHub Release，发布时间 2026-04-28）
- 评估目标：判断是否值得升级 OpenClaw，以及是否应安装新 Skill 以服务《光与朽》与下一阶段“AI 视频反向立项 -> 7 天垂直切片 -> 陌生用户测试”工作流。

## 一、结论先说

### 1）系统是否建议更新
**建议更新，但不要在正在赶《光与朽》功能/提单的高压时段裸升。**

我的判断很明确：从 `2026.4.23 -> 2026.4.26` 只落后 3 个日版，但 `2026.4.26` 这次不是小修小补，里面有一批**直接改善我们主脑工作流稳定性**的修复，值得升；同时它改动面很大，涉及 Gateway、插件、Cron、Discord、Memory、ACP/子 Agent、CLI 更新链路，**不适合在临近关键交付节点随手升级**。

**建议策略：**
- 本周找一个非高峰窗口升级到 `2026.4.26`
- 升级前保留当前配置/状态备份
- 升级后只做 4 项最小回归：`Cron 推送`、`Discord 发送`、`memory_search`、`sessions_spawn/subagent`
- 通过就继续用，不通过就立刻回退

### 2）技能是否建议安装
**建议安装 1 个：`game-quality-gates`。其余先不装。**

原因很硬：
- 它直接对应**已上线产品补丁质量**和**7 天垂直切片交付质量**，能减少计时器、清理、输入、多端音频、存档、场景切换这类最恶心的隐性 bug。
- Skill 内容以 Markdown/参考清单为主，**几乎无执行风险**，安装成本低。
- 它适合我们现在这种“一个 live 产品 + 一个快速实验流”并存的状态。

**不建议现在安装的候选：**
- `game-build-strategy`：思路对，但更像方法论封装。可读，可参考，但当前不是最高增益。
- `prd-generator-cc`：更新活跃，但我们本地已存在 `prd` 相关能力；当前瓶颈也不是 PRD 产出。
- `image-to-video-runcomfy`：方向诱人，但依赖外部 RunComfy 账号/Token/CLI。现在真正短板是“筛信号和立项判断”，不是又多一个生视频入口。

## 二、为什么建议升级到 2026.4.26

结合 `v2026.4.26` Release Notes，我认为下面几类改动和我们最相关：

### A. Cron / Discord 稳定性提升 —— 直接相关
我们大量依赖 Cron 和 Discord 推送，所以这部分是高权重。

**关键相关项：**
- Cron：修复 isolated run 成功/失败判定、失败告警、超时起算、pending slot 失效等问题
- Discord：修复显式 `user:` / `channel:` 目标保留与路由问题
- Gateway：修复 `EPIPE` 等关闭管道导致的崩溃

**对我们的价值：**
这会直接降低“定时任务明明执行了但状态不准”“消息送达目标跑偏”“推送链路偶发炸掉”的概率。对主脑型工作流，这不是锦上添花，是基本盘。

### B. Memory / Search / Recall 修复 —— 相关
**关键相关项：**
- `memory_search` / `memory_get` 执行时重新解析 active runtime config
- one-shot memory CLI 不再启动长生命周期 watcher
- 多处 QMD / embedding / status 行为修复

**对我们的价值：**
我们高度依赖记忆与知识召回。现在这轮任务里 `memory_search` 本身就出现提供方异常，虽然不是同一类问题，但 `2026.4.26` 至少说明记忆链路正在被密集修补。对于长期要靠知识系统做判断的主脑，升级收益是实打实的。

### C. ACP / 子 Agent / 编排能力 —— 相关
**关键相关项：**
- ACP/Claude 适配与 idle 完成判定修复
- `sessions_spawn` 模型别名解析修复
- `subagents.allowAgents`、子 Agent 完成投递、线程路由等修复

**对我们的价值：**
小龙儿的核心身份就是“主脑编排”。凡是修复多 Agent 调度、完成态、路由、回传的版本，价值都高。后面不管是继续拆《光与朽》，还是搭 7 天垂直切片流水线，这部分都比新玩具更重要。

### D. 更新链路自身修复 —— 很关键
Release 中明确提到：
- `CLI/update` 改进全局更新安装与校验流程，避免旧新文件混装
- `Control UI/update` 改进“Update now”后版本验证

**对我们的价值：**
这意味着这次升级本身的可靠性也比旧版强。换句话说：如果要升，现在比前几天更值得升。

## 三、为什么不建议立刻无脑升级

因为 `2026.4.26` 改动面太大，已经接近一次“基础设施大扫除”：
- 插件发现/安装/清单
- Gateway 启动路径
- Memory
- Cron
- Discord
- Browser
- Ollama/本地模型
- ACP / Subagents

这种版本对**工作流稳定性长期是利好**，但对**短期交付窗口**有潜在扰动。结论就是：

**该升，但要受控升。**

不是今晚脑子一热就升，更不是《光与朽》正在收口或主人正在赶关键包时动刀。

## 四、Skill 候选评估

## 1. `game-quality-gates`
- 来源：ClawHub inspect
- 创建/更新：2026-03-05 / 2026-03-06
- 类型：规则文档 + 参考清单，无脚本型高风险动作
- 适配度：**高**

### 为什么值得装
它抓的是最容易在小游戏/塔防/快节奏原型里漏掉的坑：
- 单一清理入口
- Buff 与基础属性冲突
- destroy 前缓存数据
- timer 生命周期
- 帧率独立逻辑
- 场景切换清理
- 音频生命周期
- 输入防抖/互斥
- 存档版本迁移
- 网络超时与降级

这非常适合：
1. 《光与朽》上线后继续 patch
2. 下一阶段 7 天垂直切片
3. AI 帮写代码时的验收 checklist

### 我的判断
**这是本轮唯一值得安装的技能。**
不是因为它最炫，而是因为它最能减少返工。

## 2. `game-build-strategy`
- 来源：ClawHub inspect
- 创建/更新：2026-03-30 / 2026-03-30
- 类型：方法论文档，无明显执行风险
- 适配度：**中高**

### 价值
它把项目分成 `yolo-super / guided-build / refactor-open / surgical-live`，并给出质量目标映射。

### 为什么先不装
这套思想对我们有启发，尤其适合区分：
- 《光与朽》：`surgical-live + live-patch`
- 新切片：`yolo-super/guided-build + first-playable/polished-prototype`

但它本质更像“把已有常识写成规则”。
当前阶段，**减少 bug 的价值 > 再加一层流程哲学**。

## 3. `prd-generator-cc`
- 来源：ClawHub inspect
- 最新版本：1.0.3，2026-04-29 更新
- 类型：PRD/需求文档生成辅助
- 适配度：**中**

### 为什么不推荐现在装
- 本地已存在 `prd` 相关能力
- 当前项目瓶颈不在“不会写 PRD”
- 主人当前更需要“验证链路”和“可上线/可测试/可买量素材”，不是再加强文档写作

**结论：先不装。**

## 4. `image-to-video-runcomfy`
- 来源：ClawHub inspect
- 最新版本：0.1.2，2026-04-29 更新
- 类型：调用 RunComfy 做 image-to-video
- 适配度：**方向相关，但当前时点不建议装**

### 为什么看起来相关
我们接下来确实会做 AI 视频反向立项、素材实验、短视频信号测试。

### 为什么现在不推荐
- 依赖外部 `runcomfy` CLI 与 `RUNCOMFY_TOKEN`
- 真正瓶颈仍是“选什么信号、做什么动词、怎么判定切片值不值得做”
- 现在贸然加一个外部视频生产通道，容易让系统继续向“多收集、多生成”滑，而不是更聚焦判断

**结论：关注，但先不装。**
等我们决定正式用 RunComfy 批量跑素材，再单独评估。

补充说明：后续异步搜索还浮出了 `youtube-shorts-automation`、`video-repurpose`、`video-marketing` 等视频分发/复用类技能，但它们更偏内容运营，不解决当前最核心的“题材判断、动词筛选、切片验证质量”问题，因此本周仍不建议引入。

## 五、最终建议（可执行）

### 系统
- **建议升级到 OpenClaw 2026.4.26**
- **执行时机：非开发高峰窗口**
- **升级后最小回归：**
  1. Cron 是否正常执行并正确判定成功/失败
  2. Discord 是否仍稳定发送到目标频道
  3. `memory_search` / `memory_get` 是否正常
  4. `sessions_spawn` / 子 Agent 回传是否正常

### 技能
- **建议安装：`game-quality-gates`**
- **暂不安装：`game-build-strategy`、`prd-generator-cc`、`image-to-video-runcomfy`**

## 六、对当前项目的直接意义

这轮评估的核心不是“追新”，而是服务《光与朽》与下一阶段验证系统：
- 升级 OpenClaw：提高 Cron/Discord/Memory/子 Agent 这条主脑基础设施的稳定性
- 安装 `game-quality-gates`：提高 live patch 和 7 天切片的交付质量，少踩隐性 bug
- 不装花哨视频技能：避免继续把注意力从“判断什么值得做”转移到“又能多生成点什么”

这就是本轮最硬的判断：
**系统该升，但要稳着升；技能只装一个真有用的，别把工具箱变成垃圾场。**

## 证据来源
- `session_status`：当前运行版本 `OpenClaw 2026.4.23 (ef88cab)`
- GitHub Releases：`https://github.com/openclaw/openclaw/releases/tag/v2026.4.26`
- GitHub Repo：`https://github.com/openclaw/openclaw`
- ClawHub inspect：`game-quality-gates` / `game-build-strategy` / `prd-generator-cc` / `image-to-video-runcomfy`
