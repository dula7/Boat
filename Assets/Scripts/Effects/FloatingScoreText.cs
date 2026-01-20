using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 浮动得分文字动画（优化版�?
/// 显示收集物品时的得分动画（如+10�?+2�?
/// 支持平滑动画、描边效果、粒子特�?
/// </summary>
public class FloatingScoreText : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("向上移动的距离（屏幕像素，默�?100像素�?")]
    public float moveDistance = 100f;
    
    [Tooltip("动画持续时间（秒，默�?2秒）")]
    public float duration = 2f;
    
    [Tooltip("淡出开始时间（相对于duration的比例，0-1，默�?0.6�?")]
    public float fadeStartRatio = 0.6f;
    
    [Tooltip("动画缓动类型")]
    public EaseType easeType = EaseType.EaseOutCubic;

    [Header("Text Settings")]
    [Tooltip("文字内容（如+10�?")]
    public string text = "+10";
    
    [Tooltip("文字大小（默�?60�?")]
    public int fontSize = 60;
    
    [Tooltip("文字颜色（默认黄色）")]
    public Color textColor = new Color(1f, 0.9f, 0.2f, 1f);  // 金黄�?
    
    [Tooltip("描边颜色（默认黑色）")]
    public Color outlineColor = Color.black;
    
    [Tooltip("描边宽度（默�?3�?")]
    public float outlineWidth = 3.5f;
    
    [Tooltip("阴影偏移（默�?2,2�?")]
    public Vector2 shadowOffset = new Vector2(2f, -2f);
    
    [Tooltip("阴影颜色（默认半透明黑色�?")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

    [Header("References")]
    [Tooltip("主摄像头（用于世界坐标转屏幕坐标�?")]
    public Camera mainCamera;

    private Text textComponent;
    private Text shadowTextComponent;    // 阴影文字
    private Text[] outlineTextComponents;  // 描边文字数组
    private Canvas canvas;
    private RectTransform rectTransform;
    private Vector3 worldPosition;
    private float startTime;
    private float initialScale = 0.5f;  // 初始缩放（弹跳效果）

    public enum EaseType
    {
        Linear,
        EaseOutQuad,
        EaseOutCubic,
        EaseOutQuart,
        EaseOutBounce,
        EaseOutElastic
    }

    /// <summary>
    /// 创建浮动得分文字（静态方法）
    /// </summary>
    /// <param name="worldPos">世界坐标位置</param>
    /// <param name="scoreText">得分文字（如"+10"�?</param>
    /// <param name="color">文字颜色（可选）</param>
    /// <param name="isDiamond">是否是钻石收集（用于触发粒子特效�?</param>
    /// <returns>创建的FloatingScoreText对象</returns>
    public static FloatingScoreText Create(Vector3 worldPos, string scoreText, Color? color = null, bool isDiamond = false)
    {
        // 查找或创建Screen Space - Overlay Canvas
        Canvas overlayCanvas = FindOrCreateCanvas();
        
        // 创建Text对象容器
        GameObject textObj = new GameObject("FloatingScoreText");
        textObj.transform.SetParent(overlayCanvas.transform, false);
        
        // 添加FloatingScoreText组件
        FloatingScoreText floatingText = textObj.AddComponent<FloatingScoreText>();
        floatingText.worldPosition = worldPos;
        floatingText.text = scoreText;
        floatingText.textColor = color ?? new Color(1f, 0.9f, 0.2f, 1f);
        floatingText.canvas = overlayCanvas;
        
        // 设置RectTransform
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 150);
        floatingText.rectTransform = rect;
        
        // 创建阴影文字（底层）
        GameObject shadowObj = new GameObject("ShadowText");
        shadowObj.transform.SetParent(textObj.transform, false);
        RectTransform shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.sizeDelta = Vector2.zero;
        shadowRect.anchoredPosition = floatingText.shadowOffset;
        
        Text shadowText = shadowObj.AddComponent<Text>();
        shadowText.text = scoreText;
        shadowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        shadowText.fontSize = floatingText.fontSize;
        shadowText.color = floatingText.shadowColor;
        shadowText.alignment = TextAnchor.MiddleCenter;
        floatingText.shadowTextComponent = shadowText;
        
        // 创建描边文字（中层，4个方向）
        floatingText.outlineTextComponents = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject outlineObj = new GameObject($"OutlineText_{i}");
            outlineObj.transform.SetParent(textObj.transform, false);
            RectTransform outlineRect = outlineObj.AddComponent<RectTransform>();
            outlineRect.anchorMin = Vector2.zero;
            outlineRect.anchorMax = Vector2.one;
            outlineRect.sizeDelta = Vector2.zero;
            
            Vector2 offset = Vector2.zero;
            switch (i)
            {
                case 0: offset = new Vector2(floatingText.outlineWidth, 0); break;  // �?
                case 1: offset = new Vector2(-floatingText.outlineWidth, 0); break; // �?
                case 2: offset = new Vector2(0, floatingText.outlineWidth); break;   // �?
                case 3: offset = new Vector2(0, -floatingText.outlineWidth); break; // �?
            }
            outlineRect.anchoredPosition = offset;
            
            Text outlineText = outlineObj.AddComponent<Text>();
            outlineText.text = scoreText;
            outlineText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            outlineText.fontSize = floatingText.fontSize;
            outlineText.color = floatingText.outlineColor;
            outlineText.alignment = TextAnchor.MiddleCenter;
            floatingText.outlineTextComponents[i] = outlineText;
        }
        
        // 创建主文字（顶层�?
        GameObject mainTextObj = new GameObject("MainText");
        mainTextObj.transform.SetParent(textObj.transform, false);
        RectTransform mainRect = mainTextObj.AddComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.sizeDelta = Vector2.zero;
        
        Text mainText = mainTextObj.AddComponent<Text>();
        mainText.text = scoreText;
        mainText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mainText.fontSize = floatingText.fontSize;
        mainText.color = floatingText.textColor;
        mainText.alignment = TextAnchor.MiddleCenter;
        mainText.fontStyle = FontStyle.Bold;  // 加粗
        floatingText.textComponent = mainText;
        
        // 设置主摄像头
        floatingText.mainCamera = Camera.main;
        if (floatingText.mainCamera == null)
        {
            floatingText.mainCamera = FindObjectOfType<Camera>();
        }
        
        // 如果是钻石收集，触发粒子特效
        if (isDiamond)
        {
            ParticleEffectManager.CreateDiamondCollectEffect(worldPos);
        }
        
        // 开始动�?
        floatingText.StartAnimation();
        
        return floatingText;
    }

    /// <summary>
    /// 查找或创建Screen Space - Overlay Canvas
    /// </summary>
    private static Canvas FindOrCreateCanvas()
    {
        // 先查找现有的Screen Space - Overlay Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.sortingOrder >= 50)
            {
                return canvas;
            }
        }
        
        // 如果没有找到，创建一个新�?
        GameObject canvasObj = new GameObject("FloatingScoreCanvas");
        Canvas overlayCanvas = canvasObj.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 100;  // 确保在最上层显示
        
        // 添加CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // 添加GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        return overlayCanvas;
    }

    /// <summary>
    /// 开始动�?
    /// </summary>
    private void StartAnimation()
    {
        startTime = Time.time;
        StartCoroutine(AnimateText());
    }

    /// <summary>
    /// 动画协程（优化版，使用缓动函数）
    /// </summary>
    private IEnumerator AnimateText()
    {
        // 初始屏幕位置
        Vector2 startScreenPos = Vector2.zero;
        if (mainCamera != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
            startScreenPos = new Vector2(screenPos.x, screenPos.y);
        }
        else
        {
            startScreenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }
        
        Vector2 endScreenPos = startScreenPos + Vector2.up * moveDistance;
        float fadeStartTime = duration * fadeStartRatio;
        
        // 设置初始位置和缩�?
        rectTransform.position = startScreenPos;
        rectTransform.localScale = Vector3.one * initialScale;
        
        while (Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration;
            float easedT = ApplyEasing(t, easeType);
            
            // 更新屏幕位置（向上移动，带缓动）
            Vector2 currentScreenPos = Vector2.Lerp(startScreenPos, endScreenPos, easedT);
            rectTransform.position = currentScreenPos;
            
            // 缩放效果（弹跳效果：�?0.5�?1.2再到1.0�?
            float scale = 0f;
            if (t < 0.3f)
            {
                // 快速放�?
                float scaleT = t / 0.3f;
                scale = Mathf.Lerp(initialScale, 1.2f, EaseOutCubic(scaleT));
            }
            else
            {
                // 缓慢回弹
                float scaleT = (t - 0.3f) / 0.7f;
                scale = Mathf.Lerp(1.2f, 1.0f, EaseOutQuad(scaleT));
            }
            rectTransform.localScale = Vector3.one * scale;
            
            // 淡出效果
            if (elapsed > fadeStartTime)
            {
                float fadeT = (elapsed - fadeStartTime) / (duration - fadeStartTime);
                Color color = textComponent.color;
                color.a = Mathf.Lerp(1f, 0f, fadeT);
                textComponent.color = color;
                
                if (shadowTextComponent != null)
                {
                    Color shadowColor = shadowTextComponent.color;
                    shadowColor.a = Mathf.Lerp(this.shadowColor.a, 0f, fadeT);
                    shadowTextComponent.color = shadowColor;
                }
            }
            
            // 轻微旋转效果（可选）
            rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2f) * 5f);
            
            yield return null;
        }
        
        // 动画结束，销毁对�?
        Destroy(gameObject);
    }

    /// <summary>
    /// 应用缓动函数
    /// </summary>
    private float ApplyEasing(float t, EaseType easeType)
    {
        switch (easeType)
        {
            case EaseType.Linear:
                return t;
            case EaseType.EaseOutQuad:
                return EaseOutQuad(t);
            case EaseType.EaseOutCubic:
                return EaseOutCubic(t);
            case EaseType.EaseOutQuart:
                return EaseOutQuart(t);
            case EaseType.EaseOutBounce:
                return EaseOutBounce(t);
            case EaseType.EaseOutElastic:
                return EaseOutElastic(t);
            default:
                return t;
        }
    }

    // 缓动函数
    private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
    private float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f)
            return 7.5625f * t * t;
        else if (t < 2f / 2.75f)
            return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
        else if (t < 2.5f / 2.75f)
            return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
        else
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
    }
    private float EaseOutElastic(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        float p = 0.3f;
        float s = p / 4f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
    }
}
