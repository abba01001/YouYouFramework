// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_DialogueDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Dialogue Entity
    public partial class Sys_DialogueEntity : ConfigEntityBase
    {
        // 编号
        public int Id;
        // 对话Id
        public int DialogueId;
        // 内容
        public string Content;
        // 启动组件
        public string EnableBlock;
        // 结束组件
        public string DisableBlock;
        // 点击方式Disabled0,ClickAnywhere1,ClickOnDialog2,ClickOnButton3
        public int ClickMode;
        // 对话类型
        public int DialogueType;
    }

    public partial class Sys_DialogueDBModel : ConfigBase<Sys_DialogueDBModel, Sys_DialogueEntity>
    {
        public override string ConfigName => "Sys_Dialogue";

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