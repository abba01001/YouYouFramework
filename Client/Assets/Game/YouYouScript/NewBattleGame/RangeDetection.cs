using UnityEngine;

public class RangeDetection : MonoBehaviour
{
    public float radius = 5f;  // 圆的半径
    public Color rangeColor = Color.red;  // 圆的颜色
    public LayerMask targetLayer;  // 目标物体所在的层

    // 定义事件：物体进入范围时触发
    public event System.Action<Collider2D> OnObjectEnterRange;
    // 定义事件：物体在范围内停留时触发
    public event System.Action<Collider2D> OnObjectStayInRange;
    // 定义事件：物体离开范围时触发
    public event System.Action<Collider2D> OnObjectExitRange;

    private CircleCollider2D circleCollider2D;

    private void Awake()
    {
        // 获取 CircleCollider2D 组件，如果没有则添加一个
        circleCollider2D = GetComponent<CircleCollider2D>();

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
        // 如果目标物体在目标层中并进入范围，触发事件
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            OnObjectEnterRange?.Invoke(other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 如果目标物体在目标层中并停留在范围内，触发事件
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            OnObjectStayInRange?.Invoke(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 如果目标物体在目标层中并离开范围，触发事件
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            OnObjectExitRange?.Invoke(other);
        }
    }
}