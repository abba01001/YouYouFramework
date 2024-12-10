using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Path = DG.Tweening.Plugins.Core.PathCore.Path;


namespace YouYou
{
    /// <summary>
    /// 游戏流程
    /// </summary>
    public class ProcedureBattle : ProcedureBase
    {
        private GameObject MapParent = null;
        internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Instance.ShowBackGround(BGType.Battle, "Assets/Game/Download/Textures/BackGround/Battle/single_map_1.png");
            BattleCtrl.Instance.Init();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Battle, 1, LoadSceneFinish);
        }

        private void LoadSceneFinish()
        {
            //初始化界面
            GameEntry.UI.OpenUIForm<FormBattle>();
            
            //初始化关卡数据
            GameEntry.Event.Dispatch(Constants.EventName.InitBattleData,null);
        }
        
        internal override void OnUpdate()
        {
            base.OnUpdate();
            BattleCtrl.Instance.Update();
        }
        internal override void OnLeave()
        {
            BattleCtrl.Instance.End();
            GameEntry.UI.CloseUIForm<FormBattle>();
            base.OnLeave();
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}