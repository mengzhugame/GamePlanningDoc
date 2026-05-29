---
type: game-teardown
status: distilled
game_name: "Monster Train"
developer: "Shiny Shoe"
publisher: "Good Shepherd Entertainment"
platform: [PC, Console]
release_date: "2020"
genre: "Roguelike Deckbuilder"
sub_genre: "卡牌构筑 + 三层车厢防守 + 双阵营协同"
core_mechanic_keywords: [三层防守, 容量, 双阵营, Champion, 单位升级, 卡牌升级, Covenant]
art_style: "奇幻怪物 / 高可读卡牌与战场"
monetization: "买断制"
torn_down_at: "2026-05-25"
torn_down_by: "codex"
sources:
  - "10_流水/游戏拆解/game_teardown_monster_train.md"
related_projects: [光与朽, Roguelike构筑与随机控制方法论_2026]
distilled_ref: "[[Roguelike构筑与随机控制方法论_2026]]"
tags: [MonsterTrain, Deckbuilder, 塔防, 双阵营, 容量策略]
---

# 拆解：《Monster Train》

## 来源: `10_流水/游戏拆解/game_teardown_monster_train.md` · 提取日期 2026-05-25

## 1. 一句话核心机制

把卡牌构筑和塔防空间结合起来，用三层车厢、容量限制、双阵营和 Champion 锚点制造高重玩防守构筑。

## 2. 核心循环

选择主阵营和副阵营 → 进入战斗 → 在三层车厢布置单位和法术 → 敌人逐层上行 → 波后获得卡牌、单位或资源 → 商店升级卡牌和单位 → 挑战 Boss 与更高 Covenant。

## 3. 玩法详解

三层车厢把卡牌战斗变成空间问题。玩家不仅要问“打多少伤害”，还要问“哪一层承伤、哪一层清杂、哪一层打 Boss”。容量限制让单位不是越多越好，而是要组合前排、输出、增益和法术节奏。

主副双阵营提供组合爆炸。主阵营给身份和 Champion，副阵营提供补足或极端协同，让每局从起点就有构筑方向。

## 4. 数值/经济

商店升级允许玩家把少数核心卡或单位推到非常强。游戏不害怕玩家做出强势 Build，因为后期 Boss 和 Covenant 也会同步提高压力。

Champion 是早期稳定锚点，减少“开局完全抽不到方向”的挫败。

## 5. 美术与感官

卡牌、单位、楼层和敌人行进方向都要高可读。三层结构如果读不清，会让玩家把失败归因于界面，而不是策略。

## 6. 变现路径

买断制适合构筑深、局内变化明显、挑战等级可长期重玩的结构。DLC 可扩展阵营、单位和卡池。

## 7. 爽点与留存机制（重点）

- 三层防守让构筑有空间表达。
- 双阵营让开局组合数量指数上升。
- Champion 作为流派锚点降低早期随机挫败。
- 强升级允许玩家做出“过强但合理”的 PvE Build。
- Covenant 提供长期挑战阶梯。

## 8. 风险点

阵营、卡牌、单位、容量和楼层规则同时出现时，学习成本很高。必须先让玩家理解“敌人怎么走、我在哪挡、容量为什么不够”，再逐步打开深度。

## 9. 跟用户已有项目的相邻度

对《光与朽》最直接的启发是“塔防不一定只有一条平面路径”。防线可以被拆成上中下层、前后排、光暗层或核心外壳层，让同一套塔和插件有空间职责差异。

## 10. 可借鉴的 3 个点

1. 容量限制比单纯金币限制更能制造空间取舍。
2. 双阵营/双模块能显著提高构筑变化。
3. 给玩家一个开局锚点，可以减少随机挫败。

## 11. 不该照抄的 1 个点

不要在早期塔防 Demo 里同时做完整卡牌、三层、双阵营和长线难度。先验证“多层空间防守 + 容量限制”是否产生新策略。

## 12. 调研来源

- 源文件：`10_流水/游戏拆解/game_teardown_monster_train.md`
