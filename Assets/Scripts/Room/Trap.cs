using UnityEngine;

/// <summary>
/// 陷阱脚本 - 玩家接触后扣血并销毁自身
/// </summary>
public class Trap : MonoBehaviour
{
    [Header("陷阱属性")]
    [Tooltip("造成的伤害值")]
    public float damage = 10f;

    [Tooltip("销毁时的特效（可选）")]
    public GameObject destroyEffect;

    [Header("音效设置")]
    [Tooltip("触发时的音效（可选）")]
    public AudioClip triggerSound;
    
    [Tooltip("音效音量 (0-1)")]
    [Range(0f, 1f)]
    public float soundVolume = 1.0f;

    [Header("破坏属性")]
    [Tooltip("是否可被玩家破坏")]
    public bool isDestroyable = true;
    
    [Tooltip("陷阱生命值")]
    public int health = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 检查是否是玩家（触发伤害）
        if (collision.CompareTag("Player"))
        {
            // 获取玩家生命值组件
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                // 造成伤害
                playerHealth.TakeDamage(damage);
                Debug.Log($"玩家触发陷阱，受到 {damage} 点伤害！");
                
                TriggerTrap();
            }
        }
        // 2. 检查是否是玩家武器（被破坏）
        else if (isDestroyable && (collision.CompareTag("Weapon") || collision.CompareTag("Projectile")))
        {
            Debug.Log($"陷阱被 {collision.name} 击中！");
            
            health--;
            if (health <= 0)
            {
                // 如果是子弹，销毁子弹
                if (collision.CompareTag("Projectile"))
                {
                    Destroy(collision.gameObject);
                }
                
                TriggerTrap();
            }
        }
    }

    /// <summary>
    /// 触发陷阱（播放特效、音效、销毁自身、触发刷怪）
    /// </summary>
    private void TriggerTrap()
    {
        // 播放音效 (在摄像机位置播放以避免距离衰减)
        if (triggerSound != null)
        {
            Vector3 playPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
            // 保持原本的 Z 轴可能有问题，所以直接用 Camera 的位置 (2D 游戏常用技巧)
            // 或者保留 XY，Z 设为 Camera.Z
            playPosition.z = -10f; // 假设摄像机在 -10

            AudioSource.PlayClipAtPoint(triggerSound, playPosition, soundVolume);
        }

        // 生成销毁特效
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // 尝试触发房间刷怪
        RoomController room = GetComponentInParent<RoomController>();
        if (room != null)
        {
            room.StartWave();
        }

        // 销毁陷阱
        Destroy(gameObject);
    }
}
