using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Path = DG.Tweening.Plugins.Core.PathCore.Path;

//操作战斗流程开始，结束

    /// <summary>
    /// 游戏流程
    /// </summary>
    public class ProcedureBattle : ProcedureBase
    {
        private GameObject MapParent = null;
        internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Instance.ShowBackGround(BGType.Battle, "Assets/Game/Download/BackGround/Battle/single_map_1.png");
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Battle, 1, () =>
            {
                LoadSceneFinish();
            });
        }

        private async UniTask LoadSceneFinish()
        {
            var scene_camera = GameObject.FindWithTag("SceneCamera");
            if (scene_camera != null) GameEntry.Instance.SceneCamera = scene_camera.GetComponent<Camera>();
            
            //初始化界面
            LevelData data = await LoadCurrentLevelAsync($"{GameEntry.Data.TempSelectMapLv}_{GameEntry.Data.TempSelectMapLvNum}");
            //初始化关卡数据
            GameEntry.Event.Dispatch(Constants.EventName.InitBattleData,data);
        }
        
        private async UniTask<LevelData> LoadCurrentLevelAsync(string levelName)
        {
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogWarning("关卡名称不能为空！");
                return null;
            }

            string fullSavePath = "Assets/Game/Download/MapLevel/" + levelName + ".json";
            AssetReferenceEntity referenceEntity = await GameEntry.Loader.LoadMainAssetAsync(fullSavePath);
    
            if (referenceEntity != null)
            {
                TextAsset obj = UnityEngine.Object.Instantiate(referenceEntity.Target as TextAsset);
                AutoReleaseHandle.Add(referenceEntity, MapParent);
                string json = obj.text;
                return JsonUtility.FromJson<LevelData>(json);
            }
            else
            {
                Debug.LogWarning($"未找到文件: {fullSavePath}");
                return null;
            }
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
