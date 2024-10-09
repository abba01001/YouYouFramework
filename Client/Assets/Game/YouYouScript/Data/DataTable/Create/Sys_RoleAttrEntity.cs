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
        public int RoleId;

        /// <summary>
        /// 等级
        /// </summary>
        public int Level;

        /// <summary>
        /// 经验
        /// </summary>
        public int Exp;

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
