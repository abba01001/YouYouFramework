// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_BGMDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_BGM Entity
    public partial class Sys_BGMEntity : ConfigEntityBase
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

    public partial class Sys_BGMDBModel : ConfigBase<Sys_BGMDBModel, Sys_BGMEntity>
    {
        public override string ConfigName => "Sys_BGM";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_BGMEntity();
                entity.Id = ms.ReadInt();
                entity.AssetFullPath = ms.ReadUTF8String();
                entity.Volume = ms.ReadFloat();
                entity.IsLoop = (byte)ms.ReadByte();
                entity.IsFadeIn = (byte)ms.ReadByte();
                entity.IsFadeOut = (byte)ms.ReadByte();
                entity.Priority = (byte)ms.ReadByte();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}