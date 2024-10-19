using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityManager : MonoBehaviour
{
    public ActivityManager()
    {
        RedDotMgr.Instance.Register(RedDotId.SubscribeGuide, GetRedDotNodes);
    }

    public List<RedDotNode> GetRedDotNodes()
    {
        List<RedDotNode> list = new List<RedDotNode>();
        
        RedDotNode rootNode = RedDotMgr.Instance.GetModuleRootNode(RedDotId.SubscribeGuide);
        //rootNode.AddChildNode(1); 有子节点
        list.Add(rootNode);
        return list;
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
