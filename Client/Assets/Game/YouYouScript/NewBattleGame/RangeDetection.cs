using System.Collections.Generic;
using UnityEngine;

public class RangeDetection : MonoBehaviour
{
    public float radius = 5f; // 圆的半径
    public Color rangeColor = Color.red; // 圆的颜色
    public LayerMask targetLayer; // 目标物体所在的层

    public event System.Action<List<Collider2D>> OnObjectsEnterRange;
    public event System.Action<List<Collider2D>> OnObjectsStayInRange;
    public event System.Action<List<Collider2D>> OnObjectsExitRange;

    private CircleCollider2D circleCollider2D;
    private HashSet<Collider2D> objectsInRange = new HashSet<Collider2D>();
    private List<Collider2D> tempEnterList = new List<Collider2D>();
    private List<Collider2D> tempExitList = new List<Collider2D>();

    private void Awake()
    {
        // 获取 CircleCollider2D 组件，如果没有则添加一个
        circleCollider2D = GetComponent<CircleCollider2D>();
        if (circleCollider2D == null)
        {
            circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
        }

        // 设置碰撞体的半径和触发器模式
        circleCollider2D.radius = radius;
        circleCollider2D.isTrigger = true;
    }

    private void OnDrawGizmos()
    {
        // 绘制圆圈范围
        Gizmos.color = rangeColor;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsInTargetLayer(other))
        {
            if (objectsInRange.Add(other)) // 确保对象是第一次进入
            {
                tempEnterList.Clear();
                tempEnterList.Add(other);
                OnObjectsEnterRange?.Invoke(tempEnterList); // 传递所有新进入的对象
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsInTargetLayer(other))
        {
            // 每帧触发停留事件，传递当前范围内的所有对象
            var stayingObjects = new List<Collider2D>(objectsInRange);
            OnObjectsStayInRange?.Invoke(stayingObjects);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (objectsInRange.Remove(other)) // 从集合中移除
        {
            tempExitList.Clear();
            tempExitList.Add(other);
            OnObjectsExitRange?.Invoke(tempExitList); // 传递所有离开的对象
        }
    }

    private bool IsInTargetLayer(Collider2D collider)
    {
        return ((1 << collider.gameObject.layer) & targetLayer) != 0;
    }
}
