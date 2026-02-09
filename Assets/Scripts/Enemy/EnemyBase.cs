using System.Collections;
using UnityEngine;

/// <summary>
/// 敌人基类 - 所有敌人的父类
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("基础属性")]
    [SerializeField]
    [Tooltip("最大生命值")]
    protected float maxHealth = 100f;
    
    [SerializeField]
    [Tooltip("移动速度")]
    protected float moveSpeed = 3f;
    
    [SerializeField]
    [Tooltip("弱点类型（克制伤害翻倍）")]
    protected DamageType weaknessType = DamageType.Blunt;

    [Header("掉落设置")]
    [SerializeField]
    [Tooltip("Dot 掉落物预制体")]
    protected GameObject dotDropPrefab;
    
    [SerializeField]
    [Tooltip("死亡时掉落的 Dot 数量")]
    protected int dropCount = 3;

    [Header("受击效果")]
    [SerializeField]
    [Tooltip("受击闪白持续时间")]
    protected float flashDuration = 0.1f;

    // 当前生命值
    protected float currentHealth;
    
    // 组件引用（从子物体中查找，支持 Enemy/Visuals 分离）
    protected SpriteRenderer spriteRenderer;
    private Color originalColor;

    protected virtual void Awake()
    {
        // 从子物体中查找 SpriteRenderer（支持 Visuals 分离）
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: 未找到 SpriteRenderer 组件，受击闪白效果将不可用");
        }
    }

    protected virtual void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">基础伤害值</param>
    /// <param name="type">伤害类型</param>
    public virtual void TakeDamage(float damage, DamageType type)
    {
        // 克制判定
        float finalDamage = damage;
        if (type == weaknessType)
        {
            finalDamage *= 2.0f; // 克制伤害翻倍
            Debug.Log($"CRITICAL! {gameObject.name} 受到克制伤害: {finalDamage}");
        }
        else
        {
            Debug.Log($"{gameObject.name} 受到普通伤害: {finalDamage}");
        }

        // 扣血
        currentHealth -= finalDamage;
        Debug.Log($"{gameObject.name} 当前生命值: {currentHealth}/{maxHealth}");

        // 播放受击闪白效果
        StartCoroutine(FlashWhite());

        // 检查死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 受击闪白效果
    /// </summary>
    protected virtual IEnumerator FlashWhite()
    {
        if (spriteRenderer == null) yield break;

        // 变白
        spriteRenderer.color = Color.white;
        
        // 等待
        yield return new WaitForSeconds(flashDuration);
        
        // 恢复原色
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} 已死亡！");

        // 掉落 Dot
        DropDots();

        // 销毁物体
        Destroy(gameObject);
    }

    /// <summary>
    /// 掉落 Dot 资源
    /// </summary>
    protected virtual void DropDots()
    {
        if (dotDropPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name} 没有设置 dotDropPrefab，无法掉落！");
            return;
        }

        // 循环生成掉落物
        for (int i = 0; i < dropCount; i++)
        {
            // 添加随机偏移，避免叠在一起
            Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
            Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            // 实例化掉落物
            Instantiate(dotDropPrefab, dropPosition, Quaternion.identity);
        }

        Debug.Log($"{gameObject.name} 掉落了 {dropCount} 个 Dot");
    }

    /// <summary>
    /// 获取当前生命值百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
