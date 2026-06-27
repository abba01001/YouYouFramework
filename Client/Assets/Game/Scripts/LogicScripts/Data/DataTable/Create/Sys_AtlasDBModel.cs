// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_AtlasDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Atlas Entity
    public partial class Sys_AtlasEntity : ConfigEntityBase
    {
        // 编号
        public int Id;
        // 路径
        public string AssetFullPath;
    }

    public partial class Sys_AtlasDBModel : ConfigBase<Sys_AtlasDBModel, Sys_AtlasEntity>
    {
        public override string ConfigName => "Sys_Atlas";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_AtlasEntity();
                entity.Id = ms.ReadInt();
                entity.AssetFullPath = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}