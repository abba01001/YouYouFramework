using GameScripts;

namespace GameScripts
{
    // Sys_BGM Entity
    public partial class Sys_BGMEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // 路径
        public string AssetFullPath;
        // 音量（0-1）
        public float Volume;
        // 是否循环
        public byte IsLoop;
        // 是否淡入
        public byte IsFadeIn;
        // 是否淡出
        public byte IsFadeOut;
        // 优先级(默认128)
        public byte Priority;
    }
}