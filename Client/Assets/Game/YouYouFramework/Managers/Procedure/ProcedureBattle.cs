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
            BattleCtrl.Instance.InitBattle();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Battle, 1, () =>
            {
                GameEntry.UI.OpenUIForm<FormBattle>();
            });
        }

        internal override void OnUpdate()
        {
            base.OnUpdate();
        }
        internal override void OnLeave()
        {
            BattleCtrl.Instance.EndBattle();
            base.OnLeave();
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}