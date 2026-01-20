using UnityEngine;
using System.Collections.Generic;
using InkBoatGame.Managers; // 引用你之前的积分管理器

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("把所有做好的SO文件都拖进这里")]
    public List<ShopItemSO> allItems;

    // 当购买或装备发生变化时，通知UI刷新
    public System.Action OnShopDataChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 跨场景保持，确保装备信息不丢失
            
            // --- 初始化默认赠送 ---
            // 第一次打开游戏，确保玩家拥有默认的 0号船 和 100号导弹
            UnlockDefaultItem(0, ItemType.Ship);
            UnlockDefaultItem(100, ItemType.Missile);
            
            Debug.Log("ShopManager: 已初始化并设置 DontDestroyOnLoad");
        }
        else
        {
            Debug.LogWarning("ShopManager: 检测到重复实例，销毁当前对象");
            Destroy(gameObject);
            return;
        }
    }

    void UnlockDefaultItem(int id, ItemType type)
    {
        if (!IsOwned(id))
        {
            PlayerPrefs.SetInt("Owned_" + id, 0);
            // 如果是该类型的第一个，顺便直接装备上
            string key = type == ItemType.Ship ? "Equipped_Ship" : "Equipped_Missile";
            if (!PlayerPrefs.HasKey(key)) PlayerPrefs.SetInt(key, id);
        }
    }

    // --- 核心逻辑 ---

    // 1. 检查是否拥有 (查存档)
    public bool IsOwned(int id)
    {
        return PlayerPrefs.GetInt("Owned_" + id, 0) == 1;
    }

    // 2. 检查是否当前装备中
    public bool IsEquipped(int id, ItemType type)
    {
        string key = type == ItemType.Ship ? "Equipped_Ship" : "Equipped_Missile";
        return PlayerPrefs.GetInt(key) == id;
    }

    // 3. 购买逻辑
    public bool BuyItem(ShopItemSO item)
    {
        if (IsOwned(item.itemID)) return true; // 已经有了

        // 情况 A：如果是 0 元免费商品
        // 直接给，不需要查积分管理器 (防止 ScoreManager 为空导致报错)
        if (item.price == 0)
        {
            PerformTransaction(item); // 执行交易
            return true;
        }

        // 情况 B：如果是付费商品
        // 先检查积分管理器是否存在，再检查积分够不够
        if (ScoreManager.Instance != null && ScoreManager.Instance.currentPoints >= item.price)
        {
            // 扣积分
            ScoreManager.Instance.AddPoints(-item.price);
            // 执行交易
            PerformTransaction(item);
            return true;
        }

        // 情况 C：积分不足，或者积分管理器丢失
        if (ScoreManager.Instance == null)
        {
            Debug.LogError("购买失败：场景里找不到 ScoreManager！无法扣款！");
        }
        else
        {
            Debug.Log($"购买失败：积分不足！需要{item.price}积分，当前只有{ScoreManager.Instance.currentPoints}积分");
        }

        return false;
    }

    // 为了代码整洁，把保存和刷新的逻辑提取出来
    void PerformTransaction(ShopItemSO item)
    {
        // 1. 记录已拥有
        PlayerPrefs.SetInt("Owned_" + item.itemID, 1);
        PlayerPrefs.Save();

        // 2. 通知 UI 刷新 (变成"已拥有"或"装备")
        OnShopDataChanged?.Invoke();
    }

    // 4. 装备逻辑
    public void EquipItem(ShopItemSO item)
    {
        if (!IsOwned(item.itemID))
        {
            Debug.Log("装备失败：你还没拥有这个物品！");
            return;
        }

        string key = item.type == ItemType.Ship ? "Equipped_Ship" : "Equipped_Missile";

        PlayerPrefs.SetInt(key, item.itemID);
        PlayerPrefs.Save();

        OnShopDataChanged?.Invoke();
    }

    // 5. 获取当前装备的预制体 (给商店预览用)
    public GameObject GetEquippedPrefab(ItemType type)
    {
        string key = type == ItemType.Ship ? "Equipped_Ship" : "Equipped_Missile";
        int id = PlayerPrefs.GetInt(key, type == ItemType.Ship ? 0 : 100); // 默认值

        ShopItemSO item = allItems.Find(x => x.itemID == id);
        return item != null ? item.prefab : null;
    }

    // 6. 获取当前装备的游戏预制体 (给游戏关卡生成用)
    public GameObject GetEquippedGamePrefab(ItemType type)
    {
        string key = type == ItemType.Ship ? "Equipped_Ship" : "Equipped_Missile";
        int id = PlayerPrefs.GetInt(key, type == ItemType.Ship ? 0 : 100); // 默认值

        ShopItemSO item = allItems.Find(x => x.itemID == id);
        if (item != null)
        {
            // 优先使用 gamePrefab，如果没有则使用 prefab（向后兼容）
            return item.gamePrefab != null ? item.gamePrefab : item.prefab;
        }
        return null;
    }

    // 7. 获取当前装备的物品ID
    public int GetEquippedItemID(ItemType type)
    {
        string key = type == ItemType.Ship ? "Equipped_Ship" : "Equipped_Missile";
        return PlayerPrefs.GetInt(key, type == ItemType.Ship ? 0 : 100); // 默认值
    }
}