using UnityEngine;

/// <summary>
/// 休息帐篷 - 玩家接触后完全恢复生命值
/// </summary>
public class RestTent : MonoBehaviour
{
    [Header("帐篷设置")]
    [Tooltip("是否已被使用")]
    public bool isUsed = false;

    [Header("特效设置")]
    [Tooltip("回血特效预制体（可选）")]
    public GameObject healEffectPrefab;

    [Header("音效设置")]
    [Tooltip("回血音效（可选）")]
    public AudioClip healSound;

    // 组件引用
    private AudioSource audioSource;
    private bool isPlayerInside = false;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        // 获取或添加 AudioSource 组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && healSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测玩家进入
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerHealth = collision.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                Debug.Log("玩家进入帐篷，准备休息...");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // 玩家在帐篷内时，执行回血
        if (collision.CompareTag("Player") && isPlayerInside && playerHealth != null)
        {
            // 执行完全回血
            playerHealth.HealFull();

            // 播放回血特效
            if (healEffectPrefab != null)
            {
                GameObject effect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f); // 2秒后销毁特效
            }

            // 播放回血音效
            if (audioSource != null && healSound != null)
            {
                audioSource.PlayOneShot(healSound);
            }

            Debug.Log("休息中... 生命值已恢复！");

            // 标记为已使用（如果需要限制使用次数，可以在这里添加逻辑）
            // isUsed = true;

            // 重置状态，避免重复触发
            isPlayerInside = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 玩家离开帐篷
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerHealth = null;
            Debug.Log("玩家离开帐篷");
        }
    }

    /// <summary>
    /// 重置帐篷状态（用于重新启用帐篷）
    /// </summary>
    public void ResetTent()
    {
        isUsed = false;
        isPlayerInside = false;
        playerHealth = null;
        Debug.Log("帐篷已重置");
    }
}
