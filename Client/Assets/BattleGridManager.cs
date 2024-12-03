using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridManager : MonoBehaviour
{
    public static BattleGridManager Instance;

    private BattleGrid RecordSelectGrid;
    private BattleGrid RecordTargetGrid;
    private Dictionary<Vector2Int, BattleGrid> gridMap = new Dictionary<Vector2Int, BattleGrid>();
    private Transform gridParent;
    public RectTransform barImage;
    public bool Selecting => RecordSelectGrid != null;

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

        
    // 示例数据交换方法
    private void SwapGrid(BattleGrid orignGrid, BattleGrid targetGrid)
    {
        orignGrid.MoveToTargetGrid(targetGrid);
        targetGrid.MoveToTargetGrid(orignGrid);
        
        List<GridCharacter> tempList = targetGrid.OccupiedCharacters;
        targetGrid.OccupiedCharacters = orignGrid.OccupiedCharacters;
        orignGrid.OccupiedCharacters = tempList;

    }

    public void OnSelectOrignGrid(BattleGrid grid)
    {
        RecordSelectGrid = grid;
        foreach (var pair in gridMap)
        {
            pair.Value.ShowGrid(true);
            pair.Value.ShowOrignSelect(grid == pair.Value);
        }
        barImage.position = grid.transform.position;
    }

    public void OnSelectTargetGrid(BattleGrid grid)
    {
        barImage.transform.localScale = Vector3.one;
        // 计算原始格子和目标格子之间的向量
        Vector2 originPos = RecordSelectGrid.Position;
        Vector2 targetPos = grid.Position;
        barImage.sizeDelta = new Vector2((targetPos - originPos).magnitude * 81f, barImage.sizeDelta.y);

        
        Vector3 direction = grid.transform.position - RecordSelectGrid.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 获取角度
        
        barImage.transform.rotation = Quaternion.Euler(0, 0, angle); // 旋转线条
        foreach (var pair in gridMap)
        {
            pair.Value.ShowTargetSelect(grid == pair.Value);
        }
    }

    public void SetTargetGrid(BattleGrid grid)
    {
        RecordTargetGrid = grid;
    }
    
    public void OnReleaseGrid()
    {
        foreach (var pair in gridMap)
        {
            pair.Value.ShowOrignSelect(false);
            pair.Value.ShowTargetSelect(false);
            pair.Value.ShowGrid(false);
        }

        if (RecordTargetGrid != null && RecordSelectGrid != RecordTargetGrid)
        {
            SwapGrid(RecordSelectGrid, RecordTargetGrid);
        }
        barImage.transform.localScale = Vector3.zero;
        RecordSelectGrid = null;
        RecordTargetGrid = null;
    }
}