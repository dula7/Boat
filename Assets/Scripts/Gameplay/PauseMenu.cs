using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject pausePanel;
    public GameObject hudPauseBtn; // HUD中的暂停按钮

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        if (hudPauseBtn) hudPauseBtn.SetActive(false);

        Time.timeScale = 0f;

        // 显示鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 暂停玩家控制脚本
        TogglePlayerScripts(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        if (hudPauseBtn) hudPauseBtn.SetActive(true);

        Time.timeScale = 1f;

        // 恢复鼠标状态（不锁定，因为准心需要跟随鼠标移动）
        // 第三人称游戏不需要锁定鼠标，摄像头旋转使用 Input.GetAxis("Mouse X/Y") 即可
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;  // 隐藏鼠标光标，但允许移动（准心会跟随）

        // 恢复玩家控制脚本
        TogglePlayerScripts(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // === 暂停/恢复玩家控制脚本 ===
    void TogglePlayerScripts(bool isOpen)
    {
        // 查找船体（使用BoatController）
        BoatController boat = FindObjectOfType<BoatController>();
        if (boat != null)
        {
            boat.enabled = isOpen;
        }

        // 暂停/恢复摄像头控制（PlayerController用于发射导弹）
        PlayerController playerController = Camera.main?.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = isOpen;
        }
    }
}
