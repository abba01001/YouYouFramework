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
        GameEntry.UI.OpenUIForm<FormLoading>();
        GameEntry.Scene.LoadSceneAction(SceneGroupName.Game, 1, () => { _ = Init();});
    }
        
    private async UniTask Init()
    {
        GameUtil.LogError("ProcedureGame 1111====>",DateTime.Now);
        Scene targetScene = SceneManager.GetSceneByName("Game");
        SceneManager.SetActiveScene(targetScene);
        LoadingGraphics.Instance.ShowProgress();
        GameUtil.LogError("ProcedureGame 222====>",DateTime.Now);

        ProjectInitSettings t = await GameEntry.Loader.LoadMainAssetAsync<ProjectInitSettings>("Assets/Game/Download/ProjectFiles/Data/Project Init Settings.asset", GameEntry.Instance.gameObject);
        t.Init();
        GameUtil.LogError("ProcedureGame 333====>",DateTime.Now);

        PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync("Assets/Game/Download/Prefab/GameRoot.prefab");
        GameUtil.LogError("ProcedureGame 444====>",DateTime.Now);

        obj.gameObject.SetActive(true);
        obj.transform.SetParent(null);
        obj.name = "GameRoot";
        SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);
        GameUtil.LogError("ProcedureGame 555====>",DateTime.Now);

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