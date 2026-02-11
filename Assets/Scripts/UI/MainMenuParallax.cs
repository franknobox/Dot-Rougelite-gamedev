using System.Collections.Generic;
using UnityEngine;

public class MainMenuParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public RectTransform transform; // UI 元素
        public float speedX;            // X轴移动强度
        public float speedY;            // Y轴移动强度
        
        [HideInInspector]
        public Vector2 initialPosition; // 初始位置，防止位置跑偏
    }

    public List<ParallaxLayer> layers;  // 在编辑器里配置多层
    public float moveRange = 50f;       // 移动范围限制 (MoveRange)
    public float smoothing = 5f;        // 平滑插值，让移动不生硬

    void Start()
    {
        // 初始化: 在 Start 中记录每个 layer 的初始 anchoredPosition，防止位置跑偏
        foreach (var layer in layers)
        {
            if (layer.transform != null)
            {
                layer.initialPosition = layer.transform.anchoredPosition;
            }
        }
    }

    void Update()
    {
        // 获取屏幕中心位置
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // 获取鼠标相对于屏幕中心的位置偏移量
        Vector2 mousePos = Input.mousePosition;

        // 计算归一化偏移量 (-1 到 1)
        // 鼠标在最左边 -> -1，鼠标在最右边 -> 1
        float normalizedX = (mousePos.x - center.x) / center.x;
        float normalizedY = (mousePos.y - center.y) / center.y;

        // 限制在 -1 到 1 之间 (防止鼠标移出屏幕导致过度偏移)
        normalizedX = Mathf.Clamp(normalizedX, -1f, 1f);
        normalizedY = Mathf.Clamp(normalizedY, -1f, 1f);

        // 遍历所有 layer
        foreach (var layer in layers)
        {
            if (layer.transform == null) continue;

            // 计算目标位置：initialPosition + (normalizedOffset * moveRange * speed)
            // 鼠标在最左边(normalizedX=-1) -> 偏移 -moveRange * speedX
            // 例如: moveRange=50, speedX=1, normalizedX=-1 => offset = -50
            float offsetX = normalizedX * moveRange * layer.speedX;
            float offsetY = normalizedY * moveRange * layer.speedY;

            Vector2 targetPosition = new Vector2(
                layer.initialPosition.x + offsetX,
                layer.initialPosition.y + offsetY
            );

            // 使用 Vector2.Lerp 让 rectTransform.anchoredPosition 平滑移动到目标位置
            layer.transform.anchoredPosition = Vector2.Lerp(layer.transform.anchoredPosition, targetPosition, Time.deltaTime * smoothing);
        }
    }
}
