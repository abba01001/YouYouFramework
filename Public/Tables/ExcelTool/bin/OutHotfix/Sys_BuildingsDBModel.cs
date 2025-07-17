
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hotfix
{
    /// <summary>
    /// Sys_Buildings数据管理
    /// </summary>
    public partial class Sys_BuildingsDBModel : DataTableDBModelBase<Sys_BuildingsDBModel, Sys_BuildingsEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Buildings"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_BuildingsEntity entity = new Sys_BuildingsEntity();
                entity.Id = ms.ReadInt();
                entity.BuildingId = ms.ReadInt();
                entity.BuildingType = ms.ReadUTF8String();
                entity.BuildingName = ms.ReadUTF8String();
                entity.Cost = ms.ReadInt();
                entity.IsInit = ms.ReadInt();
                entity.Position = ms.ReadUTF8String();
                entity.Rotation = ms.ReadUTF8String();
                entity.RegionId = ms.ReadInt();
                entity.isVisible = ms.ReadInt();
                entity.Dependencies = ms.ReadUTF8String();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}