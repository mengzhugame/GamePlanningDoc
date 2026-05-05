// ============================================================
// GameManagerTemplate.cs
// 文件位置: Assets/Scripts/Managers/GameManager.cs
// 用途：游戏管理器模板 - 游戏状态管理、场景切换
// 使用：复制后修改命名空间和项目特定逻辑
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

namespace YourGame.Core
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        Menu,       // 主菜单
        Playing,    // 游戏中
        Paused,     // 暂停
        Victory,    // 胜利
        Defeat      // 失败
    }

    /// <summary>
    /// 游戏管理器
    /// 负责游戏状态管理、场景切换
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 配置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 场景配置 ═══")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "Game";

        [Header("═══ 调试 ═══")]
        [SerializeField] private bool showDebugInfo = false;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 运行时状态
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private GameState currentState = GameState.Menu;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共属性
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 生命周期
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected override void OnSingletonAwake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnSingletonDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[GameManager] 场景加载: {scene.name}");
            }

            if (scene.name == gameSceneName)
            {
                StartGame();
            }
            else if (scene.name == mainMenuSceneName)
            {
                ChangeState(GameState.Menu);
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 场景管理
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            ChangeState(GameState.Menu);
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void LoadGameScene()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 游戏流程
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void StartGame()
        {
            Time.timeScale = 1f;
            ChangeState(GameState.Playing);
            // GameEvents.TriggerGameStart();

            if (showDebugInfo)
            {
                Debug.Log("[GameManager] 游戏开始");
            }
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;

            Time.timeScale = 0f;
            ChangeState(GameState.Paused);
            // GameEvents.TriggerGamePaused();

            if (showDebugInfo)
            {
                Debug.Log("[GameManager] 游戏暂停");
            }
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;

            Time.timeScale = 1f;
            ChangeState(GameState.Playing);
            // GameEvents.TriggerGameResumed();

            if (showDebugInfo)
            {
                Debug.Log("[GameManager] 游戏恢复");
            }
        }

        public void Victory()
        {
            if (currentState != GameState.Playing) return;

            Time.timeScale = 0f;
            ChangeState(GameState.Victory);
            // GameEvents.TriggerGameVictory();

            if (showDebugInfo)
            {
                Debug.Log("[GameManager] 游戏胜利");
            }
        }

        public void Defeat()
        {
            if (currentState != GameState.Playing) return;

            Time.timeScale = 0f;
            ChangeState(GameState.Defeat);
            // GameEvents.TriggerGameDefeat();

            if (showDebugInfo)
            {
                Debug.Log("[GameManager] 游戏失败");
            }
        }

        public void RestartGame()
        {
            LoadGameScene();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 状态管理
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            // GameEvents.TriggerGameStateChanged(newState);

            if (showDebugInfo)
            {
                Debug.Log($"[GameManager] 状态变化: {newState}");
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 调试
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 120));
            GUILayout.Label("=== GameManager ===");
            GUILayout.Label($"State: {currentState}");

            if (currentState == GameState.Playing)
            {
                if (GUILayout.Button("Force Victory")) Victory();
                if (GUILayout.Button("Force Defeat")) Defeat();
            }

            GUILayout.EndArea();
        }
#endif
    }
}
