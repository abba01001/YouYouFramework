// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_UnlockFuncDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_UnlockFunc Entity
    public partial class Sys_UnlockFuncEntity : ConfigEntityBase
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

    public partial class Sys_UnlockFuncDBModel : ConfigBase<Sys_UnlockFuncDBModel, Sys_UnlockFuncEntity>
    {
        public override string ConfigName => "Sys_UnlockFunc";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_UnlockFuncEntity();
                entity.Id = ms.ReadInt();
                entity.FuncName = ms.ReadUTF8String();
                entity.UnlockLevel = ms.ReadInt();
                entity.ShowLevel = ms.ReadInt();
                entity.FuncDetailName = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}