using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class HelperManager
{
    private List<Helper> helpers = new List<Helper>();
    private static HelperManager _instance;

    private HelperManager() { }

    public static HelperManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new HelperManager();
            }
            return _instance;
        }
    }

    // 初始化
    public async UniTask Init()
    {
        List<HelperData> list = GameEntry.Data.PlayerRoleData.restaurantData.helpers;
        if (list.Count > 0)
        {
            foreach (var helperData in list)
            {
                var newHelper = new Helper();
                helpers.Add(newHelper);
            }
        }

        GameEntry.Time.CreateTimerLoop(this, 1f, -1, (_) =>
        {
            CheckSpawnHelper();
        });
    }

    private void CheckSpawnHelper()
    {
        if (helpers.Count < GameEntry.Data.PlayerRoleData.restaurantData.helpers.Count)
        {
            var newHelper = new Helper(); 
            helpers.Add(newHelper);
        }
    }

    // 移除顾客
    public void RemoveHelper(Helper targetHelper)
    {
        var targetData = GameEntry.Data.PlayerRoleData.restaurantData.helpers
            .FirstOrDefault(customer => customer.helperId == targetHelper.HelperData.helperId);

        if (targetData != null)
        {
            // 确保数据一致性，删除顾客
            GameEntry.Data.PlayerRoleData.restaurantData.helpers.Remove(targetData);
            helpers.Remove(targetHelper);
            GameObject.Destroy(targetHelper.gameObject);
        }
    }

    // 更新所有顾客
    public void Update()
    {
        for (int i = helpers.Count - 1; i >= 0; i--)
        {
            var helper = helpers[i];
            // customer.Update();
            if (!helper.IsActive) 
            {
                helpers.RemoveAt(i);  // 从后往前删除，避免索引错乱
                RemoveHelper(helper); // 同时从游戏数据中移除
            }
        }
    }
}
