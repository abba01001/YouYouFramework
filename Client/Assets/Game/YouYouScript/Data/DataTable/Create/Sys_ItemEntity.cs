using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_Item实体
    /// </summary>
    public partial class Sys_ItemEntity : DataTableEntityBase
    {
        /// <summary>
        /// 物品id
        /// </summary>
        public int ItemId;

        /// <summary>
        /// 类型
        /// </summary>
        public int Type;

        /// <summary>
        /// 品质
        /// </summary>
        public int Quality;

        /// <summary>
        /// 阶数
        /// </summary>
        public int Stage;

        /// <summary>
        /// 名字
        /// </summary>
        public string Name;

        /// <summary>
        /// 描述文本
        /// </summary>
        public string Desc;

    }
}
