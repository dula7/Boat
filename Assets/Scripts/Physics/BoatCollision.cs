using UnityEngine;

/// <summary>
/// 船体碰撞检测系统
/// 检测船体与land、destructible标签的碰撞，触发游戏结束
/// 改进碰撞体设置，防止船头穿模
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BoatCollision : MonoBehaviour
{
    [Header("Collision Settings")]
    public string[] lethalTags = { "Land", "Destructible" };  // 致命标签
    public float minCollisionVelocity = 0.1f;  // 最小碰撞速度（降低以提高灵敏度）

    [Header("Collider Settings")]
    public bool autoSetupCollider = true;  // 自动设置碰撞体
    public Vector3 colliderSize = Vector3.one;  // 碰撞体大小（如果自动设置）
    public Vector3 colliderCenter = Vector3.zero;  // 碰撞体中心偏移
    public float colliderPadding = 0.2f;  // 碰撞体额外填充（防止穿模）

    [Header("Debug")]
    public bool enableDebugLog = true;  // 是否启用调试日志

    [Header("References")]
    public GameOverManager gameOverManager;  // 游戏结束管理器（可选，会自动查找）

    private Rigidbody rb;
    private bool isDead = false;
    private Collider boatCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 设置碰撞体
        SetupCollider();

        // 设置Rigidbody碰撞检测模式为Continuous，防止高速穿模
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // 自动查找游戏结束管理器
        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
        }

        if (gameOverManager == null)
        {
            Debug.LogWarning("BoatCollision: 未找到GameOverManager！游戏结束功能可能无法正常工作。");
        }
        else if (enableDebugLog)
        {
            Debug.Log("BoatCollision: 已找到GameOverManager");
        }
    }

    /// <summary>
    /// 设置碰撞体，防止穿模
    /// </summary>
    private void SetupCollider()
    {
        boatCollider = GetComponent<Collider>();
        
        if (boatCollider == null && autoSetupCollider)
        {
            // 尝试根据渲染器自动计算碰撞体大小
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                
                // 计算本地空间的碰撞体大小和中心
                Vector3 localSize = transform.InverseTransformVector(bounds.size);
                Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
                
                // 添加填充，特别是船头方向（通常是Z轴或X轴）
                // 船的前进方向是-transform.right，所以需要在前方（-X方向）增加填充
                localSize.x += colliderPadding * 2f;  // 前后方向
                localSize.y += colliderPadding;  // 上下方向
                localSize.z += colliderPadding;  // 左右方向
                
                boxCollider.size = localSize;
                boxCollider.center = localCenter;
                
                boatCollider = boxCollider;
                if (enableDebugLog)
                {
                    Debug.Log($"BoatCollision: 自动创建碰撞体，大小: {localSize}, 中心: {localCenter}");
                }
            }
            else
            {
                // 如果没有渲染器，使用默认大小
                BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = colliderSize;
                boxCollider.center = colliderCenter;
                boatCollider = boxCollider;
                Debug.LogWarning("BoatCollision: 船体没有Renderer，使用默认碰撞体大小。");
            }
        }
        else if (boatCollider != null && autoSetupCollider)
        {
            // 如果已有碰撞体，尝试调整大小防止穿模
            if (boatCollider is BoxCollider)
            {
                BoxCollider boxCollider = boatCollider as BoxCollider;
                Vector3 currentSize = boxCollider.size;
                
                // 增加碰撞体大小，特别是船头方向
                currentSize.x += colliderPadding * 2f;
                currentSize.y += colliderPadding;
                currentSize.z += colliderPadding;
                
                boxCollider.size = currentSize;
                if (enableDebugLog)
                {
                    Debug.Log($"BoatCollision: 调整现有碰撞体大小，新大小: {currentSize}");
                }
            }
        }

        // 确保碰撞体不是Trigger
        if (boatCollider != null)
        {
            boatCollider.isTrigger = false;
            if (enableDebugLog)
            {
                Debug.Log($"BoatCollision: 碰撞体已设置，IsTrigger: {boatCollider.isTrigger}");
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // 调试信息：记录所有碰撞
        if (enableDebugLog)
        {
            Debug.Log($"BoatCollision: 碰撞检测 - 对象: {collision.gameObject.name}, 标签: {collision.gameObject.tag}, 速度: {rb.velocity.magnitude}");
        }

        // 检查碰撞速度（降低阈值以提高灵敏度）
        if (rb.velocity.magnitude < minCollisionVelocity)
        {
            if (enableDebugLog)
            {
                Debug.Log($"BoatCollision: 速度太低 ({rb.velocity.magnitude} < {minCollisionVelocity})，忽略碰撞");
            }
            return;
        }

        // 检查碰撞对象的标签
        bool isLethal = false;
        string hitTag = "";

        // 检查碰撞对象本身的标签
        string collisionTag = collision.gameObject.tag;
        if (enableDebugLog)
        {
            Debug.Log($"BoatCollision: 检查标签 - 碰撞对象标签: '{collisionTag}'");
        }

        foreach (string tag in lethalTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                isLethal = true;
                hitTag = tag;
                if (enableDebugLog)
                {
                    Debug.Log($"BoatCollision: 匹配到致命标签: {tag}");
                }
                break;
            }
        }

        // 也检查父对象的标签（如果碰撞对象是子对象）
        if (!isLethal && collision.transform.parent != null)
        {
            string parentTag = collision.transform.parent.tag;
            if (enableDebugLog)
            {
                Debug.Log($"BoatCollision: 检查父对象标签: '{parentTag}'");
            }

            foreach (string tag in lethalTags)
            {
                if (collision.transform.parent.CompareTag(tag))
                {
                    isLethal = true;
                    hitTag = tag;
                    if (enableDebugLog)
                    {
                        Debug.Log($"BoatCollision: 父对象匹配到致命标签: {tag}");
                    }
                    break;
                }
            }
        }

        // 检查根对象的标签（如果碰撞对象在深层嵌套中）
        if (!isLethal)
        {
            Transform root = collision.transform.root;
            if (root != collision.transform && root != collision.transform.parent)
            {
                string rootTag = root.tag;
                if (enableDebugLog)
                {
                    Debug.Log($"BoatCollision: 检查根对象标签: '{rootTag}'");
                }

                foreach (string tag in lethalTags)
                {
                    if (root.CompareTag(tag))
                    {
                        isLethal = true;
                        hitTag = tag;
                        if (enableDebugLog)
                        {
                            Debug.Log($"BoatCollision: 根对象匹配到致命标签: {tag}");
                        }
                        break;
                    }
                }
            }
        }

        if (isLethal)
        {
            if (enableDebugLog)
            {
                Debug.Log($"BoatCollision: 触发游戏结束！标签: {hitTag}");
            }
            HandleDeath(collision, hitTag);
        }
        else if (enableDebugLog)
        {
            Debug.Log($"BoatCollision: 碰撞对象不是致命标签，忽略。碰撞对象标签: '{collisionTag}'");
        }
    }

    /// <summary>
    /// 处理死亡
    /// </summary>
    private void HandleDeath(Collision collision, string hitTag)
    {
        isDead = true;

        if (enableDebugLog)
        {
            Debug.Log($"BoatCollision: 处理死亡 - 标签: {hitTag}");
        }

        // 停止船体运动
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 禁用船体控制
        BoatController boatController = GetComponent<BoatController>();
        if (boatController != null)
        {
            boatController.enabled = false;
        }

        // 显示游戏结束界面
        if (gameOverManager != null)
        {
            string reason = "";
            if (hitTag == "Land")
            {
                reason = "撞到了陆地！";
            }
            else if (hitTag == "Destructible")
            {
                reason = "撞到了障碍物！";
            }
            else
            {
                reason = "撞到了障碍物！";
            }

            if (enableDebugLog)
            {
                Debug.Log($"BoatCollision: 调用GameOverManager.ShowGameOver - 原因: {reason}");
            }

            gameOverManager.ShowGameOver(reason);
        }
        else
        {
            Debug.LogWarning($"BoatCollision: GameOverManager为空！无法显示游戏结束界面。撞到了: {hitTag}");
        }
    }
}
