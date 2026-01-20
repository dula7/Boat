using UnityEngine;
using UnityEngine.UI;

public class IndicatorManager : MonoBehaviour
{
    public GameObject indicatorPrefab; // 轮播物体的预制体
    public Transform indicatorParent; // 轮播物体的父级对象
    private int pageCount = 5; // 页面数量

    private GameObject[] indicatorsArray; // 存储所有轮播物体的数组
    private int currentPage = 0; // 当前页面索引
    private Vector2 slideStartPosition; // 记录滑动的起始点
    private bool isSliding = false; // 标志是否正在执行滑动操作

    private bool isAutoSlidingPaused = false; // 标志是否暂停自动轮播
    private float autoSlideInterval = 3f; // 自动轮播间隔时间
    private float autoSlideTimer = 0f; // 计时器，用于自动轮播


    private void Start()
    {
        CreateIndicators();
        UpdateIndicators();
    }

    private void Update()
    {
        // 更新计时器
        UpdateTimer();

        // 检测左右滑动手势
        DetectSwipe();
    }

    // 创建页面指示器
    private void CreateIndicators()
    {
        indicatorsArray = new GameObject[pageCount];

        for (int i = 0; i < pageCount; i++)
        {
            GameObject indicator = Instantiate(indicatorPrefab, indicatorParent);
            indicatorsArray[i] = indicator;
        }
    }


    // 设置当前页面，并更新页面指示器
    private void SetCurrentPage(int pageIndex)
    {
        currentPage = Mathf.Clamp(pageIndex, 0, pageCount - 1);
        UpdateIndicators();
    }


    // 更新页面指示器的显示状态
    private void UpdateIndicators()
    {
        for (int i = 0; i < pageCount; i++)
        {
            // 将当前页面的轮播物体颜色设置为白色，其他页面的轮播物体颜色设置为灰色
            indicatorsArray[i].GetComponent<Image>().color = (i == currentPage) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }


    // 自动轮播方法
    private void AutoSlide()
    {
        // 如果没有正在执行滑动操作且未暂停自动轮播，则切换到下一个页面
        if (!isSliding && !isAutoSlidingPaused)
        {
            SetCurrentPage((currentPage + 1) % pageCount);
        }
    }

    // 检测左右滑动手势
    private void DetectSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 记录滑动的起始点
            if (IsInsideParent(Input.mousePosition))
            {
                slideStartPosition = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            float deltaX = Input.mousePosition.x - slideStartPosition.x;

            // 如果没有正在执行滑动操作且滑动距离足够大，则切换页面
            if (!isSliding && !isAutoSlidingPaused && Mathf.Abs(deltaX) > 50f && IsInsideParent(Input.mousePosition))
            {
                int direction = (deltaX > 0) ? -1 : 1;
                SetCurrentPage((currentPage + direction + pageCount) % pageCount);

                // 标志为正在执行滑动操作
                isSliding = true;

                // 暂停自动轮播
                PauseAutoSlide();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 重置滑动标志
            isSliding = false;

            // 恢复自动轮播
            ResumeAutoSlide();
        }
    }

    // 判断坐标是否在父物体内
    private bool IsInsideParent(Vector2 position)
    {
        RectTransform parentRect = indicatorParent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(parentRect, position);
    }


    // 更新计时器
    private void UpdateTimer()
    {
        // 如果没有正在执行滑动操作且未暂停自动轮播，则更新计时器
        if (!isSliding && !isAutoSlidingPaused)
        {
            autoSlideTimer += Time.deltaTime;

            // 如果计时器超过轮播间隔时间，则执行自动轮播
            if (autoSlideTimer >= autoSlideInterval)
            {
                AutoSlide();

                // 重置计时器
                autoSlideTimer = 0f;
            }
        }
    }

    // 暂停自动轮播
    private void PauseAutoSlide()
    {
        isAutoSlidingPaused = true;
    }

    // 恢复自动轮播，并重置计时器
    private void ResumeAutoSlide()
    {
        isAutoSlidingPaused = false;
        autoSlideTimer = 0f;
    }
}