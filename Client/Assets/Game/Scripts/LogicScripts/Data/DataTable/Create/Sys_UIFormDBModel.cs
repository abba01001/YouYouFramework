using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_UIFormDBModel : DataTableDBModelBase<Sys_UIFormDBModel, Sys_UIFormEntity>
    {
        public override string DataTableName => "Sys_UIForm";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_UIFormEntity();
                entity.Id = ms.ReadInt();
                entity.UIGroupId = (byte)ms.ReadByte();
                entity.AssetPath_Chinese = ms.ReadUTF8String();
                entity.AssetPath_English = ms.ReadUTF8String();
                entity.DisableUILayer = ms.ReadInt();
                entity.IsLock = ms.ReadInt();
                entity.CanMulit = ms.ReadInt();
                entity.ShowMode = (byte)ms.ReadByte();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}