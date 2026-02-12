using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText; // HUD上的分数
    public GameObject gameOverPanel; // 结算面板
    public TextMeshProUGUI finalScoreText; // 结算面板上的最终分数

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu"; // 主菜单场景名称

    public int currentScore = 0;

    private void Awake()
    {
        // 单例模式
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初始化
        currentScore = 0;
        UpdateScoreUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 增加分数
    /// </summary>
    /// <param name="amount">增加的数量</param>
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    /// <summary>
    /// 更新 HUD 分数显示
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    public void GameOver()
    {
        // 显示 GameoverPanel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 设置最终分数
        if (finalScoreText != null)
        {
            finalScoreText.text = currentScore.ToString();
        }

        // 暂停游戏
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void BackToMenu()
    {
        // 恢复时间流速
        Time.timeScale = 1f;

        // 加载主菜单场景
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    /// <summary>
    /// 重新开始游戏 (可选)
    /// </summary>
    public void RestartGame()
    {
        // 恢复时间流速
        Time.timeScale = 1f;
        
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
