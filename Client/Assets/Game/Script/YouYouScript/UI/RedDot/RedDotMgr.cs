using System;
using System.Collections.Generic;
using UnityEngine;

public class RedDotMgr
{
    public delegate List<RedDotNode> Handler();
    private static RedDotMgr _instace;
    public static RedDotMgr Instance
    {
        get
        {
            if (_instace == null)
            {
                _instace = new RedDotMgr();
                _instace.Init();
            }
            return _instace;
        }
    }

    private RedDotNode _rootNode = new RedDotNode(0,0);
    private Dictionary<int, Handler> _handlerDic = new Dictionary<int, Handler>();
    private List<int> _handlerIds = new List<int>();
    private int _index = 0;



    private void Init()
    {
        //Observable.Interval(TimeSpan.FromSeconds(0.1f)).Subscribe(_ =>
        //{
        //    Update();
        //});
    }

    /// <summary>
    /// 注册模块的红点，回调方法里返回当前模块下所有有红点提示的节点
    /// </summary>
    /// <param name="nodeId">模块的红点id</param>
    /// <param name="handler"></param>
    public void Register(int nodeId, Handler handler)
    {
        _handlerDic.Add(nodeId, handler);
        _handlerIds.Add(nodeId);
    }

    /// <summary>
    /// 创建一个红点
    /// </summary>
    /// <param name="node">红点对应的节点</param>
    /// <param name="parent">红点的父级</param>
    /// <param name="localPosition">红点坐标</param>
    public void CreateRedDot(RedDotNode node, Transform parent,Vector2 localPosition)
    {
        //GameObject go = GameObjManager.Instance.PopGameObject(GameObjType.RedDotItem);
        //go.name = "RedDot";
        //go.SetActive(true);
        //go.transform.SetParent(parent);
        //go.transform.localScale = Vector3.one;
        //go.transform.localPosition = localPosition;
        //RedDotItem item = go.GetComponent<RedDotItem>();
        //item.SetNode(node);
    }

    /// <summary>
    /// 获取功能模块跟节点
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public RedDotNode GetModuleRootNode(int nodeId)
    {
        return  _rootNode.GetChildNode(nodeId);
    }

    private void Update()
    {
        if(_handlerIds.Count == 0)
        {
            return;
        }
        int id = _handlerIds[_index];
        Handler handler = _handlerDic[id];
        _index++;
        if (_index >= _handlerIds.Count)
        {
            _index = 0;
        }

        RedDotNode moduleRootNode = GetModuleRootNode(id);
        moduleRootNode.Reset();
        List<RedDotNode> nodes = handler.Invoke();
        if (nodes == null)
        {
            return;
        }
        for(int i=0;i< nodes.Count; i++)
        {
            RedDotNode node = nodes[i];
            node.SetState(true);
        }
    }
}


