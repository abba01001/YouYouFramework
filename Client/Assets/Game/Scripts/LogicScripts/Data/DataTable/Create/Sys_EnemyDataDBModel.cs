using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_EnemyDataDBModel : DataTableDBModelBase<Sys_EnemyDataDBModel, Sys_EnemyDataEntity>
    {
        public override string DataTableName => "Sys_EnemyData";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_EnemyDataEntity();
                entity.Id = ms.ReadInt();
                entity.EnemyType = ms.ReadUTF8String();
                entity.DropInfo = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}