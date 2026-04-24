# Unity代码模板库使用指南

> 创建日期: 2026-02-07
> 来源: 光与朽(LightVSDecay) + 美妆叠叠乐(MZ02)
> 用途: 快速启动新项目，避免重复造轮子

---

## 一、模板库结构

```
code_templates/
├── Core/                         # 核心框架（必用）
│   ├── Singleton.cs              # 三种单例基类
│   ├── ObjectPool.cs             # 通用对象池
│   ├── IPoolable.cs              # 对象池接口
│   └── GameEvents.cs             # 事件系统模板
├── Managers/                     # 管理器模板
│   ├── GameManagerTemplate.cs    # 游戏管理器
│   ├── AudioManagerTemplate.cs   # 音效管理器
│   ├── UIManagerTemplate.cs      # UI管理器
│   └── SaveManagerTemplate.cs    # 存档管理器
├── Data/                         # 数据层模板
│   └── GameSettingsTemplate.cs   # 游戏配置SO
├── UI/                           # UI模板
│   ├── BasePanelTemplate.cs      # 面板基类
│   ├── SettlementPanelTemplate.cs # 结算面板
│   └── FloatingTextTemplate.cs   # 飘字系统
├── VFX/                          # 特效模板
│   ├── CameraShakeTemplate.cs    # 相机震动
│   └── ScreenEffectTemplate.cs   # 屏幕特效（闪白/暗角/裂纹）
└── Utils/                        # 工具类
    └── GameLoggerTemplate.cs     # 日志系统
```

---

## 二、使用方法

### 步骤1: 复制Core模块
```bash
# 新项目必须复制
cp -r code_templates/Core/* YourProject/Assets/Scripts/Core/
```

### 步骤2: 修改命名空间
将 `LightVsDecay` 替换为你的项目命名空间：
```csharp
// 修改前
namespace LightVsDecay.Core

// 修改后
namespace YourGame.Core
```

### 步骤3: 按需复制其他模块
根据项目需要复制对应模板。

---

## 三、核心模块说明

### 1. Singleton.cs（三种单例）

#### Singleton<T> - 普通单例
- 场景内有效
- 场景切换时销毁

```csharp
public class LevelManager : Singleton<LevelManager>
{
    protected override void OnSingletonAwake()
    {
        // 初始化代码
    }
}
```

#### PersistentSingleton<T> - 跨场景单例
- 使用 DontDestroyOnLoad
- 场景切换时保持

```csharp
public class GameManager : PersistentSingleton<GameManager>
{
    // 全局管理器
}
```

#### AutoSingleton<T> - 自动创建单例
- 如果场景中没有，自动创建
- 适合工具类

```csharp
public class AudioManager : AutoSingleton<AudioManager>
{
    // 自动创建
}
```

### 2. ObjectPool.cs（对象池）

```csharp
// 创建对象池
var pool = new ObjectPool<Bullet>(
    prefab: bulletPrefab,
    container: transform,
    initialSize: 20,
    maxSize: 100
);

// 获取对象
Bullet bullet = pool.Get(position, rotation);

// 回收对象
pool.Return(bullet);

// 回收全部
pool.ReturnAll();
```

### 3. GameEvents.cs（事件系统）

```csharp
// 订阅事件
void OnEnable()
{
    GameEvents.OnGameStart += HandleGameStart;
    GameEvents.OnEnemyDied += HandleEnemyDied;
}

// 取消订阅
void OnDisable()
{
    GameEvents.OnGameStart -= HandleGameStart;
    GameEvents.OnEnemyDied -= HandleEnemyDied;
}

// 触发事件
GameEvents.TriggerGameStart();
GameEvents.TriggerEnemyDied(enemyType, position, xp, coin);
```

---

## 四、新项目初始化清单

### 必做
- [ ] 复制 Core 模块
- [ ] 修改命名空间
- [ ] 创建 GameManager
- [ ] 创建 GameEvents（按需定义事件）

### 推荐
- [ ] 复制 AudioManager
- [ ] 复制 UIManager
- [ ] 创建 GameSettings（ScriptableObject）

### 可选
- [ ] SaveManager（如需存档）
- [ ] FloatingTextManager（如需飘字）

---

## 五、命名空间规范

```csharp
YourGame                    // 根命名空间
├── Core                    // 核心框架
│   └── Pool                // 对象池
├── Logic                   // 游戏逻辑
│   ├── Player              // 玩家相关
│   ├── Enemy               // 敌人相关
│   └── Boss                // Boss相关
├── UI                      // 界面系统
│   ├── Panels              // 面板
│   └── FloatingText        // 飘字
├── Data                    // 数据层
│   ├── SO                  // ScriptableObject
│   └── Runtime             // 运行时数据
├── Audio                   // 音效系统
└── VFX                     // 特效系统
```

---

## 六、版本记录

| 版本 | 日期 | 更新内容 |
|------|------|----------|
| 1.0 | 2026-02-07 | 初始版本，从光与朽提取 |

---

*使用模板时如有问题，参考原项目代码*
