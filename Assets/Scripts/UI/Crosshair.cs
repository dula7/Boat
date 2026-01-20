using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 准心系统
/// 在屏幕中心或鼠标位置显示准心
/// </summary>
public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public bool followMouse = true;  // 是否跟随鼠标
    public bool showAtCenter = false;  // 是否始终显示在屏幕中心
    public float crosshairSize = 20f;  // 准心大小
    public Color crosshairColor = Color.white;  // 准心颜色

    private RectTransform rectTransform;
    private Image crosshairImage;

    void Start()
    {
        // 获取或创建Image组件
        crosshairImage = GetComponent<Image>();
        if (crosshairImage == null)
        {
            crosshairImage = gameObject.AddComponent<Image>();
        }

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        // 设置初始属性
        crosshairImage.color = crosshairColor;
        
        // 创建简单的准心纹理（如果没有指定）
        if (crosshairImage.sprite == null)
        {
            CreateDefaultCrosshair();
        }

        // 设置初始位置
        if (showAtCenter)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    void Update()
    {
        if (followMouse && !showAtCenter)
        {
            // 跟随鼠标位置
            rectTransform.position = Input.mousePosition;
        }
        else if (showAtCenter)
        {
            // 始终显示在屏幕中心
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// 创建默认准心纹理
    /// </summary>
    private void CreateDefaultCrosshair()
    {
        // 创建一个简单的十字准心
        Texture2D texture = new Texture2D((int)crosshairSize, (int)crosshairSize);
        Color[] pixels = new Color[(int)(crosshairSize * crosshairSize)];

        int center = (int)(crosshairSize / 2);
        int thickness = 2;

        for (int y = 0; y < crosshairSize; y++)
        {
            for (int x = 0; x < crosshairSize; x++)
            {
                bool isCrosshair = false;
                
                // 绘制水平线
                if (y >= center - thickness / 2 && y <= center + thickness / 2)
                {
                    if (x >= center - crosshairSize / 4 && x <= center + crosshairSize / 4)
                    {
                        isCrosshair = true;
                    }
                }
                
                // 绘制垂直线
                if (x >= center - thickness / 2 && x <= center + thickness / 2)
                {
                    if (y >= center - crosshairSize / 4 && y <= center + crosshairSize / 4)
                    {
                        isCrosshair = true;
                    }
                }

                pixels[y * (int)crosshairSize + x] = isCrosshair ? crosshairColor : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, crosshairSize, crosshairSize), new Vector2(0.5f, 0.5f));
        crosshairImage.sprite = sprite;
    }
}

