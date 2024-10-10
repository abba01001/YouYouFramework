using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PageScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private ScrollRect rect; // 滑动组件
    private float targethorizontal = 0; // 目标滑动位置
    private bool isDrag = false; // 是否拖拽结束
    private List<float> posList = new List<float>(); // 每页的临界值
    private bool stopMove = true; // 是否停止移动
    private bool enableAutoMove = false;
    public float sensitivity = 0.2f; // 滑动灵敏度
    private float lerpSpeed = 10f; // Lerp 速度，用于更平滑的滑动过渡
    private float startDragHorizontal; // 开始拖动时的水平位置

    private float autoScrollTime = 5f; // 自动滑动时间间隔
    private float autoScrollTimer; // 自动滑动计时器
    private bool InitFinish = false;
    public int currentPageIndex { get; private set; } // 当前页码
    private Action<int> OnPageChanged; // 滑动完成回调

    public void InitParams(List<GameObject> objs, Action<int> cb, bool autoMove, int moveDirect)
    {
        Vector2 prefabSize = Vector2.zero;
        rect = GetComponent<ScrollRect>();
        if (objs != null)
        {
            foreach (var obj in objs)
            {
                obj.transform.SetParent(rect.content);
                obj.transform.position = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                prefabSize = obj.transform.GetComponent<RectTransform>().sizeDelta;
            }
        }

        rect.content.sizeDelta = new Vector2(prefabSize.x * objs.Count, rect.content.sizeDelta.y);
        rect.velocity = Vector2.one * 100;

        float contentWidth = rect.content.rect.width;
        float viewWidth = GetComponent<RectTransform>().rect.width;
        float horizontalLength = contentWidth - viewWidth;

        // 计算每页的临界值
        posList.Add(0);
        for (int i = 1; i < rect.content.transform.childCount - 1; i++)
        {
            posList.Add(viewWidth * i / horizontalLength);
        }

        posList.Add(1);
        enableAutoMove = autoMove;
        OnPageChanged = cb;
        InitFinish = true;

        if (GetPageCount() <= 1)
        {
            rect.horizontal = false;
            rect.vertical = false;
            enableAutoMove = false;
        }
        else
        {
            rect.horizontal = moveDirect == 0;
            rect.vertical = moveDirect != 0;
        }

        currentPageIndex = 0;
        if (OnPageChanged != null) OnPageChanged(currentPageIndex);
    }

    public int GetPageCount()
    {
        return rect.content.transform.childCount;
    }

    public void ResetScrollTime()
    {
        autoScrollTimer = 0;
    }

    void Update()
    {
        if (!InitFinish) return;

        HandleInputMove();
        HandleAutoMove();
    }

    private void HandleInputMove()
    {
        if (!isDrag && !stopMove)
        {
            // 使用 LerpUnclamped 来实现更加平滑且持续的滑动
            rect.horizontalNormalizedPosition = Mathf.LerpUnclamped(
                rect.horizontalNormalizedPosition,
                targethorizontal,
                Time.deltaTime * lerpSpeed);

            // 如果接近目标位置，则停止滑动
            if (Mathf.Abs(rect.horizontalNormalizedPosition - targethorizontal) < 0.001f)
            {
                stopMove = true;
            }

            if (Mathf.Abs(rect.horizontalNormalizedPosition - targethorizontal) < 0.1f)
            {
                if (OnPageChanged != null) // 调用滑动完成的回调
                    OnPageChanged(currentPageIndex);
            }
        }
    }

    private void HandleAutoMove()
    {
        if (!enableAutoMove) return;
        autoScrollTimer += Time.deltaTime;
        if (autoScrollTimer >= autoScrollTime && !isDrag)
        {
            // 到达自动滑动时间，计算下一个目标页
            int nextIndex = (currentPageIndex + 1) % posList.Count; // 跳转到下一页
            if (nextIndex == 0) // 如果到达最后一页，直接跳转到第一页
            {
                targethorizontal = posList[0]; // 跳转到第一页
                currentPageIndex = 0;
            }
            else
            {
                targethorizontal = posList[nextIndex]; // 设置下一个目标
                currentPageIndex++;
            }

            stopMove = false; // 开始移动
            autoScrollTimer = 0; // 重置计时器
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!InitFinish) return;
        isDrag = true;
        startDragHorizontal = rect.horizontalNormalizedPosition;
        autoScrollTimer = 0; // 重置自动滑动计时器
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!InitFinish) return;

        float posX = rect.horizontalNormalizedPosition;
        float dragDistance = posX - startDragHorizontal;

        // 如果拖动距离超过了一定阈值，根据方向滑动到下一页或上一页
        if (Mathf.Abs(dragDistance) > 0.05f) // 这里设置一个滑动距离阈值 0.1f
        {
            if (dragDistance > 0) // 向左滑动
            {
                currentPageIndex = Mathf.Min(currentPageIndex + 1, posList.Count - 1); // 跳到下一页
            }
            else // 向右滑动
            {
                currentPageIndex = Mathf.Max(currentPageIndex - 1, 0); // 跳到上一页
            }
        }

        // 找到最接近的页面并更新目标滑动位置
        targethorizontal = posList[currentPageIndex];
        isDrag = false;
        stopMove = false;
    }
}