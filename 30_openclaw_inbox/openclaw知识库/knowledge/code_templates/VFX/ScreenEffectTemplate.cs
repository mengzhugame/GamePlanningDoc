// ============================================================
// ScreenEffectTemplate.cs
// 文件位置: Assets/Scripts/VFX/ScreenEffect/ScreenEffectController.cs
// 用途：屏幕特效控制器模板 - 暗角、闪白、玻璃裂纹等
// 使用：复制后根据项目需求修改
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace YourGame.VFX
{
    /// <summary>
    /// 屏幕特效控制器
    /// 管理各种全屏后处理效果
    /// </summary>
    public class ScreenEffectController : Core.Singleton<ScreenEffectController>
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // UI 覆盖层引用
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ UI 覆盖层 ═══")]
        [Tooltip("闪白 Image")]
        [SerializeField] private Image flashImage;

        [Tooltip("红色暗角 Image（用于受伤效果）")]
        [SerializeField] private Image damageVignetteImage;

        [Tooltip("玻璃裂纹 Image")]
        [SerializeField] private Image glassCrackImage;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 闪白设置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 闪白设置 ═══")]
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private float flashMaxAlpha = 0.8f;
        [SerializeField] private Color flashColor = Color.white;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 受伤暗角设置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 受伤暗角设置 ═══")]
        [SerializeField] private float damageFadeInDuration = 0.1f;
        [SerializeField] private float damageFadeOutDuration = 0.3f;
        [SerializeField] private float damageMaxAlpha = 0.5f;
        [SerializeField] private Color damageColor = new Color(0.8f, 0f, 0f, 1f);

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 玻璃裂纹设置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 玻璃裂纹设置 ═══")]
        [SerializeField] private float glassCrackDuration = 0.5f;
        [SerializeField] private float glassCrackMaxAlpha = 0.8f;

        [Header("═══ 调试 ═══")]
        [SerializeField] private bool showDebugInfo = false;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 运行时状态
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private Coroutine flashCoroutine;
        private Coroutine damageCoroutine;
        private Coroutine crackCoroutine;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 生命周期
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected override void OnSingletonAwake()
        {
            InitializeOverlays();
        }

        private void InitializeOverlays()
        {
            SetImageAlpha(flashImage, 0f);
            SetImageAlpha(damageVignetteImage, 0f);
            SetImageAlpha(glassCrackImage, 0f);

            SetImageActive(flashImage, false);
            SetImageActive(damageVignetteImage, false);
            SetImageActive(glassCrackImage, false);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 闪白效果
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 播放闪白效果
        /// </summary>
        public void PlayFlash()
        {
            PlayFlash(flashMaxAlpha, flashDuration);
        }

        public void PlayFlash(float maxAlpha, float duration)
        {
            if (flashImage == null) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashCoroutine(maxAlpha, duration));

            if (showDebugInfo)
            {
                Debug.Log("[ScreenEffect] 播放闪白");
            }
        }

        private IEnumerator FlashCoroutine(float maxAlpha, float duration)
        {
            flashImage.color = flashColor;
            SetImageActive(flashImage, true);

            // 快速淡入
            float fadeInTime = duration * 0.2f;
            float elapsed = 0f;

            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                SetImageAlpha(flashImage, Mathf.Lerp(0f, maxAlpha, elapsed / fadeInTime));
                yield return null;
            }

            // 淡出
            elapsed = 0f;
            float fadeOutTime = duration * 0.8f;

            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                SetImageAlpha(flashImage, Mathf.Lerp(maxAlpha, 0f, elapsed / fadeOutTime));
                yield return null;
            }

            SetImageActive(flashImage, false);
            flashCoroutine = null;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 受伤暗角效果
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 播放受伤暗角效果
        /// </summary>
        public void PlayDamageVignette()
        {
            if (damageVignetteImage == null) return;

            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
            damageCoroutine = StartCoroutine(DamageVignetteCoroutine());

            if (showDebugInfo)
            {
                Debug.Log("[ScreenEffect] 播放受伤暗角");
            }
        }

        private IEnumerator DamageVignetteCoroutine()
        {
            damageVignetteImage.color = damageColor;
            SetImageActive(damageVignetteImage, true);

            // 淡入
            float elapsed = 0f;
            while (elapsed < damageFadeInDuration)
            {
                elapsed += Time.deltaTime;
                SetImageAlpha(damageVignetteImage, Mathf.Lerp(0f, damageMaxAlpha, elapsed / damageFadeInDuration));
                yield return null;
            }

            // 淡出
            elapsed = 0f;
            while (elapsed < damageFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                SetImageAlpha(damageVignetteImage, Mathf.Lerp(damageMaxAlpha, 0f, elapsed / damageFadeOutDuration));
                yield return null;
            }

            SetImageActive(damageVignetteImage, false);
            damageCoroutine = null;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 玻璃裂纹效果
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 播放玻璃裂纹效果
        /// </summary>
        public void PlayGlassCrack()
        {
            if (glassCrackImage == null) return;

            if (crackCoroutine != null)
            {
                StopCoroutine(crackCoroutine);
            }
            crackCoroutine = StartCoroutine(GlassCrackCoroutine());

            if (showDebugInfo)
            {
                Debug.Log("[ScreenEffect] 播放玻璃裂纹");
            }
        }

        private IEnumerator GlassCrackCoroutine()
        {
            SetImageActive(glassCrackImage, true);

            // 快速显示
            SetImageAlpha(glassCrackImage, glassCrackMaxAlpha);

            // 保持一段时间
            yield return new WaitForSeconds(glassCrackDuration * 0.3f);

            // 淡出
            float elapsed = 0f;
            float fadeOutTime = glassCrackDuration * 0.7f;

            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                SetImageAlpha(glassCrackImage, Mathf.Lerp(glassCrackMaxAlpha, 0f, elapsed / fadeOutTime));
                yield return null;
            }

            SetImageActive(glassCrackImage, false);
            crackCoroutine = null;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共接口
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>
        /// 停止所有效果
        /// </summary>
        public void StopAllEffects()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
            if (crackCoroutine != null)
            {
                StopCoroutine(crackCoroutine);
                crackCoroutine = null;
            }

            SetImageActive(flashImage, false);
            SetImageActive(damageVignetteImage, false);
            SetImageActive(glassCrackImage, false);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 工具方法
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void SetImageAlpha(Image image, float alpha)
        {
            if (image != null)
            {
                Color c = image.color;
                c.a = alpha;
                image.color = c;
            }
        }

        private void SetImageActive(Image image, bool active)
        {
            if (image != null)
            {
                image.gameObject.SetActive(active);
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 调试
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
        [ContextMenu("Test Flash")]
        private void TestFlash() => PlayFlash();

        [ContextMenu("Test Damage Vignette")]
        private void TestDamage() => PlayDamageVignette();

        [ContextMenu("Test Glass Crack")]
        private void TestCrack() => PlayGlassCrack();
#endif
    }
}
