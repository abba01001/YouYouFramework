using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_Guide实体
    /// </summary>
    public partial class Sys_GuideEntity : DataTableEntityBase
    {
        /// <summary>
        /// 引导ID
        /// </summary>
        public int GuideId;

        /// <summary>
        /// 引导类型(1对话)(2点击遮罩)
        /// </summary>
        public int GuideType;

        /// <summary>
        /// 达到X级触发(-1不是)
        /// </summary>
        public int ToLevelTrigger;

        /// <summary>
        /// 完成对话后派发事件
        /// </summary>
        public string CompleteEvent;

        /// <summary>
        /// 引导类具体方法
        /// </summary>
        public string DetailMethod;

        /// <summary>
        /// 触发对话ID(-1不是)
        /// </summary>
        public int DialogueId;

        /// <summary>
        /// 策划备注全部任务流程
        /// </summary>
        public string Progress;

    }
}
