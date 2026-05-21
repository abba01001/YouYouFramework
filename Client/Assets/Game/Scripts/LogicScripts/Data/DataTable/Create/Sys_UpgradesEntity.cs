using GameScripts;

namespace GameScripts
{
    // Sys_Upgrades Entity
    public partial class Sys_UpgradesEntity : DataTableEntityBase
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
}