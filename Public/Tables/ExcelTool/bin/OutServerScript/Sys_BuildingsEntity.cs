
using System.Collections;

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
    /// 建筑名字
    /// </summary>
    public string BuildingName;

    /// <summary>
    /// 名
    /// </summary>
    public string Name;

    /// <summary>
    /// 类型
    /// </summary>
    public string BuildingType;

    /// <summary>
    /// 生成
    /// </summary>
    public string Produce;

    /// <summary>
    /// 多少秒生成1个
    /// </summary>
    public int ProduceTime;

    /// <summary>
    /// 消耗
    /// </summary>
    public string Consume;

    /// <summary>
    /// 解锁金币
    /// </summary>
    public int Cost;

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

    /// <summary>
    /// 解锁金币
    /// </summary>
    public int Cost1;

}
