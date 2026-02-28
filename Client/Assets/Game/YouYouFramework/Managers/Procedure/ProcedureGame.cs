using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Watermelon;


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

        private async UniTask Init()
        {
            Scene targetScene = SceneManager.GetSceneByName("Game");
            SceneManager.SetActiveScene(targetScene);
            
            ///这里是入口=====>>>>>>>>
            PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync("Assets/Game/Download/Prefab/GameRoot.prefab");
            obj.gameObject.MSetActive(true);
            obj.transform.SetParent(null);
            obj.gameObject.name = "GameRoot";
            SceneManager.MoveGameObjectToScene(obj.gameObject,targetScene);
            
            initScene = true;
            AddToStack(Camera.main, GameEntry.Instance.UICamera);
            // var baseData = Camera.main.GetUniversalAdditionalCameraData();
            // baseData.cameraStack.Add(GameEntry.Instance.UICamera);
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