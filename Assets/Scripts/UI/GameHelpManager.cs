using UnityEngine;
using UnityEngine.UI;

public class GameHelpManager : MonoBehaviour
{
    public GameObject helpPanel; // 拖入 HelpPanel 自身
    public Button backgroundButton; // 拖入那个全屏大按钮

    void Start()
    {
        // 游戏一开始，先暂停游戏，并显示帮助
        ShowHelp();

        // 绑定按钮点击事件（也可以在Inspector里手动拖拽）
        backgroundButton.onClick.AddListener(CloseHelpAndStartGame);
    }

    // 显示帮助
    public void ShowHelp()
    {
        helpPanel.SetActive(true);
        Time.timeScale = 0f; //暂停时间，防止游戏背地里偷偷开始了
    }

    // 关闭帮助并开始游戏
    public void CloseHelpAndStartGame()
    {
        helpPanel.SetActive(false); // 隐藏界面
        Time.timeScale = 1f; //恢复时间，游戏正式开始！
    }
}