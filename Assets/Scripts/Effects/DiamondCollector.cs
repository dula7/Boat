using UnityEngine;

/// <summary>
/// 钻石收集�?
/// 处理钻石的收集逻辑，集成ScoreManager和LevelManager
/// 船体可以穿过钻石（使用Trigger），收集后显示得分动�?
/// </summary>
public class DiamondCollector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("钻石标签（默认：Diamond�?")]
    public string diamondTag = "Diamond";
    
    [Tooltip("是否自动检测碰撞（使用Trigger，默认开启，确保船体可以穿过�?")]
    public bool useTrigger = true;
    
    [Tooltip("收集后是否销毁钻石对象（默认开启）")]
    public bool destroyOnCollect = true;

    [Header("Debug")]
    [Tooltip("是否启用调试日志")]
    public bool enableDebugLog = false;

    private bool isCollected = false;  // 是否已收�?

    void Start()
    {
        // 自动设置标签
        if (!gameObject.CompareTag(diamondTag))
        {
            try
            {
                gameObject.tag = diamondTag;
            }
            catch
            {
                Debug.LogWarning($"DiamondCollector: 无法设置标签 '{diamondTag}'，请确保该标签已存在�?");
            }
        }

        // 自动设置碰撞体为Trigger（确保船体可以穿过）
        SetupCollider();
    }

    /// <summary>
    /// 设置碰撞体为Trigger
    /// </summary>
    private void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        
        if (col == null)
        {
            // 如果没有碰撞体，添加一个SphereCollider
            col = gameObject.AddComponent<SphereCollider>();
        }
        
        // 设置为Trigger，确保船体可以穿�?
        col.isTrigger = true;
        
        // 如果钻石有Rigidbody，设置为Kinematic，避免物理阻�?
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"DiamondCollector: 已设置Trigger模式，碰撞体类型: {col.GetType().Name}");
        }
    }

    /// <summary>
    /// Trigger检测：当船体进入钻石范�?
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // 检查是否是船体
        if (IsBoat(other.gameObject))
        {
            CollectDiamond();
        }
    }

    /// <summary>
    /// 检查是否是船体
    /// </summary>
    private bool IsBoat(GameObject obj)
    {
        if (obj == null) return false;

        // 检查是否有BoatController组件
        if (obj.GetComponent<BoatController>() != null)
        {
            return true;
        }

        // 检查是否是船体的子对象
        BoatController boat = FindObjectOfType<BoatController>();
        if (boat != null)
        {
            Transform current = obj.transform;
            while (current != null)
            {
                if (current == boat.transform)
                {
                    return true;
                }
                current = current.parent;
            }
        }

        return false;
    }

    /// <summary>
    /// 收集钻石
    /// </summary>
    private void CollectDiamond()
    {
        if (isCollected) return;

        isCollected = true;

        if (enableDebugLog)
        {
            Debug.Log($"DiamondCollector: 收集钻石！位�?: {transform.position}");
        }

        // 添加得分
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddDiamondScore(transform.position);
        }
        else
        {
            Debug.LogWarning("DiamondCollector: ScoreManager未找到！");
        }

        // 记录收集进度
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CollectDiamond();
        }
        else
        {
            Debug.LogWarning("DiamondCollector: LevelManager未找到！");
        }

        // 销毁钻石对�?
        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
        else
        {
            // 如果不销毁，禁用对象
            gameObject.SetActive(false);
        }
    }
}

