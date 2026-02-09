using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 网格槽位脚本，用于挂在 UI 的 Button 上
/// </summary>
[RequireComponent(typeof(Button))]
public class GridSlot : MonoBehaviour
{
    [Header("槽位信息")]
    [Tooltip("在5x5网格中的坐标")]
    public Vector2Int coordinate;
    
    [Tooltip("是否已填充")]
    public bool isFilled;

    [Header("UI引用")]
    [Tooltip("用于显示Dot的图片")]
    public Image iconImage;

    [Header("视觉设置")]
    [Tooltip("空状态颜色")]
    public Color emptyColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    
    [Tooltip("填充状态颜色")]
    public Color filledColor = new Color(0.2f, 0.8f, 0.3f, 1f);

    private Button button;

    private void Awake()
    {
        // 获取Button组件
        button = GetComponent<Button>();
        
        // 添加点击事件监听
        button.onClick.AddListener(OnButtonClicked);
    }

    /// <summary>
    /// 初始化槽位坐标
    /// </summary>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    public void Initialize(int x, int y)
    {
        coordinate = new Vector2Int(x, y);
        
        // 初始化为空状态
        SetFilled(false);
        
        Debug.Log($"GridSlot 初始化完成，坐标: ({x}, {y})");
    }

    /// <summary>
    /// 设置槽位填充状态
    /// </summary>
    /// <param name="status">true为填充，false为空</param>
    public void SetFilled(bool status)
    {
        isFilled = status;

        if (iconImage != null)
        {
            // 始终保持图片启用，确保按钮可以被点击
            iconImage.enabled = true;
            
            if (status)
            {
                // 填充状态：显示为不透明的填充颜色
                iconImage.color = filledColor;
            }
            else
            {
                // 空状态：显示为半透明的空状态颜色
                iconImage.color = emptyColor;
            }
        }
    }

    /// <summary>
    /// 按钮点击事件处理
    /// </summary>
    private void OnButtonClicked()
    {
        Debug.Log($"GridSlot ({coordinate.x}, {coordinate.y}) 被点击");
        
        // 调用 GridManager 的槽位点击处理方法
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnSlotClicked(this);
        }
        else
        {
            Debug.LogWarning("GridManager.Instance 未找到！");
        }
    }

    private void OnDestroy()
    {
        // 移除点击事件监听，防止内存泄漏
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
