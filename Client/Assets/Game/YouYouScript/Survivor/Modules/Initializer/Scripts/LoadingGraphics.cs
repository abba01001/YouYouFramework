using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Watermelon
{
    public class LoadingGraphics : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI loadingText;
        [SerializeField] Image backgroundImage;
        public static LoadingGraphics Instance;
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.MSetActive(false);
        }

        public void ShowProgress()
        {
            gameObject.MSetActive(true);
            OnLoading(0.0f, "Loading..");
            DOVirtual.Float(0f, 1f, 0.1f,(value) =>
            {
                loadingText.color = new Color(1f, 1f, 1f, value);
                backgroundImage.color = new Color(0.5f, 0.6f, 1f, value);
            });
        }
        
        public void StopProgress()
        {
            OnLoading(1f, "Loading..");
            DOVirtual.Float(1f, 0f, 0.6f,(value) =>
            {
                loadingText.color = new Color(1f, 1f, 1f, value);
                backgroundImage.color = new Color(0.5f, 0.6f, 1f, value);
            }).OnComplete(() =>
            {
                gameObject.MSetActive(false);
            });
        }

        public void UpdateProgress(float progress)
        {
            OnLoading(progress, "Loading..");
        }


        private void OnEnable()
        {
            GameLoading.OnLoading += OnLoading;
            GameLoading.OnLoadingFinished += OnLoadingFinished;
        }

        private void OnDisable()
        {
            GameLoading.OnLoading -= OnLoading;
            GameLoading.OnLoadingFinished -= OnLoadingFinished;
        }

        private void OnLoading(float state, string message)
        {
            int percentage = Mathf.RoundToInt(state * 100);  // 将 0-1 之间的 state 转换为 0-100 之间的百分比
            loadingText.text = message + percentage + "%";   // 显示百分比
        }

        private void OnLoadingFinished()
        {
            loadingText.DOFade(0.0f, 0.6f, unscaledTime: true);
            backgroundImage.DOFade(0.0f, 0.6f, unscaledTime: true).OnComplete(delegate
            {
                Destroy(gameObject);
            });
        }
    }
}
