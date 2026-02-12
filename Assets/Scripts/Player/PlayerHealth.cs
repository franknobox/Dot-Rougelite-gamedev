using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家生命值系统
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("生命值设置")]
    [SerializeField]
    [Tooltip("最大生命值")]
    private float maxHealth = 100f;

    [SerializeField]
    [Tooltip("当前生命值")]
    private float currentHealth;

    [Header("UI 设置")]
    [Tooltip("血条填充图片（绿色 Image）")]
    public Image healthBarFill;

    [Header("无敌帧设置")]
    [SerializeField]
    [Tooltip("受伤后的无敌时间（秒）")]
    private float invincibilityDuration = 1f;

    // 无敌状态
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    // 组件引用
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 初始化生命值
        currentHealth = maxHealth;
        
        // 初始化 UI（确保开局满血显示）
        UpdateHealthUI();
    }

    private void Update()
    {
        // 更新无敌计时器
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                
                // 恢复正常显示
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
            }
            else
            {
                // 闪烁效果
                if (spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                    spriteRenderer.color = new Color(1, 1, 1, alpha);
                }
            }
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        // 无敌状态下不受伤
        if (isInvincible) return;

        // 扣血
        currentHealth -= damage;
        Debug.Log($"玩家受到 {damage} 点伤害，当前生命值: {currentHealth}/{maxHealth}");

        // 更新 UI
        UpdateHealthUI();

        // 触发无敌帧
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // 检查死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    /// <param name="amount">恢复量</param>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"玩家恢复了 {amount} 点生命值，当前生命值: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 完全恢复生命值（用于帐篷回血）
    /// </summary>
    public void HealFull()
    {
        currentHealth = maxHealth;
        Debug.Log("生命值已完全恢复！");
        
        // 更新 UI
        UpdateHealthUI();
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Die()
    {
        Debug.Log("玩家死亡！");
        
        // 触发游戏结束
        if (GameOverManager.instance != null)
        {
            GameOverManager.instance.GameOver();
        }
        
        // 销毁玩家物体 (或者让他只是隐藏/禁用控制)
        Destroy(gameObject);
    }

    /// <summary>
    /// 获取当前生命值百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 获取最大生命值
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 更新 UI 血条显示
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            // 计算血量百分比（确保浮点数除法）
            float fillValue = currentHealth / maxHealth;
            
            // 应用到 UI
            healthBarFill.fillAmount = fillValue;
        }
    }
}
