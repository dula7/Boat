using UnityEngine;

public class Ui3DCamera : MonoBehaviour
{
    // 显示的目标
    public Transform target;
    // 物体围绕旋转的点
    public Transform pivot;
    // 与旋转的的偏移值
    public Vector3 pivotOffset = Vector3.zero;

    // 摄像机距离目标的距离
    public float distance = 10.0f;
    // 最短、最长距离
    public float minDistance = 2f;
    public float maxDistance = 15f;
    // 缩放速度
    public float zoomSpeed = 1f;

    // x，y轴的手动旋转速度
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;

    //新增：自动旋转速度
    [Header("自动旋转设置")]
    public float autoRotateSpeed = 20f; // 自转速度，正数向左转，负数向右转

    // y轴的最大、最小偏移值
    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;
    // y轴方向是不是允许旋转
    public bool allowYTilt = true;

    // 记录相机与 最后要移动的距离
    public float targetDistance;
    // 记录摄像机的x,y轴旋转
    private float x = 0.0f;
    private float y = 0.0f;
    // 记录摄像机的x,y轴旋转的目标值
    private float targetX = 0f;
    private float targetY = 0f;
    // x,y相对缓冲减速
    private float xVelocity = 1f;
    private float yVelocity = 1f;
    // 缩放相对缓冲减速
    private float zoomVelocity = 1f;


    private void Start()
    {
        // --- 修正初始视角的逻辑 ---

        // 1. 强制设定水平角度 (Yaw
        targetX = x = 135f;

        // 2. 强制设定垂直角度 (Pitch)
        targetY = y = 30f;

        // 3. 确保距离是对的
        targetDistance = distance;

        // 4. (重要) 立即应用一次位置，防止第一帧闪烁
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + pivot.position + pivotOffset;
        transform.rotation = rotation;
        transform.position = position;
    }


    private void LateUpdate()
    {
        // 如果没有旋转点，不执行旋转和缩放
        if (!pivot) return;

        // --- 1. 缩放逻辑 (滚轮) ---
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.0f) targetDistance -= zoomSpeed;
        else if (scroll < 0.0f) targetDistance += zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        // --- 2. 旋转逻辑 (核心修改部分) ---

        // 如果按下左键：手动控制
        if (Input.GetMouseButton(0))
        {
            // 获取水平方向的偏移值 (手动拖拽)
            targetX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;

            // 如果允许Y轴偏移，获取鼠标在y轴上的偏移
            if (allowYTilt)
            {
                targetY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                targetY = Mathf.Clamp(targetY, yMinLimit, yMaxLimit);
            }
        }
        else
        {
            // 这里我们只改变 targetX (水平角度)，实现绕Y轴自转
            targetX += autoRotateSpeed * Time.deltaTime;
        }

        // --- 3. 平滑应用 ---
        // Mathf.SmoothDampAngle 会自动处理从“手动”到“自动”的平滑过渡
        x = Mathf.SmoothDampAngle(x, targetX, ref xVelocity, 0.3f);
        y = allowYTilt ? Mathf.SmoothDampAngle(y, targetY, ref yVelocity, 0.3f) : targetY;
        distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, 0.5f);

        // --- 4. 设置位置和角度 ---
        // 注意：Quaternion.Euler(x, y, z) -> 这里的y对应Pitch(垂直)，x对应Yaw(水平)
        // 原脚本的变量命名有点反直觉（x变量控制的是Yaw，y变量控制的是Pitch），但我保留了原逻辑以免出错
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + pivot.position + pivotOffset;

        transform.rotation = rotation;
        transform.position = position;
    }

}