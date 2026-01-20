using UnityEngine;

/// <summary>
/// 第三人称摄像头跟随系统
/// 摄像头会跟随船体移动，可以跟随鼠标旋转视角，但不影响船体运动
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;  // 要跟随的目标（船体）

    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0, 5, -10);  // 相对于目标的偏移量（世界坐标）
    public float followSpeed = 5f;  // 跟随速度
    public bool useLocalOffset = false;  // 是否使用相对于目标方向的偏移

    [Header("Camera Rotation")]
    public float mouseSensitivity = 2f;  // 鼠标灵敏度
    public bool allowMouseRotation = true;  // 是否允许鼠标旋转视角（默认开启）
    public float minVerticalAngle = -30f;  // 最小垂直角度
    public float maxVerticalAngle = 60f;  // 最大垂直角度
    public bool lookAtTarget = false;  // 是否始终看向目标（如果允许鼠标旋转，应该关闭）

    private Vector3 currentVelocity;
    private Vector3 initialOffset;  // 保存初始偏移量
    private float currentHorizontalAngle = 0f;  // 当前水平角度
    private float currentVerticalAngle = 0f;  // 当前垂直角度

    void Start()
    {
        // 如果没有指定目标，尝试自动查找船体
        if (target == null)
        {
            // 查找带有BoatController的对象
            BoatController boat = FindObjectOfType<BoatController>();
            if (boat != null)
            {
                target = boat.transform;
            }
        }

        if (target == null)
        {
            Debug.LogError("CameraFollow: 未找到跟随目标！请在Inspector中指定target。");
            enabled = false;
            return;
        }

        // 保存初始偏移量（基于当前摄像头位置）
        if (useLocalOffset)
        {
            // 如果使用本地偏移，计算相对于目标方向的偏移
            initialOffset = target.InverseTransformDirection(transform.position - target.position);
        }
        else
        {
            // 使用世界坐标偏移
            initialOffset = offset;
        }

        // 计算初始角度（基于当前摄像头位置）
        Vector3 direction = (transform.position - target.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);
        
        // 计算水平角度（Y轴旋转）
        currentHorizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        
        // 计算垂直角度（X轴旋转）
        float horizontalDistance = Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z);
        currentVerticalAngle = Mathf.Atan2(direction.y, horizontalDistance) * Mathf.Rad2Deg;

        // 初始化摄像头位置
        UpdateCameraPosition();
    }

    /// <summary>
    /// 更新摄像头位置和旋转
    /// </summary>
    private void UpdateCameraPosition()
    {
        // 处理鼠标旋转
        if (allowMouseRotation)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // 更新角度
            currentHorizontalAngle += mouseX;
            currentVerticalAngle -= mouseY;  // 注意：Y轴反转
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        // 计算旋转后的偏移量
        Quaternion horizontalRotation = Quaternion.Euler(0, currentHorizontalAngle, 0);
        Quaternion verticalRotation = Quaternion.Euler(currentVerticalAngle, 0, 0);
        Quaternion totalRotation = horizontalRotation * verticalRotation;
        
        // 计算偏移量的长度
        float offsetDistance = initialOffset.magnitude;
        Vector3 normalizedOffset = initialOffset.normalized;
        
        // 应用旋转
        Vector3 rotatedOffset = totalRotation * normalizedOffset * offsetDistance;

        // 计算目标位置
        Vector3 targetPosition = target.position + rotatedOffset;

        // 平滑移动到目标位置
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / followSpeed);

        // 让摄像头看向目标
        Vector3 lookDirection = (target.position - transform.position).normalized;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 更新摄像头位置
        UpdateCameraPosition();
    }
}
