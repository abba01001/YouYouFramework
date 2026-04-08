using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class LaunchScene
{
    private const string PREF_KEY = "LaunchScene_SelectedMode";

    /// 场景配置
    private static readonly Dictionary<string, string> SceneMap = new()
    {
        { "game", "Assets/Game/Scene_Launch.unity" },
        { "mapEditor", "Assets/Game/Download/Scenes/Map.unity" }
    };

    static LaunchScene()
    {
        EditorApplication.update += Init;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void Init()
    {
        EditorApplication.update -= Init;
        RestoreScene();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            RestoreScene();
    }

    static void RestoreScene()
    {
        string mode = EditorPrefs.GetString(PREF_KEY, "default");

        if (mode == "default")
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }

        if (SceneMap.TryGetValue(mode, out var path))
        {
            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }
    }

    static void SaveSelection(string mode)
    {
        EditorPrefs.SetString(PREF_KEY, mode);
    }

    static bool IsSelected(string mode)
    {
        return EditorPrefs.GetString(PREF_KEY, "default") == mode;
    }

    //=====================
    // Menu
    //=====================

    [MenuItem("启动场景/游戏", true)]
    static bool ValidateGame()
    {
        Menu.SetChecked("启动场景/游戏", IsSelected("game"));
        Menu.SetChecked("启动场景/地图编辑器", IsSelected("mapEditor"));
        Menu.SetChecked("启动场景/手动选择", IsSelected("default"));

        return !EditorApplication.isPlaying;
    }

    [MenuItem("启动场景/游戏")]
    static void GameScene()
    {
        SaveSelection("game");
        RestoreScene();
    }

    [MenuItem("启动场景/地图编辑器")]
    static void MapScene()
    {
        SaveSelection("mapEditor");
        RestoreScene();
    }

    [MenuItem("启动场景/手动选择")]
    static void AnyScene()
    {
        SaveSelection("default");
        RestoreScene();
    }
}