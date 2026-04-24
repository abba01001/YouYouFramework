using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_UnlockFuncDBModel : DataTableDBModelBase<Sys_UnlockFuncDBModel, Sys_UnlockFuncEntity>
    {
        public override string DataTableName => "Sys_UnlockFunc";

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