using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public partial class Sys_UnlockFuncDBModel
    {
        public Dictionary<int, Sys_UnlockFuncEntity> IdByDic { get;private set; }

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            IdByDic = new Dictionary<int, Sys_UnlockFuncEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_UnlockFuncEntity entity = m_List[i];
                if (!IdByDic.ContainsKey(entity.Id))
                {
                    IdByDic.Add(entity.Id, entity);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Framework, "RoleAttr????????! DialogueId==" + entity.Id);
                }
            }
        }
    }
