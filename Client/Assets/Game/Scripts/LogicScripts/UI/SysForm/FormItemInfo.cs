using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class FormItemInfo : UIFormBase
    {
        [SerializeField] private Button closeBtn;
        protected override void OnEnable()
        {
            base.OnEnable();
            closeBtn.SetButtonClick(Close);
        }


        protected override void OnShow()
        {
            base.OnShow();

        }
    }
}