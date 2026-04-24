using System.Collections.Generic;

public class RedDotNode
{
    private int _id;
    private int _depth;
    private bool _active;
    private Dictionary<int, RedDotNode> _childDic;
    public RedDotNode(int id, int depth)
    {
        _id = id;
        _depth = depth;
        _childDic = new Dictionary<int, RedDotNode>();
    }

    public RedDotNode AddChildNode(int id)
    {
        if (!_childDic.ContainsKey(id))
        {
            RedDotNode child = new RedDotNode(id, _depth + 1);
            _childDic.Add(id, child);
        }
        return _childDic[id];
    }

    public RedDotNode GetChildNode(int id)
    {
        if (_childDic.ContainsKey(id))
        {
            return _childDic[id];
        }
        return AddChildNode(id);
    }

    public int GetId()
    {
        return _id;
    }

    public bool GetState()
    {
        if (_childDic.Count == 0)
        {
            return _active;
        }
        foreach (var node in _childDic)
        {
            if (node.Value.GetState())
            {
                return true;
            }
        }
        return false;
    }

    public void SetState(bool active)
    {
        _active = active;
    }

    public void Reset()
    {
        SetState(false);
        foreach (var node in _childDic)
        {
            node.Value.SetState(false);
        }
    }
}