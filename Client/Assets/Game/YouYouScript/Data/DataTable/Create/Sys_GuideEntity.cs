using System.Collections;

namespace YouYou
{
    /// <summary>
      /// Sys_UIForm实体
    /// </summary>
    public partial class Sys_GuideEntity : DataTableEntityBase
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public int TaskId;

        /// <summary>
        /// 路径（按钮/开关在UI里的位置）
        /// </summary>
        public string Path;

        /// <summary>
        /// 是否监听按钮点击
        /// </summary>
        public int CheckBtn;

        /// <summary>
        /// 是否监听开关激活
        /// </summary>
        public int CheckToggle;

        /// <summary>
        /// 是否监听事件
        /// </summary>
        public int CheckEvent;

        /// <summary>
        /// 下一个任务ID
        /// </summary>
        public int NextId;
    }
}
