using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace GameScripts
{
    [MonoSingletonPath("[Singleton]/RedDotManager")]
    public class RedDotManager : MonoBehaviour, ISingleton
    {
        public static RedDotManager Instance => MonoSingletonProperty<RedDotManager>.Instance;
        public void OnSingletonInit()
        {
            Init();
        }
        public void OnSingletonDispose()
        {
        }

        public delegate List<RedDotNode> Handler();

        private RedDotNode _rootNode = new RedDotNode(0, 0);
        private Dictionary<int, Handler> _handlerDic = new Dictionary<int, Handler>();
        private List<int> _handlerIds = new List<int>();
        private int _index = 0;

        private void Init()
        {
            Observable.Interval(TimeSpan.FromSeconds(0.1f)).Subscribe(_ =>
            {
                UpdateState();
            });
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
        public async UniTask CreateRedDot(RedDotNode node, Transform parent, Vector2 localPosition)
        {
            GameObject go = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/Prefab/Item/RedDotItem.prefab");
            go.name = "RedDot";
            go.SetActive(true);
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = localPosition;
            RedDotItem item = go.GetComponent<RedDotItem>();
            item.SetNode(node);
        }

        /// <summary>
        /// 获取功能模块跟节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RedDotNode GetModuleRootNode(int nodeId)
        {
            return _rootNode.GetChildNode(nodeId);
        }

        private void UpdateState()
        {
            if (_handlerIds.Count == 0) return;

            int id = _handlerIds[_index];
            var handler = _handlerDic[id];
    
            // 轮询索引优化
            _index = (_index + 1) % _handlerIds.Count;

            var moduleRootNode = GetModuleRootNode(id);
            // 这里如果业务逻辑允许，建议传递一个引用进去，让业务侧往里面填数据，而不是返回新 List
            var nodes = handler.Invoke(); 
    
            if (nodes != null)
            {
                // 关键优化：只在状态真正改变时才设置，避免触发过多的缓存刷新逻辑
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].SetState(true);
                }
            }
        }
    }

    public class RedDotNode
    {
        private int _id;
        private bool _active;
        // 优化：改为数组或列表，减少字典哈希查询开销
        private List<RedDotNode> _children = new List<RedDotNode>();
        private bool _cachedState; // 缓存状态，GetState 不需要递归遍历

        public RedDotNode(int id, int depth) => _id = id;

        public RedDotNode GetChildNode(int id)
        {
            foreach (var child in _children)
                if (child._id == id) return child;

            var newNode = new RedDotNode(id, 0);
            _children.Add(newNode);
            return newNode;
        }

        public bool GetState() => _cachedState; // 直接返回缓存，O(1) 复杂度

        public void SetState(bool active)
        {
            _active = active;
            UpdateCachedState();
        }

        // 优化：当子节点状态变化时，递归向上更新缓存，而不是向下递归查找
        private void UpdateCachedState()
        {
            bool oldState = _cachedState;
            _cachedState = _active || _children.Exists(c => c._cachedState);
        
            // 如果状态变了，这里可以加一个逻辑通知 UI 刷新，或者保持轮询
        }

        public void Reset()
        {
            _active = false;
            foreach (var child in _children) child.Reset();
            _cachedState = false;
        }
    }
    
    partial class RedDotId
    {
        public const int TestRedDotId = 1;//测试红点
    }
}