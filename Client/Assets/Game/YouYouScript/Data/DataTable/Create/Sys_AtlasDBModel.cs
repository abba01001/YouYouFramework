
using System.Collections;
using System.Collections.Generic;
using System;

namespace YouYou
{
    /// <summary>
    /// Sys_Atlas数据管理
    /// </summary>
    public partial class Sys_AtlasDBModel : DataTableDBModelBase<Sys_AtlasDBModel, Sys_AtlasEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Atlas"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_AtlasEntity entity = new Sys_AtlasEntity();
                entity.Id = ms.ReadInt();
                entity.AssetFullPath = ms.ReadUTF8String();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}