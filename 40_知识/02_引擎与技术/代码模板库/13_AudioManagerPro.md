# AudioManager Pro — 多源 + Config + 自动场景切换（升级版）

> **来源工程**：LightVSDecay（光与朽 · `Audio/AudioManager.cs` + `AudioConfig.cs` + `ButtonSFX.cs`）
> **提取日期**：2026-04-29
> **复用价值**：⭐⭐⭐⭐⭐
> **取代**：[[04_AudioManager]] 简化版（保留作小项目）；本文是中等以上项目的标配
> **依赖**：[[02_SingletonPattern|Singleton 基类]] 或 `AutoPersistentSingleton<T>`、[[11_GameEvents]]

---

## 适用场景

任何**有完整 BGM/SFX 体系**的项目都应该用这个升级版：

- 多 AudioSource（BGM / SFX 一次性 / 循环音效 / Boss 循环 各一）
- ScriptableObject 的 `AudioConfig` 集中管理所有音频资源 + 默认音量
- 自动场景切换 BGM（监听 `SceneManager.sceneLoaded`，按场景名切换）
- BGM 淡入淡出（不受 TimeScale 影响，用 `unscaledDeltaTime`）
- SFX 冷却防抖（同一音效高频触发时只播一次，避免叠加炸裂）
- Editor 实时调音量（`OnValidate`）
- 自动按钮音效（`ButtonSFX` 组件挂在任意 Button 上即可）

简化版（04）适合"3–5 个音效就够了"的小 Demo；本版适合战斗游戏 / 中等 UI 复杂度 / 需要场景化 BGM 的项目。

---

## 三层架构

```
AudioConfig（ScriptableObject）
  ├── BGM：mainMenuBGM / battleBGM / bossBGM
  ├── UI：buttonClick / levelUp / victoryJingle / defeatJingle
  ├── 怪物：enemyDeath / enemyExplode / ... + 各自冷却时间
  ├── 玩家：shieldBreak / shieldHit / lowHealthWarning
  ├── Boss：bossRoar / bossDash / bossSpit / ...
  └── 各分组默认音量（bgmDefaultVolume / sfxDefaultVolume / ...）
        ↓ Inspector 拖入
AudioManager（PersistentSingleton 单例）
  ├── 4 个 AudioSource：bgm / sfx / laserLoop / bossLoop
  ├── 监听 SceneManager.sceneLoaded → 自动切 BGM
  ├── 监听 GameEvents → 播对应音效
  ├── 各音效"上次播放时间"字段 → 冷却防抖
  └── 公共 API：PlayBGM / PlaySFX / PlayButtonClick / PlayEnemyDeath / ...
        ↓
ButtonSFX（挂在任意 Button 上）
  └── OnEnable: button.onClick.AddListener(AudioManager.Instance.PlayButtonClick)
```

---

## 使用方法

### Step 1：创建 AudioConfig 资产

`Project 窗口 → Create → YourGame → Audio Config` → 拖入所有音频片段、调音量。

### Step 2：业务层调用

```csharp
// BGM
AudioManager.Instance.PlayBGM(config.battleBGM);   // 自动淡入淡出

// 一次性音效
AudioManager.Instance.PlaySFX(config.victoryJingle);
AudioManager.Instance.PlayButtonClick();

// 业务封装好的方法（推荐，不绕回 config）
AudioManager.Instance.PlayEnemyDeath();   // 内部带冷却
AudioManager.Instance.PlayShieldBreak();
AudioManager.Instance.PlayBossRoar();

// 循环音效（激光 / Boss 摩擦）
AudioManager.Instance.StartLaserLoop();
AudioManager.Instance.UpdateLaserHitType(LaserHitType.Metal);   // 切音效
AudioManager.Instance.StopLaserLoop();

// 设置面板
AudioManager.Instance.BGMVolume = 0.5f;
AudioManager.Instance.SFXEnabled = false;   // 一键静音 SFX
```

### Step 3：按钮自动音效

任意 Button 上挂 `ButtonSFX` 组件即可——零代码改造。

---

## 代码骨架（不是完整版，是模板版）

完整版 1300+ 行，业务特定方法太多。下面只给模板必须有的核心结构，新项目扩展时照模式加方法。

### 1. AudioConfig.cs（ScriptableObject）

```csharp
using UnityEngine;

namespace YourGame.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "YourGame/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("═══ BGM ═══")]
        public AudioClip mainMenuBGM;
        public AudioClip battleBGM;
        public AudioClip bossBGM;          // 可选
        [Range(0f, 1f)] public float bgmDefaultVolume = 0.5f;
        public float bgmFadeDuration = 1.0f;

        [Header("═══ UI ═══")]
        public AudioClip buttonClick;
        public AudioClip levelUp;
        public AudioClip victoryJingle;
        public AudioClip defeatJingle;
        [Range(0f, 1f)] public float uiDefaultVolume = 0.7f;

        [Header("═══ 怪物 ═══")]
        public AudioClip enemyDeath;
        [Range(0f, 1f)] public float enemyDeathVolume = 0.5f;
        public float enemyDeathCooldown = 0.03f;          // 冷却（防抖关键）

        public AudioClip enemyExplode;
        [Range(0f, 1f)] public float enemyExplodeVolume = 0.4f;
        public float enemyExplodeCooldown = 0.08f;

        // 业务音效继续按这个模式扩展：clip + volume + cooldown 三件套
    }
}
```

### 2. AudioManager.cs（核心结构）

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YourGame.Audio
{
    public class AudioManager : PersistentSingleton<AudioManager>   // ← 02 基类
    {
        [SerializeField] private AudioConfig config;

        [Header("场景名（按 Inspector 配置）")]
        [SerializeField] private string mainMenuSceneName = "MainScene";
        [SerializeField] private string battleSceneName = "GameScene";

        private AudioSource bgmSource, sfxSource, laserSource, bossLoopSource;
        private float bgmVolume = 1f, sfxVolume = 1f;
        private bool bgmEnabled = true, sfxEnabled = true;
        private Coroutine bgmFadeCoroutine;
        private AudioClip currentBGM;

        // 冷却字段（每个高频音效一个）
        private float lastEnemyDeathTime = -1f;
        private float lastEnemyExplodeTime = -1f;

        // ─── 生命周期 ───────────────────────────────────────

        protected override void OnSingletonAwake()
        {
            CreateAudioSources();
            LoadVolumeSettings();
        }

        private void Start() => PlayBGMForCurrentScene();

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SubscribeToGameEvents();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnsubscribeFromGameEvents();
        }

        private void CreateAudioSources()
        {
            // 4 个 AudioSource：BGM / SFX一次性 / 激光循环 / Boss循环
            bgmSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            laserSource = gameObject.AddComponent<AudioSource>();
            bossLoopSource = gameObject.AddComponent<AudioSource>();

            bgmSource.loop = true; bgmSource.playOnAwake = false; bgmSource.priority = 0;
            sfxSource.loop = false; sfxSource.playOnAwake = false; sfxSource.priority = 128;
            laserSource.loop = true; laserSource.playOnAwake = false; laserSource.priority = 64;
            bossLoopSource.loop = true; bossLoopSource.playOnAwake = false; bossLoopSource.priority = 32;
        }

        // ─── BGM 淡入淡出 ───────────────────────────────────

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || (clip == currentBGM && bgmSource.isPlaying)) return;
            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeBGM(clip));
        }

        private IEnumerator FadeBGM(AudioClip newClip)
        {
            float duration = config.bgmFadeDuration;
            float target = bgmVolume * config.bgmDefaultVolume;

            // 淡出旧
            if (bgmSource.isPlaying)
            {
                float startVol = bgmSource.volume;
                float t = 0f;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;   // ★ 不受 TimeScale 影响
                    bgmSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
                    yield return null;
                }
                bgmSource.Stop();
            }

            // 淡入新
            currentBGM = newClip;
            bgmSource.clip = newClip;
            bgmSource.volume = 0f;
            bgmSource.Play();

            float t2 = 0f;
            while (t2 < duration)
            {
                t2 += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, target, t2 / duration);
                yield return null;
            }
        }

        // ─── SFX 冷却防抖 ───────────────────────────────────

        public void PlayEnemyDeath()
        {
            if (config?.enemyDeath == null) return;
            if (Time.unscaledTime - lastEnemyDeathTime < config.enemyDeathCooldown) return;
            lastEnemyDeathTime = Time.unscaledTime;
            PlaySFX(config.enemyDeath, config.enemyDeathVolume);
        }

        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (!sfxEnabled || clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        }

        public void PlayButtonClick()
        {
            if (config != null) PlaySFX(config.buttonClick, config.uiDefaultVolume);
        }

        // ─── 自动场景切换 BGM ──────────────────────────────

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResubscribeToGameEvents();   // GameEvents.ClearAllEvents 后重订
            StopLaserLoop();
            StopBossLoop();

            if (scene.name == mainMenuSceneName) PlayBGM(config.mainMenuBGM);
            else if (scene.name == battleSceneName) PlayBGM(config.battleBGM);
        }

        // ─── 事件订阅（与 GameEvents 协同） ─────────────────

        private void SubscribeToGameEvents()
        {
            GameEvents.OnLevelUp += _ => PlaySFX(config.levelUp, config.uiDefaultVolume);
            GameEvents.OnGameVictory += () => PlaySFX(config.victoryJingle, config.uiDefaultVolume);
            GameEvents.OnGameDefeat += () => PlaySFX(config.defeatJingle, config.uiDefaultVolume);
            GameEvents.OnEnemyDied += (_, _, _, _) => PlayEnemyDeath();
            // ... 业务事件继续添加
        }

        private void UnsubscribeFromGameEvents()
        {
            // 取消订阅，注意：用 lambda 订阅的无法 -=，需要用具名方法。这里仅示意架构
        }

        private void ResubscribeToGameEvents()
        {
            UnsubscribeFromGameEvents();
            SubscribeToGameEvents();
        }

        // ─── 设置面板 API ──────────────────────────────────

        public float BGMVolume
        {
            get => bgmVolume;
            set { bgmVolume = Mathf.Clamp01(value); PlayerPrefs.SetFloat("BGMVolume", bgmVolume); UpdateBGMVolume(); }
        }

        public bool SFXEnabled
        {
            get => sfxEnabled;
            set { sfxEnabled = value; PlayerPrefs.SetInt("SFXEnabled", value ? 1 : 0); }
        }

        private void UpdateBGMVolume()
        {
            if (bgmSource != null && config != null)
                bgmSource.volume = bgmVolume * config.bgmDefaultVolume;
        }

        private void LoadVolumeSettings()
        {
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            bgmEnabled = PlayerPrefs.GetInt("BGMEnabled", 1) == 1;
            sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
            UpdateBGMVolume();
        }

        public void StopLaserLoop() { if (laserSource.isPlaying) laserSource.Stop(); }
        public void StopBossLoop() { if (bossLoopSource.isPlaying) bossLoopSource.Stop(); }

        private void PlayBGMForCurrentScene()
        {
            if (config == null) return;
            string scene = SceneManager.GetActiveScene().name;
            if (scene == mainMenuSceneName) PlayBGM(config.mainMenuBGM);
            else if (scene == battleSceneName) PlayBGM(config.battleBGM);
        }
    }
}
```

### 3. ButtonSFX.cs（按钮自动音效）

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace YourGame.Audio
{
    [RequireComponent(typeof(Button))]
    public class ButtonSFX : MonoBehaviour
    {
        private Button button;

        private void Awake() => button = GetComponent<Button>();

        private void OnEnable()
        {
            if (button != null) button.onClick.AddListener(PlayClickSound);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(PlayClickSound);
        }

        private void PlayClickSound()
        {
            AudioManager.Instance?.PlayButtonClick();
        }
    }
}
```

---

## 关键设计决策（踩坑总结）

1. **4 个 AudioSource 分工**——BGM 占用 `priority=0`（永不被挤掉）/ SFX 一次性 `priority=128` / 循环音效（激光 + Boss）独占 source 避免被 SFX 顶掉。**绝不要**所有音效共享一个 `AudioSource.PlayOneShot`
2. **`Time.unscaledDeltaTime` 用于 BGM 淡入淡出**——游戏暂停时（`Time.timeScale=0`）BGM 不能跟着卡死。SFX 业务则用 `unscaledTime` 计算冷却（同理由）
3. **冷却防抖（`lastXxxTime`）**——同屏怪物大量死亡时，每个都播 `enemyDeath` 会让音效炸耳。30ms 冷却保证一帧最多播一次
4. **`AudioConfig` ScriptableObject 而不是 Inspector 直接配**——策划改音频不需要找程序，直接改资产
5. **场景切换自动 BGM**——监听 `SceneManager.sceneLoaded` + 按场景名 switch；不要让每个场景的代码各自调 `PlayBGM`
6. **`ResubscribeToGameEvents` 必须有**——`GameEvents.ClearAllEvents()` 在场景切换时清掉，AudioManager 是 DontDestroyOnLoad 不重新走 OnEnable，必须手动重订
7. **`BGMEnabled` / `SFXEnabled` 走 `PlayerPrefs`**——玩家在设置里关掉的偏好要持久化，下次启动还有效
8. **不要 `RuntimeInitializeOnLoadMethod` 自动创建（除非用 `AutoPersistentSingleton`）**——简单 `PersistentSingleton` 应该在 Bootstrap 场景预放 GameObject；自动创建会让 Inspector 配置（`AudioConfig` 引用）无法保留
9. **ButtonSFX 用具名方法**——OnEnable/OnDisable 成对，不能用 lambda（lambda 无法 -=）
10. **业务方法封装（`PlayEnemyDeath`、`PlayBossRoar`）**——把"播什么 clip + 什么音量 + 什么冷却"封装在 AudioManager 内，业务代码只需 `PlayEnemyDeath()`，未来换音效不用改一堆地方

---

## 与简化版（04）的差异

| 特性 | 04 简化版 | 13 Pro 版 |
|------|----------|----------|
| 配置 | Inspector 直接拖 | `AudioConfig` ScriptableObject |
| 音频源 | 1–2 个 | 4 个（BGM / SFX / 激光循环 / Boss 循环） |
| 场景切换 BGM | 业务手动调 | 自动监听 `SceneManager.sceneLoaded` |
| 淡入淡出 | 无 | 有（`unscaledDeltaTime`）|
| 冷却防抖 | 无 | 每个高频音效一个冷却字段 |
| 按钮音效 | 业务手动 | `ButtonSFX` 组件挂上即可 |
| 设置持久化 | 无 | BGM/SFX 音量 + 开关都走 `PlayerPrefs` |
| 适用项目规模 | 小 Demo | 中等以上完整项目 |

---

## 与其他模板的关系

| 模板 | 用法 |
|------|------|
| [[02_SingletonPattern]] | `AudioManager` 继承 `PersistentSingleton<T>` |
| [[11_GameEvents]] | 监听 `OnEnemyDied / OnGameVictory / OnLevelUp` 等触发音效 |
| [[09_CoinFlyAnimation]] | 金币到达时调 `PlayCoinCollect()`，带冷却 |
| [[10_FloatingTextSystem]] | 暴击 / 处决飘字配合特殊音效更爽 |

---

## 注意事项

- **WebGL/微信小游戏**：`AudioSource.PlayOneShot` 在 WebGL 上有时延迟感比真机大；需要瞬时反馈的 SFX（如按钮点击）可以预加载（`AudioSource.clip = clip; Play()`）而不是 PlayOneShot
- **AudioConfig 资产路径**：建议放 `Assets/Resources/Audio/AudioConfig.asset`，便于 `Resources.Load`
- **不要把 `AudioManager.Instance.PlayXxx` 散在 Update**——用事件驱动；`Update` 里高频调音效是性能杀手
- **MOSUS（微信小游戏不支持的 API）**：`AudioListener.pause` 在某些微信 SDK 版本下行为不一致，慎用；推荐自己维护暂停状态
