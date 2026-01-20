using UnityEngine;

/// <summary>
/// 导弹/投射物脚本
/// 处理导弹的碰撞和破碎效果
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public string protectedTag = "Unbreakable"; // 只有带有这个标签的才不会破碎
    public string landTag = "Land"; // 陆地标签，不能破碎

    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // FixedUpdate 是物理系统每一帧
    void FixedUpdate()
    {
        // 让导弹在飞，如果速度足够快
        if (rb != null && rb.velocity.sqrMagnitude > 1f)
        {
            // 导弹朝前（Z轴）方向，永远朝着当前的速度方向
            // 这样导弹向上飞时头朝上，俯冲时头朝下，非常真实
            transform.forward = rb.velocity.normalized;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. 先检查是不是"受保护"的物体（比如船体）
        // 如果碰撞的物体（或它的父对象）有"受保护"的标签，就只销毁导弹，不做任何事
        if (collision.gameObject.CompareTag(protectedTag) ||
           (collision.transform.parent != null && collision.transform.parent.CompareTag(protectedTag)))
        {
            Destroy(gameObject);
            return; // 直接结束，不执行下面的代码
        }

        // 2. 检查是否是Land标签，如果是，只销毁导弹，不触发破碎
        if (collision.gameObject.CompareTag(landTag) ||
           (collision.transform.parent != null && collision.transform.parent.CompareTag(landTag)))
        {
            // 陆地不能破碎，只销毁导弹
            Destroy(gameObject);
            return;
        }

        // 3. 如果没有被保护标签，就默认为"可破碎"的
        // 开始寻找网格
        Transform hitTransform = collision.transform;
        MeshFilter mf = hitTransform.GetComponent<MeshFilter>();

        // 有时候网格在子对象上，比如石头Prefab
        if (mf == null)
        {
            mf = hitTransform.GetComponentInChildren<MeshFilter>();
        }

        // 4. 只要有网格，就执行破碎
        if (mf != null)
        {
            GameObject targetObj = mf.gameObject;

            // 检查是否是Destructible标签（只有原始对象是Destructible时才加分，碎屑不加分）
            // 注意：碎屑即使继承了标签，也不应该加分，所以只检查原始对象
            bool isDestructible = targetObj.CompareTag("Destructible") ||
                                 (targetObj.transform.parent != null && targetObj.transform.parent.CompareTag("Destructible"));
            
            // 检查是否是碎屑（碎屑的名称通常包含"Shard"或"Core"）
            bool isShard = targetObj.name.Contains("Shard") || targetObj.name.Contains("Core");
            
            // 只有原始对象（不是碎屑）且是Destructible标签时才加分
            bool shouldAddScore = isDestructible && !isShard;

            if (VoronoiShatter.Instance != null)
            {
                VoronoiShatter.Instance.TriggerFracture(targetObj, collision.contacts[0].point);
                
                // 如果是Destructible标签的原始对象（不是碎屑），添加击碎得分
                if (shouldAddScore && ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddRockBreakScore(collision.contacts[0].point);
                }
            }
        }

        // 销毁导弹
        Destroy(gameObject);
    }
}
