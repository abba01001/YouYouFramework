using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_UnlockFunc实体
    /// </summary>
    public partial class Sys_UnlockFuncEntity : DataTableEntityBase
    {
        /// <summary>
        /// 解锁功能
        /// </summary>
        public string FuncName;

        /// <summary>
        /// 解锁功能等级
        /// </summary>
        public int UnlockLevel;

        /// <summary>
        /// 显示功能等级
        /// </summary>
        public int ShowLevel;

    }
}
