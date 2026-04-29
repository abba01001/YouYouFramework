using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using GameScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class FormLoading : UIFormBase
    {
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;
        [SerializeField] private Slider progressBar;

        private float m_TargetProgress;

        private void OnLoadingProgressChange(object value)
        {
            float varFloat = (float) value;
            textMeshProUGUI.text= string.Format("{0}%", Math.Floor(varFloat * 100));
            // txtTip.text = string.Format("正在进入场景, 加载进度 {0}%", Math.Floor(varFloat * 100));
            progressBar.value = (float)Math.Floor(varFloat * 100);
            // m_value.sizeDelta = new Vector2(varFloat * m_valueBg.sizeDelta.x, 42);
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            GameEntry.Event.AddEventListener(Constants.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
            progressBar.value = 0f;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameEntry.Event.RemoveEventListener(Constants.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
            progressBar.value = 0f;
        }
    }
}