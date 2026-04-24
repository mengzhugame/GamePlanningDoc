// ============================================================
// GameLoggerTemplate.cs
// 文件位置: Assets/Scripts/Utils/GameLogger.cs
// 用途：统一日志系统 - 支持日志级别和条件编译
// 使用：复制后根据需要调整日志级别
// ============================================================

using UnityEngine;

namespace YourGame.Core
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }

    /// <summary>
    /// 游戏日志系统
    /// 统一管理所有日志输出，支持级别过滤和条件编译
    /// </summary>
    public static class GameLogger
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 配置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static LogLevel currentLevel = LogLevel.Debug;
#else
        private static LogLevel currentLevel = LogLevel.Warning;
#endif

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共属性
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public static LogLevel CurrentLevel
        {
            get => currentLevel;
            set => currentLevel = value;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 日志方法
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 输出调试日志（仅开发版本）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void LogDebug(string message, Object context = null)
        {
            if (currentLevel >= LogLevel.Debug)
            {
                Debug.Log($"<color=gray>[DEBUG]</color> {message}", context);
            }
        }

        /// <summary>
        /// 输出信息日志
        /// </summary>
        public static void Log(string message, Object context = null)
        {
            if (currentLevel >= LogLevel.Info)
            {
                Debug.Log(message, context);
            }
        }

        /// <summary>
        /// 输出带颜色的信息日志
        /// </summary>
        public static void Log(string message, string color, Object context = null)
        {
            if (currentLevel >= LogLevel.Info)
            {
                Debug.Log($"<color={color}>{message}</color>", context);
            }
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        public static void LogWarning(string message, Object context = null)
        {
            if (currentLevel >= LogLevel.Warning)
            {
                Debug.LogWarning($"<color=yellow>[WARN]</color> {message}", context);
            }
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        public static void LogError(string message, Object context = null)
        {
            if (currentLevel >= LogLevel.Error)
            {
                Debug.LogError($"<color=red>[ERROR]</color> {message}", context);
            }
        }

        /// <summary>
        /// 输出带类名的日志
        /// </summary>
        public static void Log<T>(string message, Object context = null)
        {
            if (currentLevel >= LogLevel.Info)
            {
                Debug.Log($"<color=cyan>[{typeof(T).Name}]</color> {message}", context);
            }
        }

        /// <summary>
        /// 输出带类名的警告日志
        /// </summary>
        public static void LogWarning<T>(string message, Object context = null)
        {
            if (currentLevel >= LogLevel.Warning)
            {
                Debug.LogWarning($"<color=yellow>[{typeof(T).Name}]</color> {message}", context);
            }
        }

        /// <summary>
        /// 输出带类名的错误日志
        /// </summary>
        public static void LogError<T>(string message, Object context = null)
        {
            if (currentLevel >= LogLevel.Error)
            {
                Debug.LogError($"<color=red>[{typeof(T).Name}]</color> {message}", context);
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 断言
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 断言检查（仅开发版本）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                LogError($"[ASSERT FAILED] {message}");
            }
        }

        /// <summary>
        /// 非空断言
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void AssertNotNull(object obj, string objName)
        {
            if (obj == null)
            {
                LogError($"[ASSERT FAILED] {objName} is null!");
            }
        }
    }
}
