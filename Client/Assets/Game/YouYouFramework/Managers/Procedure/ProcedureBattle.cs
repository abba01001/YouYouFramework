using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
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
    public class ProcedureBattle : ProcedureBase
    {
        private GameObject MapParent = null;
        internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Instance.ShowBackGround(BGType.Battle, "Assets/Game/Download/Textures/BackGround/Battle/single_map_1.png");
            BattleCtrl.Instance.Init();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.Battle, 1, LoadSceneFinish);
        }

        private void LoadSceneFinish()
        {
            //初始化界面
            GameEntry.UI.OpenUIForm<FormBattle>();

            LevelData data = LoadCurrentLevel("1");
            
            //初始化关卡数据
            GameEntry.Event.Dispatch(Constants.EventName.InitBattleData,data);
        }
        
        //加载关卡
        private LevelData LoadCurrentLevel(string levelName)
        {
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogWarning("关卡名称不能为空！");
                return null;
            }
        
            string fullSavePath = "Assets/Game/Download/MapLevel/" + levelName + ".json";
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(fullSavePath);
            if (referenceEntity != null)
            {
                TextAsset obj = UnityEngine.Object.Instantiate(referenceEntity.Target as TextAsset);
                AutoReleaseHandle.Add(referenceEntity, MapParent);
                string json = obj.text;
                // if (json.StartsWith(Constants.ENCRYPTEDKEY))
                // {
                //     json = SecurityUtil.Decrypt(json.Substring(Constants.ENCRYPTEDKEY.Length));
                // }
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
            BattleCtrl.Instance.Update();
        }
        internal override void OnLeave()
        {
            BattleCtrl.Instance.End();
            GameEntry.UI.CloseUIForm<FormBattle>();
            base.OnLeave();
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}