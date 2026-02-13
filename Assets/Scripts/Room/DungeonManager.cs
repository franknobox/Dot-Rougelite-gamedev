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
    
    // 是否正在加载房间（防止重复触发）
    private bool isLoadingRoom = false;
    
    // 上一个房间的类型（用于防止连续休息房）
    private RoomType lastRoomType = RoomType.Combat;

    [Header("背景音乐设置")]
    [Tooltip("播放 BGM 的 AudioSource")]
    public AudioSource bgmAudioSource;
    [Tooltip("战斗时的 BGM")]
    public AudioClip combatBGM;
    [Tooltip("休息时的 BGM")]
    public AudioClip restBGM;
    [Tooltip("BGM 切换过渡时间")]
    public float bgmFadeDuration = 1.0f;

    private Coroutine bgmFadeCoroutine;

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
        // 防止重复加载
        if (isLoadingRoom)
        {
            Debug.LogWarning("正在加载房间中，请稍候...");
            return;
        }
        
        StartCoroutine(LoadNextRoomCoroutine());
    }

    /// <summary>
    /// 加载房间的协程
    /// </summary>
    private IEnumerator LoadNextRoomCoroutine()
    {
        // 设置加载锁
        isLoadingRoom = true;
        
        // 第一步：延迟（可选，用于过渡效果）
        yield return new WaitForSeconds(transitionDelay);

        // 第二步：销毁当前房间
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
            Debug.Log("已销毁当前房间");
            // 等待一帧确保销毁完成
            yield return null;
        }

        // 第三步：根据概率决定房间类型
        float randomValue = Random.value; // 生成 0.0 到 1.0 之间的随机数
        bool isRestRoom = randomValue < restRoomProbability;
        
        // 防止连续出现休息房
        if (lastRoomType == RoomType.Rest && isRestRoom)
        {
            isRestRoom = false; // 强制改为战斗房
            Debug.Log("上一个房间是休息房，本次生成战斗房");
        }
        
        List<GameObject> selectedList = isRestRoom ? restRoomPrefabs : combatRoomPrefabs;
        string roomTypeName = isRestRoom ? "休息房" : "战斗房";
        
        // 检查列表是否为空
        if (selectedList.Count == 0)
        {
            Debug.LogError($"无法加载{roomTypeName}：列表为空！");
            isLoadingRoom = false; // 释放加载锁
            yield break;
        }
        
        // 随机选择房间
        int randomIndex = Random.Range(0, selectedList.Count);
        GameObject selectedRoom = selectedList[randomIndex];
        
        // 检查预制体是否有效
        if (selectedRoom == null)
        {
            Debug.LogError($"错误：{roomTypeName}列表中索引 {randomIndex} 的预制体为空！");
            isLoadingRoom = false; // 释放加载锁
            yield break;
        }
        
        currentRoomInstance = Instantiate(selectedRoom);
        Debug.Log($"已加载{roomTypeName}: {selectedRoom.name}");

        // 切换 BGM
        if (bgmAudioSource != null)
        {
            AudioClip targetBGM = isRestRoom ? restBGM : combatBGM;
            PlayBGM(targetBGM);
        }
        
        // 记录当前房间类型
        lastRoomType = isRestRoom ? RoomType.Rest : RoomType.Combat;

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
            isLoadingRoom = false; // 释放加载锁
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
        
        // 释放加载锁
        isLoadingRoom = false;
    }

    /// <summary>
    /// 播放 BGM (带淡入淡出)
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmAudioSource == null) return;
        
        // 如果正在播放同一首曲子，则不切换
        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying) return;

        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(CrossFadeBGM(clip));
    }

    private IEnumerator CrossFadeBGM(AudioClip newClip)
    {
        float timer = 0f;
        float startVolume = bgmAudioSource.volume;

        // 淡出
        if (bgmAudioSource.isPlaying)
        {
            while (timer < bgmFadeDuration / 2)
            {
                timer += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, timer / (bgmFadeDuration / 2));
                yield return null;
            }
        }

        // 切换 Clip
        bgmAudioSource.clip = newClip;
        bgmAudioSource.Play();

        // 淡入
        timer = 0f;
        while (timer < bgmFadeDuration / 2)
        {
            timer += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(0f, 1f, timer / (bgmFadeDuration / 2)); // 假设最大音量为1
            yield return null;
        }

        bgmAudioSource.volume = 1f;
    }
}
