using System;
using Cysharp.Threading.Tasks;
using OctoberStudio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameScripts
{
    /// <summary>
    /// 游戏流程
    /// </summary>
    public class ProcedureBattle : ProcedureBase
    {
        public override ProcedureState StateType => ProcedureState.Battle;
        private GameObject MapParent = null;
        public override void OnEnter()
        {
            base.OnEnter();
            _ = Init();
        }
    
        public async UniTask Init()
        {
            await GameEntry.Scene.LoadSceneAsync(SceneGroupName.Game,1);
            GameEntry.Event.Dispatch(Constants.EventName.BattleSceneInitFinish);
            GameController.SetBattleingFlag(true);
            GameEntry.UI.CloseUIForm<FormMain>();
        }
    
        public override void OnLeave()
        {
            base.OnLeave();
            GameController.SetBattleingFlag(false);
        }
    }
}