using GameScripts;

namespace GameScripts
{
    // Sys_UIForm Entity
    public partial class Sys_UIFormEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // UI分组编号
        public byte UIGroupId;
        // 路径
        public string AssetPath_Chinese;
        // 路径
        public string AssetPath_English;
        // 禁用层级管理
        public int DisableUILayer;
        // 是否对象池锁定
        public int IsLock;
        // 允许多实例
        public int CanMulit;
        // 显示类型0=普通1=反切
        public byte ShowMode;
    }
}