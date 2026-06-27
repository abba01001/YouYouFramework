using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameScripts
{
    /// <summary>
    /// 游戏流程
    /// </summary>
    public class ProcedureMain : ProcedureBase
    {
        public override ProcedureState StateType => ProcedureState.Main;
        private GameObject MapParent = null;
        public override void OnEnter()
        {
            base.OnEnter();
            _ = Init();
        }
    
        private async UniTask Init()
        {
            await GameEntry.Scene.LoadSceneAsync(SceneGroupName.MainMenu, 1);
            GameEntry.UI.OpenUIForm<FormMain>();
        }
    }
}