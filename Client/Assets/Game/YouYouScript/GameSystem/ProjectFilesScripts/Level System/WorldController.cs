using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    public class WorldController
    {
        private static WorldController _instance;
        public static WorldController Instance => _instance ??= new WorldController();

        WorldsDatabase database;

        private  PlayerBehavior playerBehavior;

        private  WorldGlobalSave worldGlobalSave;

        public  WorldData CurrentWorld { get; private set; }
        public  WorldBehavior WorldBehavior { get; private set; }

        public  event SimpleCallback OnWorldLoaded;

        public async UniTask Initialise()
        {
            worldGlobalSave = SaveController.GetSaveObject<WorldGlobalSave>("worldGlobal");
            database = await GameEntry.Loader.LoadMainAssetAsync<WorldsDatabase>("Assets/Game/Download/ProjectFiles/Data/Worlds Database.asset",GameEntry.Instance.gameObject);
            await LoadCurrentWorld();
        }

        private void OnDestroy()
        {
            NavMeshController.Reset();

            MissionsController.Instance.Unload();

            FloatingCloud.Unload();

            WorldItemCollector.Destroy();

            Currency[] currencies = CurrencyController.Currencies;
            if (!currencies.IsNullOrEmpty())
            {
                foreach (Currency currency in currencies)
                {
                    if (currency != null)
                    {
                        currency.Data.Unload();
                    }
                }
            }
        }

        public void UnloadWorld(SimpleCallback onWorldUnloaded)
        {
            NavMeshController.Reset();

            playerBehavior.Unload();

            MissionsController.Instance.Unload();

            FloatingCloud.Unload();

            WorldItemCollector.Unload();

            Currency[] currencies = CurrencyController.Currencies;
            if(!currencies.IsNullOrEmpty())
            {
                foreach (Currency currency in currencies)
                {
                    if(currency != null)
                    {
                        currency.Data.DropResPool.ReturnToPoolEverything(true);
                        currency.Data.FlyingResPool.ReturnToPoolEverything(true);
                    }
                }
            }

            WorldBehavior.Unload();

            SceneManager.UnloadSceneAsync(CurrentWorld.Scene.Name, UnloadSceneOptions.None).OnCompleted(onWorldUnloaded);
        }

        public async UniTask LoadCurrentWorld()
        {
            string worldID = worldGlobalSave.worldID;
            if (string.IsNullOrEmpty(worldID))
                worldID = GetWorldData(0).ID;

            await LoadWorld(database.GetWorldByID(worldID));
        }

        public async UniTask LoadWorld(string worldID)
        {
            worldGlobalSave.worldID = worldID;
            await LoadWorld(database.GetWorldByID(worldID));
        }

        public async UniTask LoadWorld(WorldData worldData)
        {
            CurrentWorld = worldData;
            WorldItemCollector.Initialise();
            GameUtil.LogCurTimerLog("LoadWorld111====>");
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(worldData.Scene.Name, LoadSceneMode.Additive);
            GameUtil.LogCurTimerLog("LoadWorld222====>");
            loadOperation.allowSceneActivation = false;
            while (!loadOperation.isDone)
            {
                GameUtil.LogCurTimerLog($"进度===>{loadOperation.progress}");
                // 获取加载进度，从 0 到 0.9，之后会跳到 1
                float progress = loadOperation.progress < 0.9f ? loadOperation.progress : 0.9f;
                // 显示进度
                LoadingGraphics.Instance.UpdateProgress(progress);
                // 如果进度达到0.9，表示资源加载完毕，准备激活场景
                if (loadOperation.progress >= 0.9f)
                {
                    // 激活场景
                    loadOperation.allowSceneActivation = true;
                }
                await UniTask.Yield();  // 等待一帧
            }
            LoadingGraphics.Instance.StopProgress();
            Control.EnableMovementControl();
        }

        public async UniTask LoadBuildings()
        {
            
        }
        
        public async UniTask LoadEnvironment()
        {
            
        }
        
        public async UniTask SetWorld(WorldBehavior worldBehavior)
        {
            //这里加载世界入口
            //加载世界环境
   
            WorldBehavior = worldBehavior;
            WorldBehavior.Initialise();
            WorldBehavior.OnPlayerEntered();

            // Spawn player
            if (playerBehavior == null)
            {
                //这里统一改异步
                GameObject obj = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/ProjectFiles/Game/Prefabs/Player/Player.prefab");
                playerBehavior = obj.GetComponent<PlayerBehavior>();
            }
            playerBehavior.transform.position = WorldBehavior.SpawnPoint.position;
            playerBehavior.Initialise();

            DistanceToggle.Initialise(playerBehavior.transform);

            VirtualCamera mainCamera = CameraController.Instance.GetCamera(CameraType.Gameplay);
            mainCamera.SetTarget(playerBehavior.transform);

            CameraController.Instance.EnableCamera(CameraType.Gameplay);

            WorldBehavior.OnWorldLoaded();

            GameController.OnWorldLoaded(worldBehavior);

            WorldBehavior.RegisterAndRecalculateNavMesh(() =>
            {
                MissionsController.Instance.ActivateNextMission();

                WorldBehavior.OnWorldNavMeshRecalculated();

                OnWorldLoaded?.Invoke();
            });
        }

        public WorldData GetWorldData(string worldID)
        {
            return database.GetWorldByID(worldID);
        }

        public WorldData GetWorldData(int worldIndex)
        {
            return database.GetWorldByIndex(worldIndex);
        }

        public bool IsWorldExists(int worldIndex)
        {
            return database.IsWorldExists(worldIndex);
        }

        public void UpdateWorldSave(string activeMissionName)
        {
            worldGlobalSave.activeMissionName = activeMissionName;
        }
    }
}