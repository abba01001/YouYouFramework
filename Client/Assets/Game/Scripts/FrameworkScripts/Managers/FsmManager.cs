using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts
{
    /// <summary>
    /// 状态机管理器
    /// </summary>
    public class FsmManager : IDisposable
    {
        /// <summary>
        /// 状态机字典
        /// </summary>
        private Dictionary<int, FsmBase> m_FsmDic;
    
        /// <summary>
        /// 状态机的临时编号
        /// </summary>
        private int m_TemFsmId = 0;
    
    
        internal FsmManager()
        {
            m_FsmDic = new Dictionary<int, FsmBase>();
        }
    
        public void Dispose()
        {
            var enumerator = m_FsmDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.ShutDown();
            }
    
            m_FsmDic.Clear();
        }
    
        /// <summary>
        /// 创建状态机
        /// </summary>
        /// <typeparam name="T">拥有者类型</typeparam>
        /// <param name="fsmId">状态机编号</param>
        /// <param name="owner">拥有者</param>
        /// <param name="states">状态数组</param>
        /// <returns></returns>
        public Fsm<T> Create<T>(T owner, FsmState<T>[] states) where T : class
        {
            Fsm<T> fsm = new Fsm<T>(m_TemFsmId++, owner, states);
            m_FsmDic[m_TemFsmId] = fsm;
            return fsm;
        }
    
        /// <summary>
        /// 销毁状态机
        /// </summary>
        /// <param name="fsmId">状态机编号</param>
        public void DestoryFsm(int fsmId)
        {
            FsmBase fsm = null;
            if (m_FsmDic.TryGetValue(fsmId, out fsm))
            {
                m_FsmDic.Remove(fsmId);
                fsm.ShutDown();
            }
        }
    }
    
    
        /// <summary>
    /// 状态机
    /// </summary>
    /// <typeparam name="T">FSMManager</typeparam>
    public class Fsm<T> : FsmBase where T : class
    {
    	/// <summary>
    	/// 状态机拥有者
    	/// </summary>
    	public T Owner { get; private set; }
    
    	/// <summary>
    	/// 当前状态
    	/// </summary>
    	private FsmState<T> m_CurrState;
    
    	/// <summary>
    	/// 状态字典
    	/// </summary>
    	private Dictionary<sbyte, FsmState<T>> m_StateDic;
    
    	/// <summary>
    	/// 构造函数
    	/// </summary>
    	/// <param name="fsmId">状态机编号</param>
    	/// <param name="owner">拥有者</param>
    	/// <param name="states">状态数组</param>
    
    	public Fsm(int fsmId, T owner, FsmState<T>[] states) : base(fsmId)
    	{
    		m_StateDic = new Dictionary<sbyte, FsmState<T>>();
    		Owner = owner;
    
    		//把状态加入字典
    		int len = states.Length;
    		for (int i = 0; i < len; i++)
    		{
    			FsmState<T> state = states[i];
    			if (state != null)
    			{
    				state.CurrFsm = this;
    				state.OnInit();
    			}
    
    			m_StateDic[(sbyte)i] = state;
    		}
    
    		//设置默认状态
    		CurrStateType = -1;
    	}
    
    	/// <summary>
    	/// 获取状态
    	/// </summary>
    	/// <param name="stateType">状态Type</param>
    	/// <returns>状态</returns>
    	public FsmState<T> GetState(sbyte stateType)
    	{
    		FsmState<T> state = null;
    		m_StateDic.TryGetValue(stateType, out state);
    		return state;
    	}
    
    	internal void OnUpdate()
    	{
    		if (m_CurrState != null)
    		{
    			m_CurrState.OnUpdate();
    		}
    	}
    
    	/// <summary>
    	/// 切换状态
    	/// </summary>
    	/// <param name="newState"></param>
    	public FsmState<T> ChangeState(sbyte newState)
    	{
    		if (CurrStateType == newState) return m_CurrState;
    
    		if (m_CurrState != null)
    		{
    			m_CurrState.OnLeave();
    		}
    
    		CurrStateType = newState;
    		m_CurrState = m_StateDic[CurrStateType];
    
    		//进入新状态
    		m_CurrState.OnEnter();
    		return m_CurrState;
    	}
    
    
    	/// <summary>
    	/// 关闭状态机
    	/// </summary>
    	public override void ShutDown()
    	{
    		if (m_CurrState != null)
    		{
    			m_CurrState.OnLeave();
    		}
    
    		foreach (KeyValuePair<sbyte, FsmState<T>> state in m_StateDic)
    		{
    			if (state.Value == null) continue;
    			state.Value.OnDestroy();
    		}
    
    		m_StateDic.Clear();
    	}
    }
        
    /// <summary>
    /// 状态机的状态
    /// </summary>
    /// <typeparam name="T">状态机</typeparam>
    public abstract class FsmState<T> where T : class
    {
	    /// <summary>
	    /// 所属状态机
	    /// </summary>
	    public Fsm<T> CurrFsm;
    
	    /// <summary>
	    /// 所属状态机管理器
	    /// </summary>
	    protected T FsmMgr;
    
	    /// <summary>
	    /// 当前状态的内部行为是否执行完毕
	    /// </summary>
	    public bool ActionComplete { get; protected set; }
    
	    internal virtual void OnInit()
	    {
		    FsmMgr = CurrFsm.Owner;
	    }
    
	    /// <summary>
	    /// 进入状态
	    /// </summary>
	    internal virtual void OnEnter()
	    {
		    ActionComplete = false;
	    }
    
	    /// <summary>
	    /// 执行状态
	    /// </summary>
	    internal virtual void OnUpdate()
	    {
	    }
    
	    /// <summary>
	    /// 离开状态
	    /// </summary>
	    internal virtual void OnLeave()
	    {
	    }
    
	    /// <summary>
	    /// 状态机销毁时调用
	    /// </summary>
	    internal virtual void OnDestroy()
	    {
	    }
    
    }
    
    /// <summary>
    /// 状态机基类
    /// </summary>
    public abstract class FsmBase
    {
	    /// <summary>
	    /// 状态机编号
	    /// </summary>
	    public int FsmId { get; private set; }
    
	    /// <summary>
	    /// 当前状态的类型
	    /// </summary>
	    public sbyte CurrStateType;
    
	    public FsmBase(int fsmId)
	    {
		    FsmId = fsmId;
	    }
    
	    /// <summary>
	    /// 关闭状态机
	    /// </summary>
	    public abstract void ShutDown();
    
    }
}