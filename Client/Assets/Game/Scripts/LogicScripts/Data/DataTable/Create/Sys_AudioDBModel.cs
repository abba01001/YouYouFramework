using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_AudioDBModel : DataTableDBModelBase<Sys_AudioDBModel, Sys_AudioEntity>
    {
        public override string DataTableName => "Sys_Audio";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_AudioEntity();
                entity.Id = ms.ReadInt();
                entity.AssetFullPath = ms.ReadUTF8String();
                entity.Volume = ms.ReadFloat();
                entity.Priority = (byte)ms.ReadByte();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}