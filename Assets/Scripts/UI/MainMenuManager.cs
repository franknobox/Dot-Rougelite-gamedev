using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("点击开始游戏后加载的场景名称")]
    public string gameSceneName = "Gameplay";

    [Tooltip("设置面板的引用 (用来引用设置弹窗)")]
    public GameObject settingsPanel;

    [Header("Transition")]
    [Tooltip("黑屏用到的 CanvasGroup (用来控制黑屏面板)")]
    public CanvasGroup fadeCanvasGroup;

    [Tooltip("过渡时间")]
    public float fadeDuration = 1f;

    [Header("Audio")]
    [Tooltip("背景音乐")]
    public AudioClip bgmClip;
    private AudioSource audioSource;

    void Start()
    {
        // 确保 settingsPanel 一开始是隐藏的
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // 确保 fadeCanvasGroup 初始状态正确 (完全透明，不阻挡射线)
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        // 播放 BGM
        if (bgmClip != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.clip = bgmClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.3f; 
            audioSource.Play();
        }
    }

    // 开始游戏: 启动协程进行过渡
    public void OnStartGame()
    {
        StartCoroutine(TransitionToScene(gameSceneName));
    }

    private System.Collections.IEnumerator TransitionToScene(string sceneName)
    {
        if (fadeCanvasGroup != null)
        {
            // 第一步 (阻挡): 防止玩家重复点击
            fadeCanvasGroup.blocksRaycasts = true;

            // 第二步 (淡出/变黑): 平滑插值
            float timer = 0f;
            float startVolume = (audioSource != null) ? audioSource.volume : 1f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeDuration;

                fadeCanvasGroup.alpha = Mathf.MoveTowards(0f, 1f, progress);
                
                // BGM 淡出
                if (audioSource != null)
                {
                    audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                }

                yield return null;
            }
            // 确保完全变黑
            fadeCanvasGroup.alpha = 1f;
        }

        // 第三步 (加载)
        SceneManager.LoadScene(sceneName);
    }

    // 退出游戏: 退出应用程序
    public void OnQuitGame()
    {
        #if UNITY_EDITOR
            // 如果是编辑器模式，停止播放
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // 否则退出应用程序
            Application.Quit();
        #endif
    }

    // 打开设置: 设置 settingsPanel.SetActive(true)
    public void OnOpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // 关闭设置: 设置 settingsPanel.SetActive(false)
    public void OnCloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // 音量控制 (可选): 修改 AudioListener.volume
    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }
}
