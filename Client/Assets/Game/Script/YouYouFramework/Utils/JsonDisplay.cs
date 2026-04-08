#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;
using UnityEngine.InputSystem;


public class JsonDisplay : MonoBehaviour
{

    [HideInInspector]
    public string jsonString = "{\"提示\": \"游戏内按空格键\", \"作用\": \"可展示玩家数据\"}";

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jsonString = GameEntry.Data.PrintUserData();
        }
    }
}

#endif