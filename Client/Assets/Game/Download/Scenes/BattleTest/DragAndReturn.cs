using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DragAndReturn : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector3 initialPosition; // 初始位置
    private Vector3 initialRotation; // 初始旋转角度
    private Vector3 initialScale; // 初始缩放
    private RectTransform rectTransform; // RectTransform 组件（如果是UI元素）
    private Vector3 dragOffset; // 拖拽时的偏移量
    private bool isDragging = false; // 是否正在拖拽
    private bool isClickable = true; // 控制是否可以点击
    private bool isRotating = false; // 是否正在旋转
    private int siblingIndex; // 保存初始的兄弟节点索引
    
    private ArrowLine line;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition; // 使用局部位置来计算初始位置
        initialRotation = rectTransform.eulerAngles; // 获取初始角度
        initialScale = rectTransform.localScale; // 获取初始缩放比例
        siblingIndex = transform.GetSiblingIndex();
        line = ArrowEffectManager.Instance.GetArrowLine(transform);
    }

    // 点击事件
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isClickable) return; // 如果不可点击，则返回

        // 禁用点击和拖拽，直到动画结束
        isClickable = false;
        transform.SetAsLastSibling();
        dragOffset = rectTransform.localPosition - (Vector3)eventData.position;
        rectTransform.DOScale(initialScale * 1.2f, 0.2f).SetEase(Ease.OutSine); // 放大卡牌，比例可以根据需要调整
        RotateToZeroAngle();

        isDragging = true;

        line.ShowArrow(true);
        Vector3 localPosition = (Vector3)eventData.position + dragOffset;
        rectTransform.localPosition = localPosition;
        line.UpdateArrowPositions(rectTransform.localPosition);
        line.UpdateTargetSpine();
    }

    // 拖拽事件
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector3 localPosition = (Vector3)eventData.position + dragOffset;
        rectTransform.localPosition = localPosition;
        line.UpdateArrowPositions(rectTransform.localPosition);
    }

    // 松开事件
    public void OnPointerUp(PointerEventData eventData)
    {
        line.ShowArrow(false);
        isDragging = false;
        transform.SetSiblingIndex(siblingIndex);
        
        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(rectTransform.DOLocalMove(initialPosition, 0.5f).SetEase(Ease.OutSine));
        returnSequence.Join(rectTransform.DORotate(new Vector3(0, 0, initialRotation.z), 0.5f).SetEase(Ease.OutSine));
        returnSequence.Join(rectTransform.DOScale(initialScale, 0.2f).SetEase(Ease.OutSine));
        returnSequence.OnKill(() =>
        {
            rectTransform.localPosition = initialPosition;
            rectTransform.localScale = initialScale; // 确保缩放恢复
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
