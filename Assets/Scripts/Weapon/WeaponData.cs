using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器数据配置 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("基础字段")]
    [Tooltip("武器名称")]
    public string weaponName;
    
    [Tooltip("武器图标")]
    public Sprite icon;
    
    [Tooltip("武器预制体")]
    public GameObject weaponPrefab;

    [Header("战斗字段")]
    [Tooltip("伤害类型")]
    public DamageType damageType;
    
    [Tooltip("基础伤害")]
    public float baseDamage;
    
    [Tooltip("攻击速率 (每次攻击的间隔时间)")]
    public float attackRate;

    [Tooltip("子弹飞行速度 (仅远程武器有效)")]
    public float projectileSpeed = 10f;
    
    [Tooltip("最大耐久度")]
    public int maxDurability;

    [Header("拼装字段")]
    [Tooltip("5x5网格中的形状模式")]
    public List<Vector2Int> shapePattern = new List<Vector2Int>();

    [Header("回收字段")]
    [Tooltip("耐久耗尽时掉落的点数")]
    public int dropDotCount;
}
