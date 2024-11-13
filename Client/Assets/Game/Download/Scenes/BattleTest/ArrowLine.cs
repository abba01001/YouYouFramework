using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowLine : MonoBehaviour
{
    public GameObject lineObj;
    public GameObject arrowObj;
    public int MaxCount = 18;
    public Vector3 startPoint; // 起始点
    private Vector3 controlPoint1; // 控制点1
    private Vector3 controlPoint2; // 控制点2
    public Vector3 endPoint; // 目标点（Spine 动画的位置）

    public bool IsIdle => transform.localScale == Vector3.zero;
    private List<GameObject> ItemList = new List<GameObject>();

    // 存储所有Spine的Transform数组
    private GameObject[] allSpineTransforms;
    private Transform targetSpine; // 当前指向的目标Spine
    private float timer = 0;
    private float interval = 0.5f;

    public void ShowArrow(bool bo)
    {
        transform.localScale = bo ? Vector3.one : Vector3.zero;
    }

    public void InitData(Transform parent,GameObject[] enemys)
    {
        for (int i = 0; i < MaxCount; i++)
        {
            GameObject Arrow = (i == MaxCount - 1) ? arrowObj : lineObj;
            GameObject temp = Instantiate(Arrow, parent);
            ItemList.Add(temp);
        }
        transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        transform.localPosition = Vector3.zero;
        arrowObj.gameObject.SetActive(false);
        lineObj.gameObject.SetActive(false);
        allSpineTransforms = enemys;
    }

    private void Update()
    {
        if (transform.localScale == Vector3.zero) return;
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            UpdateTargetSpine();
            timer = 0;
        }

        if (ItemList.Count > 0 && targetSpine != null)
        {
            foreach (var line in ItemList)
            {
                DrawBezierCurve();
            }
        }
    }

    // 更新箭头的起点和目标点，并计算控制点
    public void UpdateArrowPositions(Vector3 cardCenter)
    {
        startPoint = cardCenter; // 每次拖拽时，将箭头的起点更新为卡牌的中心点
        if (targetSpine != null)
        {
            // 更新目标点
            Vector3 targetPosition = targetSpine.position;

            // 计算控制点，形成曲线路径
            controlPoint1 = (Vector2) startPoint + (targetPosition - startPoint) * new Vector2(-0.28f, 0.8f);
            controlPoint2 = (Vector2) startPoint + (targetPosition - startPoint) * new Vector2(0.12f, 1.4f);

            controlPoint1.z = startPoint.z; // 确保控制点和起点、终点有相同的 z 值
            controlPoint2.z = startPoint.z;
        }
    }
    
    // 更新目标Spine
    public void UpdateTargetSpine()
    {
        if (allSpineTransforms == null || allSpineTransforms.Length == 0 || transform.localScale == Vector3.zero)
        {
            return;
        }

        // 假设我们选择距离当前箭头起点最近的Spine
        Transform closestSpine = null;
        float minDistance = Mathf.Infinity;
        var t1 = startPoint;// / 150f;
        foreach (var spine in allSpineTransforms)
        {
            float distance = Vector3.Distance(t1, spine.transform.position);
            if (minDistance == Mathf.Infinity) minDistance = distance;
            GameUtil.LogError($"{spine.transform.name}==距离{distance}");
            if (distance <= minDistance)
            {
                minDistance = distance;
                closestSpine = spine.transform;
            }
        }
        
        if (closestSpine != null)
        {
            targetSpine = closestSpine;

            // 转换目标Spine位置到画布坐标系
            Vector3 worldPosition = targetSpine.position; // 获取目标Spine的世界坐标
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition); // 转换为屏幕坐标
            RectTransform canvasRectTransform = this.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, Camera.main,
                out localPoint);
            endPoint = localPoint + new Vector2(0, 250); // 根据需求微调坐标
        }
    }

    // 绘制贝塞尔曲线，并让箭头沿着路径移动
    public void DrawBezierCurve()
    {
        for (int i = 0; i < ItemList.Count; i++)
        {
            float t = i / (float) (ItemList.Count - 1); // t 是从 0 到 1 的进度值
            Vector3 position = BezierUtils.CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, endPoint);
            ItemList[i].transform.localPosition = position;

            float scaleFactor = Mathf.Lerp(0.3f, 1f, t); // 根据 t 值平滑缩放
            ItemList[i].transform.localScale = Vector3.one * scaleFactor;

            // 计算箭头的旋转角度
            if (i > 0)
            {
                Vector3 direction = ItemList[i].transform.localPosition -
                                    ItemList[i - 1].transform.localPosition;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                ItemList[i].transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
}
