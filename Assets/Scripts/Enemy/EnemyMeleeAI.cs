using UnityEngine;

/// <summary>
/// 近战敌人 AI - 追逐玩家并造成接触伤害
/// </summary>
public class EnemyMeleeAI : EnemyBase
{
    [Header("AI 设置")]
    [SerializeField]
    [Tooltip("接触伤害值")]
    private float contactDamage = 10f;

    [SerializeField]
    [Tooltip("伤害冷却时间（防止连续伤害）")]
    private float damageCooldown = 1f;

    // 玩家引用
    private Transform player;
    
    // 伤害计时器
    private float lastDamageTime = -999f;
    
    // 动画组件
    // Animator 参数约定：
    // - Bool "IsMoving": 移动时为 true
    // - Trigger "Attack": 攻击瞬间触发
    // - Trigger "Die": 死亡时触发（在 EnemyBase 中调用）
    private Animator anim;
    private SpriteRenderer sr;

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
    }

    private void Update()
    {
        // 如果找不到玩家，不移动
        if (player == null)
        {
            // 停止移动动画
            if (anim != null)
            {
                anim.SetBool("IsMoving", false);
            }
            return;
        }

        // 持续向玩家移动
        Vector2 currentPos = transform.position;
        Vector2 targetPos = player.position;
        
        // 计算移动向量
        Vector2 moveDirection = (targetPos - currentPos).normalized;
        
        // 使用 MoveTowards 平滑移动
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
        transform.position = newPos;
        
        // 更新移动动画
        bool isMoving = Vector2.Distance(currentPos, newPos) > 0.01f;
        if (anim != null)
        {
            anim.SetBool("IsMoving", isMoving);
        }

        // 面向玩家（精灵翻转）
        if (sr != null)
        {
            if (moveDirection.x < 0)
            {
                sr.flipX = true; // 向左翻转
            }
            else if (moveDirection.x > 0)
            {
                sr.flipX = false; // 向右不翻转
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查是否碰到玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            // 冷却时间检查
            if (Time.time - lastDamageTime < damageCooldown)
            {
                return;
            }

            // 触发攻击动画
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }
            
            // 尝试对玩家造成伤害
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                lastDamageTime = Time.time;
                Debug.Log($"{gameObject.name} 对玩家造成了 {contactDamage} 点伤害");
            }
            else
            {
                Debug.LogWarning("玩家物体上没有 PlayerHealth 组件！");
            }
        }
    }
}
