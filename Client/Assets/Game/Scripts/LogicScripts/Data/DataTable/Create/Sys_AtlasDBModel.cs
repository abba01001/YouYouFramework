using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_AtlasDBModel : DataTableDBModelBase<Sys_AtlasDBModel, Sys_AtlasEntity>
    {
        public override string DataTableName => "Sys_Atlas";

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