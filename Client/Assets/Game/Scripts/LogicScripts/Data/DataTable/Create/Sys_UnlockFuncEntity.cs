using GameScripts;

namespace GameScripts
{
    // Sys_UnlockFunc Entity
    public partial class Sys_UnlockFuncEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // 解锁功能
        public string FuncName;
        // 解锁功能等级
        public int UnlockLevel;
        // 显示功能等级
        public int ShowLevel;
        // 功能具体名字
        public string FuncDetailName;
    }
}