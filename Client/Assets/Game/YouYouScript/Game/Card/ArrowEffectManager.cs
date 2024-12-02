using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class ArrowEffectManager :Singleton<ArrowEffectManager>
{
    private List<ArrowLine> ArrowLineList = new List<ArrowLine>();
    private GameObject parent;
    public void Start()
    {
    }

    public async UniTask<ArrowLine> GetArrowLine(Transform head)
    {
        if (parent == null)
        {
            parent = new GameObject("ArrowEffectManager");
        }
        ArrowLine line = null;
        foreach (var value in ArrowLineList)
        {
            if (!value.IsIdle) continue;
            line = value;
            break;
        }
        if (line == null)
        {
            PoolObj t = await GameEntry.Pool.GameObjectPool.SpawnAsync(Constants.ItemPath.ArrowLine,head.transform.parent);
            t.gameObject.SetActive(true);
            line = t.GetComponent<ArrowLine>();
            line.InitData(t.transform,GameObject.FindGameObjectsWithTag("Spine"));
            line.ShowArrow(true);
            ArrowLineList.Add(line);
        }
        return line;
    }
}

