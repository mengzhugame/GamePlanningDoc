# 《美妆消消乐》游戏设计文档 (GDD)1.0

> 来源文件: `《美妆消消乐》游戏设计文档 (GDD)1.0.docx`
> 注意: 由旧格式转写为 Markdown，便于 Obsidian 统一管理。

---

## 《美妆消消乐》游戏设计文档

### 1. 游戏概述

#### 1.1 游戏基本信息

游戏名称：美妆消消乐（Makeup Match & Organize）

游戏类型：休闲益智 / 三消 + 收纳整理

目标平台：移动端（iOS/Android）、PC

开发引擎：Unity 2022.3 LTS

编程语言：C#

目标用户：18-35岁女性用户，喜欢化妆品、收纳整理和休闲游戏的玩家

#### 1.2 游戏核心概念

将经典的《羊了个羊》三消机制与收纳整理玩法相结合，玩家通过消除化妆品卡片来"制作"化妆品，然后在梳妆台空间中进行收纳整理，获得双重满足感。

#### 1.3 游戏特色

精美的化妆品主题美术风格

创新的"三消制作"到"空间收纳"的完整游戏循环

丰富的化妆品种类和收纳空间定制

渐进式难度设计，从简单到复杂的关卡挑战

### 2. 核心玩法机制

#### 2.1 三消玩法

##### 2.1.1 基础规则

卡片堆叠：关卡开始时，化妆品卡片以多层堆叠形式展示在游戏区域

点击机制：玩家点击可见的卡片，卡片自动飞入底部槽位

槽位系统：底部有7个槽位，卡片按点击顺序排列

消除规则：当有3个相同的化妆品卡片时，自动消除并获得对应化妆品

失败条件：7个槽位全部占满且无法消除时，游戏失败

胜利条件：消除场景内所有卡片，进入收纳整理环节

##### 2.1.2 卡片类型

口红类：不同品牌和色号的口红

眼影类：单色眼影、眼影盘等

粉底类：粉底液、粉饼、散粉等

护肤类：面霜、精华、乳液等

工具类：刷子、粉扑、睫毛夹等

香水类：各种香水瓶

特殊卡片：

万能卡：可与任意两个相同卡片组合消除

炸弹卡：消除时清除周围一圈卡片

冰冻卡：暂时锁定，需要消除旁边卡片解冻

#### 2.2 收纳整理玩法

##### 2.2.1 空间系统

梳妆台：主要收纳空间，有多个抽屉和台面区域

化妆包：便携收纳空间，容量有限

展示架：用于展示稀有或心爱的化妆品

收纳盒：可购买或解锁的额外收纳空间

##### 2.2.2 整理机制

拖拽放置：将获得的化妆品拖拽到合适的位置

分类整理：按照类型、品牌、颜色等进行分类

空间优化：合理利用空间，达成完美收纳

满意度评分：根据整理的整齐度、分类合理性给予评分

#### 2.3 道具系统

##### 2.3.1 游戏内道具

撤回：撤回上一步操作（限3次）

提示：高亮显示可消除的卡片

洗牌：重新排列剩余卡片位置

槽位扩展：临时增加1个槽位（30秒）

透视：查看被遮挡的卡片（5秒）

##### 2.3.2 收纳道具

收纳分隔板：自定义抽屉内部空间

标签贴纸：为收纳区域添加标签

装饰品：美化梳妆台的装饰物品

### 3. 系统设计

#### 3.1 关卡系统

##### 3.1.1 关卡结构

第一章：新手入门（1-20关）

- 简单的卡片布局

- 基础化妆品种类（3-4种）

- 小型收纳空间

第二章：进阶挑战（21-50关）

- 复杂的堆叠结构

- 增加化妆品种类（5-6种）

- 中型梳妆台解锁

第三章：专业收纳（51-100关）

- 多层嵌套布局

- 全品类化妆品（7-8种）

- 大型梳妆台+化妆包

第四章：大师之路（101-150关）

- 极限挑战布局

- 特殊卡片机制

- 自定义收纳空间

##### 3.1.2 难度递进

卡片数量：从30张递增到150张

种类数量：从3种递增到8种

堆叠层数：从2层递增到5层

特殊机制：逐步引入冰冻、炸弹等特殊卡片

#### 3.2 成就系统

##### 3.2.1 三消成就

连续消除：一次消除5组以上

完美通关：不使用道具通关

速度达人：30秒内通关

收集大师：收集所有化妆品种类

##### 3.2.2 收纳成就

整理达人：获得满分收纳评价

分类专家：完美分类所有化妆品

空间大师：100%利用收纳空间

装饰设计师：解锁所有装饰品

#### 3.3 经济系统

##### 3.3.1 货币类型

金币：基础货币，通关获得，购买道具

钻石：高级货币，购买特殊道具和皮肤

收纳币：专用于购买收纳相关物品

##### 3.3.2 获取方式

通关奖励：根据评分获得金币

每日任务：完成任务获得各类货币

成就奖励：达成成就获得钻石

观看广告：获得少量货币或道具

#### 3.4 社交系统

##### 3.4.1 好友功能

好友排行榜

赠送体力/道具

查看好友的梳妆台布置

##### 3.4.2 分享功能

分享通关截图

分享梳妆台布置

分享稀有化妆品收藏

### 4. 技术架构设计

#### 4.1 项目结构

Assets/

├── Scripts/

│   ├── Core/              # 核心系统

│   │   ├── GameManager.cs

│   │   ├── LevelManager.cs

│   │   └── SaveManager.cs

│   ├── Gameplay/          # 游戏玩法

│   │   ├── Match3/

│   │   │   ├── Card.cs

│   │   │   ├── CardStack.cs

│   │   │   ├── SlotManager.cs

│   │   │   └── MatchDetector.cs

│   │   └── Organize/

│   │       ├── DragDropHandler.cs

│   │       ├── StorageSpace.cs

│   │       └── OrganizeScoring.cs

│   ├── UI/                # 界面系统

│   │   ├── UIManager.cs

│   │   ├── MenuController.cs

│   │   └── HUDController.cs

│   ├── Data/              # 数据结构

│   │   ├── CardData.cs

│   │   ├── LevelData.cs

│   │   └── ItemData.cs

│   └── Utils/             # 工具类

│       ├── ObjectPool.cs

│       ├── AudioManager.cs

│       └── EffectManager.cs

├── Prefabs/               # 预制体

├── Materials/             # 材质

├── Textures/              # 纹理

├── Audio/                 # 音频

├── Animations/            # 动画

└── Resources/             # 资源配置

#### 4.2 核心类设计

##### 4.2.1 卡片系统

public class Card : MonoBehaviour

{

public int cardID;

public CardType cardType;

public Sprite cardSprite;

public bool isClickable;

public bool isFrozen;

public void OnClick();

public void MoveToSlot(int slotIndex);

public void Eliminate();

}

public class SlotManager : MonoBehaviour

{

public List<Card> slots = new List<Card>(7);

public void AddCard(Card card);

public void CheckMatch();

public void RemoveCards(List<Card> cards);

}

##### 4.2.2 收纳系统

public class OrganizeItem : MonoBehaviour

{

public ItemType itemType;

public Vector2 size;

public bool isDragging;

public void StartDrag();

public void EndDrag();

public bool CheckPlacement();

}

public class StorageSpace : MonoBehaviour

{

public StorageType type;

public Vector2 dimensions;

public List<OrganizeItem> storedItems;

public bool CanPlaceItem(OrganizeItem item, Vector2 position);

public void PlaceItem(OrganizeItem item);

public float CalculateScore();

}

#### 4.3 数据管理

##### 4.3.1 关卡配置（ScriptableObject）

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]

public class LevelData : ScriptableObject

{

public int levelNumber;

public List<CardLayoutData> cardLayout;

public int[] availableCardTypes;

public int targetScore;

public float timeLimit;

public RewardData rewards;

}

##### 4.3.2 存档系统

public class SaveData

{

public int currentLevel;

public int totalCoins;

public int totalDiamonds;

public List<int> unlockedItems;

public Dictionary<int, int> levelStars;

public StorageLayoutData currentStorage;

}

### 5. 界面设计

#### 5.1 主界面

Logo区域：游戏标题和装饰

开始游戏：进入关卡选择

我的梳妆台：查看和编辑收纳空间

商店：购买道具和装饰品

设置：音效、音乐、账号等设置

#### 5.2 游戏界面

顶部栏：关卡信息、暂停按钮、金币显示

游戏区域：卡片堆叠显示区

底部槽位：7个卡片槽位

道具栏：快捷使用道具按钮

进度条：显示剩余卡片数量

#### 5.3 收纳界面

梳妆台视图：3D或2.5D视角的梳妆台

物品栏：待整理的化妆品列表

工具栏：旋转视角、添加分隔板等工具

评分面板：实时显示整理评分

### 6. 美术风格

#### 6.1 整体风格

风格定位：温馨、精致、少女感

色彩方案：粉色系为主，搭配金色点缀

画面风格：2.5D卡通渲染风格

#### 6.2 化妆品设计

写实度：半写实卡通风格

品牌化：虚构品牌，但设计参考真实化妆品

辨识度：确保缩小后仍能清晰辨认

#### 6.3 场景设计

梳妆台：欧式复古或现代简约可选

背景：温馨的卧室或化妆间环境

光效：柔和的暖光，营造舒适氛围

### 7. 音效音乐

#### 7.1 背景音乐

主界面：轻松愉快的钢琴曲

游戏中：节奏轻快的轻音乐

收纳模式：舒缓的环境音乐

#### 7.2 音效设计

点击音：清脆的按钮音

消除音：悦耳的消除音效

放置音：物品放置的真实音效

成功音：完成关卡的庆祝音效

### 8. 优化与性能

#### 8.1 性能优化

对象池：复用卡片和特效对象

批处理：合并相同材质的渲染

LOD：远近不同的模型细节

纹理压缩：适配不同平台的纹理格式

#### 8.2 内存管理

资源加载：按需加载和卸载资源

纹理优化：使用图集减少DrawCall

音频压缩：使用适当的音频格式

### 9. 变现策略

#### 9.1 广告植入

激励视频：获得额外道具或复活机会

插屏广告：关卡间隔展示（控制频率）

横幅广告：非游戏界面底部展示

#### 9.2 内购项目

去广告：一次性购买去除所有广告

道具包：各类道具组合包

皮肤包：梳妆台皮肤、卡片皮肤

月卡：每日领取奖励

#### 9.3 活动运营

每日签到：连续签到获得奖励

限时活动：节日主题关卡

排行榜竞赛：周/月排行榜奖励

### 10. 开发计划

#### 10.1 开发阶段

##### 第一阶段：核心原型（2周）

实现基础三消机制

完成卡片生成和消除逻辑

基础UI框架

##### 第二阶段：玩法完善（3周）

添加收纳整理玩法

实现道具系统

关卡编辑器开发

##### 第三阶段：内容制作（4周）

制作50个关卡

完成所有化妆品美术资源

音效音乐制作

##### 第四阶段：系统完善（3周）

成就系统

经济系统平衡

社交功能

##### 第五阶段：优化测试（2周）

性能优化

Bug修复

玩家测试和调整

#### 10.2 团队配置

程序开发：2人（1主程+1客户端）

美术设计：2人（1原画+1UI）

策划设计：1人

音效音乐：外包

测试：1人

#### 10.3 预算估算

开发成本：约30-40万人民币

运营成本：月度5-10万人民币

推广费用：首期10-20万人民币

### 11. 风险评估

#### 11.1 技术风险

性能问题：大量卡片同时显示可能造成卡顿

解决方案：使用对象池、优化渲染批次

#### 11.2 市场风险

竞品竞争：三消游戏市场竞争激烈

解决方案：强化收纳玩法差异化，精准定位目标用户

#### 11.3 运营风险

用户留存：休闲游戏用户黏性较低

解决方案：持续更新内容、举办活动、社交绑定

### 12. 后续更新计划

#### 12.1 版本迭代

1.1版本：新增30个关卡、节日限定化妆品

1.2版本：好友化妆品交易系统

1.3版本：化妆间装修系统

2.0版本：品牌联动、真实化妆品授权

#### 12.2 内容扩展

化妆品品类：持续增加新的化妆品种类和形状

合成配方：季节限定合成配方

收集主题：品牌系列、色系收集等主题活动

空间扩展：更大的背包格子（4x4、5x5）挑战关卡

### 附录：核心代码示例

#### A.1 卡片匹配检测

public class MatchDetector : MonoBehaviour

{

private SlotManager slotManager;

public void CheckForMatches()

{

Dictionary<int, List<Card>> cardGroups = new Dictionary<int, List<Card>>();

// 分组相同类型的卡片

foreach (Card card in slotManager.slots)

{

if (card != null)

{

if (!cardGroups.ContainsKey(card.cardID))

{

cardGroups[card.cardID] = new List<Card>();

}

cardGroups[card.cardID].Add(card);

}

}

// 检查是否有3个或以上相同的卡片

foreach (var group in cardGroups)

{

if (group.Value.Count >= 3)

{

StartCoroutine(EliminateCards(group.Value.Take(3).ToList()));

}

}

}

private IEnumerator EliminateCards(List<Card> cards)

{

// 播放消除动画

foreach (Card card in cards)

{

card.Eliminate();

}

yield return new WaitForSeconds(0.5f);

// 从槽位移除

slotManager.RemoveCards(cards);

// 给予奖励

GameManager.Instance.AddReward(cards[0].cardType);

}

}

#### A.2 拖拽系统实现

public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler

{

private Vector3 originalPosition;

private Transform originalParent;

private CanvasGroup canvasGroup;

public void OnBeginDrag(PointerEventData eventData)

{

originalPosition = transform.position;

originalParent = transform.parent;

canvasGroup.alpha = 0.6f;

canvasGroup.blocksRaycasts = false;

}

public void OnDrag(PointerEventData eventData)

{

transform.position = Input.mousePosition;

}

public void OnEndDrag(PointerEventData eventData)

{

canvasGroup.alpha = 1f;

canvasGroup.blocksRaycasts = true;

// 检查是否可以放置

if (!CheckValidPlacement())

{

transform.position = originalPosition;

transform.SetParent(originalParent);

}

else

{

// 更新分数

OrganizeManager.Instance.UpdateScore();

}

}

}

本文档为《美妆消消乐》游戏的核心设计方案，具体实现细节可能根据开发过程中的实际情况进行调整。

文档版本：v2.0
更新日期：2025年
