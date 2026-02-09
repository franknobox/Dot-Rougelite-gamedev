using UnityEngine;

/// <summary>
/// Swordman 角色控制器（简单版）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class SwordmanController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动速度")]
    public float moveSpeed = 5f;

    [Header("攻击设置")]
    [Tooltip("攻击按键")]
    public KeyCode attackKey = KeyCode.Mouse0;
    
    [Header("武器设置")]
    [Tooltip("武器数据")]
    public WeaponData weaponData;
    
    [Tooltip("武器控制器（自动从子物体获取）")]
    private WeaponController weaponController;

    // 组件引用
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private bool isAttacking = false;
    
    // Animator 参数名称
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("SwordmanController 需要 Animator 组件！");
        }
        
        // 获取子物体中的武器控制器
        weaponController = GetComponentInChildren<WeaponController>();
        if (weaponController == null)
        {
            Debug.LogWarning("SwordmanController: 未找到 WeaponController 组件！");
        }
    }
    
    private void Start()
    {
        // 初始化武器
        if (weaponController != null && weaponData != null)
        {
            weaponController.Initialize(weaponData);
        }
        else if (weaponController != null && weaponData == null)
        {
            Debug.LogWarning("SwordmanController: weaponData 未设置！请在 Inspector 中设置武器数据。");
        }
    }

    private void Update()
    {
        // 检测攻击输入
        if (Input.GetKeyDown(attackKey) && !isAttacking)
        {
            Attack();
        }

        // 获取 WASD 输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        moveInput = new Vector2(horizontal, vertical).normalized;
        
        // 左右转向（翻转整个 GameObject 的 scale.x）
        if (!isAttacking && moveInput.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = moveInput.x > 0 ? -1 : 1; // 向右为-1，向左为1（反转）
            transform.localScale = scale;
        }
        
        // 更新动画
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        // 如果正在攻击，不移动
        if (isAttacking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 使用物理系统移动
        rb.velocity = moveInput * moveSpeed;
    }

    /// <summary>
    /// 更新动画参数
    /// </summary>
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // 设置速度参数（0 = Idle, 1 = Walk/Run）
        float speed = isAttacking ? 0f : moveInput.magnitude;
        animator.SetFloat(SpeedParam, speed);
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    private void Attack()
    {
        if (animator == null) return;

        // 触发攻击动画
        animator.SetTrigger(AttackTrigger);
        
        // 执行武器攻击
        if (weaponController != null)
        {
            weaponController.PerformAttack();
        }
    }

    // 以下方法可以在动画事件中调用
    
    /// <summary>
    /// 动画事件：攻击开始
    /// </summary>
    public void OnAttackStart()
    {
        isAttacking = true;
        
        // 执行武器攻击（如果使用动画事件，在这里调用）
        // 注意：如果在 Attack() 中已经调用了，这里就不需要再调用
        // if (weaponController != null)
        // {
        //     weaponController.PerformAttack();
        // }
    }

    /// <summary>
    /// 动画事件：攻击结束
    /// </summary>
    public void OnAttackEnd()
    {
        isAttacking = false;
    }
}
