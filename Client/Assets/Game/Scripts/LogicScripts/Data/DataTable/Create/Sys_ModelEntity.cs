using GameScripts;

namespace GameScripts
{
    // Sys_Model Entity
    public partial class Sys_ModelEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // 资源路径
        public string AssetPath;
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
}