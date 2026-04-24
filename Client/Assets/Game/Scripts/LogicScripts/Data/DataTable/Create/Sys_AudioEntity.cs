using GameScripts;

namespace GameScripts
{
    // Sys_Audio Entity
    public partial class Sys_AudioEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // 路径
        public string AssetFullPath;
        // 音量（0-1）
        public float Volume;
        // 优先级(默认128)
        public byte Priority;
    }
}