using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DunGen;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;


public class LevelEditorWindow : OdinEditorWindow
{
    private const string DefaultBasePath = "Assets/Game/Download/DunGenMap/Level/"; // 默认的保存路径
    private string basePath = DefaultBasePath; // 当前保存路径
    private GameObject MapGenerator = null;

    // 窗口显示
    [MenuItem("Tools/关卡编辑器")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("关卡编辑器");
    }

    // 关卡名称与保存按钮
    [HorizontalGroup("LevelInfo")] [LabelText("关卡名称")] [PropertySpace(10)] [OnValueChanged("UpdateSavePath")]
    public string levelName = "DefaultLevel"; // 默认关卡名

    [HorizontalGroup("LevelInfo", width: 0.3f)]
    [Button("保存关卡", ButtonSizes.Large)]
    [GUIColor(0.5f, 0.8f, 0.5f)]
    [PropertySpace(5)]
    public void SaveLevel() => SaveCurrentLevel();

    [HorizontalGroup("LevelInfo", width: 0.3f)]
    [Button("加载关卡", ButtonSizes.Large)]
    [GUIColor(0.5f, 0.8f, 0.5f)]
    [PropertySpace(5)]
    public void LoadLevel() => LoadCurrentLevel();

    // 保存路径设置
    [BoxGroup("关卡编辑器")] [HorizontalGroup("PathGroup")] [LabelText("保存路径")] [HideIf("isCustomPathEnabled")] [ReadOnly]
    public string fullSavePath;

    [HorizontalGroup("PathGroup", width: 0.2f)] [LabelText("自定义路径")] [ToggleLeft]
    public bool isCustomPathEnabled;

    [BoxGroup("关卡编辑器")] [ShowIf("isCustomPathEnabled")] [LabelText("自定义保存路径")]
    public string customPath;

    [BoxGroup("关卡编辑器")] [LabelText("加密保存")]
    public bool encryptSave = true; // 加密保存选项

    [BoxGroup("关卡编辑器")] [LabelText("是否随机生成地图")] [ReadOnly]
    public bool isRandomGeneratedDisplayed;

    [BoxGroup("关卡编辑器")] [LabelText("保存为随机生成地图")] [ToggleLeft]
    public static bool isRandomGenerated;

    private static bool hasLoadedPrefab = false;
    private static List<float> BornPos = new List<float>(3) {0, 0, 0};
    // 窗口启用时初始化
    private void OnEnable()
    {
        UpdateSavePath();
        if (!hasLoadedPrefab)
        {
            LoadDungeonGenerator();
            hasLoadedPrefab = true;
        }
    }

    private void OnDisable()
    {
        CleanUpDungeon();
        hasLoadedPrefab = false;
    }

    // 加载地图生成器
    private void LoadDungeonGenerator()
    {
        string prefabPath = "Assets/Game/Download/Prefab/DungeonMap/DungeonGenerator.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            MapGenerator = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            RuntimeDungeon runtimeDungeon = MapGenerator.GetComponent<RuntimeDungeon>();
            runtimeDungeon.GenerateOnStart = false;
        }
        else
        {
            Debug.LogWarning("未找到指定的预制体。");
        }
    }

    // 实时更新保存路径
    private void UpdateSavePath()
    {
        string path = isCustomPathEnabled ? customPath : DefaultBasePath;
        fullSavePath = Path.Combine(path, levelName + ".json");
        Repaint();
    }

    // 随机生成地图
    [HorizontalGroup("MapGenerator")]
    [Button("随机生成地图", ButtonSizes.Large)]
    [GUIColor(0.5f, 0.8f, 0.5f)]
    [PropertySpace(5)]
    public void RandomGenerate()
    {
        if (MapGenerator != null)
        {
            MapGenerator.GetComponent<RuntimeDungeon>().Generate();
        }
        else
        {
            EditorUtility.DisplayDialog("地图生成器", "请先初始化地图生成器", "确定");
        }
    }

    // 清理场景
    [HorizontalGroup("MapGenerator")]
    [Button("清理场景", ButtonSizes.Large)]
    [GUIColor(0.8f, 0.5f, 0.5f)]
    [PropertySpace(5)]
    public void CleanLevel()
    {
        bool confirm = EditorUtility.DisplayDialog("清理场景", "是否清理当前场景所有预制体", "是", "否");
        if (confirm)
        {
            CleanUpDungeon();
            LoadDungeonGenerator();
        }
    }

    // 保存关卡
    private void SaveCurrentLevel()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogWarning("关卡名称不能为空！");
            return;
        }

        string path = isCustomPathEnabled ? customPath : DefaultBasePath;
        string fullSavePath = Path.Combine(path, levelName + ".json");

        if (File.Exists(fullSavePath) && !ConfirmOverwrite(fullSavePath)) return;

        LevelData levelData = GatherLevelData();
        SaveLevelDataToFile(fullSavePath, levelData);
    }

    // 确认覆盖文件
    private bool ConfirmOverwrite(string path)
    {
        return EditorUtility.DisplayDialog("确认覆盖", $"文件 '{path}' 已经存在，是否覆盖？", "是", "否");
    }

    // 收集关卡数据
    private static LevelData GatherLevelData()
    {
        LevelData levelData = new LevelData
        {
            isRandomGenerated = isRandomGenerated,
            models = new List<LevelModelData>(),
            bornPos = BornPos
        };

        GameObject dungeon = GameObject.Find("Dungeon");
        if (dungeon != null)
        {
            foreach (Transform child in dungeon.transform)
            {
                if (child.TryGetComponent(out Tile tile))
                {
                    levelData.models.Add(new LevelModelData
                    {
                        modelPrefabName = child.gameObject.name.Replace("(Clone)", "").Trim(),
                        position = child.position,
                        rotation = child.rotation,
                        scale = child.localScale,
                    });
                }
            }
        }
        else
        {
            Debug.LogWarning("未找到 'Dungeon' 物体。");
        }

        return levelData;
    }

    // 保存关卡数据到文件
    private void SaveLevelDataToFile(string path, LevelData levelData)
    {
        string json = JsonUtility.ToJson(levelData, true);
        if (encryptSave)
        {
            json = Constants.ENCRYPTEDKEY + SecurityUtil.Encrypt(json); // 添加加密标记
        }

        File.WriteAllText(path, json);
        Debug.Log($"关卡数据已保存到: {path}");
    }

    // 加载关卡
    private void LoadCurrentLevel()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogWarning("关卡名称不能为空！");
            return;
        }

        string path = isCustomPathEnabled ? customPath : DefaultBasePath;
        string fullSavePath = Path.Combine(path, levelName + ".json");

        if (!File.Exists(fullSavePath))
        {
            Debug.LogWarning($"未找到文件: {fullSavePath}");
            return;
        }

        string json = File.ReadAllText(fullSavePath);
        if (json.StartsWith(Constants.ENCRYPTEDKEY))
        {
            json = SecurityUtil.Decrypt(json.Substring(Constants.ENCRYPTEDKEY.Length));
        }

        LoadLevelDataFromJson(json);
    }

    // 从JSON加载关卡数据
    private void LoadLevelDataFromJson(string json)
    {
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        isRandomGeneratedDisplayed = levelData.isRandomGenerated;

        CleanUpDungeon();

        GameObject dungeon = new GameObject("Dungeon");
        foreach (LevelModelData modelData in levelData.models)
        {
            InstantiatePrefab(modelData, dungeon.transform);
        }
    }

    // 清理当前场景
    private void CleanUpDungeon()
    {
        GameObject dungeon = GameObject.Find("Dungeon");
        if (dungeon != null)
        {
            DestroyImmediate(dungeon);
        }

        if (MapGenerator != null)
        {
            DestroyImmediate(MapGenerator);
        }
    }

    // 实例化预制体
    private void InstantiatePrefab(LevelModelData modelData, Transform parent)
    {
        string prefabPath = GetPrefabPath(modelData);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, modelData.position, modelData.rotation, parent);
            instance.transform.localScale = modelData.scale;
        }
        else
        {
            Debug.LogWarning($"未找到预制体: {modelData.modelPrefabName}");
        }
    }

    public static void SaveBornPos(Vector3 pos)
    {
        BornPos = new List<float>() {pos.x, pos.y, pos.z};
    }
    
    // 获取预制体路径
    private string GetPrefabPath(LevelModelData modelData)
    {
        string prefabPath = "";
        if (modelData.modelPrefabName.Contains("Castle"))
        {
            prefabPath = Constants.CASTLEPATH + $"{modelData.modelPrefabName}.prefab";
        }
        else if (modelData.modelPrefabName.Contains("Graveyard"))
        {
            prefabPath = Constants.GRAVEYARDPATH + $"{modelData.modelPrefabName}.prefab";
        }
        else
        {
            EditorUtility.DisplayDialog("加载预制体错误", $"类型{modelData.modelPrefabName}", "确定");
            return "";
        }

        return prefabPath; // 自定义路径
    }
}