using System;
using System.Collections.Generic;
using UnityEngine;

public class ArrowEffectManager : MonoBehaviour
{
    // 静态实例（单例）
    public static ArrowEffectManager Instance { get; private set; }

    public GameObject reticleBlock;
    public GameObject reticleArrow;
    public int MaxCount = 18;
    public Vector3 startPoint; // 起始点
    private Vector3 controlPoint1; // 控制点1
    private Vector3 controlPoint2; // 控制点2
    public Vector3 endPoint; // 目标点（Spine 动画的位置）

    private List<GameObject> ArrowItemList;
    private Animator Arrow_anim;
    private bool isSelect;

    private Camera mainCamera;

    // 存储所有Spine的Transform数组
    public Transform[] allSpineTransforms;
    private Transform targetSpine; // 当前指向的目标Spine

    // 最小距离阈值，只有距离大于此值时才更新目标
    public float minDistanceThreshold = 1f;

    public bool IsSelect
    {
        get => isSelect;
        set
        {
            if (isSelect != value)
            {
                isSelect = value;
                if (value)
                {
                    //PlayAnim();
                }
            }
        }
    }

    public void ShowArrow(bool bo)
    {
        transform.localScale = bo ? Vector3.one : Vector3.zero;
    }
    
    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        mainCamera = Camera.main; // 缓存 Camera.main 来提高性能
        InitData();
    }

    private void InitData()
    {
        ArrowItemList = new List<GameObject>();
        for (int i = 0; i < MaxCount; i++)
        {
            GameObject Arrow = (i == MaxCount - 1) ? reticleArrow : reticleBlock;
            GameObject temp = Instantiate(Arrow, this.transform);
            if (i == MaxCount - 1)
            {
                Arrow_anim = temp.GetComponent<Animator>();
            }
            ArrowItemList.Add(temp);
        }
    }

    private float timer = 0;
    private float interval = 0.5f;
    private void Update()
    {
        if(transform.localScale == Vector3.zero) return;
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            UpdateTargetSpine();
            timer = 0;
        }
        if (ArrowItemList.Count > 0 && targetSpine != null)
        {
            DrawBezierCurve();
        }
    }

    // 更新目标Spine
    public void UpdateTargetSpine()
    {
        if (allSpineTransforms.Length == 0)
        {
            return;
        }

        // 假设我们选择距离当前箭头起点最近的Spine
        Transform closestSpine = null;
        float minDistance = Mathf.Infinity;
        var t1 = startPoint / 150f;
        foreach (var spine in allSpineTransforms)
        {
            float distance = Vector2.Distance(t1,spine.position);
            if (minDistance == Mathf.Infinity) minDistance = distance;
            if (distance <= minDistance)
            {
                minDistance = distance;
                closestSpine = spine;
            }
            GameUtil.LogError($"spine名{spine.transform.name}===距离{distance}");
        }

        if (closestSpine != null)
        {
            targetSpine = closestSpine;

            // 转换目标Spine位置到画布坐标系
            Vector3 worldPosition = targetSpine.position;  // 获取目标Spine的世界坐标
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);  // 转换为屏幕坐标
            RectTransform canvasRectTransform = this.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, Camera.main, out localPoint);
            endPoint = localPoint + new Vector2(0, 250);  // 根据需求微调坐标
        }
    }

    // 绘制贝塞尔曲线，并让箭头沿着路径移动
    private void DrawBezierCurve()
    {
        for (int i = 0; i < ArrowItemList.Count; i++)
        {
            float t = i / (float)(ArrowItemList.Count - 1); // t 是从 0 到 1 的进度值
            Vector3 position = CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, endPoint);
            ArrowItemList[i].transform.localPosition = position;

            float scaleFactor = Mathf.Lerp(0.3f, 1f, t); // 根据 t 值平滑缩放
            ArrowItemList[i].transform.localScale = Vector3.one * scaleFactor;

            // 计算箭头的旋转角度
            if (i > 0)
            {
                Vector3 direction = ArrowItemList[i].transform.localPosition - ArrowItemList[i - 1].transform.localPosition;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                ArrowItemList[i].transform.rotation = Quaternion.Euler(0, 0, angle);
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
            controlPoint1 = (Vector2)startPoint + (targetPosition - startPoint) * new Vector2(-0.28f, 0.8f);
            controlPoint2 = (Vector2)startPoint + (targetPosition - startPoint) * new Vector2(0.12f, 1.4f);

            controlPoint1.z = startPoint.z;  // 确保控制点和起点、终点有相同的 z 值
            controlPoint2.z = startPoint.z;
        }
    }

    // 设置目标点（Spine 动画的位置）
    public void SetEndPos(Vector3 pos)
    {
        endPoint = pos;
    }

    // 计算贝塞尔曲线中的某个点
    public static Vector3 CalculateBezierPoint(float t, Vector3 startPoint, Vector3 controlPoint1,
        Vector3 controlPoint2, Vector3 endPoint)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * startPoint;
        point += 3 * uu * t * controlPoint1;
        point += 3 * u * tt * controlPoint2;
        point += ttt * endPoint;

        return point;
    }
}