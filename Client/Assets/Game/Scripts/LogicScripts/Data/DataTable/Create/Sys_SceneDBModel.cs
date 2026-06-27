// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_SceneDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Scene Entity
    public partial class Sys_SceneEntity : ConfigEntityBase
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

    public partial class Sys_SceneDBModel : ConfigBase<Sys_SceneDBModel, Sys_SceneEntity>
    {
        public override string ConfigName => "Sys_Scene";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_SceneEntity();
                entity.Id = ms.ReadInt();
                entity.SceneGroup = ms.ReadUTF8String();
                entity.AssetFullPath = ms.ReadUTF8String();
                entity.BGMId = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}