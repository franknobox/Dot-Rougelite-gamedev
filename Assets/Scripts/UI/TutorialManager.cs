using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("教程面板对象")]
    public GameObject tutorialPanel;
    
    [Tooltip("显示教程文字的文本组件")]
    public TextMeshProUGUI contentText;
    
    [Tooltip("面板的RectTransform")]
    public RectTransform panelRect;

    [Tooltip("左侧的小物体位置，作为弹出起点")]
    public RectTransform startPoint;

    [Header("Animation Settings")]
    [Tooltip("弹出动画持续时间")]
    public float animationDuration = 0.5f;

    private Coroutine currentAnimation;
    private Vector2 originalPosition; // 面板最终显示的原始位置
    private Vector3 originalScale;    // 面板最终显示的原始缩放

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            if (panelRect != null)
            {
                originalPosition = panelRect.anchoredPosition;
                originalScale = panelRect.localScale;
            }
        }
    }

    /// <summary>
    /// 显示教程
    /// </summary>
    /// <param name="content">教程内容</param>
    public void ShowTutorial(string content)
    {
        if (tutorialPanel == null || panelRect == null || startPoint == null)
        {
            Debug.LogWarning("TutorialManager missing references!");
            return;
        }

        // 设置文本
        if (contentText != null)
        {
            contentText.text = content;
        }

        // 停止当前动画
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // 开启面板
        tutorialPanel.SetActive(true);

        // 播放弹出动画
        currentAnimation = StartCoroutine(AnimatePanel(true));
    }

    /// <summary>
    /// 隐藏教程
    /// </summary>
    public void HideTutorial()
    {
        if (tutorialPanel == null || !tutorialPanel.activeSelf) return;

        // 停止当前动画
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // 播放隐藏动画
        currentAnimation = StartCoroutine(AnimatePanel(false));
    }

    /// <summary>
    /// 动画协程
    /// </summary>
    /// <param name="show">true为显示，false为隐藏</param>
    private IEnumerator AnimatePanel(bool show)
    {
        float timer = 0f;
        
        // 确定起始状态和目标状态
        Vector2 startPos = show ? (Vector2)startPoint.position : originalPosition; // 注意：这里可能需要坐标转换，简单起见假设都在Canvas下或者用世界坐标
        // UI 世界坐标转换逻辑可能复杂，这里简化处理：
        // 如果 startPoint 和 panelRect 在同一个 Canvas 下，直接用 position 可能有问题。
        // 为了简单且稳健，我们用 Scale 动画 + 位移。
        
        // 修正逻辑：使用 anchoredPosition 进行动画，假设 startPoint 是 UI 元素
        // 但用户说"左侧的一个ui小物体"，所以我们假设它也是 RectTransform。
        
        // 我们从 startPoint 的位置移动到 originalPosition
        // 将 startPoint 的世界坐标转换为 panelRect 父物体的局部坐标
        Vector2 startLocalPos = originalPosition;
        if (panelRect.parent != null)
        {
            Vector3 worldPoint = startPoint.position;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                panelRect.parent as RectTransform, 
                RectTransformUtility.WorldToScreenPoint(null, worldPoint), 
                null, 
                out localPoint
            );
            startLocalPos = localPoint;
        }

        Vector2 fromPos = show ? startLocalPos : originalPosition;
        Vector2 toPos = show ? originalPosition : startLocalPos;

        Vector3 fromScale = show ? Vector3.zero : originalScale;
        Vector3 toScale = show ? originalScale : Vector3.zero;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime; // 使用 unscaledDeltaTime 即使暂停也能播放（虽然教程通常不暂停）
            float t = timer / animationDuration;
            // 使用平滑插值 (EaseOutBack 效果更好)
            t = Mathf.Sin(t * Mathf.PI * 0.5f); 

            panelRect.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            panelRect.localScale = Vector3.Lerp(fromScale, toScale, t);

            yield return null;
        }

        panelRect.anchoredPosition = toPos;
        panelRect.localScale = toScale;

        if (!show)
        {
            tutorialPanel.SetActive(false);
        }
    }
}
