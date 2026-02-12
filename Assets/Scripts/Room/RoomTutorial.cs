using UnityEngine;

/// <summary>
/// 挂在初始房间预制体上，用于触发新手教程
/// </summary>
public class RoomTutorial : MonoBehaviour
{
    [TextArea(3, 5)]
    [Tooltip("教程显示的文本内容")]
    public string tutorialContent = "欢迎来到地牢！\n\n使用 WASD 移动\n左键 攻击\n右键 投掷武器\nTab 打开背包合成";

    [Tooltip("是否只显示一次")]
    public bool showOnce = false;

    private static bool hasShown = false;

    private void Start()
    {
        // 如果只显示一次且已经显示过，则跳过
        if (showOnce && hasShown) return;

        // 调用 TutorialManager 显示教程
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowTutorial(tutorialContent);
            hasShown = true;
        }
    }

    private void OnDestroy()
    {
        // 当房间被销毁（离开房间）时，关闭教程
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.HideTutorial();
        }
    }
}
