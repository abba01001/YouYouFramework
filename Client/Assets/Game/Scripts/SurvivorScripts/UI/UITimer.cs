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

        protected IEasingCoroutine alphaCoroutine;
        protected StageSave stageSave;
        private float tempTimer = 0f;
        private IDisposable disposable;
        public void EnableTimer()
        {
            stageSave = GameController.SaveManager.StageData;
            
            timerText.text = ToMMSS(tempTimer);
            stageSave.Time = tempTimer;
            disposable?.Dispose();
            disposable = Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
            {
                tempTimer += 1f;
                timerText.text = ToMMSS(tempTimer);
                stageSave.Time = tempTimer;
            });
        }

        public void StopTimer()
        {
            disposable?.Dispose();
        }
        
        public static string ToMMSS(float totalSeconds)
        {
            // 防止负数
            totalSeconds = Mathf.Max(0, totalSeconds);

            // 计算分钟、秒
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);

            // 格式化为两位数字，不足补0
            return $"{minutes:00}:{seconds:00}";
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