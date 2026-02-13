using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff 类型枚举
/// </summary>
public enum BuffType
{
    MaxHealth,      // 增加最大生命值
    MoveSpeed,      // 增加移动速度
    AttackDamage,   // 增加攻击力
    DotConsumption  // 减少 Dot 消耗 (减半)
}

/// <summary>
/// 玩家 Buff 管理器
/// </summary>
public class PlayerBuffs : MonoBehaviour
{
    [Header("Buff 状态")]
    public float damageMultiplier = 1.0f;
    public float dotConsumptionChance = 1.0f; // 1.0 = 正常消耗, 0.5 = 50% 概率消耗

    // 内部引用
    private SwordmanController playerController;
    private PlayerHealth playerHealth;

    // 原始值记录
    private float originalMoveSpeed;
    
    // 协程字典，用于管理不同类型的 Buff 计时器
    private Dictionary<BuffType, Coroutine> activeBuffCoroutines = new Dictionary<BuffType, Coroutine>();

    private void Awake()
    {
        playerController = GetComponent<SwordmanController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        if (playerController != null)
        {
            originalMoveSpeed = playerController.moveSpeed;
        }
    }

    /// <summary>
    /// 应用 Buff
    /// </summary>
    /// <param name="type">Buff 类型</param>
    /// <param name="value">数值 (生命值/倍率)</param>
    /// <param name="duration">持续时间</param>
    public void ApplyBuff(BuffType type, float value, float duration)
    {
        // 如果该类型的 Buff 已经在运行，停止旧的协程 (刷新持续时间)
        if (activeBuffCoroutines.ContainsKey(type) && activeBuffCoroutines[type] != null)
        {
            StopCoroutine(activeBuffCoroutines[type]);
            RevertBuff(type); // 先恢复，再重新应用，避免叠加错误
        }

        // 应用新 Buff
        switch (type)
        {
            case BuffType.MaxHealth:
                ApplyMaxHealthBuff(value);
                break;
            case BuffType.MoveSpeed:
                ApplyMoveSpeedBuff(value);
                break;
            case BuffType.AttackDamage:
                ApplyDamageBuff(value);
                break;
            case BuffType.DotConsumption:
                ApplyDotConsumptionBuff(value); // value通常为0.5
                break;
        }

        // 启动倒计时
        Coroutine coroutine = StartCoroutine(BuffTimer(type, duration));
        activeBuffCoroutines[type] = coroutine;

        Debug.Log($"应用 Buff: {type}, 值: {value}, 持续: {duration}秒");
    }

    private IEnumerator BuffTimer(BuffType type, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        RevertBuff(type);
        activeBuffCoroutines.Remove(type);
        
        Debug.Log($"Buff 结束: {type}");
    }


    // --- 具体应用逻辑 ---

    // 存储 Buff 值以便 Revert 使用
    private Dictionary<BuffType, float> activeBuffValues = new Dictionary<BuffType, float>();

    private void ApplyMaxHealthBuff(float amount)
    {
        if (playerHealth != null)
        {
            activeBuffValues[BuffType.MaxHealth] = amount;
            playerHealth.ModifyMaxHealth(amount);
        }
    }
    
    private void RevertMaxHealth()
    {
        if (playerHealth != null && activeBuffValues.ContainsKey(BuffType.MaxHealth))
        {
            float amount = activeBuffValues[BuffType.MaxHealth];
            playerHealth.ModifyMaxHealth(-amount);
            activeBuffValues.Remove(BuffType.MaxHealth);
        }
    }

    private void ApplyMoveSpeedBuff(float multiplier)
    {
        if (playerController != null)
        {
            playerController.moveSpeed = originalMoveSpeed * multiplier;
        }
    }

    private void ApplyDamageBuff(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    private void ApplyDotConsumptionBuff(float chance)
    {
        dotConsumptionChance = chance;
    }
    
    private void RevertBuff(BuffType type) 
    {
         switch (type)
        {
            case BuffType.MaxHealth:
                RevertMaxHealth();
                break;
            case BuffType.MoveSpeed:
                if (playerController != null) playerController.moveSpeed = originalMoveSpeed;
                break;
            case BuffType.AttackDamage:
                damageMultiplier = 1.0f;
                break;
            case BuffType.DotConsumption:
                dotConsumptionChance = 1.0f;
                break;
        }
    }
}
