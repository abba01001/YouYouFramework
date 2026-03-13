using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public partial class Sys_BGMDBModel
    {
        public Dictionary<string, Sys_BGMEntity> NameByDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            NameByDic = new Dictionary<string, Sys_BGMEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_BGMEntity entity = m_List[i];
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

        public Sys_BGMEntity GetEntity(string name)
        {
            if (NameByDic.ContainsKey(name))
            {
                return NameByDic[name];
            }
            GameEntry.LogError(LogCategory.Framework, "没有找到资源, Name==" + name);
            return null;
        }
    }
