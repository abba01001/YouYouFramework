// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_UIFormDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_UIForm Entity
    public partial class Sys_UIFormEntity : ConfigEntityBase
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

    public partial class Sys_UIFormDBModel : ConfigBase<Sys_UIFormDBModel, Sys_UIFormEntity>
    {
        public override string ConfigName => "Sys_UIForm";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_UIFormEntity();
                entity.Id = ms.ReadInt();
                entity.UIGroupId = (byte)ms.ReadByte();
                entity.AssetPath_Chinese = ms.ReadUTF8String();
                entity.AssetPath_English = ms.ReadUTF8String();
                entity.DisableUILayer = ms.ReadInt();
                entity.IsLock = ms.ReadInt();
                entity.CanMulit = ms.ReadInt();
                entity.ShowMode = (byte)ms.ReadByte();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}