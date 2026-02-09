using UnityEngine;

/// <summary>
/// 远程射击敌人 AI - 保持距离并射击玩家
/// </summary>
public class EnemyShooterAI : EnemyBase
{
    [Header("射击设置")]
    [SerializeField]
    [Tooltip("子弹预制体")]
    private GameObject projectilePrefab;

    [SerializeField]
    [Tooltip("射程范围")]
    private float shootingRange = 8f;

    [SerializeField]
    [Tooltip("射击间隔（秒）")]
    private float fireRate = 2f;

    [Header("移动设置")]
    [SerializeField]
    [Tooltip("保持距离（太近会后退）")]
    private float keepDistance = 5f;

    [SerializeField]
    [Tooltip("是否边退边打")]
    private bool retreatWhileShooting = true;

    // 玩家引用
    private Transform player;
    
    // 射击计时器
    private float nextFireTime = 0f;
    
    // 动画组件
    // Animator 参数约定：
    // - Bool "IsMoving": 移动时为 true
    // - Trigger "Attack": 攻击瞬间触发
    // - Trigger "Die": 死亡时触发（在 EnemyBase 中调用）
    private Animator anim;
    private SpriteRenderer sr;
    
    // 移动状态跟踪
    private Vector2 lastPosition;

    protected override void Start()
    {
        base.Start();

        // 获取动画组件（从子物体中查找）
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        
        // 安全检查
        if (anim == null)
        {
            Debug.LogWarning($"{gameObject.name}: 未找到 Animator 组件，动画将不可用");
        }
        if (sr == null)
        {
            Debug.LogWarning($"{gameObject.name}: 未找到 SpriteRenderer 组件，翻转将不可用");
        }

        // 查找玩家
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: 未找到标记为 'Player' 的物体！");
        }
        
        // 初始化位置
        lastPosition = transform.position;
    }

    private void Update()
    {
        // 如果找不到玩家，不执行 AI
        if (player == null) return;

        // 计算与玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 移动逻辑
        HandleMovement(distanceToPlayer);

        // 射击逻辑
        HandleShooting(distanceToPlayer);

        // 面向玩家
        FacePlayer();
    }

    /// <summary>
    /// 处理移动逻辑
    /// </summary>
    private void HandleMovement(float distanceToPlayer)
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = player.position;
        bool isMoving = false;

        if (distanceToPlayer > shootingRange)
        {
            // 距离太远，靠近玩家
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
            transform.position = newPos;
            isMoving = true;
        }
        else if (distanceToPlayer < keepDistance && retreatWhileShooting)
        {
            // 距离太近，后退
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, -moveSpeed * 0.5f * Time.deltaTime);
            transform.position = newPos;
            isMoving = true;
        }
        // 在 keepDistance 和 shootingRange 之间时，停止移动
        
        // 更新移动动画
        if (anim != null)
        {
            anim.SetBool("IsMoving", isMoving);
        }
    }

    /// <summary>
    /// 处理射击逻辑
    /// </summary>
    private void HandleShooting(float distanceToPlayer)
    {
        // 检查是否在射程内
        if (distanceToPlayer <= shootingRange)
        {
            // 检查射击冷却
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    /// <summary>
    /// 射击
    /// </summary>
    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: 未设置子弹预制体！");
            return;
        }

        // 触发攻击动画
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
        
        // 实例化子弹
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // 计算朝向玩家的方向
        Vector2 direction = (player.position - transform.position).normalized;

        // 设置子弹方向（假设子弹有 Projectile 脚本）
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDirection(direction);
        }
        else
        {
            // 如果没有 Projectile 脚本，尝试用 Rigidbody2D 推动
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * 10f; // 默认速度
            }
        }

        Debug.Log($"{gameObject.name} 发射了子弹");
    }

    /// <summary>
    /// 面向玩家
    /// </summary>
    private void FacePlayer()
    {
        if (sr == null || player == null) return;

        // 根据玩家位置翻转精灵
        if (player.position.x < transform.position.x)
        {
            sr.flipX = true; // 向左
        }
        else
        {
            sr.flipX = false; // 向右
        }
    }

    /// <summary>
    /// 在编辑器中显示射程和保持距离
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制射程范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        // 绘制保持距离
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, keepDistance);
    }
}
