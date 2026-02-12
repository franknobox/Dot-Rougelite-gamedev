using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 管理游戏的暂停状态和 UI 面板
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("暂停菜单界面")]
    public GameObject pausePanel;
    
    [Tooltip("设置界面")]
    public GameObject settingsPanel;

    [Header("Scene Settings")]
    [Tooltip("主菜单场景的名称")]
    public string mainMenuScene = "Menu";

    // 全局静态变量，方便其他脚本查询是否暂停
    public static bool isPaused = false;

    private void Start()
    {
        // 游戏开始时确保未暂停
        // 注意：如果从主菜单进入，isPaused 可能是上次残留的 true，所以这里重置是个好习惯
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // 检测 ESC 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 如果设置面板正在显示，优先关闭设置面板
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                // 否则切换暂停状态
                TogglePause();
            }
        }
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;

        // 设置时间缩放：暂停时为0，正常时为1
        Time.timeScale = isPaused ? 0f : 1f;

        // 控制暂停面板显示
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        // 确保切换暂停时，设置面板是关闭的（防止逻辑混乱）
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 可选：在暂停时显示/隐藏鼠标光标
        // Cursor.visible = isPaused;
        // Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// 继续游戏（供按钮调用）
    /// </summary>
    public void ResumeGame()
    {
        // 如果当前是暂停状态，调用 TogglePause 即可解除暂停
        if (isPaused)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 打开设置面板
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 关闭设置面板
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 关闭设置后，返回暂停菜单（如果当前是暂停状态）
        if (isPaused && pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void LoadMainMenu()
    {
        // 重要：在加载场景前恢复时间流速，否则新场景也会是暂停的
        Time.timeScale = 1f;
        isPaused = false;

        Debug.Log($"Loading Main Menu: {mainMenuScene}");
        SceneManager.LoadScene(mainMenuScene);
    }
}
