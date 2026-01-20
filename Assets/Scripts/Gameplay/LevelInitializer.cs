using UnityEngine;

/// <summary>
/// 关卡初始化器
/// 在关卡加载时，根据商店选择动态加载船和导弹
/// </summary>
public class LevelInitializer : MonoBehaviour
{
    [Header("替换设置")]
    [Tooltip("是否替换场景中已存在的船体（如果场景中有默认船体）")]
    public bool replaceExistingBoat = true;

    [Tooltip("是否自动更新PlayerController的导弹预制体")]
    public bool autoUpdateMissilePrefab = true;

    [Header("自动配置")]
    [Tooltip("是否自动查找并分配WaterPlane给Buoyancy组件")]
    public bool autoAssignWaterPlane = true;

    [Header("调试")]
    [Tooltip("是否启用调试日志")]
    public bool enableDebugLog = false;

    private GameObject currentBoatInstance;  // 当前实例化的船体

    void Start()
    {
        // 使用协程延迟初始化，确保 ShopManager 已经初始化完成
        StartCoroutine(DelayedInitialize());
    }

    /// <summary>
    /// 延迟初始化，确保 ShopManager 已经准备好
    /// </summary>
    private System.Collections.IEnumerator DelayedInitialize()
    {
        // 等待一帧，确保所有 Awake 和 Start 都执行完毕
        yield return null;

        // 等待 ShopManager 初始化（最多等待 10 帧）
        int maxWaitFrames = 10;
        int waitCount = 0;
        while (ShopManager.Instance == null && waitCount < maxWaitFrames)
        {
            yield return null;
            waitCount++;
        }

        if (ShopManager.Instance == null)
        {
            Debug.LogError("LevelInitializer: ShopManager 未找到！请确保 ShopManager 在 Main 场景中存在，并且已设置 DontDestroyOnLoad。");
            yield break;
        }

        // 检查 ShopManager 的 allItems 列表是否为空
        if (ShopManager.Instance.allItems == null || ShopManager.Instance.allItems.Count == 0)
        {
            Debug.LogError("LevelInitializer: ShopManager 的 allItems 列表为空！请确保在 Main 场景中配置了所有 ShopItemSO。");
            yield break;
        }

        // 现在可以安全地初始化关卡
        InitializeLevel();
    }

    /// <summary>
    /// 初始化关卡
    /// </summary>
    private void InitializeLevel()
    {
        // 1. 加载选择的船体（使用预制体自身的位置、旋转和缩放）
        LoadSelectedBoat();

        // 2. 自动分配WaterPlane给Buoyancy组件
        if (autoAssignWaterPlane)
        {
            AssignWaterPlaneToBuoyancy();
        }

        // 3. 更新导弹预制体
        if (autoUpdateMissilePrefab)
        {
            UpdateMissilePrefab();
        }
    }

    /// <summary>
    /// 加载选择的船体
    /// </summary>
    private void LoadSelectedBoat()
    {
        if (ShopManager.Instance == null)
        {
            Debug.LogError("LevelInitializer: ShopManager未找到，使用场景中的默认船体");
            return;
        }

        // 获取当前装备的船体ID
        int boatID = ShopManager.Instance.GetEquippedItemID(ItemType.Ship);
        
        if (enableDebugLog)
        {
            Debug.Log($"LevelInitializer: 当前装备的船体 ID: {boatID}");
        }

        // 获取选择的船体预制体
        GameObject boatPrefab = ShopManager.Instance.GetEquippedGamePrefab(ItemType.Ship);
        if (boatPrefab == null)
        {
            Debug.LogWarning($"LevelInitializer: 未找到船体 ID {boatID} 的游戏预制体（gamePrefab），请检查 ShopItemSO 配置！");
            Debug.LogWarning("LevelInitializer: 使用场景中的默认船体");
            return;
        }

        if (enableDebugLog)
        {
            Debug.Log($"LevelInitializer: 加载船体 ID: {boatID}, Prefab: {boatPrefab.name}");
        }

        // 如果启用替换，先删除场景中的默认船体
        if (replaceExistingBoat)
        {
            RemoveExistingBoat();
        }

        // 使用预制体自身的位置、旋转和缩放
        Vector3 spawnPosition = boatPrefab.transform.position;
        Quaternion spawnRotation = boatPrefab.transform.rotation;
        Vector3 spawnScale = boatPrefab.transform.localScale;

        if (enableDebugLog)
        {
            Debug.Log($"LevelInitializer: 使用预制体自身的 Transform - 位置: {spawnPosition}, 旋转: {spawnRotation.eulerAngles}, 缩放: {spawnScale}");
        }

        // 实例化选择的船体
        currentBoatInstance = Instantiate(boatPrefab, spawnPosition, spawnRotation);
        
        // 设置缩放（Instantiate 不会自动应用预制体的 localScale）
        currentBoatInstance.transform.localScale = spawnScale;

        currentBoatInstance.name = "PlayerBoat(Selected)";

        if (enableDebugLog)
        {
            Debug.Log($"LevelInitializer: 船体已实例化 - 位置: {currentBoatInstance.transform.position}, 旋转: {currentBoatInstance.transform.rotation.eulerAngles}, 缩放: {currentBoatInstance.transform.localScale}");
        }
    }

    /// <summary>
    /// 移除场景中已存在的船体
    /// </summary>
    private void RemoveExistingBoat()
    {
        // 查找所有带有BoatController的对象
        BoatController[] existingBoats = FindObjectsOfType<BoatController>();
        
        foreach (BoatController boat in existingBoats)
        {
            // 不要删除我们刚实例化的船体
            if (currentBoatInstance != null && boat.gameObject == currentBoatInstance)
            {
                continue;
            }

            if (enableDebugLog)
            {
                Debug.Log($"LevelInitializer: 删除场景中的默认船体: {boat.gameObject.name}");
            }

            Destroy(boat.gameObject);
        }
    }

    /// <summary>
    /// 自动查找并分配WaterPlane给Buoyancy组件
    /// </summary>
    private void AssignWaterPlaneToBuoyancy()
    {
        if (currentBoatInstance == null)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning("LevelInitializer: 当前没有实例化的船体，无法分配WaterPlane");
            }
            return;
        }

        // 查找场景中的WaterPlane对象
        GameObject waterPlane = GameObject.Find("WaterPlane");
        if (waterPlane == null)
        {
            Debug.LogError("LevelInitializer: 未找到场景中的WaterPlane对象！请确保场景中存在名为'WaterPlane'的GameObject。");
            return;
        }

        Transform waterTransform = waterPlane.transform;

        // 查找船体及其子对象上的所有Buoyancy组件
        Buoyancy[] buoyancyComponents = currentBoatInstance.GetComponentsInChildren<Buoyancy>(true);
        
        if (buoyancyComponents.Length == 0)
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"LevelInitializer: 船体 {currentBoatInstance.name} 上未找到Buoyancy组件");
            }
            return;
        }

        // 分配WaterPlane给所有Buoyancy组件
        int assignedCount = 0;
        foreach (Buoyancy buoyancy in buoyancyComponents)
        {
            if (buoyancy != null)
            {
                // 直接设置public字段
                buoyancy.water = waterTransform;
                assignedCount++;

                if (enableDebugLog)
                {
                    Debug.Log($"LevelInitializer: 已为 {buoyancy.gameObject.name} 的Buoyancy组件分配WaterPlane");
                }
            }
        }

        if (enableDebugLog)
        {
            Debug.Log($"LevelInitializer: 已为 {assignedCount} 个Buoyancy组件分配WaterPlane");
        }
    }

    /// <summary>
    /// 更新PlayerController的导弹预制体
    /// </summary>
    private void UpdateMissilePrefab()
    {
        if (ShopManager.Instance == null)
        {
            Debug.LogError("LevelInitializer: ShopManager未找到，无法更新导弹预制体");
            return;
        }

        // 获取当前装备的导弹ID
        int missileID = ShopManager.Instance.GetEquippedItemID(ItemType.Missile);
        
        if (enableDebugLog)
        {
            Debug.Log($"LevelInitializer: 当前装备的导弹 ID: {missileID}");
        }

        // 获取选择的导弹预制体
        GameObject missilePrefab = ShopManager.Instance.GetEquippedGamePrefab(ItemType.Missile);
        if (missilePrefab == null)
        {
            Debug.LogWarning($"LevelInitializer: 未找到导弹 ID {missileID} 的游戏预制体（gamePrefab），请检查 ShopItemSO 配置！");
            Debug.LogWarning("LevelInitializer: 使用场景中的默认导弹");
            return;
        }

        // 查找所有PlayerController组件
        PlayerController[] playerControllers = FindObjectsOfType<PlayerController>();
        
        foreach (PlayerController playerController in playerControllers)
        {
            if (playerController != null)
            {
                playerController.bulletPrefab = missilePrefab;
                
                if (enableDebugLog)
                {
                    // 使用已经获取的 missileID 变量，不需要重新声明
                    Debug.Log($"LevelInitializer: 已更新PlayerController的导弹预制体为 ID: {missileID}, Prefab: {missilePrefab.name}");
                }
            }
        }

        if (playerControllers.Length == 0 && enableDebugLog)
        {
            Debug.LogWarning("LevelInitializer: 场景中未找到PlayerController组件");
        }
    }

    /// <summary>
    /// 手动重新加载船体（供外部调用）
    /// </summary>
    public void ReloadBoat()
    {
        if (currentBoatInstance != null)
        {
            Destroy(currentBoatInstance);
            currentBoatInstance = null;
        }
        LoadSelectedBoat();
        
        // 重新分配WaterPlane
        if (autoAssignWaterPlane)
        {
            AssignWaterPlaneToBuoyancy();
        }
    }

    /// <summary>
    /// 手动更新导弹预制体（供外部调用）
    /// </summary>
    public void ReloadMissile()
    {
        UpdateMissilePrefab();
    }
}

