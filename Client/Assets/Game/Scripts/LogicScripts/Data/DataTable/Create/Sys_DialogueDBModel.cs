using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_DialogueDBModel : DataTableDBModelBase<Sys_DialogueDBModel, Sys_DialogueEntity>
    {
        public override string DataTableName => "Sys_Dialogue";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_DialogueEntity();
                entity.Id = ms.ReadInt();
                entity.DialogueId = ms.ReadInt();
                entity.Content = ms.ReadUTF8String();
                entity.EnableBlock = ms.ReadUTF8String();
                entity.DisableBlock = ms.ReadUTF8String();
                entity.ClickMode = ms.ReadInt();
                entity.DialogueType = ms.ReadInt();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}