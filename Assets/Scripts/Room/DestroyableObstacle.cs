using UnityEngine;

/// <summary>
/// 可破坏障碍物脚本
/// 挂在障碍物（如木桶、箱子、草丛）上，使其具有生命值，受击后销毁。
/// </summary>
public class DestroyableObstacle : MonoBehaviour
{
    [Header("属性设置")]
    [Tooltip("最大生命值（或受击次数）")]
    public float maxHealth = 3f;

    [Tooltip("是否掉落物品")]
    public bool dropItem = true;

    [Tooltip("掉落物预制体（可选）")]
    public GameObject dropItemPrefab;

    [Header("特效与音效")]
    [Tooltip("受击时的音效（可选）")]
    public AudioClip hitSound;

    [Tooltip("销毁时的音效（可选）")]
    public AudioClip destroySound;

    [Tooltip("销毁时的特效（可选）")]
    public GameObject destroyEffect;

    [Header("闪白效果")]
    [Tooltip("受击闪白持续时间")]
    public float flashDuration = 0.1f;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        // 尝试获取自身或子物体上的 SpriteRenderer
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // 处理 Trigger 碰撞 (例如子弹，或 Is Trigger = True 的近战武器)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    // 处理物理碰撞 (例如 Is Trigger = False 的物理物体)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    /// <summary>
    /// 统一处理碰撞逻辑
    /// </summary>
    private void HandleCollision(GameObject other)
    {
        // 检测玩家武器 (近战)
        if (other.CompareTag("Weapon"))
        {
            TakeDamage(1f); // 默认扣1血
            // 可以根据需要获取 WeaponController 来决定伤害值，这里简化处理
        }
        // 检测玩家子弹 (远程)
        else if (other.CompareTag("Projectile"))
        {
            TakeDamage(1f);
            Destroy(other); // 销毁子弹
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="amount">伤害数值</param>
    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} 受到伤害，剩余生命: {currentHealth}");

        // 播放受击音效
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        // 播放闪白
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashWhite());
        }

        // 检查销毁
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        // 改为红色闪烁，因为默认是非 Tint 的 White，闪白看不出来
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// 销毁逻辑
    /// </summary>
    private void Die()
    {
        // 播放销毁音效
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        // 生成销毁特效
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        // 掉落物品
        if (dropItem && dropItemPrefab != null)
        {
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        }

        // 销毁物体
        Destroy(gameObject);
    }
}
