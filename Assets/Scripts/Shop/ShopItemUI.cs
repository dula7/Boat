using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("商品数据 (手动拖进去)")]
    public ShopItemSO data; // 1. 在Inspector中直接拖入商品数据文件

    [Header("UI组件")]
    public Image iconImg;

    // 点击图标按钮，用于点击预览
    public Button iconBtn;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button actionBtn;
    public TextMeshProUGUI btnText;
    public GameObject lockIcon;

    // 2. 如果data不为空，Start会自动初始化，否则需要手动调用 Setup
    void Start()
    {
        if (data != null)
        {
            Setup(data);
        }
    }

    // 3. 监听商店数据刷新事件，自动刷新按钮状态和价格
    void OnEnable()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnShopDataChanged += RefreshState;
            // 如果 data 已经设置了，顺便刷新一次
            if (data != null) RefreshState();
        }
    }

    void OnDisable()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnShopDataChanged -= RefreshState;
    }

    // 初始化商品
    public void Setup(ShopItemSO data)
    {
        this.data = data;
        iconImg.sprite = data.icon;
        nameText.text = data.itemName;

        // --- 购买/装备按钮逻辑 ---
        actionBtn.onClick.RemoveAllListeners();
        actionBtn.onClick.AddListener(OnClickBtn);

        if (iconBtn != null)
        {
            iconBtn.onClick.RemoveAllListeners();
            iconBtn.onClick.AddListener(() => {
                // 调用预览管理器，把当前物品的 prefab 传进去
                if (PreviewManager.Instance == null)
                {
                    Debug.LogError("[ShopItemUI] PreviewManager.Instance 为 null！");
                    return;
                }
                PreviewManager.Instance.ShowPreview(data.prefab);
            });
        }

        RefreshState();
    }

    public void RefreshState()
    {
        // 防止数据为空时报错
        if (data == null || ShopManager.Instance == null) return;

        bool isOwned = ShopManager.Instance.IsOwned(this.data.itemID);
        bool isEquipped = ShopManager.Instance.IsEquipped(this.data.itemID, this.data.type);

        if (isEquipped)
        {
            // 状态：正在使用
            btnText.text = "使用中";
            actionBtn.interactable = false; // 不可点击
            if (lockIcon) lockIcon.SetActive(false);
            priceText.text = "已拥有";
        }
        else if (isOwned)
        {
            // 状态：已拥有，可装备
            btnText.text = "装备";
            actionBtn.interactable = true;
            if (lockIcon) lockIcon.SetActive(false);
            priceText.text = "已拥有";
        }
        else
        {
            // 状态：未购买
            btnText.text = "购买";
            actionBtn.interactable = true;
            if (lockIcon) lockIcon.SetActive(true);
            priceText.text = this.data.price.ToString(); // 显示价格
        }
    }

    void OnClickBtn()
    {
        if (ShopManager.Instance.IsOwned(this.data.itemID))
        {      
            ShopManager.Instance.EquipItem(this.data);
        }
        else
        {
            bool success = ShopManager.Instance.BuyItem(this.data);
        }
    }
}

