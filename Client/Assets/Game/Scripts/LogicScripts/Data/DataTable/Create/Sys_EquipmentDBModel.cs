using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_EquipmentDBModel : DataTableDBModelBase<Sys_EquipmentDBModel, Sys_EquipmentEntity>
    {
        public override string DataTableName => "Sys_Equipment";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_EquipmentEntity();
                entity.Id = ms.ReadInt();
                entity.EquipType = ms.ReadUTF8String();
                entity.TexName = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}