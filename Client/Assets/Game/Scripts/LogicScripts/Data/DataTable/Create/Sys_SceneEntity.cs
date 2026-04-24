using GameScripts;

namespace GameScripts
{
    // Sys_Scene Entity
    public partial class Sys_SceneEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // 场景组
        public string SceneGroup;
        // 场景路径
        public string AssetFullPath;
        // 背景音乐
        public string BGMId;
    }
}