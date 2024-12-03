using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridManager : MonoBehaviour
{
    public static BattleGridManager Instance;

    private BattleGrid selectedGrid;
    private Dictionary<Vector2Int, BattleGrid> gridMap = new Dictionary<Vector2Int, BattleGrid>();
    private Transform gridParent;
    private void Awake()
    {
        gridParent = transform.Find("GridLayout");
        for (int i = 0; i < gridParent.childCount; i++)
        {
            var x = i / 6 + 1;
            var y = i % 6 + 1;
            RegisterGrid(new Vector2Int(x, y),gridParent.GetChild(i).GetComponent<BattleGrid>());
        }
        Instance = this;
    }

    // 注册格子
    public void RegisterGrid(Vector2Int position, BattleGrid grid)
    {
        gridMap[position] = grid;
        gridMap[position].UpdateGridPos(position);
    }

    // 获取格子
    public BattleGrid GetGrid(Vector2Int position)
    {
        if (gridMap.ContainsKey(position))
        {
            return gridMap[position];
        }
        else
        {
            return null; // 如果没有找到，返回 null
        }
    }

    // 选择格子逻辑
    public void OnGridSelected(BattleGrid grid)
    {
        if (selectedGrid == null)
        {
            // 选择起始格子
            selectedGrid = grid;
            selectedGrid.ShowSelect(true);
        }
        else
        {
            // 设置目标格子
            if (grid != selectedGrid)
            {
                GridCharacter character = selectedGrid.OccupiedCharacters.Count > 0 ? selectedGrid.OccupiedCharacters[0] : null; // 默认选择第一个角色
                if (character != null)
                {
                    // 尝试将角色移动到目标格子
                    if (grid.TryOccupy(character))
                    {
                        selectedGrid.Release(character); // 移除角色
                        selectedGrid.ShowSelect(false);
                        selectedGrid = null; // 清除选择
                    }
                }
            }
        }
    }
}