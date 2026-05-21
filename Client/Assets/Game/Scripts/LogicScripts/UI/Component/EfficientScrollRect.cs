using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Main;
using UniRx;
using UnityEngine.UI;

namespace GameScripts
{
    public class ScrollItem : MonoBehaviour
    {
        public object Data { get; private set; }
        private int _index = -1;

        public int Index
        {
            get { return _index; }
            private set { _index = value; }
        }

        public void OnDataUpdate(object data, int index)
        {
            // 脏检查逻辑
            bool dataChanged = (data != Data);

            Data = data;
            Index = index;
            gameObject.transform.localScale = Vector3.one;

            if (dataChanged)
            {
                OnRefreshUI(data,index);
            }
        }

        // 子类只能重写表现层，无法触碰逻辑层
        protected virtual void OnRefreshUI(object data, int index) { }
        
        public virtual void Clear()
        {
            Data = null;
            Index = -1;
            // gameObject.SetActive(false);
            gameObject.transform.localScale = Vector3.zero;
        }
    }

    public class EfficientScrollRect : ScrollRect
    {
        //Item之间的距离
        private int lineSpace = 0;
        private int colSpace = 0;

        //Item的宽高
        private Vector2 cellSize = Vector2.zero;

        //默认加载的Item个数，一般比可显示个数大2~3个
        private int lineCount = 0;
        private int colCount = 0;
        private GameObject itemPrefab;
        private int _index = -1;
        private int _lastIndex = -1;
        private List<ScrollItem> _itemList = new List<ScrollItem>();
        private object[] datas;

        private float CellWidthWithSpace
        {
            get { return colSpace + cellSize.x; }
        }

        private float CellHeightWithSpace
        {
            get { return lineSpace + cellSize.y; }
        }

        public Vector2 CellSize
        {
            get { return cellSize; }
        }

        private Action ScrollCb = null;
        private Action InitCompleteCb = null;
        private Action<ScrollItem> UpdateItemCb = null;

        protected override void Awake()
        {
            base.Awake();
            this.onValueChanged.RemoveListener(OnValueChange);
            this.onValueChanged.AddListener(OnValueChange);
        }

        public void InitItemProperty(RectTransform item)
        {
            item.pivot = Vector2.up;
            item.anchorMax = Vector2.up;
            item.anchorMin = Vector2.up;
        }

        public void SetInitCompleteCb(Action cb)
        {
            InitCompleteCb = cb;
        }

        public void SetScrollCb(Action cb)
        {
            ScrollCb = cb;
        }

        public void SetUpdateItemCb(Action<ScrollItem> cb)
        {
            UpdateItemCb = cb;
        }

        public void SetSpace(int lineSpace, int colSpace)
        {
            this.lineSpace = lineSpace;
            this.colSpace = colSpace;
        }

        public void Init(GameObject prefab, object[] datas)
        {
            // 1. 处理 Content 下初始存在的子物体（比如为了排版预留的）
            if (true) // Application.isPlaying
            {
                horizontal = !vertical; // 简化逻辑
                content.pivot = Vector2.up;
                content.anchorMin = vertical ? Vector2.up : Vector2.zero;
                content.anchorMax = vertical ? Vector2.one : Vector2.up;
                content.anchoredPosition = Vector2.zero;

                // 这里的逻辑需要修改：不要 Destroy，而是直接清理或回收
                // 如果这些子物体也是池子里的，就 Despawn；如果是死的，就 Destroy
                foreach (Transform child in content)
                {
                    ScrollItem item = child.GetComponent<ScrollItem>();
                    if (item)
                    {
                        // 如果你有统一的对象池管理这些初始物体：
                        GameEntry.Pool.GameObjectPool.Despawn(child.gameObject);
                        // 如果只是临时占位，直接销毁：
                        // Destroy(child.gameObject);
                    }
                    else
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            this.datas = datas;

            Clear();

            itemPrefab = prefab;
            cellSize = (prefab.transform as RectTransform).sizeDelta;

            InitGridCount();
            InitCountentSize(this.datas.Length);

            StopMovement();

            // 3. 这里的 OnValueChange 会触发第一次的 CreateItem (异步池子加载)
            OnValueChange(Vector2.zero);
            InitCompleteCb?.Invoke();
        }

        public void UpdateDatas(object[] datas, bool partialUpdate, Action cb = null)
        {
            if (partialUpdate && this.datas != null && this.datas.Length == datas.Length)
            {
                // 只有长度完全一致时，才执行极其轻量的局部刷新
                this.datas = datas;
                foreach (var item in _itemList)
                {
                    // 增加一层保护，防止 Index 异常
                    if (item.Index >= 0 && item.Index < datas.Length)
                    {
                        item.OnDataUpdate(datas[item.Index], item.Index);
                    }
                }
            }
            else
            {
                // 只要长度变了，或者强制全量刷新，必须重新计算 Content 长度并清理
                this.datas = datas;
        
                // 1. 重新计算 Content 大小（否则滚动范围不对）
                InitCountentSize(this.datas.Length);
        
                // 2. 强制触发一次 OnValueChange 逻辑
                // 注意：这里最稳妥的做法是调用 Init，或者手动重置 _index 强制刷新
                _index = -1; 
                OnValueChange(content.anchoredPosition);
            }
            cb?.Invoke();
        }

        private void Clear()
        {
            foreach (var item in _itemList)
            {
                if (item != null)
                {
                    item.Clear();
                    GameEntry.Pool.GameObjectPool.Despawn(item.gameObject);
                }
            }

            _itemList.Clear();
            _visibleIndices.Clear();
            _index = -1;
            _lastIndex = -1;
        }

        private void InitGridCount()
        {
            if (vertical)
            {
                lineCount = Mathf.CeilToInt((viewport.rect.height + lineSpace) / CellHeightWithSpace) + 1;
                colCount = Mathf.FloorToInt((viewport.rect.width + colSpace) / CellWidthWithSpace);
                colCount = colCount < 1 ? 1 : colCount;
            }
            else
            {
                lineCount = Mathf.FloorToInt((viewport.rect.height + lineSpace) / CellHeightWithSpace);
                colCount = Mathf.CeilToInt((viewport.rect.width + colSpace) / CellWidthWithSpace) + 1;
                lineCount = lineCount < 1 ? 1 : lineCount;
            }
        }

        private readonly HashSet<int> _visibleIndices = new HashSet<int>();

        public void OnValueChange(Vector2 pos)
        {
            int startIndex = GetPosIndex();
            if (_index == startIndex) return;

            _index = startIndex;
            int maxCount = lineCount * colCount;
            int endIndex = Mathf.Min(_index + maxCount, datas.Length);

            // 1. 回收不再可见的 Item
            for (int i = _itemList.Count - 1; i >= 0; i--)
            {
                var item = _itemList[i];
                if (item.Index < _index || item.Index >= endIndex)
                {
                    _visibleIndices.Remove(item.Index);

                    //滑出边界，立刻回收
                    item.Clear();
                    GameEntry.Pool.GameObjectPool.Despawn(item.gameObject);

                    _itemList.RemoveAt(i);
                }
            }

            // 2. 显示新进入视野的 Item
            for (int i = _index; i < endIndex; i++)
            {
                if (_visibleIndices.Contains(i)) continue;
                _visibleIndices.Add(i);
                CreateItem(i).Forget(); // UniTask 调用
            }
        }

        private async UniTaskVoid CreateItem(int index)
        {
            object originalData = datas[index];
            
            GameObject obj =
                await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/Prefab/Item/BagItem.prefab");

            if (this == null || datas == null || index >= datas.Length || datas[index] != originalData)
            {
                if (obj != null) GameEntry.Pool.GameObjectPool.Despawn(obj);
                return;
            }

            int currentMaxCount = lineCount * colCount;
            if (index < _index || index >= _index + currentMaxCount || datas == null)
            {
                GameEntry.Pool.GameObjectPool.Despawn(obj);
                _visibleIndices.Remove(index); // 记得移除标记
                return;
            }

            ScrollItem itemBase = obj.GetComponent<ScrollItem>();

            // 设置层级和位置
            itemBase.transform.SetParent(content);
            itemBase.gameObject.SetActive(true);
            itemBase.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            itemBase.transform.localScale = Vector3.one;
            InitItemProperty(itemBase.transform as RectTransform);

            // 刷新内容
            UpdateIndex(itemBase, index);

            _itemList.Add(itemBase);
            UpdateLayer(itemBase, index);
            UpdateItemCb?.Invoke(itemBase);
        }

        private void UpdateLayer(ScrollItem item, int index)
        {
            if (_lastIndex < index)
            {
                item.transform.SetAsLastSibling();
            }
            else
            {
                item.transform.SetAsFirstSibling();
            }

            _lastIndex = index;
        }

        private void UpdateIndex(ScrollItem item, int index)
        {
            item.OnDataUpdate(datas[index], index);
            Vector2 targetPos = GetPosition(index);
            (item.transform as RectTransform).anchoredPosition = targetPos;
        }

        private int GetPosIndex()
        {
            if (vertical)
            {
                // 正常情况下 y 的值 均为正值
                float y = content.anchoredPosition.y;
                y = y < 0 ? 0 : y;
                return Mathf.FloorToInt(y / CellHeightWithSpace) * colCount;
            }
            else
            {
                // 正常情况下 x 的值 均为负值
                float x = content.anchoredPosition.x;
                x = x > 0 ? 0 : -x;
                return Mathf.FloorToInt(x / CellWidthWithSpace) * lineCount;
            }
        }

        public Vector3 GetPosition(int i)
        {
            if (vertical)
            {
                return new Vector3((i % colCount) * CellWidthWithSpace, (i / colCount) * -CellHeightWithSpace, 0f);
            }
            else
            {
                return new Vector3((i / lineCount) * CellWidthWithSpace, (i % lineCount) * -CellHeightWithSpace, 0f);
            }
        }

        private void InitCountentSize(int dataCount)
        {
            if (vertical)
            {
                int count = Mathf.CeilToInt(dataCount * 1f / colCount);
                content.sizeDelta = new Vector2(0, CellHeightWithSpace * count - lineSpace);
            }
            else
            {
                int count = Mathf.CeilToInt(dataCount * 1f / lineCount);
                content.sizeDelta = new Vector2(CellWidthWithSpace * count - colSpace, 0);
            }
        }

        public RectTransform GetContentRect()
        {
            return content;
        }

        public int GetDataLength()
        {
            return datas.Length;
        }

        public int GetCurIndex()
        {
            return _index;
        }

        public void SetScrollPos(int index, bool needAnim = false, float duration = 0.3f)
        {
            Vector2 pos = vertical
                ? new Vector2(content.anchoredPosition.x, index * cellSize.y)
                : new Vector2(-index * cellSize.x, content.anchoredPosition.y);
            if (!needAnim)
            {
                content.anchoredPosition = pos;
            }
            else
            {
                content.DOAnchorPos(pos, duration);
            }

            OnValueChange(pos);
        }

        public List<ScrollItem> GetItemList()
        {
            return _itemList;
        }

        protected override void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }
    }
}