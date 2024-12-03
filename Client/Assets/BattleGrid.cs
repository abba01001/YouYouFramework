using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleGrid : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,IPointerUpHandler
{
    public Vector2Int Position { get; private set; } // 格子位置
    public List<GridCharacter> OccupiedCharacters = new List<GridCharacter>(); // 存储最多3个角色

    private Image gridImage;
    private GameObject orignSelectObj;
    private GameObject targetSelectObj;
    private bool isPressed = false;
    private void Start()
    {
        gridImage = transform.Get<Image>("Root");
        orignSelectObj = transform.Find("OrignSelect").gameObject;
        targetSelectObj = transform.Find("TargetSelect").gameObject;
        orignSelectObj.SetActive(false);
        ShowGrid(false);
        ShowTargetSelect(false);
        ShowOrignSelect(false);
    }

    public void ShowGrid(bool state)
    {
        gridImage.color = state ? Color.white : Color.clear;
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
        // 标记为已按下，立即放大
        isPressed = true;
        BattleGridManager.Instance.OnSelectOrignGrid(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!BattleGridManager.Instance.Selecting) return;
        if (!isPressed) // 只有在没有被按下时，才进行缩放
        {
            BattleGridManager.Instance.OnSelectTargetGrid(this);
        }
    }

    // 鼠标松开时，恢复原始大小
    public void OnPointerUp(PointerEventData eventData)
    {
        // 标记为未按下，恢复原始大小
        if (isPressed)
        {
            isPressed = false;
            BattleGridManager.Instance.OnReleaseGrid(this);
        }
    }
        
    // 鼠标退出时，恢复原始大小
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPressed) // 只有在没有被按下时，才恢复原始大小
        {

        }
    }
}
