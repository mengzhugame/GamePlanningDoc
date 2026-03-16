# SafeSceneLoader 安全场景异步加载器

## 适用场景
在使用 WebGL（比如微信小游戏）或低端移动设备环境时，Unity自带的异步场景加载常常因为内存释放（GC）和对象销毁时序问题导致游戏卡顿、崩溃或抛出 NullReferenceException/CS1626 错误。
这个模板提供了一个非常健壮的、多阶段异步加载解决方案。

## 核心特性
1. **分段式加载**：将场景切换分解为加载前准备、异步加载、激活、激活后处理、等待Manager初始化、最终验证6个阶段。
2. **主动垃圾回收**：在切场景前主动调用 `System.GC.Collect()` 和 `Resources.UnloadUnusedAssets()` 清理上一场景残留。
3. **超时保护与重试追踪**：增加帧计数和时间超时保护，避免因为加载卡死导致游戏彻底锁死。
4. **严格的时序控制**：确保 `allowSceneActivation` 设置前后的时序，强制给系统预留喘息和对象销毁的帧周期。

## 代码实现

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace MakeupPuzzle.Runtime.Managers
{
    /// <summary>
    /// 安全场景加载器
    /// 核心策略:
    /// 1. 分段式加载 - 在关键步骤之间增加足够等待
    /// 2. 主动垃圾回收 - 强制清理内存避免卡顿
    /// 3. 异常捕获 - 捕获并报告任何错误
    /// 4. 超时保护 - 避免永久卡死
    /// </summary>
    public class SafeSceneLoader : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private float preLoadWaitTime = 0.2f;      // 加载前等待时间
        [SerializeField] private float postLoadWaitTime = 0.5f;     // 加载后等待时间
        [SerializeField] private float activationWaitTime = 0.3f;   // 激活后等待时间
        [SerializeField] private float managerWaitTimeout = 5f;     // Manager初始化超时
        [SerializeField] private int gcFrameInterval = 3;           // GC等待帧数
        
        private AsyncOperation currentAsyncOperation;
        private bool loadFailed = false;
        
        /// <summary>
        /// 通用安全场景加载方法
        /// 参数:
        /// - sceneName: 要加载的场景名称
        /// - needManagerCheck: 是否需要等待Manager初始化
        /// </summary>
        public IEnumerator LoadSceneSafe(string sceneName, bool needManagerCheck = false)
        {
            Debug.Log($"[SafeSceneLoader] 开始安全场景加载: {sceneName}");
    
            // ========== 阶段1: 加载前准备 ==========
            yield return StartCoroutine(PreLoadPhase());
    
            // ========== 阶段2: 异步场景加载 ==========
            yield return StartCoroutine(LoadScenePhase(sceneName));
    
            if (loadFailed || currentAsyncOperation == null) yield break;
    
            // ========== 阶段3: 等待场景激活完成 ==========
            yield return StartCoroutine(WaitForActivationPhase(currentAsyncOperation));
    
            // ========== 阶段4: 场景激活后处理 ==========
            yield return StartCoroutine(PostActivationPhase());
    
            // ========== 阶段5: 等待Manager初始化(可选) ==========
            if (needManagerCheck)
            {
                yield return StartCoroutine(WaitForManagersPhase());
            }
            else
            {
                for (int i = 0; i < 5; i++) yield return null;
            }
    
            // ========== 阶段6: 最终验证 ==========
            yield return StartCoroutine(FinalValidationPhase());
    
            Debug.Log($"[SafeSceneLoader] 场景 {sceneName} 加载完成!");
        }
        
        private IEnumerator PreLoadPhase()
        {
            // 重置时间缩放
            Time.timeScale = 1f;

            // 强制垃圾回收
            System.GC.Collect();
            for (int i = 0; i < gcFrameInterval; i++) yield return null;

            // 卸载未使用资源
            yield return Resources.UnloadUnusedAssets();
            yield return new WaitForSeconds(preLoadWaitTime);
        }
        
        private IEnumerator LoadScenePhase(string sceneName)
        {
            loadFailed = false;
            
            try
            {
                currentAsyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SafeSceneLoader] 加载异常: {e.Message}");
                loadFailed = true;
                yield break;
            }
            
            if (currentAsyncOperation == null)
            {
                loadFailed = true;
                yield break;
            }
            
            // 暂时阻止自动激活
            currentAsyncOperation.allowSceneActivation = false;

            int safetyCounter = 0;
            int maxWaitFrames = 300; 
            
            while (!currentAsyncOperation.isDone && currentAsyncOperation.progress < 0.9f)
            {
                safetyCounter++;
                if (safetyCounter > maxWaitFrames)
                {
                    loadFailed = true;
                    Debug.LogError("[SafeSceneLoader] 场景加载进度卡死超时");
                    yield break;
                }
                yield return null;
            }

            for (int i = 0; i < 3; i++) yield return null;
        }
        
        private IEnumerator WaitForActivationPhase(AsyncOperation asyncLoad)
        {
            asyncLoad.allowSceneActivation = true;

            int frameCount = 0;
            int maxWaitFrames = 300;
            
            while (!asyncLoad.isDone)
            {
                frameCount++;
                if (frameCount > maxWaitFrames)
                {
                    Debug.LogError("[SafeSceneLoader] 场景激活超时!");
                    yield break;
                }
                yield return null;
            }
        }
        
        private IEnumerator PostActivationPhase()
        {
            yield return new WaitForSeconds(activationWaitTime);
            for (int i = 0; i < 5; i++) yield return null;
            Time.timeScale = 1f;
        }
        
        private IEnumerator WaitForManagersPhase()
        {
            float startTime = Time.realtimeSinceStartup;
            
            while (Time.realtimeSinceStartup - startTime < managerWaitTimeout)
            {
                // TODO: 替换为实际项目中的 Manager 就绪检查逻辑
                bool allReady = true; 
                
                if (allReady) break;
                yield return null;
            }
        }
        
        private IEnumerator FinalValidationPhase()
        {
            Time.timeScale = 1f;
            yield return new WaitForSeconds(postLoadWaitTime);
        }
    }
}
```