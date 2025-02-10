#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;
using YouYou;

public class JsonDisplay : MonoBehaviour
{

    [HideInInspector]
    public string jsonString = "{\"提示\": \"游戏内按空格键\", \"作用\": \"可展示玩家数据\"}";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jsonString = GameEntry.Data.PrintUserData();
            // GameEntry.Data.InitGameData(null);
            // GameEntry.Data.SaveData(true);
        }
    }
}

#endif