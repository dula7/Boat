using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 音频管理器 - 负责管理游戏中的背景音乐（BGM）和音效（SFX）
/// 使用单例模式，跨场景保持
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("===== 背景音乐资源 =====")]
    [Tooltip("游戏关卡背景音乐（scene1, scene1two, scene2, scene2two）")]
    public AudioClip gameBGM;

    [Tooltip("其他场景背景音乐（UI, Select, Shop等）")]
    public AudioClip groundBGM;

    [Header("===== 音频源设置 =====")]
    [Tooltip("BGM 音频源（自动创建）")]
    private AudioSource bgmSource;

    [Header("===== 音量设置 =====")]
    [Range(0f, 1f)]
    [Tooltip("BGM 音量（0-1）")]
    public float bgmVolume = 1f;

    [Range(0f, 1f)]
    [Tooltip("SFX 音量（0-1）")]
    public float sfxVolume = 1f;

    [Header("===== 游戏关卡场景索引 =====")]
    [Tooltip("游戏关卡场景的索引列表（scene1=2, scene1two=3, scene2=4, scene2two=5）")]
    public int[] gameLevelSceneIndices = { 2, 3, 4, 5 };

    // 当前播放的 BGM 类型
    private BGMType currentBGMType = BGMType.None;

    /// <summary>
    /// BGM 类型枚举
    /// </summary>
    public enum BGMType
    {
        None,   // 无
        Game,   // 游戏关卡音乐
        Ground  // 其他场景音乐
    }

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            // 如果已存在实例，销毁当前对象
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 场景加载完成后，根据场景自动播放对应的 BGM
        CheckAndPlaySceneBGM();
    }

    private void OnEnable()
    {
        // 监听场景加载完成事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 取消监听场景加载完成事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 初始化音频管理器
    /// </summary>
    private void InitializeAudioManager()
    {
        // 创建 BGM 音频源
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = bgmVolume;
        }

        // 从 PlayerPrefs 加载音量设置
        LoadVolumeSettings();

        Debug.Log("AudioManager: 音频管理器初始化完成");
    }

    /// <summary>
    /// 场景加载完成时的回调
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 延迟一帧，确保场景完全加载
        Invoke(nameof(CheckAndPlaySceneBGM), 0.1f);
    }

    /// <summary>
    /// 检查当前场景并播放对应的 BGM（公开方法，供外部调用）
    /// </summary>
    public void CheckAndPlaySceneBGM()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 判断是否为游戏关卡场景
        bool isGameLevel = IsGameLevelScene(currentSceneIndex);

        if (isGameLevel)
        {
            // 游戏关卡场景，播放 game.mp3
            PlayBGM(BGMType.Game);
            Debug.Log($"AudioManager: 检测到游戏关卡场景（索引 {currentSceneIndex}: {currentSceneName}），播放游戏关卡 BGM");
        }
        else
        {
            // 其他场景，播放 ground.mp3
            PlayBGM(BGMType.Ground);
            Debug.Log($"AudioManager: 检测到其他场景（索引 {currentSceneIndex}: {currentSceneName}），播放其他场景 BGM");
        }
    }

    /// <summary>
    /// 判断是否为游戏关卡场景
    /// </summary>
    private bool IsGameLevelScene(int sceneIndex)
    {
        foreach (int levelIndex in gameLevelSceneIndices)
        {
            if (sceneIndex == levelIndex)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 播放指定类型的背景音乐
    /// </summary>
    /// <param name="bgmType">BGM 类型</param>
    public void PlayBGM(BGMType bgmType)
    {
        if (bgmSource == null)
        {
            Debug.LogError("AudioManager: BGM 音频源未初始化！");
            return;
        }

        AudioClip targetClip = null;

        switch (bgmType)
        {
            case BGMType.Game:
                targetClip = gameBGM;
                break;
            case BGMType.Ground:
                targetClip = groundBGM;
                break;
            case BGMType.None:
                StopBGM();
                return;
        }

        // 如果目标音频片段为空，输出警告
        if (targetClip == null)
        {
            Debug.LogWarning($"AudioManager: {bgmType} 类型的 BGM 音频片段未分配！");
            return;
        }

        // 如果已经在播放相同的 BGM，不重复播放
        if (currentBGMType == bgmType && bgmSource.isPlaying && bgmSource.clip == targetClip)
        {
            Debug.Log($"AudioManager: {bgmType} BGM 已在播放，跳过");
            return;
        }

        // 停止当前播放的 BGM
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }

        // 设置新的音频片段并播放
        bgmSource.clip = targetClip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();

        currentBGMType = bgmType;
        Debug.Log($"AudioManager: 开始播放 {bgmType} BGM: {targetClip.name}");
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
            currentBGMType = BGMType.None;
            Debug.Log("AudioManager: 已停止播放 BGM");
        }
    }

    /// <summary>
    /// 暂停播放背景音乐
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
            Debug.Log("AudioManager: 已暂停播放 BGM");
        }
    }

    /// <summary>
    /// 恢复播放背景音乐
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource != null && bgmSource.clip != null)
        {
            bgmSource.UnPause();
            Debug.Log("AudioManager: 已恢复播放 BGM");
        }
    }

    /// <summary>
    /// 设置 BGM 音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }

        // 保存到 PlayerPrefs
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.Save();

        Debug.Log($"AudioManager: BGM 音量已设置为 {bgmVolume}");
    }

    /// <summary>
    /// 获取当前 BGM 音量
    /// </summary>
    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    /// <summary>
    /// 设置 SFX 音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        
        // 保存到 PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();

        Debug.Log($"AudioManager: SFX 音量已设置为 {sfxVolume}");
    }

    /// <summary>
    /// 获取当前 SFX 音量
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    /// <summary>
    /// 从 PlayerPrefs 加载音量设置
    /// </summary>
    private void LoadVolumeSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }

        Debug.Log($"AudioManager: 已加载音量设置 - BGM: {bgmVolume}, SFX: {sfxVolume}");
    }

    /// <summary>
    /// 播放音效（SFX）
    /// </summary>
    /// <param name="clip">音频片段</param>
    /// <param name="volume">音量（可选，默认使用 SFX 音量）</param>
    public void PlaySFX(AudioClip clip, float volume = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: 尝试播放空的音效片段！");
            return;
        }

        // 如果未指定音量，使用默认 SFX 音量
        if (volume < 0f)
        {
            volume = sfxVolume;
        }

        // 使用 PlayOneShot 播放音效（可以同时播放多个音效）
        if (bgmSource != null)
        {
            bgmSource.PlayOneShot(clip, volume);
        }
        else
        {
            // 如果 BGM 源不存在，创建一个临时音频源
            AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);
        }
    }
}

