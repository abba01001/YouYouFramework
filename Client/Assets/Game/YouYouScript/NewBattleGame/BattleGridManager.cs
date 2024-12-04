using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using YouYou;

public class BattleGridPositionComparer : IComparer<BattleGrid>
{
    public int Compare(BattleGrid grid1, BattleGrid grid2)
    {
        if (grid1.Position.y == grid2.Position.y)
        {
            return grid1.Position.x.CompareTo(grid2.Position.x); // 比较 x
        }
        else
        {
            return grid1.Position.y.CompareTo(grid2.Position.y); // 比较 y
        }
    }
}

public class BattleGridManager : MonoBehaviour
{
    public static BattleGridManager Instance;

    private BattleGrid RecordSelectGrid;
    private BattleGrid RecordTargetGrid;
    private List<BattleGrid> girdList = new List<BattleGrid>();
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
        girdList.Sort(new BattleGridPositionComparer());
        Instance = this;
    }

    // 注册格子
    public void RegisterGrid(Vector2Int position, BattleGrid grid)
    {
        girdList.Add(grid);
        grid.UpdateGridPos(position);

    }

    //获取最近的格子
    public BattleGrid GetNearbyGrid()
    {
        foreach (var value in girdList)
        {
            if (value.CanDropCharacter())
            {
                return value;
            }
        }
        return null;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CallHero();
        }
    }

    public async void CallHero()
    {
        BattleGrid grid = GetNearbyGrid();
        if (grid != null)
        {
            float k = 0.2f;
            Vector3 A = new Vector3(0, -200, 0);
            Vector3 B = grid.transform.position;
            B = new Vector3(B.x, B.y, A.z);
            PoolObj tx = await GameEntry.Pool.GameObjectPool.SpawnAsync("Assets/Game/Download/Prefab/Effect/tx_MergerGame_01.prefab");
            tx.transform.position = A;
            Vector3[] path = BezierUtils.GetBeizerList(A, B + new Vector3(100, 100, 0), B, 20);
            PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync(Constants.ModelPath.Hero101);
            obj.gameObject.SetActive(false);
            grid.FillCharacter(obj.GetComponent<GridCharacterBase>());
            tx.transform.DOPath(path, 1f, PathType.CatmullRom).SetEase(Ease.OutCirc).OnComplete(async () =>
            {
                obj.gameObject.SetActive(true);
                obj.GetComponent<GridCharacterBase>().PlayBornAnim();
            });
        }
    }


    private void SwapGrid(BattleGrid orignGrid, BattleGrid targetGrid)
    {
        orignGrid.MoveToTargetGrid(targetGrid);
        targetGrid.MoveToTargetGrid(orignGrid);
        
        List<GridCharacterBase> tempList = targetGrid.OccupiedCharacters;
        targetGrid.OccupiedCharacters = orignGrid.OccupiedCharacters;
        orignGrid.OccupiedCharacters = tempList;

    }

    public void OnSelectOrignGrid(BattleGrid grid)
    {
        RecordSelectGrid = grid;
        foreach (var value in girdList)
        {
            value.ShowGrid(true);
            value.ShowOrignSelect(grid == value);
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
        foreach (var value in girdList)
        {
            value.ShowTargetSelect(grid == value);
        }
    }

    public void SetTargetGrid(BattleGrid grid)
    {
        RecordTargetGrid = grid;
    }
    
    public void OnReleaseGrid()
    {
        foreach (var value in girdList)
        {
            value.ShowOrignSelect(false);
            value.ShowTargetSelect(false);
            value.ShowGrid(false);
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