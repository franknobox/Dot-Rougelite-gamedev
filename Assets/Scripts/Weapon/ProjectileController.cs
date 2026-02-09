using UnityEngine;

/// <summary>
/// 飞镖/投射物控制器 - 用于远程武器
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileController : MonoBehaviour
{
    [Header("飞镖属性")]
    private float speed;
    private float damage;
    private DamageType damageType;
    private int durabilityCost;
    
    [Header("自动销毁")]
    [SerializeField] private float lifetime = 5f;
    
    private Rigidbody2D rb;
    private bool hasHit = false; // 防止重复触发

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 确保 Collider 是 Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// 初始化飞镖
    /// </summary>
    /// <param name="data">武器数据</param>
    /// <param name="direction">飞行方向（归一化向量）</param>
    public void Initialize(WeaponData data, Vector2 direction)
    {
        // 记录武器数据
        speed = data.attackRate; // 使用 attackRate 作为速度
        damage = data.baseDamage;
        damageType = data.damageType;
        durabilityCost = 1; // 每个飞镖消耗1点耐久
        
        // 设置刚体速度
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
        
        // 设置旋转朝向飞行方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // 启动自动销毁计时器
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// 碰撞检测
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 防止重复触发
        if (hasHit) return;
        
        // 碰到敌人
        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // 对敌人造成伤害（EnemyBase 会自动处理克制关系）
                enemy.TakeDamage(damage, damageType);
                
                hasHit = true;
                
                // TODO: 播放命中特效
                // Instantiate(hitEffect, transform.position, Quaternion.identity);
                
                // 销毁飞镖
                Destroy(gameObject);
            }
        }
        // 碰到墙壁
        else if (other.CompareTag("Wall"))
        {
            hasHit = true;
            
            // TODO: 播放撞墙特效
            // Instantiate(wallHitEffect, transform.position, Quaternion.identity);
            
            // 销毁飞镖
            Destroy(gameObject);
        }
    }
}
