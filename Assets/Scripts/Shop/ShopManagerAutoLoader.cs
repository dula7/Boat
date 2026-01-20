using UnityEngine;

/// <summary>
/// ShopManager 自动加载器
/// 如果场景中没有 ShopManager，会自动创建一个
/// 用于确保 ShopManager 在任何场景中都可以使用
/// </summary>
public class ShopManagerAutoLoader : MonoBehaviour
{
    [Header("自动创建设置")]
    [Tooltip("如果 ShopManager 不存在，是否自动创建")]
    public bool autoCreateIfMissing = true;

    [Tooltip("是否在 Start 时检查并创建")]
    public bool checkOnStart = true;

    void Start()
    {
        if (checkOnStart)
        {
            EnsureShopManagerExists();
        }
    }

    /// <summary>
    /// 确保 ShopManager 存在
    /// </summary>
    [ContextMenu("确保 ShopManager 存在")]
    public void EnsureShopManagerExists()
    {
        if (ShopManager.Instance != null)
        {
            Debug.Log("ShopManagerAutoLoader: ShopManager 已存在，无需创建");
            return;
        }

        if (!autoCreateIfMissing)
        {
            Debug.LogWarning("ShopManagerAutoLoader: ShopManager 不存在，但自动创建已禁用");
            return;
        }

        Debug.LogWarning("ShopManagerAutoLoader: ShopManager 不存在，正在自动创建...");

        // 创建 ShopManager GameObject
        GameObject shopManagerObj = new GameObject("ShopManager(AutoCreated)");
        ShopManager shopManager = shopManagerObj.AddComponent<ShopManager>();

        // 注意：allItems 列表需要手动配置
        // 这里只是创建了对象，用户需要在 Unity 编辑器中配置 allItems
        Debug.LogWarning("ShopManagerAutoLoader: ShopManager 已自动创建，但请确保在 Unity 编辑器中配置 allItems 列表！");
        Debug.LogWarning("ShopManagerAutoLoader: 建议在 Main 场景中手动创建并配置 ShopManager，而不是依赖自动创建。");
    }
}

