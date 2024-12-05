using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorManager : MonoBehaviour
{
    [LabelText("关卡ID")] public string levelName;
    private const string DefaultBasePath = "Assets/Game/Download/MapLevel/"; // 默认的保存路径
    public Image bg;
    private LevelData curLevelData;
    private string fullSavePath;
    private LevelStage stageData;
    private EnemyData enemyData;
    private int curStageIndex;
    private bool canGenerateEnemy;
    private float timer = 0;
    private List<Transform> wayPoints;
    private Dictionary<string, AssetBundle> abList = new Dictionary<string, AssetBundle>();
    public static string ProcessPath(string path)
    {
        if (path.StartsWith("Assets/"))
        {
            path = path.Substring("Assets/".Length);
        }
        return path.ToLower() + ".assetbundle";
    }
    
    public static string ConvertToLowerCase(string path)
    {
        string modifiedPath = path.Replace("Assets/", "");
        string fileName = Path.GetFileName(modifiedPath).ToLower();
        return fileName;
    }
    
    void Start()
    {
        fullSavePath = Path.Combine(DefaultBasePath, levelName + ".json");
        if (!File.Exists(fullSavePath))
        {
            GameUtil.LogError($"未找到文件: {fullSavePath}");
            return;
        }
        fullSavePath = Path.Combine(DefaultBasePath, levelName + ".json");
        string json = File.ReadAllText(fullSavePath);
        curLevelData = JsonUtility.FromJson<LevelData>(json);
        GeneratePath();
        ChangeBg();
    }

    private void ChangeBg()
    {
        AssetBundle ab = GetAb(string.Format("{0}/{1}", Application.persistentDataPath,"game/download/textures/background/battle.assetbundle"),curLevelData.bgPath);
        Texture2D texture = ab.LoadAsset<Texture2D>(ConvertToLowerCase(curLevelData.bgPath));
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),new Vector2(0.5f, 0.5f));
            bg.sprite = sprite;
        }
    }

    private AssetBundle GetAb(string path,string mainPath)
    {
        if (abList.ContainsKey(path))
        {
            return abList[path];
        }
        else
        {
            // 加载主 AssetBundle
            AssetBundle ab = AssetBundle.LoadFromFile(path);
        
            // 获取该 AssetBundle 的所有依赖资源路径
            string[] dependencies = AssetDatabase.GetDependencies(mainPath, true);
            foreach (var dep in dependencies)
            {
                // if (!abList.ContainsKey(dep)) // 如果依赖没有加载
                // {
                //     // 加载依赖 AssetBundle
                //     AssetBundle depAb = AssetBundle.LoadFromFile(dep);
                //     abList.Add(dep, depAb);
                // }
                //GameUtil.LogError(dep);
            }

            // 添加到 abList
            abList.Add(path, ab);
            return ab;
        }
    }
    
    private void GeneratePath()
    {
        GetAb(string.Format("{0}/{1}", Application.persistentDataPath,ProcessPath(curLevelData.path)),curLevelData.path);
        AssetBundle ab = GetAb(string.Format("{0}/{1}", Application.persistentDataPath,ProcessPath(curLevelData.path)),curLevelData.path);
        GameObject obj = Instantiate(ab.LoadAsset<GameObject>(ConvertToLowerCase(curLevelData.path)));
        BattleGridManager.Instance.ChangePath(obj);
    }

    private void CheckGenerateEnemy()
    {
        if(curLevelData == null) return;
        if(curStageIndex >= curLevelData.stages.Count) return;
        stageData = curLevelData.stages[curStageIndex];
        if (timer >= stageData.startTime)
        {
            canGenerateEnemy = true;
            curStageIndex++;
        }

        if (canGenerateEnemy)
        {
            StartCoroutine(GenerateEnemy(stageData));
            canGenerateEnemy = false;
        }
    }

    private IEnumerator GenerateEnemy(LevelStage data)
    {
        int count = 0;
        while (count < data.enemy.enemyCount)
        {
            GetAb(string.Format("{0}/{1}", Application.persistentDataPath,"game/download/textures/model.assetbundle"),data.enemy.model);
            AssetBundle ab = GetAb(string.Format("{0}/{1}", Application.persistentDataPath,ProcessPath(data.enemy.model)),data.enemy.model);
            GameObject obj = Instantiate(ab.LoadAsset<GameObject>(ConvertToLowerCase(data.enemy.model)));
            obj.GetComponent<EnemyBase>().InitPath(BattleGridManager.Instance.Path);
            obj.GetComponent<EnemyBase>().StartRun();
            yield return new WaitForSeconds(data.enemy.interval);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        CheckGenerateEnemy();
    }
}
