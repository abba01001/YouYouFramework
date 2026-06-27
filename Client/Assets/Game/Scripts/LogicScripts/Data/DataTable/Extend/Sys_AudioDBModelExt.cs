using System.Collections.Generic;
using Main;

namespace GameScripts
{
    public partial class Sys_AudioDBModel
    {
        public Dictionary<string, Sys_AudioEntity> NameByDic;
        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            NameByDic = new Dictionary<string, Sys_AudioEntity>();
            for (var i = 0; i < m_List.Count; i++)
            {
                var entity = m_List[i];
                var strs = entity.AssetFullPath.Split('.')[0].Split('/');
                if (strs.Length >= 1)
                {
                    var str = strs[strs.Length - 1];
                    if (NameByDic.ContainsKey(str))
                        Debugger.LogError(LogCategory.Framework, "名称有重复! ==" + str);
                    else
                        NameByDic.Add(str, entity);
                }
            }
        }

        public Sys_AudioEntity GetEntity(string name)
        {
            if (NameByDic.ContainsKey(name)) return NameByDic[name];
            Debugger.LogError(LogCategory.Framework, "没有找到资源, Name==" + name);
            return null;
        }
    }
}