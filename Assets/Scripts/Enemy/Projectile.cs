using UnityEngine;

/// <summary>
/// 简单的子弹脚本 - 直线飞行并造成伤害
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("子弹设置")]
    [SerializeField]
    [Tooltip("子弹飞行速度")]
    private float speed = 10f;

    [SerializeField]
    [Tooltip("子弹伤害值")]
    private float damage = 15f;

    [SerializeField]
    [Tooltip("子弹伤害类型")]
    private DamageType damageType = DamageType.Pierce;

    [SerializeField]
    [Tooltip("子弹生命周期（秒），超时自动销毁")]
    private float lifetime = 5f;

    // 组件引用
    private Rigidbody2D rb;
    
    // 飞行方向
    private Vector2 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 设置为运动学模式，不受重力影响
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    private void Start()
    {
        // 设置自动销毁
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// 设置子弹飞行方向
    /// </summary>
    /// <param name="dir">归一化的方向向量</param>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        
        // 设置速度
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }

        // 可选：旋转子弹朝向飞行方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    /// <summary>
    /// 处理碰撞逻辑（统一处理Trigger和Collision）
    /// </summary>
    private void HandleCollision(GameObject other)
    {
        // 检查是否击中玩家
        if (other.CompareTag("Player"))
        {
            // 对玩家造成伤害
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"子弹击中玩家，造成 {damage} 点伤害");
            }

            // 销毁子弹
            Destroy(gameObject);
        }
        // 检查是否击中墙壁(障碍)
        else if (other.CompareTag("Wall"))
        {
            // 销毁子弹
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 设置子弹伤害值
    /// </summary>
    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    /// <summary>
    /// 设置子弹伤害类型
    /// </summary>
    public void SetDamageType(DamageType type)
    {
        damageType = type;
    }
}
