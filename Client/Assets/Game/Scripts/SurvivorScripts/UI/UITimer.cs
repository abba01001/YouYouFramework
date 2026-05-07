using OctoberStudio.Easing;
using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class UITimer : MonoBehaviour
    {
        [SerializeField] protected TMP_Text timerText;
        protected int lastSeconds = -1;
        protected IEasingCoroutine alphaCoroutine;

        private void Update()
        {
            if (!GameController.IsBattleing) return;
            var timespan = TimeSpan.FromSeconds(StageController.GetDirectorTime());
            if(timespan.Seconds != lastSeconds)
            {
                lastSeconds = timespan.Seconds;
                timerText.text = string.Format("{0:mm\\:ss}", timespan);
                GameController.SaveManager.StageData.Time = (float)StageController.GetDirectorTime();
            }
        }

        public void Show()
        {
            alphaCoroutine.StopIfExists();
            gameObject.SetActive(true);
            alphaCoroutine = timerText.DoAlpha(1, 0.3f);
        }

        public void Hide()
        {
            alphaCoroutine.StopIfExists();
            timerText.DoAlpha(0, 0.3f).SetOnFinish(() => gameObject.SetActive(false));
        }
    }
}