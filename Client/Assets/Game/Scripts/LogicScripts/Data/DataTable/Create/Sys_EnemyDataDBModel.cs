// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_EnemyDataDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_EnemyData Entity
    public partial class Sys_EnemyDataEntity : ConfigEntityBase
    {
        // 编号
        public int Id;
        // 敌人类型
        public string EnemyType;
        // 掉落配置
        public string DropInfo;
    }

    public partial class Sys_EnemyDataDBModel : ConfigBase<Sys_EnemyDataDBModel, Sys_EnemyDataEntity>
    {
        public override string ConfigName => "Sys_EnemyData";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_EnemyDataEntity();
                entity.Id = ms.ReadInt();
                entity.EnemyType = ms.ReadUTF8String();
                entity.DropInfo = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}