// ============================================================
// FloatingTextTemplate.cs
// 文件位置: Assets/Scripts/UI/FloatingText/FloatingText.cs
// 用途：飘字系统模板 - 伤害数字、提示文本
// 使用：复制后根据项目需求修改
// ============================================================

using UnityEngine;
using TMPro;
using System.Collections;

namespace YourGame.UI.FloatingText
{
    /// <summary>
    /// 飘字类型
    /// </summary>
    public enum FloatingTextType
    {
        Normal,     // 普通伤害
        Critical,   // 暴击伤害
        Heal,       // 治疗
        Buff,       // 增益
        Debuff      // 减益
    }

    /// <summary>
    /// 飘字组件
    /// 支持对象池回收
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 配置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 组件引用 ═══")]
        [SerializeField] private TextMeshProUGUI textComponent;

        [Header("═══ 动画设置 ═══")]
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private float riseSpeed = 50f;
        [SerializeField] private float fadeDelay = 0.5f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1.2f, 1, 1f);

        [Header("═══ 类型颜色 ═══")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color healColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color buffColor = new Color(0.3f, 0.8f, 1f);
        [SerializeField] private Color debuffColor = new Color(0.8f, 0.3f, 0.8f);

        [Header("═══ 随机偏移 ═══")]
        [SerializeField] private float randomOffsetX = 30f;
        [SerializeField] private float randomOffsetY = 10f;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 运行时状态
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private RectTransform rectTransform;
        private Vector3 startPosition;
        private Color startColor;
        private bool isAnimating = false;

        // 对象池回调
        private System.Action<FloatingText> onComplete;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // Unity 生命周期
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (textComponent == null)
            {
                textComponent = GetComponent<TextMeshProUGUI>();
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共接口
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 显示飘字
        /// </summary>
        /// <param name="text">显示文本</param>
        /// <param name="worldPosition">世界坐标位置</param>
        /// <param name="type">飘字类型</param>
        /// <param name="camera">相机（用于坐标转换）</param>
        /// <param name="onComplete">完成回调（用于对象池回收）</param>
        public void Show(string text, Vector3 worldPosition, FloatingTextType type,
            Camera camera = null, System.Action<FloatingText> onComplete = null)
        {
            this.onComplete = onComplete;

            // 设置文本
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.color = GetColorForType(type);
            }

            // 转换世界坐标到屏幕坐标
            if (camera == null) camera = Camera.main;

            if (camera != null && rectTransform != null)
            {
                Vector3 screenPos = camera.WorldToScreenPoint(worldPosition);

                // 添加随机偏移
                screenPos.x += Random.Range(-randomOffsetX, randomOffsetX);
                screenPos.y += Random.Range(-randomOffsetY, randomOffsetY);

                rectTransform.position = screenPos;
            }

            startPosition = rectTransform.position;
            startColor = textComponent.color;

            // 开始动画
            StartCoroutine(AnimateCoroutine());
        }

        /// <summary>
        /// 显示飘字（屏幕坐标版本）
        /// </summary>
        public void ShowAtScreenPosition(string text, Vector2 screenPosition, FloatingTextType type,
            System.Action<FloatingText> onComplete = null)
        {
            this.onComplete = onComplete;

            // 设置文本
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.color = GetColorForType(type);
            }

            // 设置位置
            if (rectTransform != null)
            {
                rectTransform.position = screenPosition;
            }

            startPosition = rectTransform.position;
            startColor = textComponent.color;

            // 开始动画
            StartCoroutine(AnimateCoroutine());
        }

        /// <summary>
        /// 重置状态（对象池使用）
        /// </summary>
        public void Reset()
        {
            StopAllCoroutines();
            isAnimating = false;

            if (textComponent != null)
            {
                textComponent.text = "";
                textComponent.color = normalColor;
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 动画
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private IEnumerator AnimateCoroutine()
        {
            isAnimating = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / duration;

                // 上升
                Vector3 newPos = startPosition + Vector3.up * (riseSpeed * elapsed);
                rectTransform.position = newPos;

                // 缩放
                float scale = scaleCurve.Evaluate(normalizedTime);
                rectTransform.localScale = Vector3.one * scale;

                // 淡出
                if (elapsed > fadeDelay)
                {
                    float fadeProgress = (elapsed - fadeDelay) / (duration - fadeDelay);
                    Color c = startColor;
                    c.a = Mathf.Lerp(1f, 0f, fadeProgress);
                    textComponent.color = c;
                }

                yield return null;
            }

            isAnimating = false;

            // 通知完成（用于对象池回收）
            onComplete?.Invoke(this);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 工具方法
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private Color GetColorForType(FloatingTextType type)
        {
            return type switch
            {
                FloatingTextType.Critical => criticalColor,
                FloatingTextType.Heal => healColor,
                FloatingTextType.Buff => buffColor,
                FloatingTextType.Debuff => debuffColor,
                _ => normalColor
            };
        }
    }
}
