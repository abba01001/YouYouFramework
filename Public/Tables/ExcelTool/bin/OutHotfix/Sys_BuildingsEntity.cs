using System.Collections;

namespace Hotfix
{
    /// <summary>
      /// Sys_Buildings实体
    /// </summary>
    public partial class Sys_BuildingsEntity : DataTableEntityBase
    {
        /// <summary>
        /// 建筑id
        /// </summary>
        public int BuildingId;

        /// <summary>
        /// 建筑类型
        /// </summary>
        public string BuildingType;

        /// <summary>
        /// 建筑名
        /// </summary>
        public string BuildingName;

        /// <summary>
        /// 解锁金币
        /// </summary>
        public int Cost;

        /// <summary>
        /// 是否初始存在
        /// </summary>
        public int IsInit;

        /// <summary>
        /// 坐标
        /// </summary>
        public string Position;

        /// <summary>
        /// 角度
        /// </summary>
        public string Rotation;

        /// <summary>
        /// 区域Id
        /// </summary>
        public int RegionId;

        /// <summary>
        /// 显示建筑
        /// </summary>
        public int isVisible;

        /// <summary>
        /// 依赖建筑
        /// </summary>
        public string Dependencies;

    }
}
