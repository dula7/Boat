using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 场景重启管理器
/// 支持按钮点击重置当前场景，包含加载提示、数据清理等功能
/// </summary>
public class RestartScene : MonoBehaviour
{
    [Header("UI配置")]
    [Tooltip("加载提示文本（可选）")]
    public Text loadingText;
    [Tooltip("加载进度条（可选）")]
    public Slider loadingSlider;
    [Tooltip("重启按钮")]
    public Button restartButton;

    [Header("加载配置")]
    [Tooltip("加载延迟（秒），避免点击过快")]
    public float clickDelay = 0.2f;
    [Tooltip("是否显示加载提示")]
    public bool showLoadingTips = true;

    // 防止重复点击
    private bool isRestarting = false;

    private void Awake()
    {
        // 初始化UI
        if (loadingText != null) loadingText.gameObject.SetActive(false);
        if (loadingSlider != null)
        {
            loadingSlider.gameObject.SetActive(false);
            loadingSlider.value = 0;
        }
    }

    private void Start()
    {
        // 绑定按钮点击事件（如果在Inspector面板未绑定）
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        else
        {
            Debug.LogWarning("未分配重启按钮！请在Inspector面板指定Restart Button");
        }
    }

    /// <summary>
    /// 重启按钮点击事件
    /// </summary>
    public void OnRestartButtonClicked()
    {
        // 防止重复点击
        if (isRestarting) return;

        isRestarting = true;
        // 禁用按钮防止重复点击
        if (restartButton != null) restartButton.interactable = false;

        // 开始重启流程
        StartCoroutine(RestartSceneCoroutine());
    }

    /// <summary>
    /// 重启场景协程（包含加载进度显示）
    /// </summary>
    /// <returns></returns>
    private IEnumerator RestartSceneCoroutine()
    {
        // 短暂延迟，避免点击抖动
        yield return new WaitForSeconds(clickDelay);

        // 显示加载UI
        if (showLoadingTips)
        {
            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(true);
                loadingText.text = "正在重启场景...";
            }
            if (loadingSlider != null) loadingSlider.gameObject.SetActive(true);
        }

        // 清理游戏数据（可根据项目需求扩展）
        ClearGameData();

        // 获取当前场景名称
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(currentSceneName);
        // 禁止场景加载完成后自动激活
        asyncLoad.allowSceneActivation = false;

        // 显示加载进度
        while (!asyncLoad.isDone)
        {
            // 更新进度条
            if (loadingSlider != null)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // Unity加载进度到0.9才完成
                loadingSlider.value = progress;

                if (loadingText != null)
                {
                    loadingText.text = $"重启中... {Mathf.Round(progress * 100)}%";
                }
            }

            // 进度达到0.9时激活场景
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // 重置状态（场景加载完成后会重新初始化，这里是冗余保护）
        isRestarting = false;
        if (restartButton != null) restartButton.interactable = true;
    }

    /// <summary>
    /// 清理游戏数据（根据项目需求自定义）
    /// </summary>
    [System.Obsolete]
    private void ClearGameData()
    {
        // 示例：重置玩家位置、分数、物理速度等
        // 1. 重置静态分数变量
        // GameManager.Score = 0;
        // 2. 销毁所有动态生成的物体
        // GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // foreach (var enemy in enemies)
        // {
        //     Destroy(enemy);
        // }
        // 3. 重置物理系统
        Physics.autoSimulation = false;
        Physics.autoSimulation = true;

        Debug.Log("游戏数据已重置");
    }

    // 可选：通过键盘快捷键重启（比如按R键）
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.R) && !isRestarting)
    //    {
    //        OnRestartButtonClicked();
    //    }
    //}
}