// ============================================================
// SettlementPanelTemplate.cs
// 文件位置: Assets/Scripts/UI/Panels/SettlementPanel.cs
// 用途：结算面板模板 - 胜利/失败结算界面
// 使用：复制后根据项目需求修改
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace YourGame.UI.Panels
{
    /// <summary>
    /// 结算面板
    /// 胜利和失败共用同一个面板，通过切换标题区分
    /// </summary>
    public class SettlementPanel : BasePanel
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // UI 元素
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 标题 ═══")]
        [SerializeField] private GameObject victoryTitle;
        [SerializeField] private GameObject defeatTitle;

        [Header("═══ 信息面板 ═══")]
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("═══ 按钮 ═══")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button nextButton;

        [Header("═══ 数字动画 ═══")]
        [SerializeField] private float numberAnimDuration = 1.0f;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 运行时状态
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private bool isVictory = false;
        private int targetCoins = 0;
        private int targetScore = 0;
        private float playTime = 0f;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 按钮设置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected override void SetupButtons()
        {
            BindButton(retryButton, OnRetryClicked);
            BindButton(menuButton, OnMenuClicked);
            BindButton(nextButton, OnNextClicked);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共接口
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 显示结算内容
        /// </summary>
        /// <param name="victory">是否胜利</param>
        /// <param name="coins">获得金币</param>
        /// <param name="time">游戏时长</param>
        /// <param name="score">得分</param>
        public void Show(bool victory, int coins = 0, float time = 0f, int score = 0)
        {
            isVictory = victory;
            targetCoins = coins;
            targetScore = score;
            playTime = time;

            // 更新UI
            UpdateTitleDisplay();
            UpdateButtonDisplay();

            // 显示面板
            base.Show();

            // 开始数字动画
            StartCoroutine(AnimateNumbers());

            if (showDebugInfo)
            {
                Debug.Log($"[SettlementPanel] 显示结算 (胜利: {victory})");
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // UI 更新
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void UpdateTitleDisplay()
        {
            if (victoryTitle != null)
            {
                victoryTitle.SetActive(isVictory);
            }
            if (defeatTitle != null)
            {
                defeatTitle.SetActive(!isVictory);
            }
        }

        private void UpdateButtonDisplay()
        {
            // 胜利时显示"下一关"，失败时显示"重试"
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(isVictory);
            }
            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(!isVictory);
            }
        }

        private IEnumerator AnimateNumbers()
        {
            float elapsed = 0f;

            // 先设置时间（无动画）
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(playTime / 60f);
                int seconds = Mathf.FloorToInt(playTime % 60f);
                timeText.text = $"{minutes:00}:{seconds:00}";
            }

            // 数字滚动动画
            while (elapsed < numberAnimDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / numberAnimDuration;

                // 使用EaseOutQuad缓动
                t = 1f - (1f - t) * (1f - t);

                if (coinText != null)
                {
                    int displayCoins = Mathf.RoundToInt(Mathf.Lerp(0, targetCoins, t));
                    coinText.text = displayCoins.ToString();
                }

                if (scoreText != null)
                {
                    int displayScore = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, t));
                    scoreText.text = displayScore.ToString();
                }

                yield return null;
            }

            // 确保最终显示正确数值
            if (coinText != null) coinText.text = targetCoins.ToString();
            if (scoreText != null) scoreText.text = targetScore.ToString();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 按钮回调
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void OnRetryClicked()
        {
            if (showDebugInfo) Debug.Log("[SettlementPanel] 点击重试");

            Hide();

            // TODO: 调用 GameManager.Instance.RestartGame();
        }

        private void OnMenuClicked()
        {
            if (showDebugInfo) Debug.Log("[SettlementPanel] 点击返回主菜单");

            Hide();

            // TODO: 调用 GameManager.Instance.LoadMainMenu();
        }

        private void OnNextClicked()
        {
            if (showDebugInfo) Debug.Log("[SettlementPanel] 点击下一关");

            Hide();

            // TODO: 调用 GameManager.Instance.LoadNextLevel();
        }
    }
}
