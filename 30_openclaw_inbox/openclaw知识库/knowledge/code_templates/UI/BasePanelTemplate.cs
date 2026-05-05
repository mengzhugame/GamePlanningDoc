// ============================================================
// BasePanelTemplate.cs
// 文件位置: Assets/Scripts/UI/Panels/BasePanel.cs
// 用途：UI面板基类模板 - 所有面板的通用功能
// 使用：复制后根据项目需求修改
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace YourGame.UI.Panels
{
    /// <summary>
    /// UI面板基类
    /// 提供面板的通用功能：显示/隐藏、动画、按钮设置
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BasePanel : MonoBehaviour
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 配置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 动画设置 ═══")]
        [Tooltip("淡入时长")]
        [SerializeField] protected float fadeInDuration = 0.3f;

        [Tooltip("淡出时长")]
        [SerializeField] protected float fadeOutDuration = 0.2f;

        [Tooltip("是否暂停游戏")]
        [SerializeField] protected bool pauseGameWhenActive = true;

        [Header("═══ 调试 ═══")]
        [SerializeField] protected bool showDebugInfo = false;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 组件缓存
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected CanvasGroup canvasGroup;
        private Coroutine fadeCoroutine;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共属性
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public bool IsVisible => gameObject.activeSelf && canvasGroup.alpha > 0;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // Unity 生命周期
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            SetupButtons();
        }

        protected virtual void OnEnable()
        {
            if (pauseGameWhenActive)
            {
                Time.timeScale = 0f;
            }

            OnPanelShow();
        }

        protected virtual void OnDisable()
        {
            OnPanelHide();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 抽象方法（子类必须实现）
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 设置按钮回调（子类实现）
        /// </summary>
        protected abstract void SetupButtons();

        /// <summary>
        /// 面板显示时调用（子类可重写）
        /// </summary>
        protected virtual void OnPanelShow() { }

        /// <summary>
        /// 面板隐藏时调用（子类可重写）
        /// </summary>
        protected virtual void OnPanelHide() { }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共接口
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 显示面板（带淡入动画）
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (fadeInDuration > 0)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeIn());
            }
            else
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[{GetType().Name}] 显示");
            }
        }

        /// <summary>
        /// 隐藏面板（带淡出动画）
        /// </summary>
        public virtual void Hide()
        {
            if (fadeOutDuration > 0)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeOut());
            }
            else
            {
                HideImmediate();
            }

            if (showDebugInfo)
            {
                Debug.Log($"[{GetType().Name}] 隐藏");
            }
        }

        /// <summary>
        /// 立即隐藏面板（无动画）
        /// </summary>
        public void HideImmediate()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);

            if (pauseGameWhenActive)
            {
                Time.timeScale = 1f;
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 动画协程
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected virtual IEnumerator FadeIn()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            fadeCoroutine = null;
        }

        protected virtual IEnumerator FadeOut()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            fadeCoroutine = null;

            if (pauseGameWhenActive)
            {
                Time.timeScale = 1f;
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 工具方法
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 为按钮添加点击回调
        /// </summary>
        protected void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }
    }
}
