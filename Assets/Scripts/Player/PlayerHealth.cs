using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

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
    private SkeletonRenderer skeletonRenderer; // Spine 支持
    private Animator animator;

    private void Awake()
    {
        // 查找视觉组件 (可能在子物体 Visuals 上)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        skeletonRenderer = GetComponentInChildren<SkeletonRenderer>(); // 兼容 SkeletonAnimation 和 SkeletonMecanim
        animator = GetComponentInChildren<Animator>();
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
                if (skeletonRenderer != null)
                {
                    skeletonRenderer.skeleton.SetColor(Color.white);
                }
            }
            else
            {
                // 闪烁效果
                float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                Color flashColor = new Color(1, 1, 1, alpha);
                
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = flashColor;
                }
                if (skeletonRenderer != null)
                {
                    skeletonRenderer.skeleton.SetColor(flashColor);
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
    /// 修改最大生命值 (用于 Buff)
    /// </summary>
    /// <param name="amount">增加的数值 (负数则减少)</param>
    public void ModifyMaxHealth(float amount)
    {
        maxHealth += amount;
        
        // 增加最大生命值时，通常也增加当前生命值（或者是保持百分比，这里选择直接增加）
        if (amount > 0)
        {
            currentHealth += amount;
        }
        else
        {
            // 减少最大生命值时，如果当前生命值超过上限，则截断
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
        
        Debug.Log($"最大生命值变为: {maxHealth}，当前生命值: {currentHealth}");
        UpdateHealthUI();
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Die()
    {
        Debug.Log("玩家死亡！");
        
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 禁用玩家控制
        SwordmanController controller = GetComponent<SwordmanController>();
        if (controller == null) controller = GetComponentInChildren<SwordmanController>();
        if (controller != null) controller.enabled = false;

        // 禁用碰撞体
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 移除刚体物理影响
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;

        // 启动协程等待动画播完
        StartCoroutine(WaitAndGameOver());
    }

    private System.Collections.IEnumerator WaitAndGameOver()
    {
        // 默认等待时间（以防获取不到动画时长）
        float waitTime = 2f;

        if (animator != null)
        {
            // 等待一帧，确保 Animator 进入了 Die 状态
            yield return null;
            
            // 获取当前动画状态信息 (Layer 0)
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            
            // 如果确实在播放 Die 动画（根据 Tag 或者名字，这里简单判断只要不是 invalid）
            // 注意：有时切换需要时间，或者 Die 可能是个 BlendTree。
            // 更稳妥的方式是直接等待一个固定时间，或者检查 clip info。
            // 这里我们尝试获取当前 clip 长度。
            
            if (info.IsName("Die") || info.IsTag("Die"))
            {
                waitTime = info.length;
            }
            else
            {
                // 如果还没切换过去（Transition中），尝试获取下一个状态信息
                AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
                if (nextInfo.IsName("Die") || nextInfo.IsTag("Die"))
                {
                    waitTime = nextInfo.length;
                }
            }
        }

        Debug.Log($"等待死亡动画播放: {waitTime} 秒");
        yield return new WaitForSeconds(waitTime);

        // 触发游戏结束
        if (GameOverManager.instance != null)
        {
            GameOverManager.instance.GameOver();
        }

        // 销毁玩家物体 (可选，如果不想让尸体留在结算界面后)
        // Destroy(gameObject);
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
