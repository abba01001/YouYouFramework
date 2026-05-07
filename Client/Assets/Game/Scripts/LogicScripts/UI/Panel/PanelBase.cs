using System;
using System.Collections;
using System.Collections.Generic;
using GameScripts;
using UniRx;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameScripts
{
    public class PanelBase : MonoBehaviour
    {
        protected string CurPanelName = string.Empty;
        private void Awake()
        {
            OnAwake();
        }


        private void Start()
        {
            OnStart();
        }


        private void OnEnable()
        {
            Observable.NextFrame().Subscribe(_ => OnShow());
        }


        private void OnDisable()
        {
            Observable.NextFrame().Subscribe(_ => OnHide());
        }


        protected virtual void OnAwake()
        {
        }


        protected virtual void OnStart()
        {
        }

        protected virtual void OnShow()
        {
            if (CurPanelName != string.Empty)
                GameEntry.Event.AddEventListener(Constants.EventName.UpdateBtnUnlockStatus, OnUpdateBtnStatus);
        }

        protected virtual void OnHide()
        {
            GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateBtnUnlockStatus, OnUpdateBtnStatus);
        }


        protected virtual void OnUpdateBtnStatus(object user_data)
        {
        }
    }
}