using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapItemBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
    IPointerUpHandler
{
    public Text text;
    public Image bgImage;
    public List<Sprite> Sprites;
    public List<Sprite> BossSprites;


    private Vector3 originalScale; // 原始大小
    public float maxScale = 1.3f; // 最大缩放比例
    public float scaleDuration = 0.2f; // 动画时长

    public string name;
    public int layer;
    public MapItemBase DownItem;
    private List<MapItemBase> topItemList;
    private bool isPressed = false; // 标记是否被按下

    private Image iconImage; // 如果是UI图标，可以使用Image组件来改变颜色
    public Color defaultColor = Color.white; // 默认颜色
    public Color pressedColor = Color.black; // 点击时的颜色

    private void Start()
    {
        originalScale = transform.localScale;
        iconImage = GetComponentInChildren<Image>(true);
        defaultColor = iconImage.color;
    }

    public List<MapItemBase> TopItemList
    {
        get
        {
            if (topItemList == null) topItemList = new List<MapItemBase>();
            return topItemList;
        }
        set => topItemList = value;
    }

    public void RefreshUI(string name, int layer)
    {
        this.name = name;
        this.layer = layer;
        if (text != null)
        {
            text.text = name;
        }

        gameObject.name = name;
        RefreshBgImage();
        bgImage.SetNativeSize();
    }

    private void RefreshBgImage()
    {
        if (Sprites == null || Sprites.Count <= 0 || layer == 0)
        {
            return;
        }

        if (layer == MapManager.Instance.MaxLayer - 2)
        {
            bgImage.sprite = Sprites[3];
            return;
        }

        if (layer == (MapManager.Instance.MaxLayer - 1) && BossSprites.Count > 0)
        {
            int randomBoss = Random.Range(0, BossSprites.Count);
            bgImage.sprite = BossSprites[randomBoss];
            return;
        }

        int random = Random.Range(0, 30);
        if (random < 20)
        {
            bgImage.sprite = Sprites[0];
            return;
        }

        random = Random.Range(0, Sprites.Count);
        bgImage.sprite = Sprites[random];
    }

    /// <summary>
    /// 得到距离当前对象最近的对象
    /// </summary>
    /// <param name="ComparelayerDic"></param>
    /// <returns></returns>
    public MapItemBase GetNearestDistanceItem(Dictionary<int, MapItemBase> ComparelayerDic)
    {
        if (ComparelayerDic == null)
        {
            return null;
        }

        float dis = -1;
        bool isStart = false;
        MapItemBase topItem = null;
        foreach (var item in ComparelayerDic)
        {
            MapItemBase targetItem = item.Value;
            if (isStart == false)
            {
                isStart = true;
                dis = Vector3.Distance(transform.position, targetItem.transform.position);
                topItem = targetItem;
            }
            else
            {
                float nowDis = Vector3.Distance(transform.position, targetItem.transform.position);
                if (nowDis < dis)
                {
                    dis = nowDis;
                    topItem = targetItem;
                }
            }
        }

        return topItem;
    }

// 鼠标进入时，执行放大动画（但仅在没有点击时）
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isPressed) // 只有在没有被按下时，才进行缩放
        {
            transform.DOScale(originalScale * maxScale, scaleDuration).SetEase(Ease.OutQuad);
            iconImage.DOColor(pressedColor, scaleDuration);
        }
    }

    // 鼠标退出时，恢复原始大小
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPressed) // 只有在没有被按下时，才恢复原始大小
        {
            transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);
            iconImage.DOColor(defaultColor, scaleDuration);
        }
    }

    // 鼠标按下时，保持放大状态
    public void OnPointerDown(PointerEventData eventData)
    {
        // 标记为已按下，立即放大
        isPressed = true;
        transform.localScale = originalScale * maxScale;
        iconImage.DOColor(pressedColor, scaleDuration);
    }

    // 鼠标松开时，恢复原始大小
    public void OnPointerUp(PointerEventData eventData)
    {
        // 标记为未按下，恢复原始大小
        isPressed = false;
        transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);
        iconImage.DOColor(defaultColor, scaleDuration);
    }

    public void Clear()
    {
        Destroy(this.gameObject);
    }
}