using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public partial class Sys_RoleAttrDBModel
    {
        public Dictionary<int, Sys_RoleAttrEntity> IdByDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            IdByDic = new Dictionary<int, Sys_RoleAttrEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_RoleAttrEntity entity = m_List[i];
                if (!IdByDic.ContainsKey(entity.Id))
                {
                    IdByDic.Add(entity.Id, entity);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Framework, "RoleAttr配置表错误! DialogueId==" + entity.Id);
                }
            }
        }

        public Sys_RoleAttrEntity GetEntity(int task_id)
        {
            if (IdByDic.ContainsKey(task_id))
            {
                return IdByDic[task_id];
            }
            YouYou.GameEntry.LogError(LogCategory.Framework, "没有找到对话, DialogueId==" + task_id);
            return null;
        }
    }
}