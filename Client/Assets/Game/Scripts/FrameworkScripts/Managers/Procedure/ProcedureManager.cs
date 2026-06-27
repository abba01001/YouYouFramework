using System;
using System.Collections.Generic;
using Main;
using UnityEngine;

namespace GameScripts
{
    // 流程状态
    public enum ProcedureState
    {
        None,
        Launch,
        Preload,
        Main,
        Battle,
    }
    
    // 所有流程状态的基类
    public abstract class ProcedureBase
    {
        // 该状态对应的类型
        public abstract ProcedureState StateType { get; }
        
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnLeave() { }
        public virtual void OnDestroy() { }
    }
    
    // 1. 核心原理：有限状态机 (FSM)
    // FSM 的基本思想是：系统在任何时刻只能处于一种状态，且状态之间只能通过特定的规则转换。
    // 状态隔离：每个流程（如 ProcedureLaunch, ProcedureMain）都是独立的类。它们不需要知道彼此的存在，只负责自己的 OnEnter（启动）、OnUpdate（逻辑）、OnLeave（清理）。
    // 集中调度：ProcedureManager 是唯一的“守门员”。所有的切换请求都必须经过 ChangeState，这保证了切换逻辑的统一性和可追踪性。
    //
    // 2. 关键防护机制：指令队列 (Command Queue)
    // 这是你这段代码中最具工程价值的部分，专门为了解决 “递归重入（Re-entrancy）” 问题。
    // 假设流程 A 的 OnEnter 逻辑中包含了一个复杂的判断，满足条件后立刻需要切换到流程 B。
    //
    // 没有队列时：程序会陷入 A.OnEnter -> ChangeState -> B.OnEnter -> ... 的递归调用中。这会造成堆栈溢出，且由于旧状态还未完全“Leave”，会导致内存泄漏或逻辑引用冲突。
    //
    // 有队列时：
    // ChangeState 发现 m_IsProcessing 为 true。 不再立即执行切换，而是将任务“存”进 m_StateQueue。
    // 当前的 OnEnter 执行完毕。回到最外层的 ChangeState，检测到队列不为空，顺次执行 ExecuteChange。
    // 这就像是一个“排队叫号机”：无论你在哪个阶段发起切换，所有的请求都会被公平地排进队列，按顺序执行，从而保证了逻辑的线性和安全性。
    public class ProcedureManager
    {
        private readonly Dictionary<ProcedureState, ProcedureBase> m_States = new();
        private ProcedureBase m_CurrState;
        
        // 队列机制：处理递归调用
        private readonly Queue<ProcedureState> m_StateQueue = new();
        private bool m_IsProcessing = false;

        public ProcedureState CurrStateType { get; private set; } = ProcedureState.None;

        public ProcedureManager()
        {
            AddState(new ProcedureLaunch());
            AddState(new ProcedurePreload());
            AddState(new ProcedureMain());
            AddState(new ProcedureBattle());
        }

        private void AddState(ProcedureBase state) => m_States[state.StateType] = state;
        public void OnUpdate() => m_CurrState?.OnUpdate();

        // 切换状态（对外接口）
        public void ChangeState(ProcedureState newState)
        {
            // 如果已经在切换过程中，将任务放入队列，防止递归崩溃
            if (m_IsProcessing)
            {
                m_StateQueue.Enqueue(newState);
                return;
            }

            // 开始处理
            m_IsProcessing = true;

            // 执行核心切换逻辑
            ExecuteChange(newState);

            // 处理完本次切换后，检查队列中是否有后续任务
            while (m_StateQueue.Count > 0)
            {
                ProcedureState nextState = m_StateQueue.Dequeue();
                ExecuteChange(nextState);
            }

            m_IsProcessing = false;
        }

        // 真正的执行逻辑
        private void ExecuteChange(ProcedureState newState)
        {
            if (CurrStateType == newState) return;

            // 离开旧状态
            m_CurrState?.OnLeave();
            
            // 进入新状态
            if (m_States.TryGetValue(newState, out var state))
            {
                Debugger.Log($"流程切换: {CurrStateType} -> {newState}");
                
                CurrStateType = newState;
                m_CurrState = state;
                m_CurrState.OnEnter();
                
                // 加载面板逻辑
                if (newState != ProcedureState.Launch && newState != ProcedureState.Preload) 
                    ShowProgressPanel(newState);
            }
            else
            {
                Debug.LogError($"[ProcedureManager] 未找到流程状态: {newState}");
            }
        }
        
        public void Shutdown()
        {
            m_CurrState?.OnLeave();
            foreach (var state in m_States.Values) state.OnDestroy();
            m_States.Clear();
        }

        private void ShowProgressPanel(ProcedureState state)
        {
            FormLoading.Instance.Show();
            FormLoading.Instance.UpdateProgress("场景加载中", 0f);

            Action<float> onProgress = null;
            onProgress = (value) =>
            {
                FormLoading.Instance.UpdateProgress("场景加载中", value);
                if (value >= 1f)
                {
                    FormLoading.Instance.Hide();
                    GameEntry.Scene.LoadingUpdateAction -= onProgress;
                }
            };
            GameEntry.Scene.LoadingUpdateAction += onProgress;
        }
    }
}