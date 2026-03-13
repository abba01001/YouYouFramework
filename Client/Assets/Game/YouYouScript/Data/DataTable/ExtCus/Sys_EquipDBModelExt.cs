using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public partial class Sys_EquipDBModel
    {
        public Dictionary<int, Sys_EquipEntity> IdByDic { get;private set; }

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            IdByDic = new Dictionary<int, Sys_EquipEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_EquipEntity entity = m_List[i];
                if (!IdByDic.ContainsKey(entity.EquipId))
                {
                    IdByDic.Add(entity.EquipId, entity);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Framework, "RoleAttr????????! DialogueId==" + entity.Id);
                }
            }
        }

        public Sys_EquipEntity GetEntity(int modelId)
        {
            if (IdByDic.ContainsKey(modelId))
            {
                return IdByDic[modelId];
            }
            GameEntry.LogError(LogCategory.Framework, "?????????, DialogueId==" + modelId);
            return null;
        }
    }
