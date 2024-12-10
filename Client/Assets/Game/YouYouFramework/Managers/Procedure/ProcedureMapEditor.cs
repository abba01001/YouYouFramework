using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace YouYou
{
    /// <summary>
    /// 游戏流程
    /// </summary>
    public class ProcedureMapEditor : ProcedureBase
    {
        private GameObject MapParent = null;
        private const string DefaultBasePath = "Assets/Game/Download/MapLevel/"; // 默认的保存路径
        
        internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Instance.ShowBackGround(BGType.Battle,"Assets/Game/Download/Textures/BackGround/Battle/single_map_1.png");
            BattleCtrl.Instance.Init();
            GameEntry.UI.OpenUIForm<FormLoading>();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.MapEditor, 1,LoadSceneFinish);
        }

        private void LoadSceneFinish()
        {
            Scene targetScene = SceneManager.GetSceneByName("MapEditor");
            SceneManager.SetActiveScene(targetScene);
            // 弹窗输入关卡名
            LevelNameInputWindow.OpenWindow(OnLevelNameEntered);
        }
        
        // 用户输入关卡名后的回调
        private void OnLevelNameEntered(string levelName)
        {
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogError("关卡名称无效！");
                return;
            }

            // 根据输入的关卡名加载数据
            string fullSavePath = Path.Combine(DefaultBasePath, levelName + ".json");

            if (!File.Exists(fullSavePath))
            {
                GameUtil.LogError($"未找到文件: {fullSavePath}");
                return;
            }
            LevelNameInputWindow.CloseWindow();
            string json = File.ReadAllText(fullSavePath);
            var curLevelData = JsonUtility.FromJson<LevelData>(json);

            //初始化界面
            GameEntry.UI.OpenUIForm<FormBattle>();
            // 初始化关卡数据
            GameEntry.Event.Dispatch(Constants.EventName.InitBattleData, curLevelData);
        }

        internal override void OnUpdate()
        {
            base.OnUpdate();
            BattleCtrl.Instance.Update();
        }
        internal override void OnLeave()
        {
            //清理一些资源
            BattleCtrl.Instance.End();
            base.OnLeave();
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}