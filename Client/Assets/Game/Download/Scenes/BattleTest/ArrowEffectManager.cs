using System;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class ArrowEffectManager :Singleton<ArrowEffectManager>
{
    private List<ArrowLine> ArrowLineList = new List<ArrowLine>();
    private GameObject parent;
    private GameObject linePrefab;
    public void Start()
    {
    }

    public ArrowLine GetArrowLine(Transform head)
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

        if (linePrefab == null)
        {
            linePrefab = GameObject.Find("ArrowLine");
            linePrefab.gameObject.SetActive(false);
        }
        if (line == null)
        {
            GameObject t = GameObject.Instantiate(linePrefab, head.transform.parent);
            t.gameObject.SetActive(true);
            line = t.GetComponent<ArrowLine>();
            line.InitData(t.transform,GameObject.FindGameObjectsWithTag("Spine"));
            ArrowLineList.Add(line);
        }
        return line;
    }
}

