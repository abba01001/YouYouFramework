using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DragAndReturn : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector3 initialPosition; // 初始位置
    private Vector3 initialRotation; // 初始旋转角度
    private RectTransform rectTransform; // RectTransform 组件（如果是UI元素）
    private Vector3 dragOffset; // 拖拽时的偏移量
    private bool isDragging = false; // 是否正在拖拽
    private bool isClickable = true; // 控制是否可以点击
    private bool isRotating = false; // 是否正在旋转
    private int siblingIndex; // 保存初始的兄弟节点索引
    
    private Vector3 startPoint; // 起点（卡牌中心）
    private Vector3 endPoint; // 终点（屏幕中心）

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition; // 使用局部位置来计算初始位置
        initialRotation = rectTransform.eulerAngles; // 获取初始角度
        siblingIndex = transform.GetSiblingIndex();
        
        // 获取屏幕中心点
        endPoint = new Vector3(Screen.width / 2, Screen.height / 2, 0); // 屏幕中心（以像素为单位）
        startPoint = rectTransform.localPosition; // 起点为卡牌中心
    }

    // 点击事件
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isClickable) return; // 如果不可点击，则返回

        // 禁用点击和拖拽，直到动画结束
        isClickable = false;
        transform.SetAsLastSibling();
        // 计算点击时的物体局部位置偏移
        dragOffset = rectTransform.localPosition - (Vector3)eventData.position;

        // 旋转卡牌到正角度，并确保是最短路径
        RotateToZeroAngle();

        isDragging = true;

        ArrowEffectManager.Instance.ShowArrow(true);
        Vector3 localPosition = (Vector3)eventData.position + dragOffset;
        rectTransform.localPosition = localPosition;
        ArrowEffectManager.Instance.UpdateArrowPositions(rectTransform.localPosition);
        ArrowEffectManager.Instance.UpdateTargetSpine();
    }

    // 拖拽事件
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector3 localPosition = (Vector3)eventData.position + dragOffset;
        rectTransform.localPosition = localPosition;
        ArrowEffectManager.Instance.UpdateArrowPositions(rectTransform.localPosition);
    }

    // 松开事件
    public void OnPointerUp(PointerEventData eventData)
    {
        ArrowEffectManager.Instance.ShowArrow(false);
        isDragging = false;
        transform.SetSiblingIndex(siblingIndex);
        // 使用 DOTween 动画来平滑返回卡牌的位置和角度
        Sequence returnSequence = DOTween.Sequence();

        // 创建路径动画，使卡牌平滑回到初始位置
        returnSequence.Append(rectTransform.DOLocalMove(initialPosition, 0.5f).SetEase(Ease.OutSine)); // 返回初始位置

        // 同时使用动画将角度平滑恢复
        returnSequence.Join(rectTransform.DORotate(new Vector3(0, 0, initialRotation.z), 0.5f).SetEase(Ease.OutSine));

        // 在动画结束时确保卡牌精确返回初始位置
        returnSequence.OnKill(() =>
        {
            rectTransform.localPosition = initialPosition;
            // 动画结束后恢复点击
            isClickable = true; // 重新启用点击
            isRotating = false; // 重置旋转状态
        });
    }

    // 计算并旋转到正角度
    private void RotateToZeroAngle()
    {
        float currentAngle = rectTransform.eulerAngles.z;
        float targetAngle = 0f;

        // 计算最短旋转路径
        float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);

        // 使用 DOTween 旋转到目标角度
        rectTransform.DORotate(new Vector3(0, 0, currentAngle + deltaAngle), 0.3f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic);
    }
}
