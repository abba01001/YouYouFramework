using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using FrameWork;
using UnityEngine;
using UnityEngine.UI;


public enum ArrowDirection
{
    up = 90,
    down = -90,
    left = -180,
    right = 0
}

public class FormMask : UIFormBase, ICanvasRaycastFilter
{
    private static Image BlackMask; //遮罩图片
    private static Image GuideMask; //遮罩图片
    private static Material _materia;
    private static GameObject target;
    private static Tweener _topOriCloseTween; // 保存Tween引用
    private static Transform arrow; 
    protected override async UniTask Awake()
    {
        await base.Awake();
        BlackMask = transform.Get<Image>("BlackMask");
        BlackMask.gameObject.MSetActive(false);
        GuideMask = transform.Get<Image>("GuideMask");
        arrow = transform.Get<Transform>("Arrow");
        _materia = GuideMask.material;
        
        _materia.SetVector("_TopOri", new Vector4(0, 0, 2000, 0));
        GuideMask.enabled = false;
        arrow.gameObject.MSetActive(false);
    }

    public static void ShowArrow(GameObject targetTrans,ArrowDirection direction)
    {
        arrow.gameObject.MSetActive(true);
        Vector2 targetPos = GameUtil.GetCenterPosFromTrans(targetTrans.transform,GuideMask.transform.parent);
        arrow.GetComponent<RectTransform>().anchoredPosition = targetPos + GetArrowOffset(direction);
        arrow.eulerAngles = new Vector3(0, 0, (int)direction);
        if (direction == ArrowDirection.down || direction == ArrowDirection.up)
        {
            arrow.DOMoveY(-50, 0.8f).From(true).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public static void ShowMaskAnim(float value,float duration)
    {
        if (value == 1f) BlackMask.gameObject.MSetActive(true);
        BlackMask.DOFade(value, duration).OnComplete(() =>
        {
            if(value == 0f) BlackMask.gameObject.MSetActive(false);
        });
    }
    
    private static Vector2 GetArrowOffset(ArrowDirection direction)
    {
        if (direction == ArrowDirection.down) return new Vector2(0, 250);
        return Vector2.zero;
    }
    
    public static void ShowCircleMsak(GameObject targetTrans,Action action = null)
    {
        if(targetTrans == null) return;
        StopCircleMask();
        ShowBlockMask(true);
        Vector3 targetPos = GameUtil.GetCenterPosFromTrans(targetTrans.transform,GuideMask.transform.parent);
         
        _materia.SetVector("_TopOri", new Vector4(targetPos.x,targetPos.y, 1500, 0));
        _materia.DOVector(new Vector4(targetPos.x,targetPos.y, 100f, 0),"_TopOri",0.6f).OnComplete(() =>
        {
            SetTargetImage(targetTrans);
            action?.Invoke();
        });
    }

    public static void CloseCircleMask(Action action = null)
    {
        arrow.gameObject.MSetActive(false);
        ClearTarget();
        _topOriCloseTween = _materia.DOVector(new Vector4(0, 0, 2000, 0),"_TopOri",0.6f).OnComplete(() =>
        {
            ShowBlockMask(false);
            action?.Invoke();
        });
    }
    
    public static void StopCircleMask()
    {
        // 使用 Kill() 中断动画
        if (_topOriCloseTween != null && _topOriCloseTween.IsActive())
        {
            _topOriCloseTween.Kill(); // 中断动画
        }
    }
    
    public static void ShowBlockMask(bool state)
    {
        GuideMask.enabled = state;
        GuideMask.raycastTarget = state;
        GuideMask.color = state ?new Color(0, 0, 0, 0.6f) : Color.clear;
    }
    
    /// <summary>
    /// 创建圆角矩形区域
    /// </summary>
    /// <param name="pos">矩形的屏幕位置</param>
    /// <param name="pos1">左下角位置</param>
    /// <param name="pos2">右上角位置</param>
    /// <param name="CallBack">回调</param>
    public void CreateCircleRectangleMask(Vector2 pos, Vector2 widthAndHeight, float raid)
    {
        _materia.SetFloat("_MaskType", 2f);
        _materia.SetVector("_Origin", new Vector4(pos.x, pos.y, widthAndHeight.x, widthAndHeight.y));
        _materia.SetFloat("_Raid", raid);
    }
    
    /// <summary>
    /// 创建矩形点击区域
    /// </summary>
    /// <param name="pos">矩形中心点坐标</param>
    /// <param name="widthAndHeight">矩形宽高</param>
    public void CreateRectangleMask(Vector2 pos, Vector2 widthAndHeight, float raid)
    {
        _materia.SetFloat("_MaskType", 1f);
        _materia.SetVector("_Origin", new Vector4(pos.x, pos.y, widthAndHeight.x, widthAndHeight.y));
    }
    
    /// <summary>
    /// 创建双圆形点击区域
    /// </summary>
    /// <param name="pos">大圆形中心点坐标</param>
    /// <param name="rad">大圆形半径</param>
    /// <param name="pos1">小圆形中心点坐标</param>
    /// <param name="rad1">小圆形半径</param>
    public void CreateCircleMask(Vector3 pos, float rad, Vector3 pos1, float rad1)
    {
        _materia.SetFloat("_MaskType", 0f);
        _materia.SetVector("_Origin", new Vector4(pos.x, pos.y, rad, 0));
        _materia.SetVector("_TopOri", new Vector4(pos1.x, pos1.y, rad1, 0));
    }

    /// <summary>
    /// 设置目标不被Mask遮挡
    /// </summary>
    /// <param name="tg">目标</param>
    public static void SetTargetImage(GameObject tg)
    {
        target = tg;
    }

    public static void ClearTarget()
    {
        target = null;
    }
    
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        //没有目标则捕捉事件渗透
        if (target == null)
        {
            return true;
        }

        //在目标范围内做事件渗透
        return !RectTransformUtility.RectangleContainsScreenPoint(target.GetComponent<RectTransform>(),
            sp, eventCamera);
    }
}
