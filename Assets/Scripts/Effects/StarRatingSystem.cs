// StarRatingSystem.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StarRatingSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite starFilled;
    [SerializeField] private Sprite starEmpty;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private TextMeshProUGUI diamondText;  // 钻石数显示（可选）
    [SerializeField] private TextMeshProUGUI pointsText;  // 本关获得的积分显示（可选）
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;

    [Header("Animation Settings")]
    [SerializeField] private float starDelay = 0.5f;
    [SerializeField] private float starFillDuration = 0.3f;
    [SerializeField] private float scaleEffect = 1.2f;

    [Header("Rating Thresholds")]
    [SerializeField] private int oneStarThreshold = 1000;
    [SerializeField] private int twoStarThreshold = 2000;
    [SerializeField] private int threeStarThreshold = 3000;

    private int currentScore = 0;
    private int starCount = 0;

    void Start()
    {
        // 初始化关卡完成面板
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        // 重置星星
        ResetStars();

        // 绑定按钮事件
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    /// <summary>
    /// 显示通关结果
    /// </summary>
    public void ShowLevelComplete(int score)
    {
        currentScore = score;
        
        // 使用 LevelManager 的星级计算（更准确）
        if (LevelManager.Instance != null)
        {
            starCount = LevelManager.Instance.GetCurrentStars();
        }
        else
        {
            // 如果没有 LevelManager，使用分数阈值计算
            starCount = CalculateStarRating(score);
        }

        // 显示面板
        if (levelCompletePanel != null)
        {
            // 确保当前 GameObject 及其所有父对象都是激活的
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.LogWarning($"StarRatingSystem: GameObject '{gameObject.name}' 未激活，已自动激活");
            }
            
            // 激活所有父对象
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                    Debug.LogWarning($"StarRatingSystem: 父对象 '{parent.name}' 未激活，已自动激活");
                }
                parent = parent.parent;
            }

            levelCompletePanel.SetActive(true);
            
            // 使用协程延迟启动，确保 GameObject 完全激活
            StartCoroutine(DelayedStartAnimation());
        }
        else
        {
            Debug.LogError("StarRatingSystem: levelCompletePanel 为空！请检查配置。");
        }

        // 更新分数文本
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        // 更新评级文本
        if (ratingText != null)
            ratingText.text = GetRatingText(starCount);

        // 更新钻石数显示
        UpdateDiamondDisplay();

        // 更新本关获得的积分显示
        UpdatePointsDisplay();

        // 暂停游戏（停止时间）
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 更新钻石数显示
    /// </summary>
    private void UpdateDiamondDisplay()
    {
        if (diamondText == null) return;

        if (LevelManager.Instance != null)
        {
            int collected = LevelManager.Instance.GetCollectedDiamonds();
            int total = LevelManager.Instance.GetTotalDiamonds();
            diamondText.text = $"Diamonds: {collected}/{total}";
        }
        else
        {
            diamondText.text = "Diamonds: --";
        }
    }

    /// <summary>
    /// 更新本关获得的积分显示
    /// </summary>
    private void UpdatePointsDisplay()
    {
        if (pointsText == null) return;

        if (LevelManager.Instance != null && ScoreManager.Instance != null)
        {
            // 计算本关获得的积分：
            // 1. 钻石收集：每个钻石+10积分
            // 2. 击碎障碍物：每个+2积分
            // 3. 通关奖励：+100积分
            int diamonds = LevelManager.Instance.GetCollectedDiamonds();
            int currentScore = ScoreManager.Instance.GetCurrentScore();
            int baseScore = 100;  // 基础通关分数
            
            // 计算击碎的障碍物数量：(当前分数 - 基础分数 - 钻石分数) / 每个障碍物分数
            int diamondScore = diamonds * 10;
            int obstacleScore = currentScore - baseScore - diamondScore;
            int brokenObstacles = obstacleScore > 0 ? obstacleScore / 2 : 0;  // 每个障碍物+2分
            
            // 计算总积分：钻石积分 + 障碍物积分 + 通关奖励
            int levelEarnedPoints = diamondScore + (brokenObstacles * 2) + 100;

            pointsText.text = $"Points Earned: +{levelEarnedPoints}";
        }
        else
        {
            pointsText.text = "Points Earned: --";
        }
    }

    /// <summary>
    /// 计算星级（备用方法，如果 LevelManager 不存在时使用）
    /// </summary>
    private int CalculateStarRating(int score)
    {
        if (score >= threeStarThreshold)
            return 3;
        else if (score >= twoStarThreshold)
            return 2;
        else if (score >= oneStarThreshold)
            return 1;
        else
            return 0; // 如果没有达到1星阈值，返回0星（这种情况不应该发生，因为到达终点就有1星）
    }

    /// <summary>
    /// 获取评级文本
    /// </summary>
    private string GetRatingText(int stars)
    {
        switch (stars)
        {
            case 3: return "Excellent!";
            case 2: return "Great Job!";
            case 1: return "Good!";
            default: return "Try Again!";
        }
    }

    /// <summary>
    /// 星星动画协程
    /// </summary>
    private IEnumerator AnimateStarsRoutine()
    {
        // 确保所有星星初始为空
        ResetStars();

        // 逐个填充星星
        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < starCount)
            {
                yield return StartCoroutine(FillStarAnimation(starImages[i], i));
            }
            yield return new WaitForSecondsRealtime(starDelay);
        }
    }

    /// <summary>
    /// 填充星星动画
    /// </summary>
    private IEnumerator FillStarAnimation(Image starImage, int index)
    {
        float elapsedTime = 0f;
        Vector3 originalScale = starImage.transform.localScale;

        // 播放音效（如果需要）
        // AudioManager.Instance.PlaySFX("StarFill");

        while (elapsedTime < starFillDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / starFillDuration;

            // 缩放效果
            float scale = Mathf.Lerp(1f, scaleEffect, t);
            starImage.transform.localScale = originalScale * scale;

            // 透明度过渡（如果需要）
            // starImage.color = Color.Lerp(Color.clear, Color.white, t);

            yield return null;
        }

        // 设置填充星星
        starImage.sprite = starFilled;
        starImage.transform.localScale = originalScale;

        // 弹回效果
        yield return StartCoroutine(BounceEffect(starImage.transform));
    }

    /// <summary>
    /// 弹回效果
    /// </summary>
    private IEnumerator BounceEffect(Transform starTransform)
    {
        Vector3 originalScale = starTransform.localScale;
        float duration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;
            float scale = Mathf.Lerp(scaleEffect, 1f, t);
            starTransform.localScale = originalScale * scale;
            yield return null;
        }

        starTransform.localScale = originalScale;
    }

    /// <summary>
    /// 重置所有星星为空
    /// </summary>
    private void ResetStars()
    {
        foreach (Image star in starImages)
        {
            if (star != null && starEmpty != null)
                star.sprite = starEmpty;
        }
    }

    /// <summary>
    /// 继续按钮点击
    /// </summary>
    private void OnContinueClicked()
    {
        Time.timeScale = 1f;
        // 加载下一关或返回菜单
        // SceneManager.LoadScene("NextLevel");
        Debug.Log("Continue to next level");

        // 关闭面板
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }

    /// <summary>
    /// 重新开始按钮点击
    /// </summary>
    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        // 重新加载当前关卡
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Restart level");
    }

    /// <summary>
    /// 延迟启动动画（确保 GameObject 完全激活）
    /// </summary>
    private System.Collections.IEnumerator DelayedStartAnimation()
    {
        // 等待一帧，确保 GameObject 完全激活
        yield return null;
        
        // 再次检查是否激活
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(AnimateStarsRoutine());
        }
        else
        {
            Debug.LogError($"StarRatingSystem: 延迟后仍然无法启动协程，GameObject '{gameObject.name}' 未激活！");
            Debug.LogError("请检查：1. WinCanvas 是否在场景中 2. WinCanvas 及其父对象是否激活 3. StarRatingSystem 是否正确挂载");
        }
    }

    /// <summary>
    /// 提供给其他脚本调用的方法
    /// </summary>
    public void EndLevelWithScore(int score)
    {
        ShowLevelComplete(score);
    }

    void OnDestroy()
    {
        // 清理事件监听
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);
    }
}
