using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts
{
    [MonoSingletonPath("[Singleton]/ActivityManager")]
    public class ActivityManager : MonoBehaviour, ISingleton
    {
        public static RedDotManager Instance => MonoSingletonProperty<RedDotManager>.Instance;
        public void OnSingletonInit()
        {
            RedDotManager.Instance.Register(RedDotId.TestRedDotId, GetRedDotNodes);
        }
        public void OnSingletonDispose()
        {
        }

        public List<RedDotNode> GetRedDotNodes()
        {
            List<RedDotNode> list = new List<RedDotNode>();
            RedDotNode rootNode = RedDotManager.Instance.GetModuleRootNode(RedDotId.TestRedDotId);
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
}