using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地下城管理器 - 负责房间切换和玩家传送
/// </summary>
public class DungeonManager : MonoBehaviour
{
    // 单例实例
    public static DungeonManager Instance { get; private set; }

    [Header("房间设置")]
    [Tooltip("战斗房间预制体列表")]
    public List<GameObject> combatRoomPrefabs = new List<GameObject>();
    
    [Tooltip("休息房间预制体列表")]
    public List<GameObject> restRoomPrefabs = new List<GameObject>();
    
    [Header("房间生成概率")]
    [Tooltip("休息房出现概率（0-1，默认0.3代蠷30%）")]
    [Range(0f, 1f)]
    public float restRoomProbability = 0.3f;

    [Header("玩家引用")]
    [Tooltip("玩家物体的 Transform")]
    public Transform player;

    [Header("房间切换设置")]
    [Tooltip("切换房间前的延迟时间（秒）")]
    public float transitionDelay = 0.5f;

    // 当前房间实例
    private GameObject currentRoomInstance;

    private void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 验证设置
        if (player == null)
        {
            Debug.LogError("DungeonManager: 未设置玩家引用！");
        }

        if (combatRoomPrefabs.Count == 0)
        {
            Debug.LogWarning("DungeonManager: 战斗房间预制体列表为空！");
        }
        
        if (restRoomPrefabs.Count == 0)
        {
            Debug.LogWarning("DungeonManager: 休息房间预制体列表为空！");
        }

        // 查找场景中已存在的初始房间
        RoomController initialRoom = FindObjectOfType<RoomController>();
        if (initialRoom != null)
        {
            currentRoomInstance = initialRoom.gameObject;
            Debug.Log($"已找到初始房间: {currentRoomInstance.name}");
        }
    }

    /// <summary>
    /// 加载下一个随机房间
    /// </summary>
    public void LoadNextRoom()
    {
        StartCoroutine(LoadNextRoomCoroutine());
    }

    /// <summary>
    /// 加载房间的协程
    /// </summary>
    private IEnumerator LoadNextRoomCoroutine()
    {
        // 第一步：延迟（可选，用于过渡效果）
        yield return new WaitForSeconds(transitionDelay);

        // 第二步：销毁当前房间
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
            Debug.Log("已销毁当前房间");
        }

        // 第三步：根据概率决定房间类型
        float randomValue = 0.3f; // 0.0 到 1.0
        bool isRestRoom = randomValue < restRoomProbability;
        
        List<GameObject> selectedList = isRestRoom ? restRoomPrefabs : combatRoomPrefabs;
        string roomTypeName = isRestRoom ? "休息房" : "战斗房";
        
        // 检查列表是否为空
        if (selectedList.Count == 0)
        {
            Debug.LogError($"无法加载{roomTypeName}：列表为空！");
            yield break;
        }
        
        // 随机选择房间
        int randomIndex = Random.Range(0, selectedList.Count);
        GameObject selectedRoom = selectedList[randomIndex];
        
        currentRoomInstance = Instantiate(selectedRoom);
        Debug.Log($"已加载{roomTypeName}: {selectedRoom.name}");

        // 第四步：通知房间控制器并锁门（在传送玩家之前）
        RoomController roomController = currentRoomInstance.GetComponent<RoomController>();
        if (roomController != null)
        {
            roomController.OnPlayerEnter();
        }
        
        // 第五步：查找 SpawnPoint 并传送玩家
        Transform spawnPoint = currentRoomInstance.transform.Find("SpawnPoint");
        
        if (spawnPoint == null)
        {
            Debug.LogError($"错误：房间 '{selectedRoom.name}' 中未找到名为 'SpawnPoint' 的子物体！请在房间预制体中添加一个名为 'SpawnPoint' 的空物体。");
            yield break;
        }

        if (player != null)
        {
            player.position = spawnPoint.position;
            Debug.Log($"玩家已传送到: {spawnPoint.position}");
        }
        else
        {
            Debug.LogError("无法传送玩家：玩家引用为空！");
        }
    }
}
