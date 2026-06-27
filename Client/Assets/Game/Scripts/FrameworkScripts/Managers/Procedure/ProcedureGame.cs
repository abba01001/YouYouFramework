using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameScripts
{
    /// <summary>
    /// 游戏流程
    /// </summary>
    public class ProcedureGame : ProcedureBase
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
            await GameEntry.Scene.LoadSceneAsync(SceneGroupName.MainMenu, 1);
            GameEntry.UI.OpenUIForm<FormMain>();
        }
    }
}