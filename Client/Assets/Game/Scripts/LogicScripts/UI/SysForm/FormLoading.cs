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
        [SerializeField] private TextMeshProUGUI sliderText;
        [SerializeField] private TextMeshProUGUI verText;
        [SerializeField] private Slider progressBar;

        // private void OnLoadingProgressChange(object userdata)
        // {
        //     float value = (float) userdata;
        //     sliderText.text= string.Format("{0}%", Math.Floor(value * 100));
        //     progressBar.value = (float)Math.Floor(value * 100);
        // }
        
        private void OnLoadingProgressChange(object userdata)
        {
            float rawValue = Mathf.Clamp01((float)userdata);
            int progress = Mathf.RoundToInt(rawValue * 100);

            // 文字始终保持白色，但根据 5 个阶段改变其“明度（Alpha/Brightness）”
            string alpha;
            if (progress <= 20)      alpha = "#88FFFFFF"; // 半透明白
            else if (progress <= 40) alpha = "#AAFFFFFF"; 
            else if (progress <= 60) alpha = "#CCFFFFFF"; 
            else if (progress <= 80) alpha = "#EEFFFFFF"; 
            else                     alpha = "#FFFFFF";   // 100% 纯白全亮

            // 提示文字稍微暗一点，形成对比
            string tipColor = "#FFFFFF";

            sliderText.text = $"<color={tipColor}>资源载入中 </color><color={alpha}>{progress}%</color>";
            progressBar.value = progress;
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            GameEntry.Event.AddEventListener(Constants.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
            progressBar.value = 0f;
            ShowVersion();
        }

        private void ShowVersion()
        {
            var instance = CheckVersionCtrl.Instance;
            string labelColor = "#FFFFFF"; // 标签：冷灰色
            string valueColor = "#FFFFFF"; // 数值：纯白
            string updateColor = "#5EEAD4"; // 发现更新：青翠色（更有科技感）

            string local = string.IsNullOrEmpty(instance.LocalPackageVersion) 
                ? $"v{Application.version}" 
                : $"v{instance.LocalPackageVersion}";

            string remote = string.IsNullOrEmpty(instance.RemotePackageVersion) 
                ? "---" 
                : $"v{instance.RemotePackageVersion}";

            bool isUpdate = !string.IsNullOrEmpty(instance.RemotePackageVersion) && (instance.LocalPackageVersion != instance.RemotePackageVersion);
            string remoteFinalColor = isUpdate ? updateColor : valueColor;

            verText.text = 
                $"<color={labelColor}> 当前版本：</color><color={valueColor}>{local}</color>\n" +
                $"<color={labelColor}> 云端版本：</color><color={remoteFinalColor}>{remote}</color>";
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameEntry.Event.RemoveEventListener(Constants.EventName.LoadingSceneUpdate, OnLoadingProgressChange);
            progressBar.value = 0f;
        }
    }
}