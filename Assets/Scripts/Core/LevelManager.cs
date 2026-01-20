using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡管理器（单例模式）
/// 判定关卡通关及三星评价逻辑
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    [Tooltip("当前关卡总钻石数量（每关固定5个）")]
    public int totalDiamonds = 5;
    
    [Tooltip("2星评价所需的最低分数（默认130分）")]
    public int twoStarScoreThreshold = 130;

    [Header("Debug")]
    [Tooltip("是否启用调试日志")]
    public bool enableDebugLog = false;

    private int collectedDiamonds = 0;  // 已收集的钻石数量
    private bool hasReachedFinish = false;  // 是否已到达终点
    private int currentStars = 0;  // 当前星级

    void Awake()
    {
        // 单例模式：确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 跨场景保持
            ResetLevel();
            
            // 监听场景加载事件，在场景切换时重置关卡数据
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // 如果已存在实例，销毁当前对象
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // 取消监听场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 场景加载完成时的回调
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 每次场景加载时重置关卡数据
        ResetLevel();
        
        // 同时重置 ScoreManager 的关卡分数
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
    }

    /// <summary>
    /// 重置关卡数据（用于重新开始关卡）
    /// </summary>
    public void ResetLevel()
    {
        collectedDiamonds = 0;
        hasReachedFinish = false;
        currentStars = 0;
        
        if (enableDebugLog)
        {
            Debug.Log("LevelManager: 重置关卡数据");
        }
    }

    /// <summary>
    /// 收集钻石
    /// </summary>
    public void CollectDiamond()
    {
        if (collectedDiamonds < totalDiamonds)
        {
            collectedDiamonds++;
            
            if (enableDebugLog)
            {
                Debug.Log($"LevelManager: 收集钻石 {collectedDiamonds}/{totalDiamonds}");
            }
        }
    }

    /// <summary>
    /// 获取已收集的钻石数量
    /// </summary>
    /// <returns>已收集的钻石数量</returns>
    public int GetCollectedDiamonds()
    {
        return collectedDiamonds;
    }

    /// <summary>
    /// 设置已到达终点
    /// </summary>
    public void SetReachedFinish()
    {
        if (!hasReachedFinish)
        {
            hasReachedFinish = true;
            CalculateStars();
            
            if (enableDebugLog)
            {
                Debug.Log($"LevelManager: 已到达终点，当前星级: {currentStars}");
            }
        }
    }

    /// <summary>
    /// 判定通关状态
    /// </summary>
    /// <returns>是否通关</returns>
    public bool IsLevelCompleted()
    {
        return hasReachedFinish;
    }

    /// <summary>
    /// 计算星级评价
    /// </summary>
    /// <returns>星级（1-3星）</returns>
    public int CalculateStars()
    {
        if (!hasReachedFinish)
        {
            currentStars = 0;
            return 0;
        }

        // 1星：玩家抵达终点即可获得
        currentStars = 1;

        // 2星：当前总分 ≥ 130分
        if (ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.GetCurrentScore();
            if (currentScore >= twoStarScoreThreshold)
            {
                currentStars = 2;
            }
        }

        // 3星：收集完当前关卡全部5个钻石
        if (collectedDiamonds >= totalDiamonds)
        {
            currentStars = 3;
        }

        if (enableDebugLog)
        {
            Debug.Log($"LevelManager: 计算星级 - 收集钻石: {collectedDiamonds}/{totalDiamonds}, 得分: {(ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0)}, 星级: {currentStars}");
        }

        return currentStars;
    }

    /// <summary>
    /// 返回当前星级结果
    /// </summary>
    /// <returns>当前星级（1-3星）</returns>
    public int GetCurrentStars()
    {
        return currentStars;
    }

    /// <summary>
    /// 获取关卡总钻石数量
    /// </summary>
    /// <returns>总钻石数量</returns>
    public int GetTotalDiamonds()
    {
        return totalDiamonds;
    }
}

