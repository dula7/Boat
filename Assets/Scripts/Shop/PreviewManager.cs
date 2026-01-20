using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager Instance { get; private set; }

    [Header("3D 生成点")]
    public Transform spawnPoint;   // 模型生成的位置 (也可以作为旋转的 Pivot 点)

    [Header("UI 组件")]
    // 注意：这个面板在UI层级下的名字是 "PreviewWindow"
    public GameObject previewPanel;

    // 注意：这个按钮是半透明背景色的 "BackgroundBtn"
    public Button closeButton;

    private GameObject currentModel; // 当前正在展示的模型

    void Awake()
    {
        // 单例模式：确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            
            // ⚠️ 关键修复：DontDestroyOnLoad 只能用于根对象
            // 如果当前对象是子对象，需要先将其移到根层级
            if (transform.parent != null)
            {
                transform.SetParent(null);  // 移到根层级
            }
            
            DontDestroyOnLoad(gameObject);  // 跨场景保持，确保场景切换时不会丢失
            
            // 监听场景加载事件，在场景切换后重新查找UI组件
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // 如果已存在实例，销毁当前对象
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        // 取消监听场景加载事件
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;  // 清空 Instance
        }
    }

    void Start()
    {
        // 初始化UI组件引用
        InitializeUIComponents();
    }

    /// <summary>
    /// 场景加载完成时的回调
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 检查 Instance 是否还是自己
        if (Instance != this)
        {
            Debug.LogError("[PreviewManager] 错误！OnSceneLoaded 时 Instance != this！");
        }
        
        // 场景加载后，重新查找UI组件
        InitializeUIComponents();
    }

    /// <summary>
    /// 初始化UI组件引用（从场景中查找）
    /// </summary>
    private void InitializeUIComponents()
    {
        // 如果Inspector中没有手动指定，则自动查找
        if (spawnPoint == null)
        {
            GameObject spawnPointObj = GameObject.Find("ModelSpawnPoint");
            if (spawnPointObj != null)
            {
                spawnPoint = spawnPointObj.transform;
            }
            else
            {
                Debug.LogWarning("[PreviewManager] 未找到 ModelSpawnPoint，请在Inspector中手动指定");
            }
        }

        if (previewPanel == null)
        {
            GameObject panelObj = GameObject.Find("PreviewWindow");
            if (panelObj != null)
            {
                previewPanel = panelObj;
            }
            else
            {
                Debug.LogWarning("[PreviewManager] 未找到 PreviewWindow，请在Inspector中手动指定");
            }
        }

        if (closeButton == null)
        {
            GameObject btnObj = GameObject.Find("BackgroundBtn");
            if (btnObj != null)
            {
                closeButton = btnObj.GetComponent<Button>();
                if (closeButton == null)
                {
                    Debug.LogWarning("[PreviewManager] BackgroundBtn 没有 Button 组件");
                }
            }
            else
            {
                Debug.LogWarning("[PreviewManager] 未找到 BackgroundBtn，请在Inspector中手动指定");
            }
        }

        // 绑定关闭按钮的功能
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePreview);
        }

        // 确保一开始是关闭的
        HidePreview();
    }

    // === 核心功能：展示预览 ===
    public void ShowPreview(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("PreviewManager: ShowPreview 接收到的 prefab 为空");
            return;
        }

        // 检查必要的组件是否存在
        if (previewPanel == null)
        {
            Debug.LogError("PreviewManager: previewPanel 未设置！无法显示预览");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("PreviewManager: spawnPoint 未设置！无法生成模型");
            return;
        }

        // 1. 显示预览面板
        previewPanel.SetActive(true);

        // 2. 清理旧模型
        if (currentModel != null) Destroy(currentModel);

        // 3. 生成新模型
        // 注意：直接生成，暂时不设父物体，避免位置错误
        currentModel = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // 4. (关键) 将模型设为 spawnPoint 的子对象
        // 因为我们的 Ui3DCamera 是围绕 spawnPoint 旋转的，模型必须作为子对象才能正确居中
        currentModel.transform.SetParent(spawnPoint);

        // 5. 重置坐标、旋转、缩放 (非常重要！)
        // 这一步确保模型位置在父对象的中心，并且视野正确
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity; // 强制重置角度
        currentModel.transform.localScale = Vector3.one;            // 强制恢复缩放
    }

    // === 关闭预览 ===
    public void HidePreview()
    {
        // 关闭 UI
        if (previewPanel != null)
        {
            previewPanel.SetActive(false);
        }

        // 清理模型
        if (currentModel != null) Destroy(currentModel);
    }


}

