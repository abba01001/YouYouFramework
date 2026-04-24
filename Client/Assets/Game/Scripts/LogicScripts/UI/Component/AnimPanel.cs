using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AnimPanel : MonoBehaviour
{
    private float BegScale;
    private Button m_Button;

    void Awake()
    {
        BegScale = transform.localScale.x;
    }

    private void OnEnable()
    {
        DoAnim();
    }

    public void DoAnim()
    {
        transform.localScale = BegScale * Vector3.one * 1.05f;
        transform.DOScale(BegScale, 0.05f).SetUpdate(true);
    }
}