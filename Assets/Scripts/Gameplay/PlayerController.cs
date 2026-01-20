using UnityEngine;

/// <summary>
/// 玩家控制器 - 只负责发射导弹，不控制船体运动
/// 船体运动由BoatController控制
/// 发射方向跟随鼠标位置，从船体上方发射
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float shootForce = 30f;
    [Tooltip("发射冷却时间（秒），0.5秒内最多发射1枚")]
    public float shootCooldown = 0.5f;  // 冷却时间0.5秒

    [Header("Launch Position")]
    public Transform launchPoint;  // 发射点Transform（可选，如果未指定则自动创建）
    public Vector3 launchOffset = new Vector3(0, 1.5f, 0.5f);  // 相对于船体的发射偏移（向上1.5米，向前0.5米）
    public bool autoFindBoat = true;  // 是否自动查找船体
    public bool autoCreateLaunchPoint = true;  // 如果未指定launchPoint，是否自动创建

    [Header("Aiming")]
    public bool useMousePosition = true;  // 是否使用鼠标位置作为瞄准点
    public float maxAimDistance = 1000f;  // 最大瞄准距离

    private Camera mainCamera;
    private Transform boatTransform;  // 船体Transform引用
    private bool launchPointCreated = false;  // 标记是否自动创建了launchPoint
    private float lastShootTime = -1f;  // 上次发射时间（初始化为-1表示从未发射过）

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerController: 未找到主摄像头！");
        }

        // 查找船体
        if (autoFindBoat)
        {
            FindBoat();
        }

        // 设置发射点
        SetupLaunchPoint();
    }

    /// <summary>
    /// 自动查找船体
    /// </summary>
    private void FindBoat()
    {
        if (boatTransform == null)
        {
            BoatController boat = FindObjectOfType<BoatController>();
            if (boat != null)
            {
                boatTransform = boat.transform;
            }
            else
            {
                Debug.LogWarning("PlayerController: 未找到船体！导弹将从摄像机位置发射。");
            }
        }
    }

    /// <summary>
    /// 设置发射点，确保它跟随船体移动
    /// </summary>
    private void SetupLaunchPoint()
    {
        // 如果指定了发射点，确保它是船体的子对象
        if (launchPoint != null)
        {
            // 如果launchPoint不是船体的子对象，尝试找到船体并设置为子对象
            if (boatTransform == null && autoFindBoat)
            {
                FindBoat();
            }

            if (boatTransform != null)
            {
                // 检查launchPoint是否是boatTransform的子对象
                if (!IsChildOf(launchPoint, boatTransform))
                {
                    // 如果不是，将其设置为船体的子对象
                    launchPoint.SetParent(boatTransform, true);
                    Debug.Log("PlayerController: launchPoint已设置为船体的子对象，将跟随船体移动。");
                }
            }
        }
        // 如果没有指定发射点，自动创建一个
        else if (autoCreateLaunchPoint)
        {
            if (boatTransform == null && autoFindBoat)
            {
                FindBoat();
            }

            if (boatTransform != null)
            {
                // 创建发射点GameObject
                GameObject launchPointObj = new GameObject("LaunchPoint");
                launchPoint = launchPointObj.transform;
                
                // 设置为船体的子对象
                launchPoint.SetParent(boatTransform, false);
                
                // 设置本地位置（相对于船体的偏移）
                launchPoint.localPosition = launchOffset;
                launchPoint.localRotation = Quaternion.identity;
                launchPoint.localScale = Vector3.one;
                
                launchPointCreated = true;
                Debug.Log($"PlayerController: 已自动创建launchPoint作为船体的子对象，位置: {launchOffset}");
            }
        }
    }

    /// <summary>
    /// 检查transform是否是parent的子对象（包括子对象的子对象）
    /// </summary>
    private bool IsChildOf(Transform transform, Transform parent)
    {
        if (transform == null || parent == null)
            return false;

        Transform current = transform;
        while (current != null)
        {
            if (current == parent)
                return true;
            current = current.parent;
        }
        return false;
    }

    void Update()
    {
        // 如果自动创建了launchPoint，确保它仍然跟随船体
        if (launchPointCreated && launchPoint != null && boatTransform != null)
        {
            // 如果launchPoint不再是船体的子对象，重新设置
            if (!IsChildOf(launchPoint, boatTransform))
            {
                launchPoint.SetParent(boatTransform, true);
                launchPoint.localPosition = launchOffset;
            }
        }

        HandleShooting();
    }

    /// <summary>
    /// 获取导弹发射位置
    /// </summary>
    private Vector3 GetLaunchPosition()
    {
        // 优先使用指定的发射点
        if (launchPoint != null)
        {
            return launchPoint.position;
        }

        // 如果指定了船体，使用船体位置 + 偏移
        if (boatTransform != null)
        {
            // 将本地偏移转换为世界坐标
            Vector3 worldOffset = boatTransform.TransformDirection(launchOffset);
            return boatTransform.position + worldOffset;
        }

        // 如果都没有，使用摄像头位置（向后兼容）
        if (mainCamera != null)
        {
            return mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 处理导弹发射
    /// </summary>
    void HandleShooting()
    {
        // 鼠标右键发射导弹（GetMouseButtonDown(1) 表示右键）
        if (Input.GetMouseButtonDown(1))
        {
            // 检查冷却时间：如果距离上次发射时间小于冷却时间，则不允许发射
            float timeSinceLastShoot = Time.time - lastShootTime;
            if (lastShootTime >= 0f && timeSinceLastShoot < shootCooldown)
            {
                // 冷却期间，屏蔽发射指令
                return;
            }

            if (bulletPrefab != null && mainCamera != null)
            {
                Vector3 targetPoint;
                Vector3 spawnPos = GetLaunchPosition();

                // 确保船体引用有效
                if (boatTransform == null && autoFindBoat)
                {
                    FindBoat();
                }

                if (useMousePosition)
                {
                    // 使用鼠标位置作为瞄准点
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, maxAimDistance))
                    {
                        targetPoint = hit.point;
                    }
                    else
                    {
                        targetPoint = ray.GetPoint(maxAimDistance);
                    }
                }
                else
                {
                    // 使用屏幕中心
                    Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, maxAimDistance))
                    {
                        targetPoint = hit.point;
                    }
                    else
                    {
                        targetPoint = ray.GetPoint(maxAimDistance);
                    }
                }

                // 计算发射方向（从发射位置指向目标点）
                Vector3 shootDirection = (targetPoint - spawnPos).normalized;

                // 确保方向有效
                if (shootDirection == Vector3.zero)
                {
                    // 如果方向无效，使用船体前方或摄像头前方
                    if (boatTransform != null)
                    {
                        // 船的前进方向是-transform.right
                        shootDirection = -boatTransform.right;
                    }
                    else
                    {
                        shootDirection = mainCamera.transform.forward;
                    }
                }

                // 创建导弹并设置方向
                GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(shootDirection));

                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb == null) bulletRb = bullet.AddComponent<Rigidbody>();

                bulletRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                bulletRb.velocity = shootDirection * shootForce;

                // 设置忽略碰撞（防止导弹一生成就碰撞船体）
                if (boatTransform != null)
                {
                    Collider boatCollider = boatTransform.GetComponent<Collider>();
                    if (boatCollider != null)
                    {
                        Collider bulletCollider = bullet.GetComponent<Collider>();
                        if (bulletCollider == null)
                        {
                            bulletCollider = bullet.AddComponent<SphereCollider>();
                        }
                        Physics.IgnoreCollision(bulletCollider, boatCollider, true);
                    }
                }

                Destroy(bullet, 5f);

                // 更新上次发射时间
                lastShootTime = Time.time;
            }
        }
    }
}
