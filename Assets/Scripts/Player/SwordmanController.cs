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
    [Tooltip("武器数据（可选，用于初始武器）")]
    public WeaponData weaponData;
    
    [Tooltip("武器挂载点（手部位置）")]
    public Transform handTransform;
    
    // 当前武器的脚本引用
    private WeaponController currentWeapon;

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
        
        // 如果没有设置手部挂载点，尝试自动查找
        if (handTransform == null)
        {
            // 尝试查找名为 "Hand" 或 "WeaponMount" 的子物体
            Transform hand = transform.Find("Hand");
            if (hand == null) hand = transform.Find("WeaponMount");
            
            if (hand != null)
            {
                handTransform = hand;
                Debug.Log($"自动找到手部挂载点: {handTransform.name}");
            }
            else
            {
                Debug.LogWarning("SwordmanController: 未设置 handTransform！请在 Inspector 中设置或创建名为 'Hand' 的子物体。");
            }
        }
        
        // 获取子物体中的武器控制器（用于初始武器）
        currentWeapon = GetComponentInChildren<WeaponController>();
        if (currentWeapon == null)
        {
            Debug.LogWarning("SwordmanController: 未找到初始武器！");
        }
    }
    
    private void Start()
    {
        // 初始化初始武器
        if (currentWeapon != null && weaponData != null)
        {
            currentWeapon.Initialize(weaponData);
        }
        else if (currentWeapon != null && weaponData == null)
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
        if (currentWeapon != null)
        {
            currentWeapon.PerformAttack();
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
    
    /// <summary>
    /// 装备新武器（运行时动态装备）
    /// </summary>
    /// <param name="weaponPrefab">武器预制体</param>
    /// <param name="data">武器数据（可选，如果为null则不初始化）</param>
    public void EquipWeapon(GameObject weaponPrefab, WeaponData data = null)
    {
        if (handTransform == null)
        {
            Debug.LogError("SwordmanController: handTransform 未设置，无法装备武器！");
            return;
        }
        
        if (weaponPrefab == null)
        {
            Debug.LogError("SwordmanController: weaponPrefab 为空，无法装备武器！");
            return;
        }
        
        // 清理旧武器：销毁手部挂载点下的所有子物体
        foreach (Transform child in handTransform)
        {
            Destroy(child.gameObject);
            Debug.Log($"已销毁旧武器: {child.name}");
        }
        
        // 生成新武器
        GameObject newWeaponObj = Instantiate(weaponPrefab, handTransform);
        
        // 归位：确保武器贴合手部
        newWeaponObj.transform.localPosition = Vector3.zero;
        newWeaponObj.transform.localRotation = Quaternion.identity;
        
        // 获取武器控制器组件
        currentWeapon = newWeaponObj.GetComponent<WeaponController>();
        
        if (currentWeapon == null)
        {
            Debug.LogError($"SwordmanController: 武器预制体 '{weaponPrefab.name}' 缺少 WeaponController 组件！");
            Destroy(newWeaponObj);
            return;
        }
        
        // 如果提供了武器数据，则初始化
        if (data != null)
        {
            currentWeapon.Initialize(data);
            Debug.Log($"成功装备武器: {weaponPrefab.name}，已使用 WeaponData 初始化");
        }
        else
        {
            Debug.Log($"成功装备武器: {weaponPrefab.name}，使用预制体默认设置");
        }
    }
}
