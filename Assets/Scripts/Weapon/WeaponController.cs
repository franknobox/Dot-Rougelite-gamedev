using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器攻击类型枚举
/// </summary>
public enum WeaponAttackType
{
    Melee,   // 近战（碰撞体检测）
    Ranged   // 远程（发射投射物）
}

/// <summary>
/// 武器控制器 - 支持近战和远程武器
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("武器类型")]
    [Tooltip("武器攻击类型：近战或远程")]
    public WeaponAttackType attackType = WeaponAttackType.Melee;
    
    [Header("武器数据")]
    private WeaponData weaponData;
    
    [Header("近战武器设置")]
    [Tooltip("武器的碰撞体，用于近战武器检测敌人碰撞")]
    public Collider2D weaponCollider;
    
    [Header("远程武器设置")]
    [Tooltip("投射物预制体，用于远程武器")]
    public GameObject projectilePrefab;
    
    [Tooltip("发射点位置（可选，默认为武器当前位置）")]
    public Transform firePoint;
    
    [Header("攻击设置")]
    [Tooltip("攻击延迟（碰撞体开启前的等待时间）")]
    public float attackDelay = 0.1f;
    [SerializeField] private float attackDuration = 0.2f;
    
    // 当前耐久度
    public int CurrentDurability { get; private set; }
    
    // 当前攻击已命中的敌人列表（防止重复伤害，仅用于近战）
    private List<EnemyBase> hitEnemies = new List<EnemyBase>();

    // Buff系统引用
    private PlayerBuffs playerBuffs;

    private void Awake()
    {
        // 获取 PlayerBuffs 引用 (通常在父物体 Player 上)
        playerBuffs = GetComponentInParent<PlayerBuffs>();
        if (playerBuffs == null)
        {
            // 如果不在父物体上，尝试在当前物体或子物体找（比如手持的情况）
            playerBuffs = GetComponent<PlayerBuffs>();
            if (playerBuffs == null && transform.root != null)
            {
                playerBuffs = transform.root.GetComponent<PlayerBuffs>();
            }
        }
        // 只有近战武器需要禁用碰撞体
        if (attackType == WeaponAttackType.Melee)
        {
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }
            else
            {
                Debug.LogWarning($"[WeaponController] 近战武器未设置 weaponCollider！");
            }
        }
    }

    /// <summary>
    /// 初始化武器
    /// </summary>
    /// <param name="data">武器数据</param>
    public void Initialize(WeaponData data)
    {
        weaponData = data;
        CurrentDurability = data.maxDurability;
        Debug.Log($"[WeaponController] 武器已初始化: {data.weaponName}, 类型: {attackType}, 伤害: {data.baseDamage}, 耐久: {CurrentDurability}");
    }

    /// <summary>
    /// 获取攻击速率 (间隔时间)
    /// </summary>
    public float GetAttackRate()
    {
        return weaponData != null ? weaponData.attackRate : 0.5f; // 默认 0.5s
    }


    /// <summary>
    /// 执行攻击（统一接口）
    /// </summary>
    /// <summary>
    /// 执行攻击（统一接口）
    /// </summary>
    /// <param name="direction">攻击方向 (可选，默认为空则使用默认方向)</param>
    public void PerformAttack(Vector2? direction = null)
    {
        if (weaponData == null)
        {
            Debug.LogWarning("[WeaponController] 武器未初始化，无法攻击！");
            return;
        }
        
        // 根据武器类型执行对应的攻击逻辑
        switch (attackType)
        {
            case WeaponAttackType.Melee:
                PerformMeleeAttack();
                break;
                
            case WeaponAttackType.Ranged:
                PerformRangedAttack(direction);
                break;
        }
    }

    /// <summary>
    /// 执行近战攻击
    /// </summary>
    private void PerformMeleeAttack()
    {
        Debug.Log($"[WeaponController] 执行近战攻击");
        
        // 清空已命中敌人列表
        hitEnemies.Clear();
        
        // 启用武器碰撞体 (使用协程处理延迟和持续时间)
        if (weaponCollider != null)
        {
            StartCoroutine(MeleeAttackCoroutine());
        }
        else
        {
            Debug.LogError($"[WeaponController] 近战武器的 weaponCollider 未设置！");
            return;
        }
        
        // 扣除耐久度
        DecrementDurability();
    }

    /// <summary>
    /// 近战攻击协程：延迟 -> 开启碰撞体 -> 持续 -> 关闭碰撞体
    /// </summary>
    private IEnumerator MeleeAttackCoroutine()
    {
        // 1. 等待攻击延迟 (Wind up)
        if (attackDelay > 0f)
        {
            yield return new WaitForSeconds(attackDelay);
        }

        // 2. 开启碰撞体
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }

        // 3. 等待攻击持续时间 (Active)
        yield return new WaitForSeconds(attackDuration);

        // 4. 关闭碰撞体
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    /// <summary>
    /// 执行远程攻击
    /// </summary>
    /// <summary>
    /// 执行远程攻击
    /// </summary>
    private void PerformRangedAttack(Vector2? direction)
    {
        Debug.Log($"[WeaponController] 执行远程攻击");
        
        // 检查投射物预制体
        if (projectilePrefab == null)
        {
            Debug.LogError("[WeaponController] 远程武器的 projectilePrefab 未设置！");
            return;
        }
        
        // 确定发射位置
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        
        // 计算发射方向
        Vector2 fireDirection;
        if (direction.HasValue)
        {
            // 如果指定了方向（例如鼠标指向），则使用该方向
            fireDirection = direction.Value.normalized;
        }
        else
        {
            // 否则使用默认朝向（玩家面朝向）
            fireDirection = transform.parent != null ? 
                new Vector2(transform.parent.localScale.x > 0 ? -1 : 1, 0) : 
                Vector2.right;
        }
        
        // 实例化投射物
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        // 初始化投射物
        ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
        if (projectileController != null)
        {
            // 初始化投射物 (传入 tag 和方向)
            // 计算最终伤害
            float damageMultiplier = playerBuffs != null ? playerBuffs.damageMultiplier : 1.0f;
            projectileController.Initialize(weaponData, fireDirection, damageMultiplier);
        }
        else
        {
            Debug.LogError("[WeaponController] 投射物预制体缺少 ProjectileController 组件！");
            Destroy(projectile);
            return;
        }
        
        // 扣除耐久度
        DecrementDurability();
    }

    /// <summary>
    /// 扣除耐久度并检查是否损坏
    /// </summary>
    private void DecrementDurability()
    {
        CurrentDurability--;
        Debug.Log($"[WeaponController] 武器耐久度: {CurrentDurability}/{weaponData.maxDurability}");
        
        // 检查耐久度是否耗尽
        if (CurrentDurability <= 0)
        {
            BreakWeapon();
        }
    }

    /// <summary>
    /// 在攻击持续时间后禁用武器碰撞体
    /// </summary>
    // DisableWeaponCollider 已被合并到 MeleeAttackCoroutine 中，可以移除或保留为空

    /// <summary>
    /// 碰撞检测 - 当武器碰到敌人时触发（仅用于近战武器）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只有近战武器才处理碰撞
        if (attackType != WeaponAttackType.Melee) return;
        
        Debug.Log($"[WeaponController] OnTriggerEnter2D - 碰到了: {other.gameObject.name}, Tag: {other.tag}");
        
        // 检查是否碰到敌人
        if (!other.CompareTag("Enemy"))
        {
            Debug.Log($"[WeaponController] 不是敌人，忽略");
            return;
        }
        
        // 获取敌人的 EnemyBase 组件
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogWarning($"[WeaponController] {other.gameObject.name} 没有 EnemyBase 组件！");
            return;
        }
        
        // 防止重复伤害：检查这个敌人是否已经被这次攻击命中过
        if (hitEnemies.Contains(enemy))
        {
            Debug.Log($"[WeaponController] {enemy.gameObject.name} 已经被这次攻击命中过，忽略");
            return;
        }
        
        Debug.Log($"[WeaponController] 命中敌人: {enemy.gameObject.name}, 造成伤害: {weaponData.baseDamage}, 类型: {weaponData.damageType}");
        
        Debug.Log($"[WeaponController] 命中敌人: {enemy.gameObject.name}, 造成伤害: {weaponData.baseDamage}, 类型: {weaponData.damageType}");
        
        // 计算伤害 (应用 Buff)
        float damageMultiplier = playerBuffs != null ? playerBuffs.damageMultiplier : 1.0f;
        float finalDamage = weaponData.baseDamage * damageMultiplier;

        // 对敌人造成伤害
        enemy.TakeDamage(finalDamage, weaponData.damageType);
        
        // 将敌人加入已命中列表
        hitEnemies.Add(enemy);
        
        // TODO: 播放打击音效
        // AudioSource.PlayClipAtPoint(hitSound, transform.position);
    }

    /// <summary>
    /// 武器损坏时触发
    /// </summary>
    private void BreakWeapon()
    {
        Debug.Log($"[WeaponController] 武器损坏！掉落 {weaponData.dropDotCount} 个点数");
        
        // 掉落点数
        if (GridManager.Instance != null)
        {
            GridManager.Instance.AddDots(weaponData.dropDotCount);
        }
        
        // 销毁武器
        Destroy(gameObject);
    }
}
