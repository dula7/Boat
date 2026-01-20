using UnityEngine;

/// <summary>
/// 商店物品数据（ScriptableObject）
/// 用于创建商店物品配置
/// </summary>
[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    [Header("物品信息")]
    public int itemID;          // 内部ID（规则：船是0-99，导弹是100-199，不要重复）
    public string itemName;     // 物品名称
    public int price;           // 价格
    public Sprite icon;         // UI显示的图标
    public ItemType type;       // 是船还是导弹

    [Header("预制体设置")]
    [Tooltip("展示用预制体（用于商店界面预览，可以没有游戏逻辑脚本）")]
    public GameObject prefab;   // 对应3D模型（用于商店展示）

    [Tooltip("游戏用预制体（用于游戏关卡，必须包含完整的游戏逻辑脚本，如BoatController、PlayerController等）")]
    public GameObject gamePrefab;   // 游戏关卡中使用的预制体（包含完整游戏逻辑）
}

public enum ItemType
{
    Ship,
    Missile
}
