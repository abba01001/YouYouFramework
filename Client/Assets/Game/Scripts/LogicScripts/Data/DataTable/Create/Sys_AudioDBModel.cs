// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_AudioDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Audio Entity
    public partial class Sys_AudioEntity : ConfigEntityBase
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

    public partial class Sys_AudioDBModel : ConfigBase<Sys_AudioDBModel, Sys_AudioEntity>
    {
        public override string ConfigName => "Sys_Audio";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_AudioEntity();
                entity.Id = ms.ReadInt();
                entity.AssetFullPath = ms.ReadUTF8String();
                entity.Volume = ms.ReadFloat();
                entity.Priority = (byte)ms.ReadByte();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}