// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_LocalizationDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Localization Entity
    public partial class Sys_LocalizationEntity : ConfigEntityBase
    {
        // 编号(该列必须存在)
        public int Id;
        // Key(该列必须存在)
        public string Key;
        // 中文
        public string Chinese;
        // 英文
        public string English;
    }

    public partial class Sys_LocalizationDBModel : ConfigBase<Sys_LocalizationDBModel, Sys_LocalizationEntity>
    {
        public override string ConfigName => "Sys_Localization";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_LocalizationEntity();
                entity.Id = ms.ReadInt();
                entity.Key = ms.ReadUTF8String();
                entity.Chinese = ms.ReadUTF8String();
                entity.English = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}