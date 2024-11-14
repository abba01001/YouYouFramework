using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class BaseCard : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    protected Vector3 initialPosition; // 初始位置
    protected Vector3 initialRotation; // 初始旋转角度
    protected Vector3 initialScale; // 初始缩放
    protected RectTransform rectTransform; // RectTransform 组件
    protected Vector3 dragOffset; // 拖拽时的偏移量
    protected bool isDragging = false; // 是否正在拖拽
    protected bool isClickable = true; // 是否可以点击
    private int siblingIndex; // 保存初始的兄弟节点索引
    protected ArrowLine line; // 箭头效果
    protected bool EnableShowArrowLine { get; set; } // 控制是否显示箭头，默认为显示
    private bool InitComplete = false;

    public void InitTrans()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition;
        initialRotation = rectTransform.eulerAngles;
        initialScale = rectTransform.localScale;
        siblingIndex = transform.GetSiblingIndex();
        EnableShowArrowLine = true;
        InitComplete = true;
    }

    // 点击事件
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!InitComplete) return;
        if (!isClickable) return;
        isClickable = false; // 禁止其他点击操作
        transform.SetAsLastSibling(); // 将卡牌置于最上层
        dragOffset = rectTransform.localPosition - (Vector3) eventData.position;
        rectTransform.DOScale(initialScale * 1.2f, 0.2f).SetEase(Ease.OutSine);
        RotateToZeroAngle();
        isDragging = true;
        Vector3 localPosition = (Vector3) eventData.position + dragOffset;
        rectTransform.localPosition = localPosition;
        HandleLineArrow(1);
    }

    //state 0释放，1点击，2拖拽
    private void HandleLineArrow(int state)
    {
        if (!InitComplete) return;
        if (!EnableShowArrowLine) return;
        // 如果箭头效果需要显示
        if (line == null && state is 1 or 2) line = ArrowEffectManager.Instance.GetArrowLine(transform);
        if (state == 1)
        {
            line.ShowArrow(true);
            line.transform.SetAsLastSibling();
            line.UpdateArrowPositions(rectTransform.localPosition);
            line.UpdateTargetSpine();
        }

        if (state == 2)
        {
            line.ShowArrow(true);
            line.UpdateArrowPositions(rectTransform.localPosition);
        }

        if (state == 0)
        {
            line.ShowArrow(false); // 隐藏箭头
            line = null; // 释放箭头
        }
    }

    // 拖拽事件
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!InitComplete) return;
        if (!isDragging) return;
        HandleLineArrow(2);
        Vector3 localPosition = (Vector3) eventData.position + dragOffset;
        rectTransform.localPosition = localPosition;
    }

    // 松开事件
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!InitComplete) return;
        if (line == null) return;
        HandleLineArrow(0);

        isDragging = false;
        transform.SetSiblingIndex(siblingIndex); // 恢复原来的兄弟节点顺序

        // 恢复卡牌的初始位置、旋转、缩放
        Sequence returnSequence = DOTween.Sequence();
        returnSequence.Append(rectTransform.DOLocalMove(initialPosition, 0.5f).SetEase(Ease.OutSine));
        returnSequence.Join(rectTransform.DORotate(new Vector3(0, 0, initialRotation.z), 0.5f).SetEase(Ease.OutSine));
        returnSequence.Join(rectTransform.DOScale(initialScale, 0.2f).SetEase(Ease.OutSine));

        returnSequence.OnKill(() =>
        {
            rectTransform.localPosition = initialPosition;
            rectTransform.localScale = initialScale; // 确保缩放恢复
            isClickable = true; // 重新启用点击
        });
    }

    // 旋转到目标角度
    protected void RotateToZeroAngle()
    {
        float currentAngle = rectTransform.eulerAngles.z;
        float targetAngle = 0f;

        float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);
        rectTransform.DORotate(new Vector3(0, 0, currentAngle + deltaAngle), 0.3f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic);
    }
}