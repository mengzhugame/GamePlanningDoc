# AudioManager 音频管理器

## 适用场景
任何 Unity 项目都需要管理背景音乐和音效。此模板提供一个简洁健壮的单例音频管理器，适配微信小游戏等 WebGL 环境。

## 核心特性
1. 持久化单例（DontDestroyOnLoad），切换场景不重新初始化
2. 使用 `PlayerPrefs` 持久化保存音乐/音效开关状态
3. 通过 `AudioType` 枚举 + `AudioClipDatabase` 集中管理所有音效资源，解耦播放与资源引用
4. 固定音量设计（移动/小游戏场景不需要复杂的音量滑块）
5. Editor 下实时修改音量参数立即生效（`OnValidate`）

## 配套文件
- `AudioType.cs`：音效类型枚举，所有音效类型在此统一定义
- `AudioClipDatabase.cs`：`ScriptableObject`，在 Inspector 中将 AudioClip 与 AudioType 一一对应

## 使用方法
```csharp
// 播放音效（通过枚举类型）
AudioManager.Instance?.PlaySFX(AudioType.ButtonClick);

// 切换音乐开关
AudioManager.Instance?.SetMusicEnabled(false);

// 查询状态
bool isMusicOn = AudioManager.Instance.IsMusicEnabled;
```

## 代码实现

```csharp
// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundSource;

    [Header("固定音量")]
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.3f;
    [SerializeField][Range(0f, 1f)] private float soundVolume = 0.7f;

    [Header("音效数据库")]
    [SerializeField] private AudioClipDatabase audioDatabase;

    private const string MUSIC_ENABLED_KEY = "MusicEnabled";
    private const string SOUND_ENABLED_KEY = "SoundEnabled";

    private bool isMusicEnabled = true;
    private bool isSoundEnabled = true;

    public bool IsMusicEnabled => isMusicEnabled;
    public bool IsSoundEnabled => isSoundEnabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        isMusicEnabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
        isSoundEnabled = PlayerPrefs.GetInt(SOUND_ENABLED_KEY, 1) == 1;
        ApplyMusicVolume();
        ApplySoundVolume();
    }

    // ===== 音乐控制 =====
    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicVolume();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = isMusicEnabled ? musicVolume : 0f;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()  => musicSource?.Stop();
    public void PauseMusic() => musicSource?.Pause();
    public void ResumeMusic()=> musicSource?.UnPause();

    // ===== 音效控制 =====
    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;
        PlayerPrefs.SetInt(SOUND_ENABLED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplySoundVolume();
    }

    private void ApplySoundVolume()
    {
        if (soundSource != null)
            soundSource.volume = isSoundEnabled ? soundVolume : 0f;
    }

    /// <summary>
    /// 通过 AudioType 枚举播放音效（推荐用法）
    /// </summary>
    public void PlaySFX(AudioType type, float volumeScale = 1.0f)
    {
        if (!isSoundEnabled || audioDatabase == null || soundSource == null) return;
        AudioClip clip = audioDatabase.GetClip(type);
        if (clip != null) soundSource.PlayOneShot(clip, volumeScale);
    }

    /// <summary>
    /// 直接传入 AudioClip 播放（兼容旧代码）
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (!isSoundEnabled || soundSource == null || clip == null) return;
        soundSource.PlayOneShot(clip);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyMusicVolume();
            ApplySoundVolume();
        }
    }
#endif
}
```
