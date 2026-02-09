using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 房间类型枚举
/// </summary>
public enum RoomType
{
    Combat,  // 战斗房
    Rest     // 休息房
}

/// <summary>
/// 房间控制器 - 挂在房间预制体的根节点
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("房间设置")]
    [Tooltip("房间类型")]
    public RoomType roomType = RoomType.Combat;

    [Header("门引用")]
    [Tooltip("房间内所有门的列表")]
    public List<GameObject> doors = new List<GameObject>();

    [Header("敌人生成点")]
    [Tooltip("敌人生成点列表（战斗房使用）")]
    public List<Transform> enemySpawnPoints = new List<Transform>();
    
    [Tooltip("敌人预制体列表（随机选择）")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    // 是否已经进入过房间
    private bool hasEntered = false;
    
    // 当前房间内的敌人列表
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    // 是否已经解锁门
    private bool doorsUnlocked = false;
    
    private void Awake()
    {
        // 如果 doors 列表为空，自动查找房间内所有的 Door 组件
        if (doors.Count == 0)
        {
            Door[] foundDoors = GetComponentsInChildren<Door>();
            foreach (Door door in foundDoors)
            {
                doors.Add(door.gameObject);
            }
        }
    }
    
    private void Start()
    {
        // 如果房间是场景中原有的（不是通过 DungeonManager 加载）
        // 在 Start 中自动初始化
        if (!hasEntered)
        {
            OnPlayerEnter();
        }
    }

    /// <summary>
    /// 当玩家进入房间时调用
    /// </summary>
    public void OnPlayerEnter()
    {
        // 防止重复触发
        if (hasEntered) return;
        hasEntered = true;

        Debug.Log($"玩家进入 {roomType} 类型的房间");

        // 根据房间类型执行不同逻辑
        switch (roomType)
        {
            case RoomType.Combat:
                // 战斗房：锁门并生成敌人
                LockDoors();
                SpawnEnemies();
                break;

            case RoomType.Rest:
                // 休息房：什么都不做
                break;
        }
    }

    /// <summary>
    /// 锁定所有门（战斗时使用）
    /// </summary>
    public void LockDoors()
    {
        if (doors.Count == 0)
        {
            Debug.LogWarning("RoomController: 门列表为空，无法锁定");
            return;
        }
        
        foreach (GameObject doorObj in doors)
        {
            if (doorObj == null) continue;

            Collider2D doorCollider = doorObj.GetComponentInChildren<Collider2D>();
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }
        }
    }

    /// <summary>
    /// 解锁所有门（击败所有敌人后调用）
    /// </summary>
    public void UnlockDoors()
    {
        foreach (GameObject doorObj in doors)
        {
            if (doorObj == null) continue;

            Collider2D doorCollider = doorObj.GetComponentInChildren<Collider2D>();
            if (doorCollider != null)
            {
                doorCollider.enabled = true;
            }
        }

        Debug.Log("所有敌人已被击败，门已解锁！");
    }

    /// <summary>
    /// 生成敌人（战斗房使用）
    /// </summary>
    private void SpawnEnemies()
    {
        // 检测场景中所有标记为 "Enemy" 的敌人（不限于子物体）
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject enemy in allEnemies)
        {
            activeEnemies.Add(enemy);
        }
        
        // 如果设置了敌人预制体和生成点，额外生成敌人
        if (enemyPrefabs.Count > 0 && enemySpawnPoints.Count > 0)
        {
            foreach (Transform spawnPoint in enemySpawnPoints)
            {
                if (spawnPoint == null) continue;

                int randomIndex = Random.Range(0, enemyPrefabs.Count);
                GameObject enemyPrefab = enemyPrefabs[randomIndex];
                
                if (enemyPrefab == null) continue;

                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                enemy.transform.SetParent(transform);
                activeEnemies.Add(enemy);
            }
        }
        
        // 检查是否有敌人
        if (activeEnemies.Count == 0)
        {
            UnlockDoors();
        }
    }

    private void Update()
    {
        // 如果是战斗房且门未解锁，检查敌人数量
        if (roomType == RoomType.Combat && !doorsUnlocked && hasEntered)
        {
            // 清理已经被销毁的敌人引用
            int beforeCount = activeEnemies.Count;
            activeEnemies.RemoveAll(enemy => enemy == null);
            
            // 如果所有敌人都死了，解锁门
            if (activeEnemies.Count == 0 && beforeCount > 0)
            {
                OnAllEnemiesDefeated();
            }
        }
    }
    
    /// <summary>
    /// 当所有敌人被击败时调用（由敌人管理系统调用）
    /// </summary>
    public void OnAllEnemiesDefeated()
    {
        Debug.Log("所有敌人已被击败！");
        UnlockDoors();
        doorsUnlocked = true;
    }

    /// <summary>
    /// 获取房间内剩余敌人数量（供外部查询）
    /// </summary>
    public int GetRemainingEnemyCount()
    {
        // 清理已经被销毁的敌人引用
        activeEnemies.RemoveAll(enemy => enemy == null);
        return activeEnemies.Count;
    }
}
