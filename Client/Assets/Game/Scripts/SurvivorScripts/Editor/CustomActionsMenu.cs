using GameScripts;
using UnityEngine;
using UnityEditor;
using OctoberStudio.Abilities;

namespace OctoberStudio.Save
{
    public static class SaveActionsMenu
    {
        [MenuItem("工具类/October/Delete Save File", priority = 3)]
        private static void DeleteSaveFile()
        {
            PlayerPrefs.DeleteAll();
            SaveManager.DeleteSaveFile();
        }

        [MenuItem("工具类/October/Delete Save File", true)]
        private static bool DeleteSaveFileValidation()
        {
            return !Application.isPlaying;
        }

        [MenuItem("工具类/October/Open All Stages", priority = 2)]
        private static void OpenAllStages()
        {
            var stageSave = GameController.SaveManager.StageData;

            string[] guiID = AssetDatabase.FindAssets("t:StagesDatabase");

            if (guiID != null)
            {
                var database = AssetDatabase.LoadAssetAtPath<StagesDatabase>(AssetDatabase.GUIDToAssetPath(guiID[0]));

                if(database != null)
                {
                    stageSave.SetMaxReachedStageId(database.StagesCount - 1);

                    EditorApplication.isPlaying = false;
                }
            }
        }

        [MenuItem("工具类/October/Open All Stages", true)]
        private static bool OpenAllStagesValidation()
        {
            return Application.isPlaying;
        }

        [MenuItem("工具类/October/Get 1K Gold", priority = 1)]
        private static void GetGold()
        {
            GameEntry.Data.AddProp((int)PropEnum.Coin,1000);
        }

        [MenuItem("工具类/October/Get 1K Gold", true)]
        private static bool GetGoldValidation()
        {
            return Application.isPlaying;
        }
    }
}