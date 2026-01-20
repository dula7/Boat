// 文件名：BoatController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    public float moveSpeed = 25f;
    public float turnSpeed = 3.5f;
    public float maxSpeed = 40f;
    public float stabilityForce = 2f;  // 稳定性参数
    public float stabilityDamping = 0.5f;  // 稳定性阻尼，用于快速恢复平衡
    public float maxTiltAngle = 15f;  // 最大倾斜角度，超过此角度会强制恢复

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 调整这些参数可以获得更真实的水上移动效果
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // 降低重心提高稳定性
        rb.angularDrag = 1f;  // 增加角阻力防止过度旋转

        // 从设置中加载速度参数
        LoadSettings();
    }

    /// <summary>
    /// 从SettingsManager或PlayerPrefs加载速度设置
    /// </summary>
    private void LoadSettings()
    {
        // 优先从SettingsManager读取
        if (SettingsManager.Instance != null)
        {
            moveSpeed = SettingsManager.Instance.GetMoveSpeed();
            turnSpeed = SettingsManager.Instance.GetTurnSpeed();
            Debug.Log($"BoatController: 已从SettingsManager加载设置 - 移动速度: {moveSpeed}, 转向速度: {turnSpeed}");
        }
        else
        {
            // 如果SettingsManager不存在，从PlayerPrefs读取
            float savedMoveSpeed = PlayerPrefs.GetFloat("BoatMoveSpeed", moveSpeed);
            float savedTurnSpeed = PlayerPrefs.GetFloat("BoatTurnSpeed", turnSpeed);
            
            moveSpeed = savedMoveSpeed;
            turnSpeed = savedTurnSpeed;
            
            Debug.Log($"BoatController: 已从PlayerPrefs加载设置 - 移动速度: {moveSpeed}, 转向速度: {turnSpeed}");
        }
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical"); // W/S 控制垂直移动
        turnInput = Input.GetAxis("Horizontal"); // A/D 控制转向
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyStability();
        LimitSpeed();
    }

    /// <summary>
    /// 应用移动力
    /// </summary>
    private void ApplyMovement()
    {
        // 前进/后退
        Vector3 moveForce = -transform.right * moveInput * moveSpeed; // 船w轴x负方向为前进
        rb.AddForce(moveForce, ForceMode.Acceleration);

        // 转向
        if (Mathf.Abs(moveInput) > 0.1f) // 只在移动时转向
        {
            float turnTorque = turnInput * turnSpeed;
            rb.AddTorque(0f, turnTorque, 0f, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// 限制最大速度
    /// </summary>
    private void LimitSpeed()
    {
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    /// <summary>
    /// 应用稳定性，防止船体倾斜
    /// </summary>
    private void ApplyStability()
    {
        // 方法1: 预测上方向量并施加恢复扭矩
        Vector3 predictedUp = Quaternion.AngleAxis(
            rb.angularVelocity.magnitude * Mathf.Rad2Deg * stabilityForce / moveSpeed,
            rb.angularVelocity
        ) * transform.up;

        Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(torqueVector * stabilityForce * stabilityForce, ForceMode.Acceleration);

        // 方法2: 直接检查倾斜角度，如果倾斜过大，强制恢复
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
        if (tiltAngle > maxTiltAngle)
        {
            // 计算恢复扭矩，使船体快速回到平衡状态
            Vector3 currentUp = transform.up;
            Vector3 targetUp = Vector3.up;
            Vector3 correctionTorque = Vector3.Cross(currentUp, targetUp);
            
            // 根据倾斜角度调整恢复强度
            float correctionStrength = (tiltAngle / maxTiltAngle) * stabilityForce * 2f;
            rb.AddTorque(correctionTorque * correctionStrength, ForceMode.Acceleration);
        }

        // 方法3: 添加角速度阻尼，减少不必要的旋转（只保留Y轴旋转用于转向）
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, new Vector3(0, rb.angularVelocity.y, 0), stabilityDamping * Time.fixedDeltaTime);
    }
}
