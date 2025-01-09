using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_Equip实体
    /// </summary>
    public partial class Sys_EquipEntity : DataTableEntityBase
    {
        /// <summary>
        /// 装备id
        /// </summary>
        public int EquipId;

        /// <summary>
        /// 类型
        /// </summary>
        public int Type;

        /// <summary>
        /// 攻击范围
        /// </summary>
        public float AttackRange;

        /// <summary>
        /// 格子容纳数
        /// </summary>
        public int GridContain;

        /// <summary>
        /// 攻击间隔
        /// </summary>
        public float AttackInterval;

        /// <summary>
        /// 红
        /// </summary>
        public int Hp;

        /// <summary>
        /// 蓝
        /// </summary>
        public int Mp;

        /// <summary>
        /// 在HeroPanel里显示
        /// </summary>
        public int InHeroPanel;

        /// <summary>
        /// Panel显示的Icon
        /// </summary>
        public string HeroPanelIcon;

    }
}
