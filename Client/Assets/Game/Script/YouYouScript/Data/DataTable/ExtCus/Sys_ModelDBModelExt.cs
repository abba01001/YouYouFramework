using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public partial class Sys_ModelDBModel
    {
        public Dictionary<int, Sys_ModelEntity> IdByDic { get;private set; }

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            IdByDic = new Dictionary<int, Sys_ModelEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_ModelEntity entity = m_List[i];
                if (!IdByDic.ContainsKey(entity.ModelId))
                {
                    IdByDic.Add(entity.ModelId, entity);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Framework, "RoleAttr���ñ����! DialogueId==" + entity.Id);
                }
            }
        }

        public Sys_ModelEntity GetEntity(int modelId)
        {
            if (IdByDic.ContainsKey(modelId))
            {
                return IdByDic[modelId];
            }
            GameEntry.LogError(LogCategory.Framework, "û���ҵ��Ի�, DialogueId==" + modelId);
            return null;
        }
    }
