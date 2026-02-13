using System.Collections;
using System.Collections;
using UnityEngine;
using Spine.Unity;

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

    [Header("得分设置")]
    [SerializeField]
    [Tooltip("击杀此敌人获得的分数")]
    protected int scoreValue = 10;

    [Header("音效设置")]
    [SerializeField]
    [Tooltip("受击音效（可选）")]
    protected AudioClip hitSound;

    [SerializeField]
    [Tooltip("死亡音效（可选）")]
    protected AudioClip dieSound;

    // 当前生命值
    protected float currentHealth;
    
    // 组件引用（从子物体中查找，支持 Enemy/Visuals 分离）
    protected SpriteRenderer spriteRenderer;
    // Spine支持
    protected SkeletonMecanim skeletonMecanim;
    
    private Color originalColor;
    private Color spineOriginalColor = Color.white; // Spine 默认通常是 White

    protected virtual void Awake()
    {
        // 从子物体中查找 SpriteRenderer（支持 Visuals 分离）
        // 从子物体中查找 SpriteRenderer（支持 Visuals 分离）
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 尝试查找 Spine 组件
        skeletonMecanim = GetComponentInChildren<SkeletonMecanim>();
        if (skeletonMecanim == null)
        {
            // 如果两个都没有找到，才报警
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"{gameObject.name}: 未找到 SpriteRenderer 或 SkeletonMecanim 组件，受击闪白效果将不可用");
            }
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

        // 播放受击音效
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

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
        // 变红 (因为原图通常是白色的，闪白看不出来)
        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        if (skeletonMecanim != null) skeletonMecanim.skeleton.SetColor(Color.red);
        
        // 等待
        yield return new WaitForSeconds(flashDuration);
        
        // 恢复原色
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (skeletonMecanim != null) skeletonMecanim.skeleton.SetColor(spineOriginalColor);
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} 已死亡！");

        // 播放死亡音效
        if (dieSound != null)
        {
            AudioSource.PlayClipAtPoint(dieSound, transform.position);
        }

        // 增加分数
        if (GameOverManager.instance != null)
        {
            GameOverManager.instance.AddScore(scoreValue);
        }

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
