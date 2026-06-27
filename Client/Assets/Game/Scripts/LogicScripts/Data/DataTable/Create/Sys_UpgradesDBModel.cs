// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_UpgradesDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Upgrades Entity
    public partial class Sys_UpgradesEntity : ConfigEntityBase
    {
        // 编号
        public int Id;
        // 属性类型
        public string UpgradeType;
        // 等级
        public int Level;
        // 
        public float AddValue;
        // 
        public float MultiplyValue;
    }

    public partial class Sys_UpgradesDBModel : ConfigBase<Sys_UpgradesDBModel, Sys_UpgradesEntity>
    {
        public override string ConfigName => "Sys_Upgrades";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_UpgradesEntity();
                entity.Id = ms.ReadInt();
                entity.UpgradeType = ms.ReadUTF8String();
                entity.Level = ms.ReadInt();
                entity.AddValue = ms.ReadFloat();
                entity.MultiplyValue = ms.ReadFloat();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}