using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


namespace FrameWork
{
    [RequireComponent(typeof(Canvas))] //脚本依赖
    [RequireComponent(typeof(GraphicRaycaster))] //脚本依赖
    public class UIFormBase : MonoBehaviour
    {
        public Sys_UIFormEntity SysUIForm { get; private set; }

        public Canvas CurrCanvas { get; private set; }

        public float CloseTime { get; private set; }

        public string Name { get; private set; }

        //打开时调用
        public static Action ActionOpen;

        //反切时调用
        public Action OnBack;
        public Action OnClose;

        //是否活跃
        internal bool IsActive = true;

        protected object userData;

        private readonly Vector2 DEFAULT_POSITION = new Vector2(0, 0);
        private readonly Vector2 HIDE_POSITION = new Vector2(0, -2000);
        protected Image fadeImage;
        protected RectTransform panelRectTransform;

        protected virtual async UniTask Awake()
        {
            Name = transform.name;
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
            CurrCanvas = GetComponent<Canvas>();
            if (transform.Find("Fade"))
            {
                fadeImage = transform.Find("Fade").GetComponent<Image>();
            }

            if (transform.Find("Shadow"))
            {
                panelRectTransform = transform.Find("Shadow").GetComponent<RectTransform>();
            }
        }

        protected async virtual void Start()
        {
            await UniTask.Yield();

            //这里是禁用所有按钮的导航功能，因为用不上, 还可能有意外BUG
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Navigation navigation = buttons[i].navigation;
                navigation.mode = Navigation.Mode.None;
                buttons[i].navigation = navigation;
            }
        }

        protected virtual void OnEnable()
        {
            Observable.NextFrame().Subscribe(_ => OnShow());
        }

        protected virtual void OnDisable()
        {
            GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateBtnUnlockStatus, OnUpdateBtnStatus);
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnUpdateBtnStatus(object user_data = null)
        {
        }

        protected virtual void OnShow()
        {
            GameEntry.Event.AddEventListener(Constants.EventName.UpdateBtnUnlockStatus, OnUpdateBtnStatus);
        }


        public void Close()
        {
            GameEntry.UI.CloseUIForm(this);
        }

        internal void Init(Sys_UIFormEntity sysUIForm)
        {
            SysUIForm = sysUIForm;
        }

        internal void ToOpen(object user_data)
        {
            this.userData = user_data;
            //设置UI层级
            if (SysUIForm.DisableUILayer != 1) GameEntry.UI.UILayer.SetSortingOrder(this, true);

            //UI打开时的委托
            if (ActionOpen != null)
            {
                Action onOpenBegin = ActionOpen;
                ActionOpen = null;
                onOpenBegin();
            }

            GameEntry.Event.Dispatch(Constants.EventName.PopupAction, new PopupActionEvent(Name, UIActionType.ShowUI));
        }

        internal void ToClose()
        {
            //进行层级管理 减少层级
            if (SysUIForm.DisableUILayer != 1) GameEntry.UI.UILayer.SetSortingOrder(this, false);
            OnClose?.Invoke();
            CloseTime = Time.time;
            GameEntry.UI.HideUI(this);
            GameEntry.UI.UIPool.EnQueue(this);
            GameEntry.Event.Dispatch(Constants.EventName.PopupAction, new PopupActionEvent(Name, UIActionType.HideUI));
        }
    }
}