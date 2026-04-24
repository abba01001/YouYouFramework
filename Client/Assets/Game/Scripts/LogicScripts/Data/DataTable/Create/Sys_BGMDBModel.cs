using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_BGMDBModel : DataTableDBModelBase<Sys_BGMDBModel, Sys_BGMEntity>
    {
        public override string DataTableName => "Sys_BGM";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_BGMEntity();
                entity.Id = ms.ReadInt();
                entity.AssetFullPath = ms.ReadUTF8String();
                entity.Volume = ms.ReadFloat();
                entity.IsLoop = (byte)ms.ReadByte();
                entity.IsFadeIn = (byte)ms.ReadByte();
                entity.IsFadeOut = (byte)ms.ReadByte();
                entity.Priority = (byte)ms.ReadByte();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}