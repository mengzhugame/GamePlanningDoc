// ============================================================
// UIManagerTemplate.cs
// 文件位置: Assets/Scripts/UI/UIManager.cs
// 用途：UI管理器模板 - 统一管理面板显示隐藏
// 使用：复制后修改命名空间和项目特定逻辑
// ============================================================

using UnityEngine;

namespace YourGame.UI
{
    /// <summary>
    /// UI管理器
    /// 统一管理所有UI面板的显示/隐藏
    /// </summary>
    public class UIManager : Core.Singleton<UIManager>
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 面板引用
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 面板引用 ═══")]
        [Tooltip("暂停面板")]
        [SerializeField] private GameObject pausePanel;

        [Tooltip("设置面板")]
        [SerializeField] private GameObject settingsPanel;

        [Tooltip("结算面板")]
        [SerializeField] private GameObject settlementPanel;

        [Header("═══ 调试 ═══")]
        [SerializeField] private bool showDebugInfo = false;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 运行时状态
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private GameObject currentActivePanel = null;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共属性
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public bool IsAnyPanelActive => currentActivePanel != null && currentActivePanel.activeSelf;
        public GameObject ActivePanel => currentActivePanel;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 生命周期
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected override void OnSingletonAwake()
        {
            HideAllPanels();
        }

        private void OnEnable()
        {
            // 订阅游戏事件
            // GameEvents.OnGameVictory += OnGameVictory;
            // GameEvents.OnGameDefeat += OnGameDefeat;
            // GameEvents.OnGamePaused += OnGamePaused;
            // GameEvents.OnGameResumed += OnGameResumed;
        }

        private void OnDisable()
        {
            // 取消订阅
            // GameEvents.OnGameVictory -= OnGameVictory;
            // GameEvents.OnGameDefeat -= OnGameDefeat;
            // GameEvents.OnGamePaused -= OnGamePaused;
            // GameEvents.OnGameResumed -= OnGameResumed;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 事件回调
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void OnGameVictory()
        {
            ShowSettlementPanel(true);
        }

        private void OnGameDefeat()
        {
            ShowSettlementPanel(false);
        }

        private void OnGamePaused()
        {
            ShowPausePanel();
        }

        private void OnGameResumed()
        {
            HidePausePanel();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 面板控制 - 暂停
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void ShowPausePanel()
        {
            ShowPanel(pausePanel);
        }

        public void HidePausePanel()
        {
            HidePanel(pausePanel);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 面板控制 - 设置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void ShowSettingsPanel()
        {
            ShowPanel(settingsPanel);
        }

        public void HideSettingsPanel()
        {
            HidePanel(settingsPanel);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 面板控制 - 结算
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void ShowSettlementPanel(bool isVictory)
        {
            HideAllPanels();
            ShowPanel(settlementPanel);

            // 通知结算面板控制器
            // var controller = settlementPanel.GetComponent<SettlementPanelController>();
            // controller?.Show(isVictory);

            if (showDebugInfo)
            {
                Debug.Log($"[UIManager] 结算面板显示 (胜利: {isVictory})");
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 通用方法
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void ShowPanel(GameObject panel)
        {
            if (panel == null)
            {
                Debug.LogWarning("[UIManager] 尝试显示空面板");
                return;
            }

            panel.SetActive(true);
            currentActivePanel = panel;

            if (showDebugInfo)
            {
                Debug.Log($"[UIManager] 显示面板: {panel.name}");
            }
        }

        private void HidePanel(GameObject panel)
        {
            if (panel == null) return;

            panel.SetActive(false);

            if (currentActivePanel == panel)
            {
                currentActivePanel = null;
            }
        }

        public void HideAllPanels()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (settlementPanel != null) settlementPanel.SetActive(false);

            currentActivePanel = null;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 调试
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 200, 250, 190, 100));
            GUILayout.Label("=== UIManager ===");
            GUILayout.Label($"Active: {(currentActivePanel != null ? currentActivePanel.name : "None")}");

            if (GUILayout.Button("Show Pause")) ShowPausePanel();
            if (GUILayout.Button("Hide All")) HideAllPanels();

            GUILayout.EndArea();
        }
#endif
    }
}
