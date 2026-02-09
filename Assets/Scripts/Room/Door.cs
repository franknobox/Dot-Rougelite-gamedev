using UnityEngine;

/// <summary>
/// 门触发器 - 挂在房间的门上
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Door : MonoBehaviour
{
    private void Awake()
    {
        // 确保 Collider 设置为 Trigger
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家触碰
        if (other.CompareTag("Player"))
        {
            // 调用 DungeonManager 加载下一个房间
            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance.LoadNextRoom();
            }
        }
    }
}
