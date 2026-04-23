using System.Collections;
using System.Collections.Generic;
using FrameWork;
using UnityEngine;

    public partial class Sys_DialogueDBModel
    {
        public Dictionary<int, Sys_DialogueEntity> IdByDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            IdByDic = new Dictionary<int, Sys_DialogueEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_DialogueEntity entity = m_List[i];
                if (!IdByDic.ContainsKey(entity.DialogueId))
                {
                    IdByDic.Add(entity.DialogueId, entity);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Framework, "Dialogue���ñ����! DialogueId==" + entity.DialogueId);
                }
            }
        }

        public Sys_DialogueEntity GetEntity(int task_id)
        {
            if (IdByDic.ContainsKey(task_id))
            {
                return IdByDic[task_id];
            }
            GameEntry.LogError(LogCategory.Framework, "û���ҵ��Ի�, DialogueId==" + task_id);
            return null;
        }
    }
