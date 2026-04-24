using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_GuideDBModel : DataTableDBModelBase<Sys_GuideDBModel, Sys_GuideEntity>
    {
        public override string DataTableName => "Sys_Guide";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_GuideEntity();
                entity.Id = ms.ReadInt();
                entity.GuideId = ms.ReadInt();
                entity.GuideType = ms.ReadInt();
                entity.EventTrigger = ms.ReadUTF8String();
                entity.ToLevelTrigger = ms.ReadInt();
                entity.NextGuideId = ms.ReadInt();
                entity.ClickWidth = ms.ReadUTF8String();
                entity.TimeToClose = ms.ReadFloat();
                entity.ShowForm = ms.ReadUTF8String();
                entity.ClickArrow = ms.ReadUTF8String();
                entity.TriggerScene = ms.ReadInt();
                entity.DialogueId = ms.ReadInt();
                entity.Progress = ms.ReadUTF8String();
                entity.IsEnable = ms.ReadInt();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}