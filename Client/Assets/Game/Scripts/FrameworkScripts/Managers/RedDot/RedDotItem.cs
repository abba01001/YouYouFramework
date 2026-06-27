using System;
using System.Collections;
using System.Collections.Generic;
using GameScripts;
using Main;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class RedDotItem : MonoBehaviour
{
    private RedDotNode _node;
    private Transform _child;
    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (_child != null) return;
        _child = transform.Find("Child");
    }

    void Update()
    {
        OnUpdate();
    }

    private void OnUpdate()
    {
        if (_node == null)
        {
            SetVisible(false);
            return;
        }
        SetVisible(_node.GetState());
    }

    private void SetVisible(bool visible)
    {
        if(_child.gameObject.activeSelf == visible)
        {
            return;
        }
        _child.gameObject.SetActive(visible);
    }

    public void SetNode(RedDotNode node)
    {
        if (_node != null && _node == node) return;
        _node = node;
    }
}