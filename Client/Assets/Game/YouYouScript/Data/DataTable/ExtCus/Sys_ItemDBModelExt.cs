using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public partial class Sys_ItemDBModel
    {
        public Dictionary<int, Sys_ItemEntity> IdByDic { get;private set; }

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            IdByDic = new Dictionary<int, Sys_ItemEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_ItemEntity entity = m_List[i];
                if (!IdByDic.ContainsKey(entity.ItemId))
                {
                    IdByDic.Add(entity.ItemId, entity);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Framework, "RoleAttr????????! DialogueId==" + entity.Id);
                }
            }
        }

        public Sys_ItemEntity GetEntity(int modelId)
        {
            if (IdByDic.ContainsKey(modelId))
            {
                return IdByDic[modelId];
            }
            GameEntry.LogError(LogCategory.Framework, "?????????, DialogueId==" + modelId);
            return null;
        }
    }
