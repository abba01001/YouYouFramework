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
                if (!IdByDic.ContainsKey(entity.ModelId))
                {
                    IdByDic.Add(entity.ModelId, entity);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Framework, "RoleAttr配置表错误! DialogueId==" + entity.Id);
                }
            }
        }

        public Sys_RoleAttrEntity GetEntity(int modelId)
        {
            if (IdByDic.ContainsKey(modelId))
            {
                return IdByDic[modelId];
            }
            YouYou.GameEntry.LogError(LogCategory.Framework, "没有找到对话, DialogueId==" + modelId);
            return null;
        }
    }
}