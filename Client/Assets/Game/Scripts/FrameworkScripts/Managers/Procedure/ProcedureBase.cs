using System.Collections;
using System.Collections.Generic;

using Main;
using UnityEngine;

namespace GameScripts
{
    /// <summary>
    /// 流程状态基类
    /// </summary>
    public class ProcedureBase : FsmState<ProcedureManager>
    {
        internal override void OnEnter()
        {
            base.OnEnter();
            Debugger.Log(CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnEnter()");
        }
    
        internal override void OnUpdate()
        {
            base.OnUpdate();
        }
    
        internal override void OnLeave()
        {
            base.OnLeave();
            Debugger.Log(CurrFsm.GetState(CurrFsm.CurrStateType).ToString() + "==>> OnLeave()");
        }
    
        internal override void OnDestroy()
        {
            base.OnDestroy();
    
        }
    }
}