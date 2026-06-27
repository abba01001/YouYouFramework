// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_ModelDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Model Entity
    public partial class Sys_ModelEntity : ConfigEntityBase
    {
        // 编号
        public int Id;
        // 类型
        public string ModelType;
        // 敌人类型
        public string EnemyType;
        // 速度
        public float Speed;
        // 伤害
        public float Damage;
        // 血量
        public float Hp;
        // 存活时间
        public float LifeTime;
    }

    public partial class Sys_ModelDBModel : ConfigBase<Sys_ModelDBModel, Sys_ModelEntity>
    {
        public override string ConfigName => "Sys_Model";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_ModelEntity();
                entity.Id = ms.ReadInt();
                entity.ModelType = ms.ReadUTF8String();
                entity.EnemyType = ms.ReadUTF8String();
                entity.Speed = ms.ReadFloat();
                entity.Damage = ms.ReadFloat();
                entity.Hp = ms.ReadFloat();
                entity.LifeTime = ms.ReadFloat();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}