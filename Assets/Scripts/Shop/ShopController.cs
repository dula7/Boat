using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopController : MonoBehaviour
{
    [Header("分组容器 (手动拖进去)")]
    public GameObject shipGroup;    // 放所有船的那个父物体
    public GameObject missileGroup; // 放所有导弹的那个父物体

    [Header("左侧页签按钮")]
    public Button shipTabBtn;
    public Button missileTabBtn;

    [Header("积分显示")]
    [Tooltip("显示积分余额的Text组件（在商店界面顶部，手动拖入）")]
    public TextMeshProUGUI pointsText;  // 积分显示文本

    // 当商店界面 被激活(打开) 时
    void OnEnable()
    {
        // 解锁鼠标，并显示出来
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 使用协程延迟更新，确保ScoreManager已经初始化
        StartCoroutine(DelayedUpdatePoints());
    }

    /// <summary>
    /// 延迟更新积分显示（确保ScoreManager已经初始化）
    /// </summary>
    private System.Collections.IEnumerator DelayedUpdatePoints()
    {
        // 等待一帧，确保所有Awake和Start都执行完毕
        yield return null;
        
        // 如果ScoreManager还没初始化，再等待一帧
        int maxWaitFrames = 5;  // 最多等待5帧
        int waitCount = 0;
        while (ScoreManager.Instance == null && waitCount < maxWaitFrames)
        {
            yield return null;
            waitCount++;
        }
        
        // 更新积分显示
        UpdatePointsDisplay();
        
        // 确保订阅事件（先取消之前的订阅，避免重复）
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPointsChanged -= UpdatePointsDisplay;  // 先取消，避免重复
            ScoreManager.Instance.OnPointsChanged += UpdatePointsDisplay;
        }
    }

    // 当商店界面 被隐藏(关闭) 时
    void OnDisable()
    {
        // 注意：不在OnDisable中锁定鼠标，因为：
        // 1. 商店场景本身就是UI场景，需要显示鼠标
        // 2. 场景切换时OnDisable也会被调用，不应该锁定鼠标
        // 如果需要在其他场景中锁定鼠标，应该在那个场景的脚本中处理

        // 取消订阅积分变化事件
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPointsChanged -= UpdatePointsDisplay;
        }
    }

    void Start()
    {
        shipTabBtn.onClick.AddListener(ShowShips);
        missileTabBtn.onClick.AddListener(ShowMissiles);

        // 默认显示船
        ShowShips();

        // 如果商店界面一开始就是激活的，在Start中也更新一次积分显示
        if (gameObject.activeInHierarchy)
        {
            UpdatePointsDisplay();
            
            // 订阅积分变化事件
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnPointsChanged += UpdatePointsDisplay;
            }
        }
    }

    void ShowShips()
    {
        shipGroup.SetActive(true);
        missileGroup.SetActive(false);
    }

    void ShowMissiles()
    {
        shipGroup.SetActive(false);
        missileGroup.SetActive(true);
    }

    /// <summary>
    /// 更新积分显示
    /// </summary>
    void UpdatePointsDisplay()
    {
        if (pointsText != null && ScoreManager.Instance != null)
        {
            int currentPoints = ScoreManager.Instance.currentPoints;
            pointsText.text = $"积分: {currentPoints}";
        }
        else if (pointsText != null)
        {
            pointsText.text = "积分: 0";
        }
    }

    /// <summary>
    /// 更新积分显示（带参数版本，用于事件回调）
    /// </summary>
    void UpdatePointsDisplay(int points)
    {
        if (pointsText != null)
        {
            pointsText.text = $"积分: {points}";
        }
    }
}