using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace YouYou
{
	/// <summary>
	/// 事件管理器
	/// </summary>
	public class EventManager 
	{
		/// <summary>
		/// 通用事件
		/// </summary>
		public CommonEvent Common { get; private set; }

		internal EventManager()
		{
			Common = new CommonEvent();
		}

        public void Dispatch(string key)
        {
            Common.Dispatch(key);
        }
        public void Dispatch(string key, object userData)
        {
            Common.Dispatch(key, userData);
        }

        public void AddEventListener(string key, CommonEvent.OnActionHandler handler)
        {
            Common.AddEventListener(key, handler);
        }
        public void RemoveEventListener(string key, CommonEvent.OnActionHandler handler)
        {
            Common.RemoveEventListener(key, handler);
        }
        public void RemoveEventListenerAll(string key)
        {
            Common.RemoveEventListenerAll(key);
        }
    }
}
