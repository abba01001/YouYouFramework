using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_Dialogue实体
    /// </summary>
    public partial class Sys_DialogueEntity : DataTableEntityBase
    {
        /// <summary>
        /// 对话Id
        /// </summary>
        public int DialogueId;

        /// <summary>
        /// 类型
        /// </summary>
        public string Type;

        /// <summary>
        /// 内容
        /// </summary>
        public string Content;

        /// <summary>
        /// 启动组件
        /// </summary>
        public string EnableBlock;

        /// <summary>
        /// 结束组件
        /// </summary>
        public string DisableBlock;

        /// <summary>
        /// 触发条件
        /// </summary>
        public string TriggerCondition;

        /// <summary>
        /// 对话类型
        /// </summary>
        public int DialogueType;

    }
}
