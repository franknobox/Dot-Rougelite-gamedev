using UnityEngine;
using Spine.Unity;

/// <summary>
/// 近战敌人 AI - 使用简单的状态机（发呆/巡逻/追击/攻击）
/// </summary>
public enum State { Idle, Patrol, Chase, Attack }

public class EnemyMeleeAI : EnemyBase
{
    [Header("AI State")]
    public State currentState;

    [Header("AI Configure")]
    [Tooltip("巡逻半径")]
    public float patrolRange = 3f;
    [Tooltip("发现玩家的距离")]
    public float detectionRange = 5f;
    [Tooltip("巡逻移动速度")]
    public float patrolSpeed = 2f; 
    [Tooltip("追击移动速度")]
    public float chaseSpeed = 4f;
    [Tooltip("攻击距离")]
    public float attackRange = 1.5f;

    [Header("AI Settings")]
    [SerializeField]
    [Tooltip("接触伤害值")]
    private float contactDamage = 10f;

    [SerializeField]
    [Tooltip("伤害冷却时间（防止连续伤害）")]
    private float damageCooldown = 1f;

    [SerializeField]
    [Tooltip("巡逻到达目的点后的发呆时间")]
    private float idleDuration = 2f;

    // 玩家引用
    private Transform player;
    
    // 伤害计时器
    private float lastDamageTime = -999f;
    
    // 动画组件
    // private SkeletonMecanim skeletonMecanim; // 已在父类 EnemyBase 中定义
    private Animator anim;

    // 状态机变量
    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private float idleTimer;

    protected override void Start()
    {
        base.Start();

        // 记录初始位置作为巡逻中心
        startPosition = transform.position;
        currentState = State.Patrol;
        PickNewPatrolTarget();

        // 获取Spine Mecanim组件（父类已处理）
        // skeletonMecanim = GetComponentInChildren<SkeletonMecanim>();

        if (skeletonMecanim != null)
        {
            anim = skeletonMecanim.GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogError($"{gameObject.name}: SkeletonMecanim上未找到 Animator 组件！");
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: 未找到 SkeletonMecanim 组件！");
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
        if (player == null) return;

        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Attack:
                HandleAttack();
                break;
        }

        UpdateAnimation();
    }

    /// <summary>
    /// 处理发呆状态
    /// </summary>
    private void HandleIdle()
    {
        // 计时结束切换到巡逻
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            PickNewPatrolTarget();
            currentState = State.Patrol;
        }

        // 检查是否有玩家进入警戒范围
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer < detectionRange)
        {
            currentState = State.Chase;
        }
    }

    /// <summary>
    /// 处理巡逻状态
    /// </summary>
    private void HandlePatrol()
    {
        // 移动到巡逻目标点
        MoveTo(patrolTarget, patrolSpeed);

        // 到达目标点，切换到发呆
        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f)
        {
            idleTimer = idleDuration;
            currentState = State.Idle;
        }

        // 检查是否有玩家进入警戒范围
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer < detectionRange)
        {
            currentState = State.Chase;
        }
    }

    /// <summary>
    /// 处理追击状态
    /// </summary>
    private void HandleChase()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // 如果玩家跑太远，切回巡逻
        // 给一点缓冲区域避免在边缘反复横跳 (detectionRange * 1.5f)
        if (distToPlayer > detectionRange * 1.5f)
        {
            currentState = State.Patrol;
            PickNewPatrolTarget();
            return;
        }

        // 如果距离小于攻击范围，切换到攻击
        if (distToPlayer <= attackRange)
        {
            currentState = State.Attack;
            return;
        }

        // 向玩家移动
        MoveTo(player.position, chaseSpeed);
    }

    // 攻击计时器
    private float lastAttackTime = -999f;

    /// <summary>
    /// 处理攻击状态
    /// </summary>
    private void HandleAttack()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // 如果玩家离开攻击范围，切回追击
        // 增加滞后判断（1.5倍范围），防止在临界点频繁反复切换状态导致抽搐
        if (distToPlayer > attackRange * 1.5f)
        {
            currentState = State.Chase;
            return;
        }

        // 面向玩家
        Vector2 direction = (player.position - transform.position).normalized;
        Flip(direction.x);

        // 攻击逻辑
        // 使用单独的 lastAttackTime 控制动画触发频率，避免 Update 中每帧重复 SetTrigger 导致动画重置/抽搐
        // 这里可以直接复用 damageCooldown 作为攻击间隔，或者单独定义
        if (Time.time - lastAttackTime >= damageCooldown)
        {
             // 触发攻击动画
            if (anim != null)
            {
                anim.SetTrigger("Attack");
                lastAttackTime = Time.time;
                
                // 启动武器伤害判定协程
                if (weaponCollider != null)
                {
                    StartCoroutine(EnableWeaponHitbox());
                }
            }
        }
    }

    /// <summary>
    /// 移动逻辑
    /// </summary>
    private void MoveTo(Vector2 target, float speed)
    {
        Vector2 currentPos = transform.position;
        Vector2 direction = (target - currentPos).normalized;
        Vector2 newPos = Vector2.MoveTowards(currentPos, target, speed * Time.deltaTime);
        transform.position = newPos;

        // 面向目标
        if (direction.sqrMagnitude > 0.001f)
        {
            Flip(direction.x);
        }
    }

    /// <summary>
    /// 翻转角色
    /// </summary>
    private void Flip(float moveX)
    {
        if (skeletonMecanim != null)
        {
            if (moveX < -0.01f)
            {
                skeletonMecanim.Skeleton.ScaleX = -1; // 向左
            }
            else if (moveX > 0.01f)
            {
                skeletonMecanim.Skeleton.ScaleX = 1; // 向右
            }
        }
    }

    /// <summary>
    /// 选择新的巡逻点
    /// </summary>
    private void PickNewPatrolTarget()
    {
        // 在初始位置周围随机找一个点
        Vector2 randomPoint = Random.insideUnitCircle * patrolRange;
        patrolTarget = startPosition + randomPoint;
    }

    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimation()
    {
        if (anim == null) return;

        bool isMoving = false;

        // 只有在Patrol且未到达，或Chase状态下才算Moving
        // A simple heuristic: if we moved significantly this frame?
        // Or based on state.
        
        if (currentState == State.Patrol)
        {
            if (Vector2.Distance(transform.position, patrolTarget) > 0.1f)
            {
                isMoving = true;
            }
        }
        else if (currentState == State.Chase)
        {
            // Chase状态下通常一直在动，除非被Attack打断
            isMoving = true;
        }

        anim.SetBool("IsMoving", isMoving);
    }

    [Header("Attack Settings")]
    [Tooltip("武器碰撞体脚本引用（如果不赋值则只使用身体碰撞伤害）")]
    public EnemyWeapon weaponCollider;
    
    [Tooltip("是否启用身体接触伤害（如果不仅靠武器攻击）")]
    public bool enableBodyDamage = true;

    [Tooltip("攻击前摇时间（动画播放多久后启用武器判定）")]
    public float attackDelay = 0.2f;

    [Tooltip("攻击判定持续时间")]
    public float attackDuration = 0.5f;

    /// <summary>
    /// 控制武器碰撞体启用的协程
    /// </summary>
    private System.Collections.IEnumerator EnableWeaponHitbox()
    {
        //等待前摇
        yield return new WaitForSeconds(attackDelay);

        if (weaponCollider != null)
        {
            weaponCollider.isDamageEnabled = true;
            // weaponCollider.damage = contactDamage; // 不要覆盖武器自身的伤害设置，使用 EnemyWeapon 面板上的数值
            // Debug.Log("Weapon Hitbox Enabled");
        }

        // 持续一段时间
        yield return new WaitForSeconds(attackDuration);

        if (weaponCollider != null)
        {
            weaponCollider.isDamageEnabled = false;
            // Debug.Log("Weapon Hitbox Disabled");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果禁用了身体接触伤害，则直接返回
        if (!enableBodyDamage) return;

        // 保持原有的碰撞伤害逻辑
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time - lastDamageTime < damageCooldown)
            {
                return;
            }

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                lastDamageTime = Time.time;
                
                Debug.Log($"{gameObject.name} (Body) 对玩家造成了 {contactDamage} 点伤害");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 画出巡逻范围（绿色）
        Gizmos.color = Color.green;
        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.DrawWireSphere(center, patrolRange);

        // 画出警戒范围（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 画出攻击范围（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
