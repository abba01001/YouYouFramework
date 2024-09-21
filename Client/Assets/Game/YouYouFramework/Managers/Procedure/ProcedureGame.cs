using System.Collections;
using System.Collections.Generic;
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
            InitFormBattle();
            InitPlayer();
            InitMap();
            //初始化玩家
            //初始地图
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
            playerCamera.GetComponent<PlayerCamera>().InitParams(new object[] { playerCtrl.transform, rotateJoy });
        }

        private async void InitMap()
        {
            var parent = new GameObject("Map");
            PoolObj poolObj = await GameEntry.Pool.GameObjectPool.SpawnAsync(PrefabName.DungeonGenerator,parent.transform);
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