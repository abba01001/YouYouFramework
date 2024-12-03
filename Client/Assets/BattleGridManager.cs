using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGridManager : MonoBehaviour
{
    public static BattleGridManager Instance;

    private BattleGrid selectedOrignGrid;
    private Dictionary<Vector2Int, BattleGrid> gridMap = new Dictionary<Vector2Int, BattleGrid>();
    private Transform gridParent;
    public RectTransform barImage;
    public bool Selecting => selectedOrignGrid != null;

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
        if (selectedOrignGrid == null)
        {
            // 选择起始格子
            selectedOrignGrid = grid;
            selectedOrignGrid.ShowOrignSelect(true);
        }
        else
        {
            // 设置目标格子
            if (grid != selectedOrignGrid)
            {
                GridCharacter character = selectedOrignGrid.OccupiedCharacters.Count > 0 ? selectedOrignGrid.OccupiedCharacters[0] : null; // 默认选择第一个角色
                if (character != null)
                {
                    // 尝试将角色移动到目标格子
                    if (grid.TryOccupy(character))
                    {
                        selectedOrignGrid.Release(character); // 移除角色
                        selectedOrignGrid.ShowOrignSelect(false);
                        selectedOrignGrid = null; // 清除选择
                    }
                }
            }
        }
    }

    public void OnSelectOrignGrid(BattleGrid grid)
    {
        selectedOrignGrid = grid;
        foreach (var pair in gridMap)
        {
            pair.Value.ShowGrid(true);
            pair.Value.ShowOrignSelect(grid == pair.Value);
        }

        barImage.position = grid.transform.position;
    }

    public void OnSelectTargetGrid(BattleGrid grid)
    {
        // 计算原始格子和目标格子之间的向量
        Vector2 originPos = selectedOrignGrid.Position;
        Vector2 targetPos = grid.Position;
        Vector3 direction = targetPos - originPos;

        // 计算两者之间的距离
        //float distance = direction.magnitude;
        float distance = Vector2.Distance(targetPos,originPos) * 81f; // 根据需要的比例缩放距离

        // 更新 barImage 的大小（宽度）
        barImage.sizeDelta = new Vector2(distance, barImage.sizeDelta.y);

        // 计算条形图的旋转角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 更新 barImage 的旋转
        barImage.transform.rotation = Quaternion.Euler(0, 0, angle);


        foreach (var pair in gridMap)
        {
            pair.Value.ShowTargetSelect(grid == pair.Value);
        }
    }

    public void OnReleaseGrid(BattleGrid grid)
    {
        selectedOrignGrid = null;
        foreach (var pair in gridMap)
        {
            pair.Value.ShowOrignSelect(false);
            pair.Value.ShowTargetSelect(false);
            pair.Value.ShowGrid(false);
        }
    }
}