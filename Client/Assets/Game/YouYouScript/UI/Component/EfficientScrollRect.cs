using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class ScrollItem : MonoBehaviour
{
    public object Data { get; private set; }
    private int _index = -1;

    public int Index
    {
        get { return _index; }
        private set { _index = value; }
    }

    public virtual void OnDataUpdate(object data, int index)
    {
        Index = index;
        Data = data;
        // gameObject.SetActive(true);
        gameObject.transform.localScale = Vector3.one;
    }

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
    //private GameObjType objType;
    private int _index = -1;
    private int _lastIndex = -1;
    private List<ScrollItem> _itemList = new List<ScrollItem>();
    private object[] datas;

    private List<ScrollItem> _uselessList = new List<ScrollItem>(); //将未显示出来的Item存入未使用队列里面，等待需要使用的时候直接取出

    private float CellWidthWithSpace
    {
        get { return colSpace + cellSize.x; }
    }

    private float CellHeightWithSpace
    {
        get { return lineSpace + cellSize.y; }
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

    // public void Init(GameObjType type, GameObject prefab, object[] datas)
    // {
    //     // 防止在Editor模式下被运行
    //     if (true) //Application.isPlaying)
    //     {
    //         horizontal = vertical ? false : true;
    //         content.pivot = Vector2.up;
    //         content.anchorMin = vertical ? Vector2.up : Vector2.zero;
    //         content.anchorMax = vertical ? Vector2.one : Vector2.up;
    //         content.anchoredPosition = Vector2.zero;
    //         content.sizeDelta = new Vector2(viewport.rect.width, viewport.rect.height);
    //         foreach (Transform child in content)
    //         {
    //             ScrollItem item = child.GetComponent<ScrollItem>();
    //             if (item)
    //             {
    //                 InitItemProperty(item.transform as RectTransform);
    //                 _uselessList.Add(item);
    //             }
    //             else
    //             {
    //                 Destroy(child.gameObject);
    //             }
    //         }
    //     }
    //
    //     this.datas = datas;
    //     Clear();
    //
    //     itemPrefab = prefab;
    //     objType = type;
    //     cellSize = (prefab.transform as RectTransform).sizeDelta;
    //
    //     InitGridCount();
    //     InitCountentSize(this.datas.Length);
    //
    //     StopMovement();
    //     OnValueChange(Vector2.zero);
    //     InitCompleteCb?.Invoke();
    // }

    public void UpdateDatas(object[] datas, bool partialUpdate, Action cb = null)
    {
        if (partialUpdate)
        {
            this.datas = datas;
            foreach (var item in _itemList)
            {
                item.OnDataUpdate(datas[item.Index], item.Index);
            }
        }
        else
        {
            //Init(objType, itemPrefab, datas);
        }
    }

    private void Clear()
    {
        _uselessList.AddRange(_itemList);
        _itemList.Clear();
        foreach (var item in _uselessList)
        {
            item.Clear();
        }

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

    public void OnValueChange(Vector2 pos)
    {
        int index = GetPosIndex();
        if (_index != index && index > -1)
        {
            _index = index;
            for (int i = _itemList.Count; i > 0; i--)
            {
                ScrollItem item = _itemList[i - 1];
                if (item.Index < index || (item.Index >= index + lineCount * colCount))
                {
                    _itemList.Remove(item);
                    _uselessList.Add(item);
                    item.Clear();
                }
            }

            int dataCount = datas != null ? datas.Length : 0;
            for (int i = _index; i < _index + lineCount * colCount; i++)
            {
                if (i >= dataCount) continue;
                if (_itemList.Find((x) => x.Index == i)) continue;
                CreateItem(i);
            }
        }

        ScrollCb?.Invoke();
    }

    private void CreateItem(int index)
    {
        ScrollItem itemBase;
        int count = _uselessList.Count;
        if (count > 0)
        {
            itemBase = _uselessList[count - 1];
            _uselessList.RemoveAt(count - 1);
        }
        else
        {
            itemBase = new ScrollItem();//GameObjManager.Instance.PopGameObject(objType, itemPrefab).GetComponent<ScrollItem>();
            itemBase.transform.SetParent(content);
            itemBase.gameObject.SetActive(true);
            itemBase.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            itemBase.transform.localScale = Vector3.one;
            InitItemProperty(itemBase.transform as RectTransform);
        }

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
        (item.transform as RectTransform).anchoredPosition = GetPosition(index);
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
        foreach (var item in _itemList)
        {
            //GameObjManager.Instance.PushGameObject(item.gameObject);
        }
    }
}