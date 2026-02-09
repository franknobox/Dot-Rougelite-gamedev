using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 网格管理器 - 5x5背包系统，用于合成 Roguelite 武器
/// </summary>
public class GridManager : MonoBehaviour
{
    private const int GRID_SIZE = 5;
    
    public static GridManager Instance { get; private set; }

    [Header("网格预制体设置")]
    [Tooltip("格子预制体")]
    public GameObject slotPrefab;
    
    [Tooltip("格子的父节点容器")]
    public Transform gridContainer;

    [Header("武器配方")]
    [Tooltip("所有可能的武器配方 ScriptableObject")]
    public List<WeaponData> allWeaponRecipes = new List<WeaponData>();

    [Header("资源管理")]
    [Tooltip("当前拥有的Dot资源总量")]
    public int currentDots = 10;
    
    [Tooltip("显示Dot数量的UI文本")]
    public TextMeshProUGUI dotCountText;
    
    [Tooltip("确认按钮引用")]
    public Button confirmButton;

    [Header("UI开关")]
    [Tooltip("组装面板父节点")]
    public GameObject assemblyPanel;
    
    [Tooltip("切换UI的按键")]
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("网格数据")]
    [Tooltip("5x5格子的二维数组，用于通过坐标访问")]
    private GridSlot[,] slotMatrix = new GridSlot[5, 5];
    
    [Tooltip("逻辑数据层，true代表有点")]
    private bool[,] logicalGrid = new bool[5, 5];
    
    // UI状态标记
    private bool isUIOpen = false;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 临时激活面板以确保网格生成正常工作
        // 即使在编辑器中面板是隐藏的，也能正常初始化
        if (assemblyPanel != null)
        {
            assemblyPanel.SetActive(true);
        }
        
        GenerateGrid();
        
        // 初始化UI
        UpdateUI();
        
        // 绑定确认按钮事件
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmPressed);
        }
        
        // 默认关闭UI
        CloseUI();
    }
    
    private void Update()
    {
        // 检测切换按键
        if (Input.GetKeyDown(toggleKey))
        {
            if (isUIOpen)
            {
                CloseUI();
            }
            else
            {
                OpenUI();
            }
        }
    }

    /// <summary>
    /// 生成5x5网格
    /// </summary>
    private void GenerateGrid()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("slotPrefab 未分配！请在Inspector中拖入格子预制体。");
            return;
        }

        if (gridContainer == null)
        {
            Debug.LogError("gridContainer 未分配！请在Inspector中拖入格子的父节点。");
            return;
        }

        // 清空容器中已有的子物体
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // 双重循环生成5x5网格
        // 注意：(0,0) 对应左上角第一个格子（Grid Layout Group的默认布局）
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                // 实例化格子预制体到容器下
                GameObject slotObj = Instantiate(slotPrefab, gridContainer);
                
                // 设置物体名称，方便调试
                slotObj.name = $"Slot_{x}_{y}";

                // 获取 GridSlot 组件
                GridSlot slot = slotObj.GetComponent<GridSlot>();
                
                if (slot != null)
                {
                    // 初始化槽位坐标
                    slot.Initialize(x, y);
                    
                    // 存入二维数组以便管理
                    slotMatrix[x, y] = slot;
                }
                else
                {
                    Debug.LogError($"预制体 {slotPrefab.name} 上没有 GridSlot 组件！");
                }

                // 初始化逻辑网格为空
                logicalGrid[x, y] = false;
            }
        }

        Debug.Log("5x5 网格生成完成！");
    }

    /// <summary>
    /// 槽位点击事件处理（支持放置和悔棋）
    /// </summary>
    /// <param name="slot">被点击的槽位</param>
    public void OnSlotClicked(GridSlot slot)
    {
        int x = slot.coordinate.x;
        int y = slot.coordinate.y;

        // 边界检查
        if (!IsValidCoordinate(x, y)) return;

        // 逻辑分支：根据格子当前状态决定操作
        if (!logicalGrid[x, y])
        {
            // 格子是空的：尝试放置 Dot
            if (currentDots > 0)
            {
                // 资源足够，扣除资源并放置
                currentDots--;
                logicalGrid[x, y] = true;
                slot.SetFilled(true);
                
                Debug.Log($"在格子 ({x}, {y}) 放置了 Dot，剩余资源: {currentDots}");
            }
            else
            {
                Debug.LogWarning("Dot 资源不足，无法放置！");
            }
        }
        else
        {
            // 格子是满的：允许悔棋，取回 Dot
            currentDots++;
            logicalGrid[x, y] = false;
            slot.SetFilled(false);
            
            Debug.Log($"从格子 ({x}, {y}) 取回了 Dot，剩余资源: {currentDots}");
        }

        // 更新 UI 显示
        UpdateUI();
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (dotCountText != null)
        {
            dotCountText.text = $"Dots: {currentDots}";
        }
    }

    /// <summary>
    /// 确认按钮点击事件处理
    /// </summary>
    public void OnConfirmPressed()
    {
        Debug.Log("玩家按下确认按钮，开始扫描匹配...");
        
        // 调用全盘扫描
        bool isMatch = ScanForMatches();
        
        if (isMatch)
        {
            // 匹配成功：保持扣除状态，清空网格UI（不返还点数）
            Debug.Log("武器生成成功！");
            ClearGridUI();
            
            // 生成成功后自动关闭UI，让玩家回到战斗
            CloseUI();
        }
        else
        {
            // 匹配失败：返还资源并重置网格
            Debug.Log("合成失败，资源返还");
            
            // 统计并返还资源
            int usedDots = CountActiveDots();
            currentDots += usedDots;
            Debug.Log($"返还 {usedDots} 个 Dot，当前总量: {currentDots}");
            
            // 重置所有格子
            ClearGridUI();
        }
        
        // 更新UI
        UpdateUI();
    }

    /// <summary>
    /// 扫描整个网格，检查是否有任何武器图案匹配
    /// </summary>
    /// <returns>如果匹配成功返回true，否则返回false</returns>
    private bool ScanForMatches()
    {
        // 统计当前网格上亮起的总点数
        int totalDotsOnGrid = CountActiveDots();

        // 遍历整个网格的每一个格子作为潜在的基准点（Anchor）
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                // 遍历所有武器配方
                foreach (WeaponData weapon in allWeaponRecipes)
                {
                    if (weapon == null || weapon.shapePattern == null || weapon.shapePattern.Count == 0)
                    {
                        continue;
                    }

                    // 检查从当前位置开始是否匹配该武器的形状
                    if (IsMatch(weapon, x, y))
                    {
                        // 获取武器所需的点数
                        int weaponRequiredDots = weapon.shapePattern.Count;
                        
                        // 纯度检查：武器所需点数必须等于网格上的总点数
                        if (weaponRequiredDots == totalDotsOnGrid)
                        {
                            Debug.Log($"在位置 ({x}, {y}) 匹配到武器图案: {weapon.weaponName}！");
                            Debug.Log($"纯度检查通过：武器需要 {weaponRequiredDots} 个点，网格上有 {totalDotsOnGrid} 个点");
                            
                            // 触发合成
                            CraftWeapon(weapon, x, y);
                            
                            // 匹配成功，返回 true
                            return true;
                        }
                        else
                        {
                            Debug.LogWarning($"形状匹配但纯度检查失败：武器需要 {weaponRequiredDots} 个点，但网格上有 {totalDotsOnGrid} 个点");
                            Debug.LogWarning("结构含杂质/形状不纯！");
                            // 继续检查其他武器配方
                        }
                    }
                }
            }
        }
        
        // 没有找到匹配，返回 false
        return false;
    }

    /// <summary>
    /// 检查从指定基准点开始是否匹配武器的形状图案
    /// </summary>
    /// <param name="weapon">要检查的武器数据</param>
    /// <param name="startX">基准点X坐标</param>
    /// <param name="startY">基准点Y坐标</param>
    /// <returns>如果匹配返回true，否则返回false</returns>
    private bool IsMatch(WeaponData weapon, int startX, int startY)
    {
        // 遍历该武器 shapePattern 里的所有 offset 点
        foreach (Vector2Int offset in weapon.shapePattern)
        {
            // 计算绝对坐标
            int targetX = startX + offset.x;
            int targetY = startY + offset.y;

            // 越界检查
            if (!IsValidCoordinate(targetX, targetY)) return false;

            // 状态检查：如果该位置没有 Dot，返回 false
            if (!logicalGrid[targetX, targetY]) return false;
        }

        // 所有点都符合，匹配成功
        return true;
    }

    /// <summary>
    /// 合成武器
    /// </summary>
    /// <param name="weapon">匹配的武器数据</param>
    /// <param name="anchorX">锚点X坐标</param>
    /// <param name="anchorY">锚点Y坐标</param>
    private void CraftWeapon(WeaponData weapon, int anchorX, int anchorY)
    {
        Debug.Log($"开始合成武器: {weapon.weaponName}");

        // 消耗掉参与合成的格子
        foreach (Vector2Int offset in weapon.shapePattern)
        {
            int x = anchorX + offset.x;
            int y = anchorY + offset.y;

            // 边界检查
            if (IsValidCoordinate(x, y))
            {
                SetLogicalState(x, y, false);
            }
        }

        // TODO: 实例化武器到游戏场景中
        // GameObject weaponObj = Instantiate(weapon.weaponPrefab);
        // WeaponController controller = weaponObj.GetComponent<WeaponController>();
        // controller.Initialize(weapon);

        Debug.Log($"武器 {weapon.weaponName} 合成成功！");
    }

    /// <summary>
    /// 获取指定坐标的槽位
    /// </summary>
    public GridSlot GetSlot(int x, int y)
    {
        return IsValidCoordinate(x, y) ? slotMatrix[x, y] : null;
    }

    /// <summary>
    /// 获取指定坐标的逻辑状态
    /// </summary>
    public bool GetLogicalState(int x, int y)
    {
        return IsValidCoordinate(x, y) && logicalGrid[x, y];
    }

    /// <summary>
    /// 设置指定坐标的逻辑状态
    /// </summary>
    public void SetLogicalState(int x, int y, bool state)
    {
        if (!IsValidCoordinate(x, y)) return;
        
        logicalGrid[x, y] = state;
        if (slotMatrix[x, y] != null)
        {
            slotMatrix[x, y].SetFilled(state);
        }
    }

    /// <summary>
    /// 清空整个网格UI（仅重置视觉，不返还资源）
    /// </summary>
    private void ClearGridUI()
    {
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                SetLogicalState(x, y, false);
            }
        }
        Debug.Log("网格UI已清空");
    }
    
    /// <summary>
    /// 清空整个网格（公共方法，用于外部调用）
    /// </summary>
    public void ClearGrid()
    {
        ClearGridUI();
        Debug.Log("网格已清空");
    }
    
    /// <summary>
    /// 检查坐标是否在有效范围内
    /// </summary>
    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < GRID_SIZE && y >= 0 && y < GRID_SIZE;
    }
    
    /// <summary>
    /// 统计当前网格上亮起的点数
    /// </summary>
    private int CountActiveDots()
    {
        int count = 0;
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                if (logicalGrid[x, y]) count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// 打开UI
    /// </summary>
    private void OpenUI()
    {
        if (assemblyPanel != null)
        {
            assemblyPanel.SetActive(true);
        }
        
        // 暂停游戏时间
        Time.timeScale = 0f;
        
        isUIOpen = true;
    }
    
    /// <summary>
    /// 增加 Dot 资源（用于拾取物等外部调用）
    /// </summary>
    /// <param name="amount">增加的数量</param>
    public void AddDots(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("尝试增加的 Dot 数量必须大于 0");
            return;
        }
        
        currentDots += amount;
        UpdateUI();
        
        Debug.Log($"增加了 {amount} 个 Dot，当前总量: {currentDots}");
    }
    
    /// <summary>
    /// 关闭UI
    /// </summary>
    private void CloseUI()
    {
        if (assemblyPanel != null)
        {
            assemblyPanel.SetActive(false);
        }
        
        // 恢复游戏时间
        Time.timeScale = 1f;
        
        isUIOpen = false;
    }
}
