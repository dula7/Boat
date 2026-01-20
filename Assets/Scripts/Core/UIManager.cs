using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace InkBoatGame.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("===== 核心界面引用 =====")]
        public GameObject startPanel;       // 1. 开始界面
        public GameObject levelSelectPanel; // 2. 关卡选择界面
        public GameObject settingsPanel;    // 3. 设置界面
        public GameObject shopPanel;        // 4. 商店界面
        public GameObject winPanel;         // 5. 成功界面
        public GameObject losePanel;        // 6. 失败界面
        public GameObject hudPanel;         // 游戏中的HUD (分数、暂停按钮)

        [Header("===== HUD 组件 =====")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI diamondText;

        [Header("===== 结算组件 (Win) =====")]
        public Image[] stars; // 3颗星星
        public Color starLitColor = Color.yellow;
        public Color starDimColor = Color.gray;
        public TextMeshProUGUI winScoreText;

        [Header("===== 设置组件 =====")]
        public Slider bgmSlider;
        public Slider sfxSlider;

        // 用于存储所有面板的列表，方便管理
        private List<GameObject> allPanels;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // 初始化面板列表
            allPanels = new List<GameObject> { startPanel, levelSelectPanel, settingsPanel, shopPanel, winPanel, losePanel, hudPanel };
        }

        private void Start()
        {
            // 游戏启动时显示开始界面
            ShowPanel(startPanel);

            // 确保鼠标在UI场景中可见（UI场景需要显示鼠标）
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 初始化音量条数值
            if (AudioManager.Instance != null)
            {
                if (bgmSlider != null)
                {
                    bgmSlider.value = AudioManager.Instance.GetBGMVolume();
                }
                if (sfxSlider != null)
                {
                    sfxSlider.value = AudioManager.Instance.GetSFXVolume();
                }
            }
        }

        #region 面板管理系统 (核心逻辑)

        /// <summary>
        /// 通用方法：显示指定面板，隐藏其他所有面板（除了HUD可能的特殊情况）
        /// </summary>
        public void ShowPanel(GameObject panelToShow)
        {
            foreach (var panel in allPanels)
            {
                if (panel != null)
                    panel.SetActive(panel == panelToShow);
            }
        }

        /// <summary>
        /// 叠加显示面板（例如：在游戏进行中打开设置，不隐藏HUD）
        /// </summary>
        public void ShowPopup(GameObject popupPanel)
        {
            if (popupPanel != null) popupPanel.SetActive(true);
        }

        /// <summary>
        /// 关闭叠加面板
        /// </summary>
        public void ClosePopup(GameObject popupPanel)
        {
            if (popupPanel != null) popupPanel.SetActive(false);
        }

        #endregion

        #region 按钮点击事件绑定 (供UI Button OnClick调用)

        // 1. 开始界面逻辑
        public void OnBtn_StartGame()
        {
            ShowPanel(levelSelectPanel); // 点击开始进入关卡选择
        }

        public void OnBtn_Shop()
        {
            ShowPanel(shopPanel); // 进入商店
        }

        // 2. 关卡选择逻辑
        public void OnBtn_SelectLevel(int levelIndex)
        {
            // 这里调用场景加载逻辑
            // SceneManager.LoadScene("Level" + levelIndex);
            // 进入游戏后显示HUD
            ShowPanel(hudPanel);
        }

        public void OnBtn_BackToMain()
        {
            ShowPanel(startPanel);
        }

        // 3. 设置界面逻辑
        public void OnBtn_OpenSettings()
        {
            ShowPopup(settingsPanel); // 设置通常是弹窗
            Time.timeScale = 0; // 暂停时间
        }

        public void OnBtn_CloseSettings()
        {
            ClosePopup(settingsPanel);
            Time.timeScale = 1; // 恢复时间
        }

        // 音量控制
        public void OnSlider_BGM(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetBGMVolume(value);
            }
        }

        /// <summary>
        /// SFX 音量滑块回调
        /// </summary>
        public void OnSlider_SFX(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
        }

        // 4. 商店逻辑
        public void OnBtn_BuyItem(int itemId)
        {
            // 处理购买逻辑扣除积分
            Debug.Log($"购买了物品 ID: {itemId}");
        }

        // 5 & 6. 游戏结束逻辑
        public void GameWin(int starCount, int score)
        {
            ShowPopup(winPanel);
            winScoreText.text = "Score: " + score;
            // 简单的星星显示逻辑
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].color = (i < starCount) ? starLitColor : starDimColor;
            }
            Time.timeScale = 0;
        }

        public void GameLose()
        {
            ShowPopup(losePanel);
            Time.timeScale = 0;
        }

        public void OnBtn_NextLevel()
        {
            Time.timeScale = 1;
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void OnBtn_Retry()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// 退出游戏按钮点击事件
        /// </summary>
        public void OnBtn_QuitGame()
        {
            Debug.Log("退出游戏");
            
            // 在编辑器中，Application.Quit() 不会真正退出，所以添加提示
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        #endregion
    }
}