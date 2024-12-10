using UnityEditor;
using UnityEditor.SceneManagement;

public class LaunchScene
{
    private const string PREF_KEY_SELECTED_MODE = "LaunchScene_SelectedMode"; // 保存选择的键名
    private static bool isSelectGame => EditorPrefs.GetString(PREF_KEY_SELECTED_MODE, "default") == "game";
    private static bool isSelectMapEditor => EditorPrefs.GetString(PREF_KEY_SELECTED_MODE, "default") == "mapEditor";
    private static bool isDefault => EditorPrefs.GetString(PREF_KEY_SELECTED_MODE, "default") == "default";

    [MenuItem("启动场景/游戏", true)]
    static bool ValidatePlayModeUseFirstScene()
    {
        Menu.SetChecked("启动场景/游戏", isSelectGame);
        Menu.SetChecked("启动场景/关卡编辑器", isSelectMapEditor);
        Menu.SetChecked("启动场景/手动选择", isDefault);
        return !EditorApplication.isPlaying;
    }

    [MenuItem("启动场景/游戏")]
    static void GameScene()
    {
        SaveSelection("game");
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Game/Scene_Launch.unity");
    }

    [MenuItem("启动场景/关卡编辑器")]
    static void MapEditorScene()
    {
        SaveSelection("mapEditor");
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Game/Scene_Launch.unity");
    }

    [MenuItem("启动场景/手动选择")]
    static void AnyScene()
    {
        SaveSelection("default");
        EditorSceneManager.playModeStartScene = null;
    }

    // 保存选择状态到 EditorPrefs
    static void SaveSelection(string mode)
    {
        EditorPrefs.SetString(PREF_KEY_SELECTED_MODE, mode);
    }
}