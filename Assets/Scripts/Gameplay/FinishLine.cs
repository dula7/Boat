using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 缁堢偣妫€娴嬬郴缁?
/// 妫€娴嬭埞浣撳埌杈剧粓鐐癸紝瑙﹀彂鑳滃埄
/// </summary>
public class FinishLine : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("缁堢偣鏍囩锛堢敤浜庤瘑鍒粓鐐瑰璞★級")]
    public string finishLineTag = "Finish";  // 终点标签（默认：Finish）
    
    [Header("Detection")]
    [Tooltip("妫€娴嬫ā寮忥細Trigger锛堣Е鍙戝櫒锛夋垨 Collision锛堢鎾烇級")]
    public DetectionMode detectionMode = DetectionMode.Trigger;
    
    [Tooltip("妫€娴嬪崐寰勶紙鐢ㄤ簬Sphere妫€娴嬫ā寮忥級")]
    public float detectionRadius = 5f;
    
    [Tooltip("鏄惁鎸佺画妫€娴嬶紙姣忓抚妫€娴嬶級")]
    public bool continuousDetection = false;

    [Header("References")]
    [Tooltip("娓告垙缁撴潫绠＄悊鍣紙鍙€夛紝浼氳嚜鍔ㄦ煡鎵撅級")]
    public GameOverManager gameOverManager;

    [Header("Debug")]
    [Tooltip("鏄惁鍚敤璋冭瘯鏃ュ織")]
    public bool enableDebugLog = true;

    private bool hasReachedFinish = false;  // 鏄惁宸插埌杈剧粓鐐?
    private Transform boatTransform;  // 鑸逛綋Transform寮曠敤
    private bool isInitialized = false;  // 是否已初始化

    public enum DetectionMode
    {
        Trigger,      // 浣跨敤Trigger妫€娴嬶紙鎺ㄨ崘锛?
        Collision,    // 浣跨敤纰版挒妫€娴?
        Sphere        // 浣跨敤鐞冨舰鑼冨洿妫€娴?
    }

    void Awake()
    {
        // 监听场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 取消监听场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // 鑷姩鏌ユ壘鑸逛綋
        BoatController boat = FindObjectOfType<BoatController>();
        if (boat != null)
        {
            boatTransform = boat.transform;
        }
        else
        {
            Debug.LogWarning("FinishLine: 鏈壘鍒拌埞浣擄紒");
        }

        // 鑷姩鏌ユ壘娓告垙缁撴潫绠＄悊鍣?
        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
        }

        if (gameOverManager == null)
        {
            Debug.LogWarning("FinishLine: 鏈壘鍒癎ameOverManager锛佽儨鍒╁姛鑳藉彲鑳芥棤娉曟甯稿伐浣溿€?");
        }
        else if (enableDebugLog)
        {
            Debug.Log("FinishLine: 宸叉壘鍒癎ameOverManager");
        }

        // 鏍规嵁妫€娴嬫ā寮忚缃鎾炰綋
        // 使用协程延迟初始化，确保所有系统都已初始化完成
        StartCoroutine(DelayedInitialize());
    }

    /// <summary>
    /// 场景加载完成时的回调
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重置状态，准备重新初始化
        isInitialized = false;
        hasReachedFinish = false;
        gameOverManager = null;
        boatTransform = null;
        
        // 延迟重新初始化，确保新场景的所有对象都已加载
        StartCoroutine(DelayedInitialize());
    }

    /// <summary>
    /// 延迟初始化，确保所有系统都已初始化完成
    /// </summary>
    private System.Collections.IEnumerator DelayedInitialize()
    {
        // 等待一帧，确保所有 Awake 和 Start 都执行完毕
        yield return null;
        
        // 等待最多 30 帧（约 0.5 秒），确保动态加载的船体也已加载
        // LevelInitializer 可能需要更多时间加载船体
        int maxWaitFrames = 30;
        int waitCount = 0;
        
        while (waitCount < maxWaitFrames)
        {
            // 尝试查找船体
            if (boatTransform == null)
            {
                BoatController boat = FindObjectOfType<BoatController>();
                if (boat != null)
                {
                    boatTransform = boat.transform;
                    if (enableDebugLog)
                    {
                        Debug.Log($"FinishLine: 找到船体: {boat.name}");
                    }
                }
            }

            // 尝试查找 GameOverManager
            if (gameOverManager == null)
            {
                gameOverManager = FindObjectOfType<GameOverManager>();
                if (gameOverManager != null && enableDebugLog)
                {
                    Debug.Log($"FinishLine: 找到 GameOverManager: {gameOverManager.name}");
                }
            }

            // GameOverManager 找到后就可以继续，船体可以稍后找到
            if (gameOverManager != null)
            {
                // 如果船体也找到了，可以提前退出
                if (boatTransform != null)
                {
                    break;
                }
            }

            yield return null;
            waitCount++;
        }

        // 最终检查
        if (boatTransform == null)
        {
            Debug.LogWarning("FinishLine: 未找到船体！将在触发时重新查找。");
        }
        else if (enableDebugLog)
        {
            Debug.Log($"FinishLine: 已找到船体: {boatTransform.name}");
        }

        if (gameOverManager == null)
        {
            Debug.LogWarning("FinishLine: 未找到 GameOverManager，胜利功能可能无法正常工作。");
        }
        else if (enableDebugLog)
        {
            Debug.Log("FinishLine: 初始化完成，已找到 GameOverManager");
        }

        // 根据检测模式设置碰撞体
        SetupCollider();
        
        isInitialized = true;
        
        // 如果船体还没找到，启动持续检查协程
        if (boatTransform == null)
        {
            StartCoroutine(ContinuousBoatCheck());
        }
    }

    /// <summary>
    /// 持续检查船体（用于动态加载的船体）
    /// </summary>
    private System.Collections.IEnumerator ContinuousBoatCheck()
    {
        // 持续检查最多 5 秒（300 帧）
        int maxWaitFrames = 300;
        int waitCount = 0;
        
        while (boatTransform == null && waitCount < maxWaitFrames)
        {
            BoatController boat = FindObjectOfType<BoatController>();
            if (boat != null)
            {
                boatTransform = boat.transform;
                if (enableDebugLog)
                {
                    Debug.Log($"FinishLine: 持续检查中找到船体: {boat.name}");
                }
                yield break; // 找到了，退出协程
            }
            
            yield return null;
            waitCount++;
        }
        
        if (boatTransform == null && enableDebugLog)
        {
            Debug.LogWarning("FinishLine: 持续检查 5 秒后仍未找到船体，将在触发时重新查找。");
        }
    }

    /// <summary>
    /// 鏍规嵁妫€娴嬫ā寮忚缃鎾炰綋
    /// </summary>
    private void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        
        if (detectionMode == DetectionMode.Trigger)
        {
            // 濡傛灉娌℃湁纰版挒浣擄紝娣诲姞涓€涓狟oxCollider
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider>();
            }
            
            // 璁剧疆涓篢rigger
            col.isTrigger = true;
            
            if (enableDebugLog)
            {
                Debug.Log($"FinishLine: 宸茶缃甌rigger妯″紡锛岀鎾炰綋绫诲瀷: {col.GetType().Name}");
            }
        }
        else if (detectionMode == DetectionMode.Collision)
        {
            // 纭繚涓嶆槸Trigger
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider>();
            }
            col.isTrigger = false;
            
            if (enableDebugLog)
            {
                Debug.Log($"FinishLine: 宸茶缃瓹ollision妯″紡锛岀鎾炰綋绫诲瀷: {col.GetType().Name}");
            }
        }
        // Sphere妯″紡涓嶉渶瑕佺鎾炰綋锛屼娇鐢ㄨ窛绂绘娴?
    }

    void Update()
    {
        // 濡傛灉宸插埌杈剧粓鐐癸紝涓嶅啀妫€娴?
        if (hasReachedFinish) return;

        // Sphere妯″紡锛氭瘡甯ф娴嬭窛绂?
        if (detectionMode == DetectionMode.Sphere)
        {
            if (boatTransform != null)
            {
                float distance = Vector3.Distance(transform.position, boatTransform.position);
                if (distance <= detectionRadius)
                {
                    OnReachFinish();
                }
            }
        }
        // 濡傛灉鍚敤鎸佺画妫€娴嬶紝鍦║pdate涓篃浼氭鏌ワ紙涓昏鐢ㄤ簬璋冭瘯锛?
        else if (continuousDetection && detectionMode == DetectionMode.Trigger)
        {
            // Trigger妯″紡閫氬父涓嶉渶瑕佸湪Update涓娴嬶紝OnTriggerEnter浼氳嚜鍔ㄥ鐞?
        }
    }

    /// <summary>
    /// Trigger妫€娴嬶細褰撹埞浣撹繘鍏ョ粓鐐瑰尯鍩?
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (hasReachedFinish) return;

        // 如果还未初始化完成，尝试重新查找
        if (!isInitialized)
        {
            TryReinitialize();
        }

        // 如果 GameOverManager 为空，尝试重新查找
        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
            if (gameOverManager != null && enableDebugLog)
            {
                Debug.Log($"FinishLine: OnTriggerEnter 中重新找到 GameOverManager: {gameOverManager.name}");
            }
        }

        // 如果 boatTransform 为空，尝试重新查找（船体可能是动态加载的）
        if (boatTransform == null)
        {
            BoatController boat = FindObjectOfType<BoatController>();
            if (boat != null)
            {
                boatTransform = boat.transform;
                if (enableDebugLog)
                {
                    Debug.Log($"FinishLine: OnTriggerEnter 中重新找到船体: {boat.name}");
                }
            }
        }

        if (enableDebugLog)
        {
            Debug.Log($"FinishLine: OnTriggerEnter - 对象: {other.gameObject.name}, 标签: {other.gameObject.tag}, FinishLine标签: {gameObject.tag}");
        }

        // 检查是否是船体
        bool isBoat = IsBoat(other.gameObject);
        // 检查是否是终点对象
        bool isFinish = IsFinishObject(other.gameObject);

        if (enableDebugLog)
        {
            Debug.Log($"FinishLine: 检测结果 - IsBoat: {isBoat}, IsFinish: {isFinish}");
        }

        // 检查是否是船体，并且碰撞对象（或当前对象）有Finish标签
        if (isBoat && isFinish)
        {
            if (enableDebugLog)
            {
                Debug.Log($"FinishLine: 妫€娴嬪埌鑸逛綋杩涘叆缁堢偣鍖哄煙锛佸璞?: {other.gameObject.name}");
            }
            OnReachFinish();
        }
    }

    /// <summary>
    /// 纰版挒妫€娴嬶細褰撹埞浣撶鎾炲埌缁堢偣
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (hasReachedFinish) return;

        if (detectionMode == DetectionMode.Collision)
        {
            // 如果 GameOverManager 为空，尝试重新查找
            if (gameOverManager == null)
            {
                gameOverManager = FindObjectOfType<GameOverManager>();
                if (gameOverManager != null && enableDebugLog)
                {
                    Debug.Log($"FinishLine: OnCollisionEnter 中重新找到 GameOverManager: {gameOverManager.name}");
                }
            }

            // 检查是否是船体，并且碰撞对象（或当前对象）有Finish标签
            if (IsBoat(collision.gameObject) && IsFinishObject(collision.gameObject))
            {
                if (enableDebugLog)
                {
                    Debug.Log($"FinishLine: 妫€娴嬪埌鑸逛綋纰版挒缁堢偣锛佸璞?: {collision.gameObject.name}");
                }
                OnReachFinish();
            }
        }
    }

    /// <summary>
    /// 检查是否是船体
    /// </summary>
    private bool IsBoat(GameObject obj)
    {
        if (obj == null) return false;

        // 方式1：直接检查碰撞对象是否有BoatController组件（最可靠）
        // 这样即使 boatTransform 为空也能检测到船体
        BoatController boatController = obj.GetComponent<BoatController>();
        if (boatController != null)
        {
            // 如果 boatTransform 为空，现在更新它
            if (boatTransform == null)
            {
                boatTransform = obj.transform;
                if (enableDebugLog)
                {
                    Debug.Log($"FinishLine: 在 IsBoat 中更新 boatTransform: {obj.name}");
                }
            }
            return true;
        }

        // 方式2：检查是否是船体的子对象（如果 boatTransform 已设置）
        if (boatTransform != null)
        {
            Transform current = obj.transform;
            while (current != null)
            {
                if (current == boatTransform)
                {
                    return true;
                }
                current = current.parent;
            }
        }
        else
        {
            // 如果 boatTransform 为空，尝试在整个层级中查找 BoatController
            Transform current = obj.transform;
            while (current != null)
            {
                boatController = current.GetComponent<BoatController>();
                if (boatController != null)
                {
                    boatTransform = current;
                    if (enableDebugLog)
                    {
                        Debug.Log($"FinishLine: 在 IsBoat 层级查找中更新 boatTransform: {current.name}");
                    }
                    return true;
                }
                current = current.parent;
            }
        }

        return false;
    }

    /// <summary>
    /// 检查是否是终点对象（通过标签）
    /// </summary>
    private bool IsFinishObject(GameObject obj)
    {
        if (obj == null) return false;

        // 方式1：检查当前对象（挂载FinishLine脚本的对象）是否有Finish标签
        // 这是推荐的方式：FinishLine脚本挂载在终点对象上，终点对象有"Finish"标签
        if (gameObject.CompareTag(finishLineTag))
        {
            return true;
        }

        // 方式2：检查碰撞对象是否有Finish标签
        // 如果船体撞到任何有"Finish"标签的物体，也会触发
        if (obj.CompareTag(finishLineTag))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 尝试重新初始化（在触发时如果引用丢失）
    /// </summary>
    private void TryReinitialize()
    {
        // 重新查找船体
        if (boatTransform == null)
        {
            BoatController boat = FindObjectOfType<BoatController>();
            if (boat != null)
            {
                boatTransform = boat.transform;
                if (enableDebugLog)
                {
                    Debug.Log($"FinishLine: 重新找到船体: {boat.name}");
                }
            }
        }

        // 重新查找 GameOverManager
        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
            if (gameOverManager != null && enableDebugLog)
            {
                Debug.Log($"FinishLine: 重新找到 GameOverManager: {gameOverManager.name}");
            }
        }
    }

    /// <summary>
    /// 到达终点时的处理
    /// </summary>
    private void OnReachFinish()
    {
        if (hasReachedFinish) return;

        // 如果 GameOverManager 为空，尝试重新查找
        if (gameOverManager == null)
        {
            TryReinitialize();
        }

        // 如果仍然找不到，记录错误
        if (gameOverManager == null)
        {
            Debug.LogError("FinishLine: 无法找到 GameOverManager！胜利界面无法显示。请检查场景中是否有 GameOverManager 对象。");
            return;
        }

        hasReachedFinish = true;

        if (enableDebugLog)
        {
            Debug.Log("FinishLine: 鑸逛綋鍒拌揪缁堢偣锛佽Е鍙戣儨鍒╋紒");
        }

        // 鍋滄鑸逛綋杩愬姩
        if (boatTransform != null)
        {
            Rigidbody rb = boatTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 閫氱煡LevelManager宸插埌杈剧粓鐐?
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetReachedFinish();
            int stars = LevelManager.Instance.GetCurrentStars();
            
            // 通关奖励：成功通过一关获得100积分
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddVictoryReward();
            }
            
            // 鏄剧ず鑳滃埄鐣岄潰锛屽寘鍚槦绾т俊鎭?
            if (gameOverManager != null)
            {
                string victoryMessage = $"成功到达终点！\n获得 {stars} 星评价！\n通关奖励：100积分";
                gameOverManager.ShowVictory(victoryMessage);
            }
            else
            {
                Debug.LogWarning("FinishLine: GameOverManager涓虹┖锛佹棤娉曟樉绀鸿儨鍒╃晫闈€€?");
            }
        }
        else
        {
            // 濡傛灉娌℃湁LevelManager锛屼娇鐢ㄩ粯璁ゆ秷鎭?
            if (gameOverManager != null)
            {
                gameOverManager.ShowVictory("鎴愬姛鍒拌揪缁堢偣锛?");
            }
            else
            {
                Debug.LogWarning("FinishLine: GameOverManager涓虹┖锛佹棤娉曟樉绀鸿儨鍒╃晫闈€€?");
            }
        }
    }

    /// <summary>
    /// 鍦⊿cene瑙嗗浘涓粯鍒舵娴嬭寖鍥达紙浠匰phere妯″紡锛?
    /// </summary>
    void OnDrawGizmos()
    {
        if (detectionMode == DetectionMode.Sphere)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
        else
        {
            // 缁樺埗纰版挒浣撹寖鍥?
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = Color.green;
                if (col is BoxCollider)
                {
                    BoxCollider bc = col as BoxCollider;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(bc.center, bc.size);
                }
                else if (col is SphereCollider)
                {
                    SphereCollider sc = col as SphereCollider;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(sc.center, sc.radius);
                }
            }
        }
    }
}

