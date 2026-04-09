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
        GameEntry.Scene.LoadSceneAction(SceneGroupName.Game, 1, () => { _ = Init();});
    }
        
    private async UniTask Init()
    {
        Scene targetScene = SceneManager.GetSceneByName("Game");
        SceneManager.SetActiveScene(targetScene);
        LoadingGraphics.Instance.ShowProgress();
        
        ProjectInitSettings t = await GameEntry.Loader.LoadMainAssetAsync<ProjectInitSettings>("Assets/Game/Download/ProjectFiles/Data/Project Init Settings.asset", GameEntry.Instance.gameObject);
        t.Init();

        GameEntry.UI.OpenUIForm<FormGame>();
        
        GameObject obj = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/Prefab/GameController.prefab");
        await obj.GetComponent<GameController>().Init();
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
    }
    internal override void OnDestroy()
    {
        base.OnDestroy();
    }
}