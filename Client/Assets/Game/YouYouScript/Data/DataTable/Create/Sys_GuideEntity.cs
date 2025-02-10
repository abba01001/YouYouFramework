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
        /// 通过事件进行触发
        /// </summary>
        public string EventTrigger;

        /// <summary>
        /// 达到X级触发(-1不是)
        /// </summary>
        public int ToLevelTrigger;

        /// <summary>
        /// 下一个引导id
        /// </summary>
        public int NextGuideId;

        /// <summary>
        /// 点击遮罩组件
        /// </summary>
        public string ClickWidth;

        /// <summary>
        /// X时间后关闭遮罩
        /// </summary>
        public float TimeToClose;

        /// <summary>
        /// 点击是否需要箭头
        /// </summary>
        public string ClickArrow;

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
