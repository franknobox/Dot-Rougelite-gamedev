using UnityEngine;

/// <summary>
/// 物体上下浮动效果 - 适用于buff道具、拾取物等
/// </summary>
public class FloatingEffect : MonoBehaviour
{
    [Header("浮动设置")]
    [Tooltip("浮动幅度（上下移动的距离）")]
    public float floatAmplitude = 0.15f;

    [Tooltip("浮动速度")]
    public float floatSpeed = 3f;

    [Header("可选：旋转效果")]
    [Tooltip("是否启用旋转")]
    public bool enableRotation = false;

    [Tooltip("旋转速度（度/秒）")]
    public float rotationSpeed = 50f;

    private Vector3 startPosition;

    private void Start()
    {
        // 记录初始位置
        startPosition = transform.position;
    }

    private void Update()
    {
        // 上下浮动效果（使用正弦波）
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // 可选的旋转效果
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }
}
