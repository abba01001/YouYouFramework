using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_SceneDBModel : DataTableDBModelBase<Sys_SceneDBModel, Sys_SceneEntity>
    {
        public override string DataTableName => "Sys_Scene";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_SceneEntity();
                entity.Id = ms.ReadInt();
                entity.SceneGroup = ms.ReadUTF8String();
                entity.AssetFullPath = ms.ReadUTF8String();
                entity.BGMId = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}