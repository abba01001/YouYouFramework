using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    [DisallowMultipleComponent]
    public class AnimPanel : MonoBehaviour
    {
        private float BegScale;
        private Button m_Button;

        void Awake()
        {
            BegScale = transform.localScale.x;
            GameUtil.LogError("1111111");
        }

        private void OnEnable()
        {
            transform.localScale = BegScale * Vector3.one * 1.05f;
            transform.DOScale(BegScale, 0.05f).SetUpdate(true);
        }
    }
}