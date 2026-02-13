using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff 生成器 - 在房间内随机生成 Buff 道具
/// </summary>
public class BuffSpawner : MonoBehaviour
{
    [Header("生成设置")]
    [Tooltip("生成的概率 (0-1)")]
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;

    [Tooltip("可能的生成点位（如果没有指定，则默认使用当前物体位置）")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Tooltip("可生成的 Buff 道具预制体列表")]
    public List<GameObject> buffPrefabs = new List<GameObject>();

    private void Start()
    {
        TrySpawnBuff();
    }

    /// <summary>
    /// 尝试生成 Buff
    /// </summary>
    public void TrySpawnBuff()
    {
        // 1. 检查是否应该生成
        if (Random.value > spawnChance)
        {
            return;
        }

        // 2. 检查是否有可用的预制体
        if (buffPrefabs == null || buffPrefabs.Count == 0)
        {
            Debug.LogWarning("BuffSpawner: 没有配置 Buff 预制体！");
            return;
        }

        // 3. 选择一个随机 Buff
        GameObject selectedBuff = buffPrefabs[Random.Range(0, buffPrefabs.Count)];

        // 4. 选择生成位置
        Vector3 spawnPosition = transform.position;
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
            if (point != null)
            {
                spawnPosition = point.position;
            }
        }

        // 5. 生成实例
        Instantiate(selectedBuff, spawnPosition, Quaternion.identity);
        Debug.Log($"生成了 Buff: {selectedBuff.name} 在 {spawnPosition}");
    }
}
