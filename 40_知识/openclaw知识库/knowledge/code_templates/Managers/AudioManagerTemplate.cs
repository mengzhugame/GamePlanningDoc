// ============================================================
// AudioManagerTemplate.cs
// 文件位置: Assets/Scripts/Managers/AudioManager.cs
// 用途：音频管理器模板 - BGM和SFX播放控制
// 使用：复制后修改命名空间和项目特定逻辑
// ============================================================

using UnityEngine;
using System.Collections;

namespace YourGame.Core
{
    /// <summary>
    /// 音频管理器
    /// 负责BGM和SFX的播放控制
    /// </summary>
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 配置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Header("═══ 音效资源 ═══")]
        [SerializeField] private AudioClip menuBGM;
        [SerializeField] private AudioClip gameBGM;
        [SerializeField] private AudioClip victoryBGM;
        [SerializeField] private AudioClip defeatBGM;

        [Header("═══ 音量配置 ═══")]
        [Range(0f, 1f)]
        [SerializeField] private float defaultBGMVolume = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float defaultSFXVolume = 0.7f;

        [Header("═══ 调试 ═══")]
        [SerializeField] private bool showDebugInfo = false;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 音频组件
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private AudioSource bgmSource;
        private AudioSource sfxSource;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 音量设置
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private const string PREF_BGM_VOLUME = "BGMVolume";
        private const string PREF_SFX_VOLUME = "SFXVolume";
        private const string PREF_BGM_ENABLED = "BGMEnabled";
        private const string PREF_SFX_ENABLED = "SFXEnabled";

        private float bgmVolume;
        private float sfxVolume;
        private bool bgmEnabled = true;
        private bool sfxEnabled = true;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 公共属性
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public float BGMVolume
        {
            get => bgmVolume;
            set
            {
                bgmVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_BGM_VOLUME, bgmVolume);
                UpdateBGMVolume();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_SFX_VOLUME, sfxVolume);
            }
        }

        public bool BGMEnabled
        {
            get => bgmEnabled;
            set
            {
                bgmEnabled = value;
                PlayerPrefs.SetInt(PREF_BGM_ENABLED, value ? 1 : 0);
                UpdateBGMVolume();
            }
        }

        public bool SFXEnabled
        {
            get => sfxEnabled;
            set
            {
                sfxEnabled = value;
                PlayerPrefs.SetInt(PREF_SFX_ENABLED, value ? 1 : 0);
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 生命周期
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        protected override void OnSingletonAwake()
        {
            CreateAudioSources();
            LoadSettings();
        }

        private void CreateAudioSources()
        {
            // BGM Source
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

            // SFX Source
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            if (showDebugInfo)
            {
                Debug.Log("[AudioManager] 音频源创建完成");
            }
        }

        private void LoadSettings()
        {
            bgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOLUME, defaultBGMVolume);
            sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, defaultSFXVolume);
            bgmEnabled = PlayerPrefs.GetInt(PREF_BGM_ENABLED, 1) == 1;
            sfxEnabled = PlayerPrefs.GetInt(PREF_SFX_ENABLED, 1) == 1;

            UpdateBGMVolume();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // BGM 控制
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void PlayBGM(AudioClip clip, float fadeTime = 0.5f)
        {
            if (clip == null) return;

            if (fadeTime > 0 && bgmSource.isPlaying)
            {
                StartCoroutine(CrossfadeBGM(clip, fadeTime));
            }
            else
            {
                bgmSource.clip = clip;
                bgmSource.Play();
            }

            if (showDebugInfo)
            {
                Debug.Log($"[AudioManager] 播放BGM: {clip.name}");
            }
        }

        public void StopBGM(float fadeTime = 0.5f)
        {
            if (fadeTime > 0)
            {
                StartCoroutine(FadeOutBGM(fadeTime));
            }
            else
            {
                bgmSource.Stop();
            }
        }

        public void PlayMenuBGM() => PlayBGM(menuBGM);
        public void PlayGameBGM() => PlayBGM(gameBGM);
        public void PlayVictoryBGM() => PlayBGM(victoryBGM, 0f);
        public void PlayDefeatBGM() => PlayBGM(defeatBGM, 0f);

        private void UpdateBGMVolume()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = bgmEnabled ? bgmVolume : 0f;
            }
        }

        private IEnumerator CrossfadeBGM(AudioClip newClip, float fadeTime)
        {
            float startVolume = bgmSource.volume;

            // Fade out
            float timer = 0f;
            while (timer < fadeTime)
            {
                timer += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
                yield return null;
            }

            // Switch clip
            bgmSource.clip = newClip;
            bgmSource.Play();

            // Fade in
            timer = 0f;
            float targetVolume = bgmEnabled ? bgmVolume : 0f;
            while (timer < fadeTime)
            {
                timer += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, targetVolume, timer / fadeTime);
                yield return null;
            }
        }

        private IEnumerator FadeOutBGM(float fadeTime)
        {
            float startVolume = bgmSource.volume;
            float timer = 0f;

            while (timer < fadeTime)
            {
                timer += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.volume = bgmEnabled ? bgmVolume : 0f;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // SFX 控制
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || !sfxEnabled) return;

            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);

            if (showDebugInfo)
            {
                Debug.Log($"[AudioManager] 播放SFX: {clip.name}");
            }
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip == null || !sfxEnabled) return;

            AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volumeScale);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 调试
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 200, 140, 190, 100));
            GUILayout.Label("=== AudioManager ===");
            GUILayout.Label($"BGM: {(bgmSource.isPlaying ? bgmSource.clip?.name : "None")}");
            GUILayout.Label($"BGM Vol: {bgmVolume:P0} ({(bgmEnabled ? "ON" : "OFF")})");
            GUILayout.Label($"SFX Vol: {sfxVolume:P0} ({(sfxEnabled ? "ON" : "OFF")})");
            GUILayout.EndArea();
        }
#endif
    }
}
