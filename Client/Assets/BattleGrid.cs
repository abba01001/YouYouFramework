using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleGrid : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]private Vector2Int Position; // 格子位置
    public List<GridCharacter> OccupiedCharacters = new List<GridCharacter>(); // 存储最多3个角色

    private Image gridImage;
    private GameObject selectionIndicator;

    private void Awake()
    {
        gridImage = GetComponent<Image>();
        selectionIndicator = transform.Find("Select").gameObject;
        selectionIndicator.SetActive(false);
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

    public void ShowSelect(bool state)
    {
        selectionIndicator.SetActive(state);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BattleGridManager.Instance.OnGridSelected(this);
    }
}
