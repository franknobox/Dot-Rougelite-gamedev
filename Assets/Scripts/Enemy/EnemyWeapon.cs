using UnityEngine;

/// <summary>
/// 挂载在敌人武器/手部碰撞体上的脚本，用于造成伤害
/// </summary>
public class EnemyWeapon : MonoBehaviour
{
    [Tooltip("造成的伤害值（由AI脚本设置）")]
    public float damage = 10f;

    [Tooltip("是否启用伤害判定（由AI脚本控制）")]
    public bool isDamageEnabled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDamageEnabled) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"{transform.parent.name} 的武器击中了玩家，造成 {damage} 点伤害");
                
                // 击中一次后可以暂时关闭判定，防止单次攻击造成多次伤害（可选）
                // isDamageEnabled = false; 
            }
        }
    }
}
