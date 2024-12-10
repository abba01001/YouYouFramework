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
        internal override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Instance.ShowBackGround(BGType.Battle,"Assets/Game/Download/Textures/BackGround/Battle/single_map_1.png");
            GameEntry.UI.OpenUIForm<FormLoading>();
            GameEntry.Scene.LoadSceneAction(SceneGroupName.MapEditor, 1,Init);
        }

        private void Init()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            Scene targetScene = SceneManager.GetSceneByName("MapEditor");
            SceneManager.SetActiveScene(targetScene);
            GameEntry.UI.OpenUIForm<FormBattle>();
        }

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