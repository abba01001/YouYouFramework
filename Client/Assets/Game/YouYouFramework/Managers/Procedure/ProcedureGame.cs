using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Watermelon;


/// <summary>
/// 游戏流程
/// </summary>
public class ProcedureGame : ProcedureBase
{
    private GameObject MapParent = null;
    internal override void OnEnter()
    {
        base.OnEnter();
        // GameEntry.UI.OpenUIForm<FormLoading>();
        GameEntry.Scene.LoadSceneAction(SceneGroupName.Game, 1, () => { _ = Init();});
    }
        
    private async UniTask Init()
    {
        GameUtil.LogCurTimerLog("ProcedureGame 1111====>");
        Scene targetScene = SceneManager.GetSceneByName("Game");
        SceneManager.SetActiveScene(targetScene);
        LoadingGraphics.Instance.ShowProgress();
        
        GameUtil.LogCurTimerLog("加载Project Init Settings.asset前====>");
        ProjectInitSettings t = await GameEntry.Loader.LoadMainAssetAsync<ProjectInitSettings>("Assets/Game/Download/ProjectFiles/Data/Project Init Settings.asset", GameEntry.Instance.gameObject);
        t.Init();
        GameUtil.LogCurTimerLog("加载Project Init Settings.asset后====>");

        GameUtil.LogCurTimerLog("加载FormGame前====>");
        GameEntry.UI.OpenUIForm<FormGame>();
        GameUtil.LogCurTimerLog("加载FormGame后====>");
        
        GameUtil.LogCurTimerLog("加载GameController前====>");
        GameObject obj = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/Prefab/GameController.prefab");
        await obj.GetComponent<GameController>().Init();
        GameUtil.LogCurTimerLog("加载GameController后====>");
        obj.gameObject.SetActive(true);
        obj.transform.SetParent(null);
        obj.name = "GameRoot";
        SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);

    }
        
    internal override void OnUpdate()
    {
        base.OnUpdate();
    }
    internal override void OnLeave()
    {
        base.OnLeave();
        GameEntry.UI.CloseUIForm<FormMain>();
    }
    internal override void OnDestroy()
    {
        base.OnDestroy();
    }
}