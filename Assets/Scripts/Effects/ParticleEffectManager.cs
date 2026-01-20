using UnityEngine;

/// <summary>
/// 粒子特效管理�?
/// 管理收集钻石等特效的粒子系统
/// </summary>
public class ParticleEffectManager : MonoBehaviour
{
    private static ParticleEffectManager instance;
    public static ParticleEffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("ParticleEffectManager");
                instance = obj.AddComponent<ParticleEffectManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    [Header("Diamond Collect Effect")]
    [Tooltip("钻石收集粒子特效预制体（可选，如果为空则程序生成）")]
    public GameObject diamondCollectEffectPrefab;
    
    [Tooltip("环形光波特效预制体（可选，如果为空则程序生成）")]
    public GameObject ringWaveEffectPrefab;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 创建钻石收集粒子特效（包含消散粒子和环形光波�?
    /// </summary>
    /// <param name="position">特效位置（世界坐标）</param>
    public static void CreateDiamondCollectEffect(Vector3 position)
    {
        // 创建消散粒子特效
        if (Instance.diamondCollectEffectPrefab != null)
        {
            // 使用预制�?
            GameObject effect = Instantiate(Instance.diamondCollectEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        else
        {
            // 程序生成粒子特效
            Instance.CreateDiamondEffectProgrammatically(position);
        }
        
        // 同步触发环形光波特效
        CreateRingWaveEffect(position);
    }
    
    /// <summary>
    /// 创建环形光波特效
    /// </summary>
    /// <param name="position">特效位置（世界坐标）</param>
    public static void CreateRingWaveEffect(Vector3 position)
    {
        if (Instance.ringWaveEffectPrefab != null)
        {
            // 使用预制�?
            GameObject effect = Instantiate(Instance.ringWaveEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        else
        {
            // 程序生成环形光波特效
            Instance.CreateRingWaveEffectProgrammatically(position);
        }
    }

    /// <summary>
    /// 程序生成钻石收集粒子特效
    /// </summary>
    private void CreateDiamondEffectProgrammatically(Vector3 position)
    {
        GameObject effectObj = new GameObject("DiamondCollectEffect");
        effectObj.transform.position = position;
        
        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 1.5f;
        main.startSpeed = 8f;  // 增加速度，让粒子更活�?
        main.startSize = 0.8f;  // 增大粒子大小（从0.3增加�?0.8�?
        main.startColor = new Color(0.2f, 0.8f, 1f, 1f);  // 青色
        main.maxParticles = 80;  // 增加最大粒子数
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        
        // 发射模块
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 50),  // 开始时发射50个粒子（增加�?
            new ParticleSystem.Burst(0.2f, 30)  // 0.2秒后发射30个粒子（增加�?
        });
        
        // 形状模块（球形）
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.8f;  // 增大发射半径
        shape.radiusThickness = 1f;  // 使用半径厚度来控制粒子分�?
        
        // 速度限制
        var limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
        limitVelocityOverLifetime.enabled = true;
        limitVelocityOverLifetime.dampen = 0.5f;
        
        // 颜色渐变
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.2f, 0.8f, 1f), 0.0f),  // 青色
                new GradientColorKey(new Color(1f, 1f, 1f), 0.5f),      // 白色
                new GradientColorKey(new Color(0.2f, 0.8f, 1f), 1.0f)   // 青色
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;
        
        // 大小渐变
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 0.0f);
        sizeCurve.AddKey(0.2f, 1.0f);
        sizeCurve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
        
        // 旋转
        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(0f, 360f);
        
        // 重力
        var forceOverLifetime = ps.forceOverLifetime;
        forceOverLifetime.enabled = true;
        forceOverLifetime.y = -2f;  // 轻微向下
        
        // 自动销�?
        Destroy(effectObj, 3f);
    }
    
    /// <summary>
    /// 程序生成环形光波特效
    /// </summary>
    private void CreateRingWaveEffectProgrammatically(Vector3 position)
    {
        GameObject effectObj = new GameObject("RingWaveEffect");
        effectObj.transform.position = position;
        
        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 1.0f;  // 较短的生命周期，快速扩�?
        main.startSpeed = 12f;  // 快速向外扩�?
        main.startSize = 0.5f;
        main.startColor = new Color(0.2f, 0.9f, 1f, 1f);  // 亮青�?
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.startRotation3D = true;
        
        // 发射模块 - 单次爆发
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 200)  // 开始时发射200个粒子形成环�?
        });
        
        // 形状模块（环形）
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;  // 起始半径很小，形成环�?
        shape.radiusMode = ParticleSystemShapeMultiModeValue.Random;
        shape.radiusSpread = 0.1f;
        shape.arc = 360f;  // 完整圆形
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        shape.arcSpread = 0f;
        
        // 速度限制 - 让粒子向外扩散后逐渐减�?
        var limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
        limitVelocityOverLifetime.enabled = true;
        limitVelocityOverLifetime.dampen = 0.3f;
        limitVelocityOverLifetime.limit = 15f;
        
        // 颜色渐变 - 从亮青色到透明
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.2f, 0.9f, 1f, 1f), 0.0f),  // 亮青�?
                new GradientColorKey(new Color(1f, 1f, 1f, 1f), 0.3f),      // 白色
                new GradientColorKey(new Color(0.2f, 0.9f, 1f, 0.5f), 0.7f),  // 半透明青色
                new GradientColorKey(new Color(0.2f, 0.9f, 1f, 0f), 1.0f)   // 完全透明
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.3f),
                new GradientAlphaKey(0.5f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;
        
        // 大小渐变 - 从大到小
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 1.0f);  // 开始较�?
        sizeCurve.AddKey(0.5f, 0.8f);
        sizeCurve.AddKey(1.0f, 0.3f);  // 结束时较�?
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);
        
        // 旋转 - 让粒子旋�?
        var rotationOverLifetime = ps.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(0f, 180f);
        
        // 纹理表动画（可选，让粒子有闪烁效果�?
        var textureSheetAnimation = ps.textureSheetAnimation;
        textureSheetAnimation.enabled = false;  // 如果需要可以启�?
        
        // 渲染模块 - 使用Additive混合模式，让光波更亮
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = 0f;
        
        // 自动销�?
        Destroy(effectObj, 2f);
    }
}

