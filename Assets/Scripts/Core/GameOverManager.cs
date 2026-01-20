using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 游戏结束管理器
/// 处理游戏结束逻辑和UI显示
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("UI References (方式1: 直接引用场景中的UI)")]
    public GameObject gameOverPanel;  // 游戏结束面板
    public UnityEngine.UI.Text gameOverText;  // 游戏结束文本（旧版UI.Text）
    public TextMeshProUGUI gameOverTextTMP;  // 游戏结束文本（TextMeshPro）
    public UnityEngine.UI.Button restartButton;  // 重新开始按钮
    public UnityEngine.UI.Button quitButton;  // 退出按钮

    [Header("UI References (方式2: 使用Prefab动态实例化)")]
    [Tooltip("游戏结束UI预制体（Canvasgameover.prefab），如果设置了此字段，会自动实例化并查找组件")]
    public GameObject gameOverPrefab;  // 游戏结束UI预制体

    [Header("Settings")]
    public string gameOverMessage = "游戏结束";
    public string collisionMessage = "撞到了障碍物！";
    public string victoryMessage = "恭喜！到达终点！";
    public float fadeInSpeed = 2f;  // 淡入速度

    private bool isGameOver = false;
    private bool isVictory = false;
    private CanvasGroup canvasGroup;
    private GameObject instantiatedPrefab;  // 实例化的prefab对象

    void Start()
    {
        // 如果设置了prefab，优先使用prefab动态实例化
        if (gameOverPrefab != null && gameOverPanel == null)
        {
            InstantiateGameOverPrefab();
        }

        // 初始化UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
        }

        // 设置按钮事件
        SetupButtons();
    }

    /// <summary>
    /// 实例化游戏结束UI预制体
    /// </summary>
    private void InstantiateGameOverPrefab()
    {
        if (gameOverPrefab == null) return;

        // 实例化prefab
        instantiatedPrefab = Instantiate(gameOverPrefab);
        instantiatedPrefab.name = "GameOverCanvas(Instance)";

        // 查找Canvas下的GameOver面板
        Transform gameOverTransform = instantiatedPrefab.transform.Find("GameOver");
        if (gameOverTransform != null)
        {
            gameOverPanel = gameOverTransform.gameObject;
        }
        else
        {
            // 如果找不到GameOver，尝试查找第一个Panel
            Canvas canvas = instantiatedPrefab.GetComponent<Canvas>();
            if (canvas != null)
            {
                // 查找Canvas下的第一个Panel
                RectTransform[] children = canvas.GetComponentsInChildren<RectTransform>();
                foreach (var child in children)
                {
                    if (child.name.Contains("GameOver") || child.name.Contains("Panel"))
                    {
                        gameOverPanel = child.gameObject;
                        break;
                    }
                }
            }
        }

        // 查找文本组件（TextMeshPro）
        if (gameOverPanel != null)
        {
            gameOverTextTMP = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (gameOverTextTMP == null)
            {
                // 如果找不到TextMeshPro，尝试查找旧的UI.Text
                gameOverText = gameOverPanel.GetComponentInChildren<UnityEngine.UI.Text>();
            }
        }

        // 查找按钮
        if (gameOverPanel != null)
        {
            UnityEngine.UI.Button[] buttons = gameOverPanel.GetComponentsInChildren<UnityEngine.UI.Button>();
            foreach (var button in buttons)
            {
                if (button.name.ToLower().Contains("restart") || button.name.ToLower().Contains("重新"))
                {
                    restartButton = button;
                }
                else if (button.name.ToLower().Contains("back") || button.name.ToLower().Contains("quit") || 
                         button.name.ToLower().Contains("退出") || button.name.ToLower().Contains("返回"))
                {
                    quitButton = button;
                }
            }
        }

        Debug.Log($"GameOverManager: 已实例化游戏结束UI预制体。Panel: {gameOverPanel?.name}, Text: {gameOverTextTMP?.name ?? gameOverText?.name}, Restart: {restartButton?.name}, Quit: {quitButton?.name}");
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();  // 清除prefab中可能已有的监听器
            restartButton.onClick.AddListener(RestartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();  // 清除prefab中可能已有的监听器
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    /// <summary>
    /// 显示游戏结束界面
    /// </summary>
    /// <param name="reason">游戏结束原因</param>
    public void ShowGameOver(string reason = "")
    {
        if (isGameOver) return;

        isGameOver = true;

        // 死亡惩罚：清空本关获得的所有积分（钻石积分 + 障碍物积分）
        // 注意：只清空本关获得的积分，不影响之前关卡获得的积分
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetLevelPoints();
        }

        // 显示UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // 设置文本（优先使用TextMeshPro）
            string message = gameOverMessage;
            if (!string.IsNullOrEmpty(reason))
            {
                message += "\n" + reason;
            }
            message += "\n死亡惩罚：本关积分清零";

            if (gameOverTextTMP != null)
            {
                gameOverTextTMP.text = message;
            }
            else if (gameOverText != null)
            {
                gameOverText.text = message;
            }

            // 淡入效果（使用协程，不受Time.timeScale影响）
            StartCoroutine(FadeInPanel());
        }

        // 禁用玩家控制
        DisablePlayerControls();

        // 停止时间（在淡入完成后）
        StartCoroutine(StopTimeAfterFade());

        Debug.Log("Game Over: " + reason);
    }

    /// <summary>
    /// 淡入面板
    /// </summary>
    private System.Collections.IEnumerator FadeInPanel()
    {
        if (canvasGroup == null) yield break;

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += fadeInSpeed * Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 淡入完成后停止时间
    /// </summary>
    private System.Collections.IEnumerator StopTimeAfterFade()
    {
        yield return new WaitForSecondsRealtime(0.5f);  // 等待淡入完成
        Time.timeScale = 0f;  // 停止时间
    }

    /// <summary>
    /// 禁用玩家控制
    /// </summary>
    private void DisablePlayerControls()
    {
        // 禁用船体控制
        BoatController boat = FindObjectOfType<BoatController>();
        if (boat != null)
        {
            boat.enabled = false;
        }

        // 禁用导弹发射
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
        }

        // 禁用摄像头跟随
        CameraFollow camera = FindObjectOfType<CameraFollow>();
        if (camera != null)
        {
            camera.enabled = false;
        }
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 恢复时间
        Time.timeScale = 1f;

        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 退出游戏（或返回主菜单）
    /// </summary>
    public void QuitGame()
    {
        // 恢复时间
        Time.timeScale = 1f;

        // 返回主菜单（场景索引0）
        SceneManager.LoadScene(0);
    }

    [Header("Victory UI (胜利界面)")]
    [Tooltip("胜利界面系统（StarRatingSystem），如果设置了此字段，会使用完整的胜利界面")]
    public StarRatingSystem starRatingSystem;  // 胜利界面系统

    /// <summary>
    /// 显示胜利界面
    /// </summary>
    /// <param name="message">胜利消息（可选）</param>
    public void ShowVictory(string message = "")
    {
        if (isGameOver || isVictory) return;

        isVictory = true;
        isGameOver = true;  // 也设置为游戏结束，禁用控制

        // 优先使用 StarRatingSystem 显示完整的胜利界面
        if (starRatingSystem != null)
        {
            // 确保 StarRatingSystem 所在的 GameObject 及其所有父对象都是激活的
            // 因为如果父对象未激活，即使子对象激活了，activeInHierarchy 也会是 false
            GameObject starRatingObj = starRatingSystem.gameObject;
            
            // 激活当前对象
            if (!starRatingObj.activeSelf)
            {
                starRatingObj.SetActive(true);
                Debug.Log($"GameOverManager: 已激活 StarRatingSystem 所在的 GameObject: {starRatingObj.name}");
            }
            
            // 激活所有父对象（确保整个层级都是激活的）
            Transform parent = starRatingObj.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                    Debug.Log($"GameOverManager: 已激活父对象: {parent.name}");
                }
                parent = parent.parent;
            }
            
            // 再次检查是否激活（等待一帧让 Unity 更新状态）
            if (!starRatingSystem.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("GameOverManager: StarRatingSystem GameObject 仍然未激活，可能需要等待一帧");
                // 使用协程延迟一帧再调用
                StartCoroutine(DelayedShowVictory());
                return;
            }

            // 获取当前关卡分数
            int currentScore = 0;
            if (ScoreManager.Instance != null)
            {
                currentScore = ScoreManager.Instance.GetCurrentScore();
            }

            // 调用 StarRatingSystem 显示完整界面
            starRatingSystem.ShowLevelComplete(currentScore);

            // 禁用玩家控制
            DisablePlayerControls();

            Debug.Log($"Victory: 使用 StarRatingSystem 显示胜利界面，分数: {currentScore}");
            return;
        }

        // 如果没有 StarRatingSystem，使用旧的简单文本显示方式
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // 设置文本（优先使用TextMeshPro）
            string victoryMsg = victoryMessage;
            if (!string.IsNullOrEmpty(message))
            {
                victoryMsg += "\n" + message;
            }
            // 添加积分奖励信息（通关奖励固定为100积分）
            if (ScoreManager.Instance != null)
            {
                victoryMsg += $"\n获得奖励：100积分";
            }

            if (gameOverTextTMP != null)
            {
                gameOverTextTMP.text = victoryMsg;
            }
            else if (gameOverText != null)
            {
                gameOverText.text = victoryMsg;
            }

            // 淡入效果（使用协程，不受Time.timeScale影响）
            StartCoroutine(FadeInPanel());
        }

        // 禁用玩家控制
        DisablePlayerControls();

        // 停止时间（在淡入完成后）
        StartCoroutine(StopTimeAfterFade());

        Debug.Log("Victory: " + message);
    }

    /// <summary>
    /// 检查是否游戏结束
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }

    /// <summary>
    /// 检查是否胜利
    /// </summary>
    public bool IsVictory()
    {
        return isVictory;
    }

    /// <summary>
    /// 延迟显示胜利界面（等待 GameObject 激活）
    /// </summary>
    private System.Collections.IEnumerator DelayedShowVictory()
    {
        // 等待一帧，让 Unity 更新 GameObject 的激活状态
        yield return null;

        if (starRatingSystem != null && starRatingSystem.gameObject.activeInHierarchy)
        {
            // 获取当前关卡分数
            int currentScore = 0;
            if (ScoreManager.Instance != null)
            {
                currentScore = ScoreManager.Instance.GetCurrentScore();
            }

            // 调用 StarRatingSystem 显示完整界面
            starRatingSystem.ShowLevelComplete(currentScore);

            // 禁用玩家控制
            DisablePlayerControls();

            Debug.Log($"Victory: 延迟显示胜利界面，分数: {currentScore}");
        }
        else
        {
            Debug.LogError("GameOverManager: 延迟后仍然无法激活 StarRatingSystem，请检查 WinCanvas 配置！");
        }
    }
}
