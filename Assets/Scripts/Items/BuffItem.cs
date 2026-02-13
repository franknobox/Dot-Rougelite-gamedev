using UnityEngine;

/// <summary>
/// 通用 Buff 道具
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BuffItem : MonoBehaviour
{
    [Header("Buff 设置")]
    [Tooltip("Buff 类型")]
    public BuffType buffType;

    [Tooltip("Buff 数值 (生命值/速度倍率/伤害倍率/Dot消耗概率)")]
    public float buffValue;

    [Tooltip("持续时间 (秒)")]
    public float duration;

    [Header("视觉/听觉反馈")]
    [Tooltip("拾取音效")]
    public AudioClip pickupSound;
    
    [Tooltip("音效音量 (0-1)")]
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;

    [Tooltip("拾取特效预制体")]
    public GameObject pickupEffect;

    private void Awake()
    {
        // 确保 Collider 是 Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 尝试获取 PlayerBuffs 组件
            PlayerBuffs playerBuffs = other.GetComponent<PlayerBuffs>();
            if (playerBuffs == null)
            {
                playerBuffs = other.GetComponentInParent<PlayerBuffs>();
            }

            if (playerBuffs != null)
            {
                // 应用 Buff
                playerBuffs.ApplyBuff(buffType, buffValue, duration);
                
                // 播放音效
                if (pickupSound != null)
                {
                    // 优先寻找听者位置，确保通过 2D/3D 混合或纯位置播放能被听到
                    Vector3 playPos = transform.position;
                    AudioListener listener = FindObjectOfType<AudioListener>();
                    if (listener != null)
                    {
                         playPos = listener.transform.position;
                    }
                    else if (Camera.main != null)
                    {
                         playPos = Camera.main.transform.position;
                    }
                    
                    // 只要位置重合，PlayClipAtPoint 就会以最大音量播放（且不受 3D 衰减影响太大，因为距离为0）
                    AudioSource.PlayClipAtPoint(pickupSound, playPos, soundVolume);
                }

                // 播放特效
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // 销毁物体
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("BuffItem: 玩家身上没有 PlayerBuffs 组件！");
            }
        }
    }
}
