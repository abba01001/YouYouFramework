using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_Model实体
    /// </summary>
    public partial class Sys_ModelEntity : DataTableEntityBase
    {
        /// <summary>
        /// 模型id
        /// </summary>
        public int ModelId;

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

    }
}
