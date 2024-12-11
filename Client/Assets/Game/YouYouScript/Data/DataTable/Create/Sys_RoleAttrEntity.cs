using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_RoleAttr实体
    /// </summary>
    public partial class Sys_RoleAttrEntity : DataTableEntityBase
    {
        /// <summary>
        /// 角色id
        /// </summary>
        public int ModelId;

        /// <summary>
        /// 攻击范围
        /// </summary>
        public float AttackRange;

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

    }
}
