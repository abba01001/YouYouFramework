
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hotfix
{
    /// <summary>
    /// Sys_Item数据管理
    /// </summary>
    public partial class Sys_ItemDBModel : DataTableDBModelBase<Sys_ItemDBModel, Sys_ItemEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Item"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_ItemEntity entity = new Sys_ItemEntity();
                entity.Id = ms.ReadInt();
                entity.ItemId = ms.ReadInt();
                entity.Type = ms.ReadInt();
                entity.Icon = ms.ReadUTF8String();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}