using GameScripts;

namespace GameScripts
{
    // Sys_Dialogue Entity
    public partial class Sys_DialogueEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // 对话Id
        public int DialogueId;
        // 内容
        public string Content;
        // 启动组件
        public string EnableBlock;
        // 结束组件
        public string DisableBlock;
        // 点击方式Disabled0,ClickAnywhere1,ClickOnDialog2,ClickOnButton3
        public int ClickMode;
        // 对话类型
        public int DialogueType;
    }
}