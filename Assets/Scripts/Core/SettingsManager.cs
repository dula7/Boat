using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 设置管理器 - 负责管理游戏的所有设置选项
/// 包括音频设置、游戏参数设置和用户数据管理
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("===== UI 引用 =====")]
    [Tooltip("设置面板（整个设置界面）")]
    public GameObject settingsPanel;

    [Tooltip("关闭设置按钮")]
    public Button closeButton;

    [Header("===== UI 隐藏设置 =====")]
    [Tooltip("需要隐藏的UI元素列表（打开设置时隐藏，关闭设置时恢复）")]
    public List<GameObject> uiElementsToHide = new List<GameObject>();

    [Tooltip("是否自动隐藏Canvas下除设置面板外的所有Panel（如果列表为空则使用此选项）")]
    public bool autoHideOtherPanels = true;

    [Tooltip("自动隐藏时排除的GameObject名称（不会被隐藏）")]
    public List<string> excludeFromAutoHide = new List<string>();

    // 保存UI元素的状态（用于恢复）
    private Dictionary<GameObject, bool> uiElementStates = new Dictionary<GameObject, bool>();

    [Header("===== 音频设置 UI =====")]
    [Tooltip("BGM 音量滑块")]
    public Slider bgmVolumeSlider;

    [Tooltip("SFX 音量滑块")]
    public Slider sfxVolumeSlider;

    [Tooltip("BGM 音量显示文本（可选）")]
    public TextMeshProUGUI bgmVolumeText;

    [Tooltip("SFX 音量显示文本（可选）")]
    public TextMeshProUGUI sfxVolumeText;

    [Header("===== 游戏参数设置 UI =====")]
    [Tooltip("小船移动速度滑块")]
    public Slider moveSpeedSlider;

    [Tooltip("小船转向速度滑块")]
    public Slider turnSpeedSlider;

    [Tooltip("移动速度显示文本（可选）")]
    public TextMeshProUGUI moveSpeedText;

    [Tooltip("转向速度显示文本（可选）")]
    public TextMeshProUGUI turnSpeedText;

    [Header("===== 用户数据管理 UI =====")]
    [Tooltip("清空所有数据按钮")]
    public Button clearAllDataButton;

    [Tooltip("只清空积分按钮")]
    public Button clearPointsButton;

    [Header("===== 默认值设置 =====")]
    [Tooltip("默认移动速度")]
    [Range(10f, 50f)]
    public float defaultMoveSpeed = 25f;

    [Tooltip("默认转向速度")]
    [Range(1f, 10f)]
    public float defaultTurnSpeed = 3.5f;

    [Tooltip("移动速度范围")]
    public Vector2 moveSpeedRange = new Vector2(10f, 50f);

    [Tooltip("转向速度范围")]
    public Vector2 turnSpeedRange = new Vector2(1f, 10f);

    // 当前设置值
    private float currentMoveSpeed;
    private float currentTurnSpeed;

    // PlayerPrefs 键名
    private const string MOVE_SPEED_KEY = "BoatMoveSpeed";
    private const string TURN_SPEED_KEY = "BoatTurnSpeed";

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 初始化设置
        InitializeSettings();

        // 绑定UI事件
        SetupUIEvents();

        // 默认隐藏设置面板
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 初始化设置（从PlayerPrefs加载）
    /// </summary>
    private void InitializeSettings()
    {
        // 加载小船速度设置
        currentMoveSpeed = PlayerPrefs.GetFloat(MOVE_SPEED_KEY, defaultMoveSpeed);
        currentTurnSpeed = PlayerPrefs.GetFloat(TURN_SPEED_KEY, defaultTurnSpeed);

        // 更新UI滑块
        if (moveSpeedSlider != null)
        {
            moveSpeedSlider.minValue = moveSpeedRange.x;
            moveSpeedSlider.maxValue = moveSpeedRange.y;
            moveSpeedSlider.value = currentMoveSpeed;
        }

        if (turnSpeedSlider != null)
        {
            turnSpeedSlider.minValue = turnSpeedRange.x;
            turnSpeedSlider.maxValue = turnSpeedRange.y;
            turnSpeedSlider.value = currentTurnSpeed;
        }

        // 加载音频设置（从AudioManager）
        if (AudioManager.Instance != null)
        {
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = AudioManager.Instance.GetBGMVolume();
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            }
        }

        // 更新显示文本
        UpdateDisplayTexts();

        Debug.Log($"SettingsManager: 设置已初始化 - 移动速度: {currentMoveSpeed}, 转向速度: {currentTurnSpeed}");
    }

    /// <summary>
    /// 设置UI事件绑定
    /// </summary>
    private void SetupUIEvents()
    {
        // 关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseSettings);
        }

        // BGM 音量滑块
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.RemoveAllListeners();
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        // SFX 音量滑块
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // 移动速度滑块
        if (moveSpeedSlider != null)
        {
            moveSpeedSlider.onValueChanged.RemoveAllListeners();
            moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);
        }

        // 转向速度滑块
        if (turnSpeedSlider != null)
        {
            turnSpeedSlider.onValueChanged.RemoveAllListeners();
            turnSpeedSlider.onValueChanged.AddListener(OnTurnSpeedChanged);
        }

        // 清空所有数据按钮
        if (clearAllDataButton != null)
        {
            clearAllDataButton.onClick.RemoveAllListeners();
            clearAllDataButton.onClick.AddListener(OnClearAllData);
        }

        // 只清空积分按钮
        if (clearPointsButton != null)
        {
            clearPointsButton.onClick.RemoveAllListeners();
            clearPointsButton.onClick.AddListener(OnClearPoints);
        }
    }

    /// <summary>
    /// 打开设置界面
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            // 保存并隐藏其他UI元素
            HideOtherUIElements();

            // 显示设置面板
            settingsPanel.SetActive(true);
            Debug.Log("SettingsManager: 设置界面已打开");
        }
        else
        {
            Debug.LogWarning("SettingsManager: 设置面板未分配！");
        }
    }

    /// <summary>
    /// 关闭设置界面
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            
            // 恢复其他UI元素的状态
            RestoreOtherUIElements();
            
            Debug.Log("SettingsManager: 设置界面已关闭");
        }
    }

    /// <summary>
    /// 隐藏其他UI元素
    /// </summary>
    private void HideOtherUIElements()
    {
        uiElementStates.Clear();

        // 如果指定了需要隐藏的UI元素列表，使用列表
        if (uiElementsToHide != null && uiElementsToHide.Count > 0)
        {
            foreach (GameObject uiElement in uiElementsToHide)
            {
                if (uiElement != null && uiElement != settingsPanel)
                {
                    // 保存当前状态
                    uiElementStates[uiElement] = uiElement.activeSelf;
                    // 隐藏元素
                    uiElement.SetActive(false);
                }
            }
        }
        // 否则自动查找Canvas下的其他Panel
        else if (autoHideOtherPanels)
        {
            // 查找Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // 查找Canvas下的所有Panel
                RectTransform[] allRectTransforms = canvas.GetComponentsInChildren<RectTransform>(true);
                foreach (RectTransform rectTransform in allRectTransforms)
                {
                    GameObject obj = rectTransform.gameObject;
                    
                    // 跳过设置面板本身
                    if (obj == settingsPanel)
                        continue;

                    // 跳过排除列表中的对象
                    if (excludeFromAutoHide != null && excludeFromAutoHide.Contains(obj.name))
                        continue;

                    // 只处理Panel（有Image或Panel组件的对象，或者是Canvas的直接子对象）
                    bool isPanel = obj.GetComponent<Image>() != null || 
                                   obj.name.Contains("Panel") || 
                                   obj.transform.parent == canvas.transform;

                    if (isPanel && obj.activeSelf)
                    {
                        // 保存当前状态
                        uiElementStates[obj] = true;
                        // 隐藏元素
                        obj.SetActive(false);
                        Debug.Log($"SettingsManager: 已隐藏UI元素: {obj.name}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 恢复其他UI元素的状态
    /// </summary>
    private void RestoreOtherUIElements()
    {
        foreach (var kvp in uiElementStates)
        {
            if (kvp.Key != null)
            {
                kvp.Key.SetActive(kvp.Value);
            }
        }
        uiElementStates.Clear();
    }

    #region 音频设置

    /// <summary>
    /// BGM 音量改变回调
    /// </summary>
    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }

        if (bgmVolumeText != null)
        {
            bgmVolumeText.text = $"{(int)(value * 100)}%";
        }
    }

    /// <summary>
    /// SFX 音量改变回调
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"{(int)(value * 100)}%";
        }
    }

    #endregion

    #region 游戏参数设置

    /// <summary>
    /// 移动速度改变回调
    /// </summary>
    private void OnMoveSpeedChanged(float value)
    {
        currentMoveSpeed = value;
        PlayerPrefs.SetFloat(MOVE_SPEED_KEY, currentMoveSpeed);
        PlayerPrefs.Save();

        if (moveSpeedText != null)
        {
            moveSpeedText.text = $"{currentMoveSpeed:F1}";
        }

        // 如果当前场景中有船体，立即应用设置
        ApplyBoatSettings();

        Debug.Log($"SettingsManager: 移动速度已设置为 {currentMoveSpeed}");
    }

    /// <summary>
    /// 转向速度改变回调
    /// </summary>
    private void OnTurnSpeedChanged(float value)
    {
        currentTurnSpeed = value;
        PlayerPrefs.SetFloat(TURN_SPEED_KEY, currentTurnSpeed);
        PlayerPrefs.Save();

        if (turnSpeedText != null)
        {
            turnSpeedText.text = $"{currentTurnSpeed:F1}";
        }

        // 如果当前场景中有船体，立即应用设置
        ApplyBoatSettings();

        Debug.Log($"SettingsManager: 转向速度已设置为 {currentTurnSpeed}");
    }

    /// <summary>
    /// 应用小船设置到当前场景的船体（如果存在）
    /// </summary>
    public void ApplyBoatSettings()
    {
        BoatController boat = FindObjectOfType<BoatController>();
        if (boat != null)
        {
            boat.moveSpeed = currentMoveSpeed;
            boat.turnSpeed = currentTurnSpeed;
            Debug.Log($"SettingsManager: 已应用设置到船体 - 移动速度: {currentMoveSpeed}, 转向速度: {currentTurnSpeed}");
        }
    }

    /// <summary>
    /// 获取当前移动速度设置
    /// </summary>
    public float GetMoveSpeed()
    {
        return currentMoveSpeed;
    }

    /// <summary>
    /// 获取当前转向速度设置
    /// </summary>
    public float GetTurnSpeed()
    {
        return currentTurnSpeed;
    }

    #endregion

    #region 用户数据管理

    /// <summary>
    /// 清空所有数据按钮回调
    /// </summary>
    private void OnClearAllData()
    {
        // 直接执行（实际项目中应该添加确认对话框）
        ClearPlayer clearPlayer = FindObjectOfType<ClearPlayer>();
        if (clearPlayer != null)
        {
            clearPlayer.ClearAllData();
            Debug.Log("SettingsManager: 已清空所有用户数据");
        }
        else
        {
            // 如果没有ClearPlayer组件，直接调用PlayerPrefs
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("SettingsManager: 已清空所有PlayerPrefs数据");
        }

        // 重置设置到默认值
        InitializeSettings();
    }

    /// <summary>
    /// 只清空积分按钮回调
    /// </summary>
    private void OnClearPoints()
    {
        ClearPlayer clearPlayer = FindObjectOfType<ClearPlayer>();
        if (clearPlayer != null)
        {
            clearPlayer.ClearPoints();
            Debug.Log("SettingsManager: 已清空积分");
        }
        else
        {
            // 如果没有ClearPlayer组件，直接调用PlayerPrefs
            PlayerPrefs.DeleteKey("PlayerPoints");
            PlayerPrefs.Save();
            Debug.Log("SettingsManager: 已清空积分（PlayerPrefs）");
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 更新所有显示文本
    /// </summary>
    private void UpdateDisplayTexts()
    {
        if (bgmVolumeText != null && bgmVolumeSlider != null)
        {
            bgmVolumeText.text = $"{(int)(bgmVolumeSlider.value * 100)}%";
        }

        if (sfxVolumeText != null && sfxVolumeSlider != null)
        {
            sfxVolumeText.text = $"{(int)(sfxVolumeSlider.value * 100)}%";
        }

        if (moveSpeedText != null)
        {
            moveSpeedText.text = $"{currentMoveSpeed:F1}";
        }

        if (turnSpeedText != null)
        {
            turnSpeedText.text = $"{currentTurnSpeed:F1}";
        }
    }

    #endregion
}

