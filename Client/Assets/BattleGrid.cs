using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BattleGrid : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler,IPointerUpHandler
{
    public Vector2Int Position { get; private set; } // 格子位置
    public Transform ModelRoot { get; private set; }
    public List<GridCharacter> OccupiedCharacters = new List<GridCharacter>(); // 存储最多3个角色
    private List<RectTransform> ModelRootList = new List<RectTransform>();
    private GameObject frameObj;
    private GameObject orignSelectObj;
    private GameObject targetSelectObj;
    private Image rangeImage;
    private bool isPressed = false;
    private bool isDrage = false;
    private void Start()
    {
        ModelRoot = transform.Get<Transform>("ModelRoot");
        frameObj = transform.Get<Transform>("Frame").GameObject();
        orignSelectObj = transform.Find("OrignSelect").gameObject;
        targetSelectObj = transform.Find("TargetSelect").gameObject;
        rangeImage = transform.Get<Image>("Range");
        orignSelectObj.SetActive(false);
        ShowRange(false);
        ShowGrid(false);
        ShowTargetSelect(false);
        ShowOrignSelect(false);
        InitGridCharacter();
    }

    private void InitGridCharacter()
    {
        for (int i = 0; i < ModelRoot.childCount; i++)
        {
            var child = ModelRoot.GetChild(i);
            GridCharacter gridCharacter = child.GetComponent<GridCharacter>();
            if (gridCharacter)
            {
                gridCharacter.RefreshParentGrid(this);
                OccupiedCharacters.Add(gridCharacter);
            }
            else
            {
                ModelRootList.Add(child.GetComponent<RectTransform>());
            }
        }
        RefreshRootPos(OccupiedCharacters.Count);

        int index = 0;
        foreach (var character in OccupiedCharacters)
        {
            character.SetTargetPos(GetMovePos(this, index));
            index++;
        }
    }

//1个节点  root1    5,0
//2个节点  root1   -5,13
//        root2    12,-3
//3个节点          -5，18
//                    18，10
//              -5,-1
    public void MoveToTargetGrid(BattleGrid targetGrid)
    {
        targetGrid.RefreshRootPos(OccupiedCharacters.Count);
        int index = 0;
        foreach (var character in OccupiedCharacters)
        {
            character.MoveTo(GetMovePos(targetGrid,index), () =>
            {
                character.RefreshParentGrid(targetGrid);
            });
            index++;
        }
    }

    private Vector3 GetMovePos(BattleGrid targetGrid,int curIndex)
    {
        return targetGrid.ModelRootList[curIndex].transform.position;
    }
    
    public void RefreshRootPos(int totalCount)
    {
        Vector2 pos1 = default;
        Vector2 pos2 = default;
        Vector2 pos3 = default;
        if (totalCount == 1)
        {
            pos1 = new Vector2(5, 0);
        }
        else if (totalCount == 2)
        {
            pos1 = new Vector2(-5, 13);
            pos2 = new Vector2(12, -3);
        }
        else if (totalCount == 3)
        {
            pos1 = new Vector2(-5, 18);
            pos2 = new Vector2(18, 10);
            pos3 = new Vector2(-5, -1);
        }
        ModelRootList[0].anchoredPosition = pos1;
        ModelRootList[1].anchoredPosition = pos2;
        ModelRootList[2].anchoredPosition = pos3;
    }
    
    public void ShowRange(bool state)
    {
        rangeImage.gameObject.SetActive(state);
        if (state)
        {
            
        }
    }
    
    public void ShowGrid(bool state)
    {
        frameObj.SetActive(state);
    }

    public void ShowTargetSelect(bool state)
    {
        targetSelectObj.SetActive(state);
    }

    public void ShowOrignSelect(bool state)
    {
        orignSelectObj.SetActive(state);
    }
    
    public void UpdateGridPos(Vector2Int pos)
    {
        Position = pos;
    }

    // 尝试占领格子，最多只能放3个角色
    public bool TryOccupy(GridCharacter character)
    {
        if (OccupiedCharacters.Count < 3)
        {
            OccupiedCharacters.Add(character);
            character.transform.SetParent(transform);
            character.transform.localPosition = new Vector3(0, OccupiedCharacters.Count * 50, 0); // 每个角色在格子内有偏移
            return true;
        }
        return false; // 如果格子已满，返回 false
    }

    // 从格子中移除角色
    public void Release(GridCharacter character)
    {
        if (OccupiedCharacters.Contains(character))
        {
            OccupiedCharacters.Remove(character);
        }
    }


    // 鼠标按下时，保持放大状态
    public void OnPointerDown(PointerEventData eventData)
    {
        if (OccupiedCharacters.Count <= 0) return;
        //ShowRange(true);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!BattleGridManager.Instance.Selecting) return;
        BattleGridManager.Instance.SetTargetGrid(this);
        BattleGridManager.Instance.OnSelectTargetGrid(this);
    }

    public void OnMouseDrag()
    {
        BattleGridManager.Instance.OnSelectOrignGrid(this);
        isDrage = true;
    }
    
    // 鼠标松开时，恢复原始大小
    public void OnPointerUp(PointerEventData eventData)
    {
        // 标记为未按下，恢复原始大小
        if (isDrage)
        {
            BattleGridManager.Instance.OnReleaseGrid();
            isPressed = false;
        }
    }
}
