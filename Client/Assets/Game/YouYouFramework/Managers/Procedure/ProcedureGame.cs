using System.Collections;
using System.Collections.Generic;
using System.IO;
using DunGen;
using DunGen.DungeonCrawler;
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
    public class ProcedureGame : ProcedureBase
    {
        internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.UI.OpenUIForm<FormLoading>();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Main, 3,Init);
        }

        private void Init()
        {
            Scene targetScene = SceneManager.GetSceneByName("Main_3");
            SceneManager.SetActiveScene(targetScene);

            Material main3Skybox = Resources.Load<Material>("CloudyNight"); // 确保路径正确
            // 更新渲染设置中的天空盒
            if (main3Skybox != null)
            {
                RenderSettings.skybox = main3Skybox;
                DynamicGI.UpdateEnvironment(); // 更新全局光照（如果需要）
            }
            
            InitFormBattle();
            InitPlayer();
        }

        private void InitFormBattle()
        {
            GameEntry.UI.OpenUIForm<FormBattle>();
        }
        
        private async void InitPlayer()
        {
            var parent = new GameObject("Player");
            
            PoolObj playerCtrl = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.PlayerCtrl,parent.transform);
            PoolObj playerModel = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.Archer,playerCtrl.transform);
            PoolObj playerCamera = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.PlayerCamera,parent.transform);
            
            
            
            YouYouJoystick rotateJoy = GameEntry.UI.GetUIForm<FormBattle>("FormBattle").GetRotateJoystick();
            YouYouJoystick moveJoy = GameEntry.UI.GetUIForm<FormBattle>("FormBattle").GetMoveJoystick();
            Animator animator = playerModel.GetComponentInChildren<Animator>(true);
            playerCtrl.GetComponent<PlayerCtrl>().InitParams(new object[] { animator,moveJoy, playerCamera.GetComponent<Camera>() });
            playerCtrl.GetComponent<ClickableObjectHandler>().InitParams(new object[] { playerCamera.GetComponent<Camera>() });
            playerCamera.GetComponent<PlayerCamera>().InitParams(new object[] { playerCtrl.transform, rotateJoy });
            
            var parent1 = new GameObject("Map");
            PoolObj dungeonGenerator = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.DungeonGenerator,parent1.transform);
            RuntimeDungeon runtimeDungeon = dungeonGenerator.GetComponent<RuntimeDungeon>();
            runtimeDungeon.GenerateOnStart = false;
            LoadCurrentLevel(GameEntry.Player.GetPlayerRoleData().levelId.ToString());
            //dungeonGenerator.GetComponent<DungeonSetup>().InitParams(new object[] {playerCtrl.transform, rotateJoy});
        }
        
        // 加载关卡
        private void LoadCurrentLevel(string levelName)
        {
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogWarning("关卡名称不能为空！");
                return;
            }

            string path = "Assets/Game/Download/DunGenMap/Level/";
            string fullSavePath = path + levelName + ".json";

            if (!File.Exists(fullSavePath))
            {
                Debug.LogWarning($"未找到文件: {fullSavePath}");
                return;
            }

            string json = File.ReadAllText(fullSavePath);
            if (json.StartsWith(Constants.ENCRYPTEDKEY))
            {
                json = SecurityUtil.Decrypt(json.Substring(Constants.ENCRYPTEDKEY.Length));
            }

            LoadLevelDataFromJson(json);
        }
        
        private void LoadLevelDataFromJson(string json)
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(json);
            GameObject dungeon = GameObject.Find("Map");
            foreach (LevelModelData modelData in levelData.models)
            {
                InstantiatePrefab(modelData, dungeon.transform);
            }
            GameEntry.LogError(levelData.bornPos[0],"---",levelData.bornPos[1],"---",levelData.bornPos[2]);
            GameEntry.Event?.Dispatch(EventName.UpdatePlayerPos,new Vector3(levelData.bornPos[0],levelData.bornPos[1],levelData.bornPos[2]));
        }
        
        // 实例化预制体
        private void InstantiatePrefab(LevelModelData modelData, Transform parent)
        {
            string prefabPath = GetPrefabPath(modelData);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject instance = GameObject.Instantiate(prefab, modelData.position, modelData.rotation, parent);
                instance.transform.localScale = modelData.scale;
            }
            else
            {
                Debug.LogWarning($"未找到预制体: {modelData.modelPrefabName}");
            }
        }
        
        // 获取预制体路径
        private string GetPrefabPath(LevelModelData modelData)
        {
            string prefabPath = "";
            if (modelData.modelPrefabName.Contains("Castle"))
            {
                prefabPath = Constants.CASTLEPATH + $"{modelData.modelPrefabName}.prefab";
            }
            else if (modelData.modelPrefabName.Contains("Graveyard"))
            {
                prefabPath = Constants.GRAVEYARDPATH + $"{modelData.modelPrefabName}.prefab";
            }
            else
            {
                EditorUtility.DisplayDialog("加载预制体错误", $"类型{modelData.modelPrefabName}", "确定");
                return "";
            }
            return prefabPath; // 自定义路径
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
}