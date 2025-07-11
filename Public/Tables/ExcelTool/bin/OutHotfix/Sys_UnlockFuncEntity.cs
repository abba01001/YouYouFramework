using System.Collections;

namespace Hotfix
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

        /// <summary>
        /// 功能具体名字
        /// </summary>
        public string FuncDetailName;

    }
}
