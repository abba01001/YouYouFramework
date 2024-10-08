using UnityEditor;
using UnityEditor.SceneManagement;

public class LanuchScene
{
    [MenuItem("启动场景/游戏", true)]
    static bool ValidatePlayModeUseFirstScene()
    {
        SceneAsset game = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Game/Scene_Launch.unity");
        Menu.SetChecked("启动场景/游戏", EditorSceneManager.playModeStartScene == game);
        Menu.SetChecked("启动场景/手动选择", EditorSceneManager.playModeStartScene == null);
        return !EditorApplication.isPlaying;
    }

    [MenuItem("启动场景/游戏")]
    static void GameScene()
    {
        Menu.SetChecked("启动场景/关卡编辑器", false);
        Menu.SetChecked("启动场景/游戏", true);
        Menu.SetChecked("启动场景/手动选择", false);
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Game/Scene_Launch.unity");
        EditorSceneManager.playModeStartScene = scene;
    }

    [MenuItem("启动场景/关卡编辑器")]
    static void MapEditorScene()
    {
        Menu.SetChecked("启动场景/关卡编辑器", true);
        Menu.SetChecked("启动场景/游戏", false);
        Menu.SetChecked("启动场景/手动选择", false);
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Game/MapEditor.unity");
        EditorSceneManager.playModeStartScene = scene;
    }

    [MenuItem("启动场景/手动选择")]
    static void AnyScene()
    {
        Menu.SetChecked("启动场景/游戏", false);
        Menu.SetChecked("启动场景/关卡编辑器", false);
        // Menu.SetChecked("启动场景/关卡编辑器", false);
        Menu.SetChecked("启动场景/手动选择", true);
        EditorSceneManager.playModeStartScene = null;
    }
}