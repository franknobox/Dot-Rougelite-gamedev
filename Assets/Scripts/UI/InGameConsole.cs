using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 实机运行时的简易控制台
/// 挂载到场景中任意物体上即可 (比如 DungeonManager 或 MainCamera)
/// </summary>
public class InGameConsole : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("是否显示控制台")]
    public bool showConsole = true;
    
    [Tooltip("最多显示多少行日志")]
    public int maxLogCount = 50;

    [Tooltip("字体大小")]
    public int fontSize = 20;

    [Header("窗口大小比例 (0.1 ~ 1.0)")]
    [Range(0.1f, 1f)]
    public float widthRatio = 0.4f;
    [Range(0.1f, 1f)]
    public float heightRatio = 0.4f;

    // 存储日志的结构体
    struct LogEntry
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }

    private List<LogEntry> logs = new List<LogEntry>();
    private Vector2 scrollPosition;
    private bool showStackTrace = false;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        LogEntry entry = new LogEntry
        {
            message = logString,
            stackTrace = stackTrace,
            type = type
        };

        logs.Add(entry);

        // 限制日志数量
        if (logs.Count > maxLogCount)
        {
            logs.RemoveAt(0);
        }

        // 自动滚动到底部
        scrollPosition.y = float.MaxValue;
    }

    private void OnGUI()
    {
        if (!showConsole) return;

        // 设置 GUI 样式
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;

        // 绘制背景框 (左下角，占据屏幕宽度的 widthRatio，高度的 heightRatio)
        float consoleWidth = Screen.width * widthRatio;
        float consoleHeight = Screen.height * heightRatio;
        float margin = 20f;
        
        Rect windowRect = new Rect(margin, Screen.height - consoleHeight - margin, consoleWidth, consoleHeight);
        GUI.Box(windowRect, "Debug Console");

        // 绘制缩放按钮 (右上角)
        if (GUI.Button(new Rect(windowRect.x + windowRect.width - 60, windowRect.y, 25, 20), "+"))
        {
            fontSize = Mathf.Min(fontSize + 2, 50);
        }
        if (GUI.Button(new Rect(windowRect.x + windowRect.width - 30, windowRect.y, 25, 20), "-"))
        {
            fontSize = Mathf.Max(fontSize - 2, 10);
        }

        // 绘制滚动区域
        Rect scrollRect = new Rect(windowRect.x + 5, windowRect.y + 25, windowRect.width - 10, windowRect.height - 30);
        Rect contentRect = new Rect(0, 0, scrollRect.width - 20, logs.Count * (fontSize + 5));

        scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, contentRect);

        float currentY = 0;
        foreach (LogEntry log in logs)
        {
            // 根据日志类型设置颜色
            switch (log.type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    style.normal.textColor = Color.red;
                    break;
                case LogType.Warning:
                    style.normal.textColor = Color.yellow;
                    break;
                default:
                    style.normal.textColor = Color.white;
                    break;
            }

            // 绘制日志内容
            string displayMessage = log.message;
            if (showStackTrace && (log.type == LogType.Error || log.type == LogType.Exception))
            {
                displayMessage += "\n" + log.stackTrace;
            }

            float height = style.CalcHeight(new GUIContent(displayMessage), contentRect.width);
            Rect labelRect = new Rect(0, currentY, contentRect.width, height);
            
            GUI.Label(labelRect, displayMessage, style);
            currentY += height;
        }

        GUI.EndScrollView();

        // 绘制开关按钮 (右上角小按钮用来开关控制台显示，防止挡路)
        /*
        if (GUI.Button(new Rect(windowRect.x, windowRect.y - 30, 100, 25), showStackTrace ? "Hide Stack" : "Show Stack"))
        {
            showStackTrace = !showStackTrace;
        }
        */
    }

    private void Update()
    {
        // 按下 ` 键 (Tab上面那个) 切换显示/隐藏
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            showConsole = !showConsole;
        }
    }
}
