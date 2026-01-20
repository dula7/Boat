using UnityEngine;

/// <summary>
/// 船体水波生成�?
/// 当船移动时，会在水面上产生水波效�?
/// 这个脚本应该挂在船体上，与InteractiveObj配合工作
/// </summary>
public class BoatRippleGenerator : MonoBehaviour
{
    [Header("Ripple Settings")]
    public float rippleUpdateInterval = 0.1f;  // 更新水波的间隔时�?
    public float minMoveDistance = 0.05f;  // 最小移动距离才产生水波

    private Vector3 lastPosition;
    private float lastUpdateTime;
    private Ripple rippleSystem;

    void Start()
    {
        lastPosition = transform.position;
        lastUpdateTime = Time.time;

        // 查找水波系统
        rippleSystem = FindObjectOfType<Ripple>();
        if (rippleSystem == null)
        {
            Debug.LogWarning("BoatRippleGenerator: 未找到Ripple系统�?");
        }
    }

    void Update()
    {
        // 检查是否到了更新时�?
        if (Time.time - lastUpdateTime < rippleUpdateInterval)
            return;

        // 检查船是否移动了足够的距离
        float moveDistance = Vector3.Distance(transform.position, lastPosition);
        if (moveDistance > minMoveDistance)
        {
            // 更新水波
            UpdateRipple();
            lastPosition = transform.position;
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// 更新水波效果
    /// </summary>
    private void UpdateRipple()
    {
        if (rippleSystem == null || rippleSystem.mainCamera == null)
            return;

        // 将船的位置投影到水面�?
        RaycastHit hit;
        Vector3 boatPosition = transform.position;
        
        // 向下发射射线，找到水�?
        if (Physics.Raycast(boatPosition, Vector3.down, out hit, 100f))
        {
            // 检查是否击中了水面（可以根据Tag或Layer判断�?
            if (hit.collider != null)
            {
                // 获取水面的Renderer
                Renderer waterRenderer = hit.collider.GetComponent<Renderer>();
                if (waterRenderer != null)
                {
                    // 计算UV坐标
                    Vector2 uv = hit.textureCoord;
                    
                    // 调用Ripple系统的DrawAt方法（需要将其改为public�?
                    // 或者通过其他方式更新InteractiveRT
                    // 这里我们需要修改Ripple.cs来支持外部调�?
                }
            }
        }
    }
}

