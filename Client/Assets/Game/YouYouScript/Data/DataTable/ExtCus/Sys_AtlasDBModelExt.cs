using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public partial class Sys_AtlasDBModel
    {
        public Dictionary<string, Sys_AtlasEntity> NameByDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            NameByDic = new Dictionary<string, Sys_AtlasEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_AtlasEntity entity = m_List[i];
                string[] strs = entity.AssetFullPath.Split('.')[0].Split('/');
                if (strs.Length >= 1)
                {
                    string str = strs[strs.Length - 1];
                    if (NameByDic.ContainsKey(str))
                    {
                        GameEntry.LogError(LogCategory.Framework, "名称有重复! ==" + str);
                    }
                    else
                    {
                        NameByDic.Add(str, entity);
                    }
                }
            }
        }

        public Sys_AtlasEntity GetEntity(string name)
        {
            if (NameByDic.ContainsKey(name))
            {
                return NameByDic[name];
            }
            YouYou.GameEntry.LogError(LogCategory.Framework, "没有找到资源, Name==" + name);
            return null;
        }
    }
}