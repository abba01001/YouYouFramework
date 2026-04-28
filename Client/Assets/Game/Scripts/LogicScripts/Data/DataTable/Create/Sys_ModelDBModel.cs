using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_ModelDBModel : DataTableDBModelBase<Sys_ModelDBModel, Sys_ModelEntity>
    {
        public override string DataTableName => "Sys_Model";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_ModelEntity();
                entity.Id = ms.ReadInt();
                entity.AssetPath = ms.ReadUTF8String();
                entity.ModelType = ms.ReadUTF8String();
                entity.EnemyType = ms.ReadUTF8String();
                entity.Speed = ms.ReadFloat();
                entity.Damage = ms.ReadFloat();
                entity.Hp = ms.ReadFloat();
                entity.LifeTime = ms.ReadFloat();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}