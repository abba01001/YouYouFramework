using System.Collections;
using System.Collections.Generic;
using System;
using FrameWork;

/// <summary>
/// Sys_UnlockFunc数据管理
/// </summary>
public partial class Sys_UnlockFuncDBModel : DataTableDBModelBase<Sys_UnlockFuncDBModel, Sys_UnlockFuncEntity>
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public override string DataTableName { get { return "Sys_UnlockFunc"; } }

    /// <summary>
    /// 加载列表
    /// </summary>
    protected override void LoadList(MMO_MemoryStream ms)
    {
        int rows = ms.ReadInt();
        int columns = ms.ReadInt();

        for (int i = 0; i < rows; i++)
        {
            Sys_UnlockFuncEntity entity = new Sys_UnlockFuncEntity();
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
