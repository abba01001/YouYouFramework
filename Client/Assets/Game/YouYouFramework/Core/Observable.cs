using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

namespace Main
{
    public class Observable<T> : Singleton<T>
        where T : new()
    {
        private CommonEvent CommonEvent;
        public Observable()
        {
            CommonEvent = new CommonEvent();
        }

        public void Dispatch(string key)
        {
            CommonEvent.Dispatch(key.ToString());
        }
    
        public void Dispatch(string key, object userData)
        {
            CommonEvent.Dispatch(key, userData);
        }

        public void AddEventListener(string key, CommonEvent.OnActionHandler handler)
        {
            CommonEvent.AddEventListener(key, handler);
        }
        public void RemoveEventListener(string key, CommonEvent.OnActionHandler handler)
        {
            CommonEvent.RemoveEventListener(key, handler);
        }
        public void RemoveEventListenerAll(string key)
        {
            CommonEvent.RemoveEventListenerAll(key);
        }
    }
}