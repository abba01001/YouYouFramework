using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FrameWork;
using Main;
using UnityEngine;
using UnityEngine.UI;


public class TipModel
{
    public string text;
    public float delayClose = 0.8f;
}

/// <summary>
/// 提示窗口
/// </summary>
public class FormTip : UIFormBase
{
    [SerializeField] private Text _text;
    [SerializeField] private RectTransform _bg;
    [SerializeField] private CanvasGroup _canvasGroup;
    protected override void OnEnable()
    {
        base.OnEnable();
        transform.localScale = Vector3.one * 0.7f;
        _text.text = String.Empty;
        _bg.sizeDelta = new Vector2(300, 42);
        _canvasGroup.alpha = 1;
    }

    protected override void OnShow()
    {
        base.OnShow();
        TipModel model = this.userData as TipModel;
        _text.text = model.text;
        _bg.sizeDelta = new Vector2(Mathf.Clamp(_text.preferredWidth + 30, 300, 730), 42);
        _canvasGroup.DOFade(0f, 0.5f).SetDelay(model.delayClose).OnComplete(() =>
        {
            GameEntry.UI.CloseUIForm<FormTip>();
        }).SetEase(Ease.OutQuad);;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuart);
    }
    
}
