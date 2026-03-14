# Unity 安全单例模式基类 (Singleton & PersistentSingleton)

## 适用场景
Unity项目中经常需要用到单例模式来管理全局状态。这里的模板提供了两种安全的单例实现：
1. **普通场景内单例**：切换场景时销毁，并在应用退出时防止重建。
2. **持久化单例 (DontDestroyOnLoad)**：跨场景持久化存在，并处理了线程锁和应用退出。

## 核心特性
- **禁止自动创建实例**：强制开发者在场景中预先放置组件，避免隐式的游戏对象生成导致难以排查的问题。
- **防止重复实例**：在 `Awake` 阶段检测到重复实例会自动销毁，保证唯一性。
- **生命周期安全**：监听 `OnApplicationQuit` 和 `OnDestroy`，避免在编辑器停止运行或退出游戏时抛出 NullReferenceException（"CS1626错误"等场景销毁时序问题）。

## 代码实现

### 1. 普通单例 (Singleton.cs)

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MakeupPuzzle.Core
{
    /// <summary>
    /// 普通单例基类（终极修复版）
    /// 用途：为场景内单例提供基础实现
    /// 特性：
    /// 1. 禁止自动创建实例（必须在场景中预先存在）
    /// 2. 场景切换时自动清理静态引用
    /// 3. 防止重复实例
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static bool applicationIsQuitting = false;
        
        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }
                
                // 🔧 关键修复：不自动创建实例，只查找现有实例
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    
                    // 如果找不到，给出警告但不创建
                    if (instance == null)
                    {
                        Debug.LogWarning($"[Singleton] No instance of {typeof(T)} found in scene. " +
                                       "Please add a GameObject with this component to the scene.");
                    }
                }
                
                return instance;
            }
        }
        
        protected virtual void Awake()
        {
            // 如果已有实例且不是自己
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            // 设置自己为单例实例
            instance = this as T;
        }
        
        /// <summary>
        /// 销毁时清理静态引用
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 只有当被销毁的是当前实例时，才清空静态引用
            if (instance == this)
            {
                instance = null;
            }
        }
        
        /// <summary>
        /// 应用退出时标记
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
    }
}
```

### 2. 持久化单例 (PersistentSingleton.cs)

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MakeupPuzzle.Core
{
    /// <summary>
    /// 持久化单例基类
    /// 用途：跨场景保持的单例，自动调用 DontDestroyOnLoad
    /// </summary>
    public class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static object lockObject = new object();
        private static bool applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<T>();

                        if (instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
```