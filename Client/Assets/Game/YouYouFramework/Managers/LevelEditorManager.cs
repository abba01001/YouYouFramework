using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class LevelEditorManager : MonoBehaviour
{
    [LabelText("关卡ID")] public string levelName;
    private const string DefaultBasePath = "Assets/Game/Download/MapLevel/"; // 默认的保存路径
    private LevelData curLevelData;
    private string fullSavePath;
    private LevelStage stageData;
    private EnemyData enemyData;
    private int curStageIndex;
    private bool canGenerateEnemy;
    private float timer = 0;
    private List<Transform> wayPoints;
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
            GenerateEnemy(stageData);
            canGenerateEnemy = false;
        }
    }

    private async void GenerateEnemy(LevelStage data)
    {
        int count = 0;
        while (count < data.enemy.enemyCount)
        {
            PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync(Constants.ModelPath.Enemy21001);
            obj.GetComponent<EnemyBase>().InitPath(BattleGridManager.Instance.Path);
            obj.GetComponent<EnemyBase>().StartRun();
            count++;
            await UniTask.Delay(TimeSpan.FromSeconds(data.enemy.interval));
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        CheckGenerateEnemy();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BattleGridManager.Instance.CallHero();
        }
    }
}
