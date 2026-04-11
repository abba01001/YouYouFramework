using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// 游戏流程
/// </summary>
public class ProcedureGame : ProcedureBase
{
    private GameObject MapParent = null;
    internal override void OnEnter()
    {
        base.OnEnter();
        GameEntry.Scene.LoadSceneAction(SceneGroupName.Game, 1, () => { _ = Init();});
    }
        
    private async UniTask Init()
    {


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