using UnityEngine;

/// <summary>
/// 当玩家靠近时显示目标物体（如画布），离开时隐藏
/// </summary>
public class ProximityDisplay : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("需要显示/隐藏的物体（通常是子物体画布）")]
    public GameObject targetObject;

    [Tooltip("显示的触发距离")]
    public float detectionRange = 3f;

    private Transform player;

    private void Start()
    {
        // 查找玩家
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 如果没有指定目标物体，尝试查找第一个子物体（排除自己）
        if (targetObject == null && transform.childCount > 0)
        {
            targetObject = transform.GetChild(0).gameObject;
        }

        // 初始隐藏
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: ProximityDisplay 没有设置目标物体！");
        }
    }

    private void Update()
    {
        if (player == null || targetObject == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool shouldBeVisible = distance <= detectionRange;

        // 直接且高效的状态切换
        if (shouldBeVisible != targetObject.activeSelf)
        {
            targetObject.SetActive(shouldBeVisible);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器中显示检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
