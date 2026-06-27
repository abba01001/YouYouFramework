// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_EquipmentDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Equipment Entity
    public partial class Sys_EquipmentEntity : ConfigEntityBase
    {
        // 编号
        public int Id;
        // 类型
        public string EquipType;
        // 贴图名
        public string TexName;
    }

    public partial class Sys_EquipmentDBModel : ConfigBase<Sys_EquipmentDBModel, Sys_EquipmentEntity>
    {
        public override string ConfigName => "Sys_Equipment";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_EquipmentEntity();
                entity.Id = ms.ReadInt();
                entity.EquipType = ms.ReadUTF8String();
                entity.TexName = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}