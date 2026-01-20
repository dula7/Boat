using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum PageScrollType
{
    Horizontal,   //0
    Vertical      //1
}

public class PageScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    #region 字段
    protected ScrollRect rect;
    protected int pageCount;
    private RectTransform content;
    protected float[] pages;

    public float moveTime = 0.3f;
    private float timer = 0;
    private float startMovePos;
    protected int currentPage = 0;

    private bool isDraging = false;
    private bool isMoving = false;

    // 是否开启自动滚动
    public bool IsAutoScroll;
    public float AutoScrollTime = 2;
    private float AutoScrollTimer = 0;

    public PageScrollType pageScrollType = PageScrollType.Horizontal;   //默认水平滚动

    // 拖动阈值，用于判断是否切换页面
    public float dragThreshold = 0.1f;

    #endregion

    #region 事件
    public Action<int> OnPageChange;       //切换到某一页时触发
    #endregion

    #region Unity回调
    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Update()
    {
        ListenerMove();
        ListenerAutoScroll();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDraging = true;
        isMoving = false; // 拖动时停止自动移动
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDraging = false;
        AutoScrollTimer = 0;

        // 计算应该滚动到的页面
        int targetPage = CalculateTargetPage();
        ScrollToPage(targetPage);
    }
    #endregion

    #region 方法
    public void Init()
    {
        rect = transform.GetComponent<ScrollRect>();
        if (rect == null)
        {
            throw new Exception("未找到ScrollRect组件!");
        }

        content = rect.content; // 直接从ScrollRect获取content，更可靠
        if (content == null)
        {
            throw new Exception("未找到Content组件!");
        }

        pageCount = content.childCount;
        if (pageCount <= 1)
        {
            // 允许只有一页，但禁用滚动功能
            pages = new float[1];
            pages[0] = pageScrollType == PageScrollType.Horizontal ? 0 : 1;
            return;
        }

        pages = new float[pageCount];
        for (int i = 0; i < pages.Length; i++)
        {
            switch (pageScrollType)
            {
                case PageScrollType.Horizontal:
                    pages[i] = i * (1.0f / (pageCount - 1));
                    break;
                case PageScrollType.Vertical:
                    pages[i] = 1 - i * (1.0f / (pageCount - 1));
                    break;
            }
        }
    }

    // 监听移动
    public void ListenerMove()
    {
        if (isMoving && pageCount > 1)
        {
            timer += Time.deltaTime / moveTime;
            timer = Mathf.Clamp01(timer);

            switch (pageScrollType)
            {
                case PageScrollType.Horizontal:
                    rect.horizontalNormalizedPosition = Mathf.Lerp(startMovePos, pages[currentPage], timer);
                    break;
                case PageScrollType.Vertical:
                    rect.verticalNormalizedPosition = Mathf.Lerp(startMovePos, pages[currentPage], timer);
                    break;
            }

            if (timer >= 1)
            {
                isMoving = false;
            }
        }
    }

    // 监听自动滚动
    public void ListenerAutoScroll()
    {
        if (isDraging || !IsAutoScroll || pageCount <= 1)
            return;

        AutoScrollTimer += Time.deltaTime;
        if (AutoScrollTimer >= AutoScrollTime)
        {
            AutoScrollTimer = 0;
            // 滚动到下一页
            currentPage = (currentPage + 1) % pageCount;
            ScrollToPage(currentPage);
        }
    }

    public void ScrollToPage(int page)
    {
        if (pageCount <= 1) return;

        // 确保页面索引在有效范围内
        page = Mathf.Clamp(page, 0, pageCount - 1);
        if (page == currentPage) return;

        isMoving = true;
        currentPage = page;
        timer = 0;

        switch (pageScrollType)
        {
            case PageScrollType.Horizontal:
                startMovePos = rect.horizontalNormalizedPosition;
                break;
            case PageScrollType.Vertical:
                startMovePos = rect.verticalNormalizedPosition;
                break;
        }

        OnPageChange?.Invoke(currentPage);
    }

    // 计算目标页面
    private int CalculateTargetPage()
    {
        if (pageCount <= 1) return 0;

        float currentPos;
        switch (pageScrollType)
        {
            case PageScrollType.Horizontal:
                currentPos = rect.horizontalNormalizedPosition;
                break;
            case PageScrollType.Vertical:
                currentPos = rect.verticalNormalizedPosition;
                break;
            default:
                return currentPage;
        }

        // 计算与当前页的偏移
        float offset = currentPos - pages[currentPage];

        // 如果偏移超过阈值，则切换到相邻页面
        if (offset > dragThreshold && currentPage < pageCount - 1)
        {
            return currentPage + 1;
        }
        else if (offset < -dragThreshold && currentPage > 0)
        {
            return currentPage - 1;
        }

        // 否则返回当前页
        return currentPage;
    }
    #endregion
}