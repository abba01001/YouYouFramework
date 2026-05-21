using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_UpgradesDBModel : DataTableDBModelBase<Sys_UpgradesDBModel, Sys_UpgradesEntity>
    {
        public override string DataTableName => "Sys_Upgrades";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_UpgradesEntity();
                entity.Id = ms.ReadInt();
                entity.UpgradeType = ms.ReadUTF8String();
                entity.Level = ms.ReadInt();
                entity.AddValue = ms.ReadFloat();
                entity.MultiplyValue = ms.ReadFloat();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}