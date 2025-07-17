using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public partial class Sys_BuildingsDBModel
    {
        public Dictionary<int, Sys_BuildingsEntity> IdByDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            IdByDic = new Dictionary<int, Sys_BuildingsEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_BuildingsEntity entity = m_List[i];
                if (!IdByDic.ContainsKey(entity.BuildingId))
                {
                    IdByDic.Add(entity.BuildingId, entity);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Framework, "Dialogue���ñ����! DialogueId==" + entity.BuildingId);
                }
            }
        }

        public Sys_BuildingsEntity GetEntity(int task_id)
        {
            if (IdByDic.ContainsKey(task_id))
            {
                return IdByDic[task_id];
            }
            YouYou.GameEntry.LogError(LogCategory.Framework, "û���ҵ��Ի�, DialogueId==" + task_id);
            return null;
        }
    }
}