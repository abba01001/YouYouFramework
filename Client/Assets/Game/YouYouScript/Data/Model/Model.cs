using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class TestModel
{
    public string path;
    public long version;
}

[Serializable]
public class LevelModelData
{
    public string modelPrefabName;  // 模型的Prefab路径
    public Vector3 position;         // 模型的位置
    public Quaternion rotation;      // 模型的旋转
    public Vector3 scale;            // 模型的缩放
    
    public LevelModelData() 
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        scale = Vector3.one;
    }
}

[Serializable]
public class PlayerRoleData
{
    public List<float> playerBornPos;
    public List<float> playerPos;
    public List<float> playerRotate;
    public List<float> cameraRotate;
    public bool firstEntryLevel;
    public int levelId;
    public PlayerRoleData()
    {
        playerBornPos = new List<float>();
        playerPos = new List<float>();
        playerRotate = new List<float>();
        cameraRotate = new List<float>();
        firstEntryLevel = true;
        levelId = 1;
    }
}

[Serializable]
public class LevelData
{
    public List<LevelModelData> models;  // 存储模型数据的列表
    public bool isRandomGenerated; // 添加随机地图标志
    public List<float> bornPos;
    public LevelData()
    {
        models = new List<LevelModelData>();
        isRandomGenerated = false;
        bornPos = new List<float>(3) {0, 0, 0};
    }
}
