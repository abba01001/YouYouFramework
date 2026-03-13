using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Watermelon;
using Object = UnityEngine.Object;


namespace YouYou
{
    /// <summary>
    /// 游戏流程
    /// </summary>
    public class ProcedureGame : ProcedureBase
    {
        private GameObject MapParent = null;
        private bool initScene;
        internal override void OnEnter()
        {
            base.OnEnter();
            // GameEntry.Instance.ShowBackGround(BGType.Main, "Assets/Game/Download/BackGround/Home/home_map_1.png");
            GameEntry.UI.OpenUIForm<FormLoading>();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Game, 1, () => { Init();});
        }

        private async UniTask InitSetting()
        {
            ProjectInitSettings t = await GameEntry.Loader.LoadMainAssetAsync<ProjectInitSettings>("Assets/Game/Download/ProjectFiles/Data/Project Init Settings.asset", GameEntry.Instance.gameObject);
            t.Init();
        }
        
        private async UniTask Init()
        {
            GameUtil.LogError("8888888888888888");
            Scene targetScene = SceneManager.GetSceneByName("Game");
            SceneManager.SetActiveScene(targetScene);
            GameUtil.LogError("11111111111111");
            
            ///这里是入口=====>>>>>>>>
            GameUtil.LogError("11111111111111");

            await InitSetting();
            
            try
            {
                
                PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync("Assets/Game/Download/Prefab/GameRoot.prefab");
                obj.gameObject.SetActive(true);
                GameUtil.LogError($"==============GameRoot状态>{obj.gameObject.activeSelf}");
                obj.transform.SetParent(null);
                obj.name = "GameRoot";
                SceneManager.MoveGameObjectToScene(obj.gameObject, targetScene);
            }
            catch (Exception ex)
            {
                GameUtil.LogError("Prefab加载/激活异常: " + ex);
            }
            GameUtil.LogError("22222222222");
            
            initScene = true;
            GameUtil.LogError("3333333333");
        }
        
        public static void AddToStack(Camera baseCam, Camera overlayCam)
        {
            if (baseCam == null || overlayCam == null) return;
            var baseData = baseCam.GetUniversalAdditionalCameraData();
            var overlayData = overlayCam.GetUniversalAdditionalCameraData();
            if (baseData == null || overlayData == null) return;
            overlayData.renderType = CameraRenderType.Overlay;
            if (!baseData.cameraStack.Contains(overlayCam)) baseData.cameraStack.Add(overlayCam);
        }

        internal override void OnUpdate()
        {
            base.OnUpdate();
            // if (!initScene) return;
            // if (!BuildingSystem.Instance.InitFinish) return;
            //
            // BuildingSystem.Instance.Update();
            // CustomerSystem.Instance.Update();
            // WorkerSystem.Instance.Update();
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
}