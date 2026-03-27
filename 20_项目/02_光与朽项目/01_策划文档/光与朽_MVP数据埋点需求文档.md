# 《光与朽》(Light VS Decay) - MVP 数据埋点需求文档

**文档状态**：草稿 / MVP版
**更新日期**：2026-03-28

## 1. 埋点目标
本阶段为游戏首发 MVP 版本的核心数据埋点，主要用于：
1. 监控玩家的新手期流失节点（第一关漏斗）。
2. 分析局内战斗难度卡点（波次与 Boss 胜率）。
3. 统计核心广告位的触发率和 eCPM 指标。

*建议接入方案：微信小游戏官方数据助手 或 友盟(Umeng) 极简版SDK。*

---

## 2. 事件埋点列表

### 2.1 基础与流程漏斗 (Funnel)
这类埋点用于计算玩家从打开游戏到真正开始玩的第一波流失率。

| 事件ID | 事件名称 | 触发时机 | 携带参数 (Params) | 备注说明 |
| :--- | :--- | :--- | :--- | :--- |
| `game_start` | 进入游戏 | 游戏加载完成，进入主界面 | - | 基础进入率 |
| `privacy_agree` | 同意隐私协议 | 玩家点击隐私协议弹窗的“同意” | - | 检查是否因合规弹窗流失 |
| `tutorial_complete`| 新手引导完成 | 玩家完成第一次强制引导操作 | - | 新手漏斗底端 |

### 2.2 关卡与战斗数据 (Battle)
这类埋点用于分析数值平衡和难度曲线。

| 事件ID | 事件名称 | 触发时机 | 携带参数 (Params) | 备注说明 |
| :--- | :--- | :--- | :--- | :--- |
| `level_enter` | 进入关卡 | 玩家点击“开始战斗”进入特定章节 | `chapter_id` (int): 章节编号 | 记录关卡参与度 |
| `wave_complete` | 波次完成 | 玩家成功清空当前波次怪物 | `chapter_id`, `wave_id` (int): 波次编号 | 寻找最难波次卡点 |
| `player_death` | 玩家死亡 | 玩家血量归零 | `chapter_id`, `wave_id`, `survive_time` (float): 存活时间 | 分析挫败感来源 |
| `boss_encounter` | 遇到Boss | 刷出 Boss 实体 | `boss_id` (string) | - |
| `boss_kill` | 击杀Boss | Boss 血量归零 | `boss_id` (string) | 计算 Boss 胜率 |

### 2.3 商业化广告数据 (IAA)
**最高优先级**。用于计算 ARPU 和广告渗透率。

| 事件ID | 事件名称 | 触发时机 | 携带参数 (Params) | 备注说明 |
| :--- | :--- | :--- | :--- | :--- |
| `ad_btn_click` | 点击广告按钮 | 玩家点击任意带有视频图标的按钮 | `ad_placement` (string): e.g., "revive"(复活), "reroll"(技能刷新) | 统计拉起率 |
| `ad_show_success`| 广告展示成功 | 成功调用SDK并拉起视频 | `ad_placement` | 检查无填充率 |
| `ad_reward_claim`| 广告播放发奖 | 玩家看完广告并获得奖励 | `ad_placement` | 计算有效广告展示 |

### 2.4 局外养成系统 (Growth)
用于分析玩家对成长系统的依赖度和红点系统的有效性。

| 事件ID | 事件名称 | 触发时机 | 携带参数 (Params) | 备注说明 |
| :--- | :--- | :--- | :--- | :--- |
| `tech_upgrade` | 科技树升级 | 玩家消耗金币成功升级某项科技 | `tech_id` (string), `level` (int) | 分析哪条科技树最受欢迎 |
| `equip_merge` | 装备合成 | 玩家成功合成高阶装备 | `item_id` (string), `quality` (int) | 分析装备获取进度 |

---

## 3. 下一步行动 (Action Items)
1. 程序评估接入哪家 SDK（推荐微信自带数据接口）。
2. 在代码中封装一个统一的 `AnalyticsManager.LogEvent(eventId, params)` 静态方法，方便后续扩展。
3. 结合广告系统的接入，优先完成 `2.3 商业化广告数据` 的埋点。
