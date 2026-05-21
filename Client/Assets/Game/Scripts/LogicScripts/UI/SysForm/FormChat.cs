using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class FormChat : UIFormBase
    {
        [SerializeField] private Button closeBtn;
        
        protected override async UniTask Awake()
        {
            await base.Awake();
            closeBtn.SetButtonClick(Close);
        }
        
        public override void Close()
        {
            base.Close();
        }


        protected override void OnShow()
        {
            base.OnShow();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        
    }
}