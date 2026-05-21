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
        private GameObject MapParent = null;
    
        internal override void OnEnter()
        {
            base.OnEnter();
            _ = Init();
        }
    
        private async UniTask Init()
        {
            await GameEntry.Scene.LoadSceneAsync(SceneGroupName.MainMenu, 1);
            GameEntry.UI.OpenUIForm<FormMain>();
        }
    
        internal override void OnUpdate()
        {
            base.OnUpdate();
        }
    
        internal override void OnLeave()
        {
            base.OnLeave();
        }
    
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}