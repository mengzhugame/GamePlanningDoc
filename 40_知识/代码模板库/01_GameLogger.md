# GameLogger 全局日志管理器

## 适用场景
Unity项目中的日志管理，特别是需要发版到微信小游戏或其他平台时，需要一键关闭所有Debug日志以提升性能。

## 核心特性
1. 使用 `const bool` 在编译期剔除代码，做到零性能开销。
2. 统一日志格式，支持颜色区分。
3. 区分普通日志、警告日志（默认开启）、错误日志（默认开启）和性能日志。

## 代码实现

```csharp
// GameLogger.cs - 全局日志管理器（兼容版 - 使用 const bool）
// 用途：统一管理所有Debug.Log输出，支持一键开关
// 使用方法：
// 1. 开发时保留 ENABLE_LOG = true
// 2. 打包时改为 ENABLE_LOG = false
// 3. 代码中使用 GameLogger.Log() 替代 Debug.Log()

using UnityEngine;

namespace MakeupPuzzle.Core
{
    /// <summary>
    /// 全局日志管理器（兼容版）
    /// 功能：
    /// 1. 使用 const bool 控制日志输出
    /// 2. 统一日志格式，方便调试和追踪
    /// </summary>
    public static class GameLogger
    {
        // ==================== 🔧 日志开关（打包时改为 false） ====================
        private const bool ENABLE_LOG = false;  // ← 开发时 true，打包时 false
        // ==========================================================================
        
        // ==================== 普通日志 ====================
        
        /// <summary>
        /// 普通日志（白色）
        /// </summary>
        public static void Log(string message)
        {
            if (ENABLE_LOG)
            {
                UnityEngine.Debug.Log(message);
            }
        }
        
        /// <summary>
        /// 带标签的日志
        /// </summary>
        public static void Log(string tag, string message)
        {
            if (ENABLE_LOG)
            {
                UnityEngine.Debug.Log($"[{tag}] {message}");
            }
        }
        
        /// <summary>
        /// 带颜色的日志
        /// </summary>
        public static void LogColor(string message, string color)
        {
            if (ENABLE_LOG)
            {
                UnityEngine.Debug.Log($"<color={color}>{message}</color>");
            }
        }
        
        // ==================== 警告日志 ====================
        
        /// <summary>
        /// 警告日志（黄色）- 总是输出
        /// </summary>
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
        
        /// <summary>
        /// 带标签的警告日志
        /// </summary>
        public static void LogWarning(string tag, string message)
        {
            UnityEngine.Debug.LogWarning($"[{tag}] {message}");
        }
        
        // ==================== 错误日志 ====================
        
        /// <summary>
        /// 错误日志（红色）- 总是输出
        /// </summary>
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
        
        /// <summary>
        /// 带标签的错误日志
        /// </summary>
        public static void LogError(string tag, string message)
        {
            UnityEngine.Debug.LogError($"[{tag}] {message}");
        }
        
        // ==================== 性能日志 ====================
        
        /// <summary>
        /// 性能相关日志（青色）- 用于性能分析
        /// </summary>
        public static void LogPerformance(string message)
        {
            if (ENABLE_LOG)
            {
                UnityEngine.Debug.Log($"<color=cyan>[Performance] {message}</color>");
            }
        }
        
        /// <summary>
        /// 带计时的性能日志
        /// </summary>
        public static void LogPerformance(string tag, float timeMs)
        {
            if (ENABLE_LOG)
            {
                UnityEngine.Debug.Log($"<color=cyan>[Performance] {tag}: {timeMs:F2}ms</color>");
            }
        }
    }
}
```