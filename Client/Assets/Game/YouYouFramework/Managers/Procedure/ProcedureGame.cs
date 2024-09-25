using System.Collections;
using System.Collections.Generic;
using DunGen.DungeonCrawler;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
            //dungeonGenerator.GetComponent<DungeonSetup>().InitParams(new object[] {playerCtrl.transform, rotateJoy});
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