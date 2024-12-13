using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace YouYou
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
            GameEntry.Instance.ShowBackGround(BGType.Main, "Assets/Game/Download/Textures/BackGround/Home/home_map_1.png");
            GameEntry.UI.OpenUIForm<FormLoading>();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Main, 1,Init);
        }

        private void Init()
        {
            Scene targetScene = SceneManager.GetSceneByName("Main");
            SceneManager.SetActiveScene(targetScene);
            // Material main3Skybox = Resources.Load<Material>("CloudyNight"); // 确保路径正确
            // // 更新渲染设置中的天空盒
            // if (main3Skybox != null)
            // {
            //     RenderSettings.skybox = main3Skybox;
            //     DynamicGI.UpdateEnvironment();
            // }
            
            GameEntry.UI.OpenUIForm<FormMain>();
            
            // var mapParent = GameObject.Find("Map");
            // if (mapParent != null)
            // {
            //     GameObject.Destroy(mapParent);
            // }
            // MapParent = new GameObject("Map");

        }

        private async void InitPlayer()
        {
            // var parent = new GameObject("Player");
            //
            // PoolObj playerCtrl = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.PlayerCtrl,parent.transform);
            // PoolObj playerModel = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.Archer,playerCtrl.transform);
            // PoolObj playerCamera = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.PlayerCamera,parent.transform);
            //
            // YouYouJoystick rotateJoy = GameEntry.UI.GetUIForm<FormBattle>("FormBattle").GetRotateJoystick();
            // YouYouJoystick moveJoy = GameEntry.UI.GetUIForm<FormBattle>("FormBattle").GetMoveJoystick();
            // Animator animator = playerModel.GetComponentInChildren<Animator>(true);
            // playerCtrl.GetComponent<PlayerCtrl>().InitParams(new object[] { animator,moveJoy, playerCamera.GetComponent<Camera>() });
            // playerCamera.GetComponent<PlayerCamera>().InitParams(new object[] { playerCtrl.transform, rotateJoy });
            
            // PoolObj dungeonGenerator = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.DungeonGenerator,parent1.transform);
            // RuntimeDungeon runtimeDungeon = dungeonGenerator.GetComponent<RuntimeDungeon>();
            // runtimeDungeon.GenerateOnStart = false;
            //LoadCurrentLevel(GameEntry.Data.PlayerRoleData.levelId.ToString());
            //dungeonGenerator.GetComponent<DungeonSetup>().InitParams(new object[] {playerCtrl.transform, rotateJoy});
        }
        
        // 加载关卡
        // private void LoadCurrentLevel(string levelName)
        // {
        //     if (string.IsNullOrEmpty(levelName))
        //     {
        //         Debug.LogWarning("关卡名称不能为空！");
        //         return;
        //     }
        //
        //     string fullSavePath = "Assets/Game/Download/DunGenMap/Level/" + levelName + ".json";
        //     AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(fullSavePath);
        //     if (referenceEntity != null)
        //     {
        //         TextAsset obj = UnityEngine.Object.Instantiate(referenceEntity.Target as TextAsset);
        //         AutoReleaseHandle.Add(referenceEntity, MapParent);
        //
        //         string json = obj.text;
        //         if (json.StartsWith(Constants.ENCRYPTEDKEY))
        //         {
        //             json = SecurityUtil.Decrypt(json.Substring(Constants.ENCRYPTEDKEY.Length));
        //         }
        //         LoadLevelDataFromJson(json);
        //     }
        //     else
        //     {
        //         Debug.LogWarning($"未找到文件: {fullSavePath}");
        //     }
        // }
        //
        // private void LoadLevelDataFromJson(string json)
        // {
        //     LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        //     Transform parent = MapParent.transform;
        //     foreach (LevelModelData modelData in levelData.models)
        //     {
        //         InstantiatePrefab(modelData, parent);
        //     }
        //     GameEntry.Event?.Dispatch(Constants.EventName.UpdatePlayerPos,new Vector3(levelData.bornPos[0],levelData.bornPos[1],levelData.bornPos[2]));
        // }
        //

        internal override void OnUpdate()
        {
            base.OnUpdate();
        }
        internal override void OnLeave()
        {
            //清理一些资源
            base.OnLeave();
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}