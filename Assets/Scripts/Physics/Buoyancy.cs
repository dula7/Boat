using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    public Transform water;      // 水面物体
    public float buoyancyForce = 15f;  // 浮力强度
    public float waterDrag = 1f;       // 水中阻力
    public float airDrag = 0f;         // 空气中阻力

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 检查水面对象是否已分配
        if (water == null)
        {
            Debug.LogError("Water对象未分配给Buoyancy脚本！");
            enabled = false; // 禁用脚本
        }
    }

    void FixedUpdate()
    {
        if (water == null) return; // 安全检查

        // 获取水面高度
        float waterHeight = water.position.y;
        float objectHeight = transform.position.y;

        if (objectHeight < waterHeight)
        {
            float depth = waterHeight - objectHeight;

            // 浮力计算：深度 × 浮力系数
            Vector3 force = Vector3.up * buoyancyForce * depth;
            rb.AddForce(force, ForceMode.Force);

            // 应用水中阻力
            rb.drag = waterDrag;
        }
        else
        {
            // 物体在水面上时，应用空气阻力
            rb.drag = airDrag;
        }
    }

    /// <summary>
    /// 在编辑器中绘制调试信息
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && water != null)
        {
            Vector3 position = transform.position;
            float waterHeight = water.position.y;

            // 绘制水面位置
            Gizmos.color = Color.cyan;
            Vector3 waterSurfacePos = new Vector3(position.x, waterHeight, position.z);
            Gizmos.DrawWireSphere(waterSurfacePos, 0.3f);

            // 绘制连接线
            Gizmos.DrawLine(position, waterSurfacePos);

            // 绘制当前深度
            if (position.y < waterHeight)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(position, new Vector3(position.x, waterHeight, position.z));
            }
        }
    }
}