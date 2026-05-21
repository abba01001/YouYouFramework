using GameScripts;

namespace GameScripts
{
    // Sys_EnemyData Entity
    public partial class Sys_EnemyDataEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // 敌人类型
        public string EnemyType;
        // 掉落配置
        public string DropInfo;
    }
}