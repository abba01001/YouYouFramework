
using System.Collections;

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
    /// 点击方式Disabled0,ClickAnywhere1,ClickOnDialog2,ClickOnButton3
    /// </summary>
    public int ClickMode;

    /// <summary>
    /// 对话类型
    /// </summary>
    public int DialogueType;

}
