using System.Collections;
using System.Collections.Generic;
using System.IO;
using DunGen;
using DunGen.DungeonCrawler;
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
            BattleCtrl.Instance.Init();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Battle, 1, LoadSceneFinish);
        }

        private void LoadSceneFinish()
        {
            //初始化玩家、敌人
            BattleCtrl.Instance.InitBattleData(null);
            
            //初始化界面
            GameEntry.UI.OpenUIForm<FormBattle>();
        }
        
        internal override void OnUpdate()
        {
            base.OnUpdate();
            BattleCtrl.Instance.Update();
        }
        internal override void OnLeave()
        {
            BattleCtrl.Instance.End();
            base.OnLeave();
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}