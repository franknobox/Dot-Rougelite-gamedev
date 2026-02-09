using UnityEngine;

/// <summary>
/// Dot 拾取物 - 玩家触碰后增加资源
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DotPickup : MonoBehaviour
{
    [Header("拾取设置")]
    [Tooltip("拾取后增加的 Dot 数量")]
    public int dotAmount = 1;

    [Header("音效设置")]
    [Tooltip("拾取音效")]
    public AudioClip pickupSound;

    private void Awake()
    {
        // 确保 Collider 设置为 Trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家触碰
        if (other.CompareTag("Player"))
        {
            // 增加玩家的 Dot 资源
            if (GridManager.Instance != null)
            {
                GridManager.Instance.AddDots(dotAmount);
                Debug.Log($"玩家拾取了 {dotAmount} 个 Dot");
            }
            else
            {
                Debug.LogError("GridManager 实例未找到！");
            }

            // 播放拾取音效
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 销毁拾取物
            Destroy(gameObject);
        }
    }
}
