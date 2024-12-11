
using System.Collections;
using System.Collections.Generic;
using System;

namespace YouYou
{
    /// <summary>
    /// Sys_RoleAttr数据管理
    /// </summary>
    public partial class Sys_RoleAttrDBModel : DataTableDBModelBase<Sys_RoleAttrDBModel, Sys_RoleAttrEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_RoleAttr"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_RoleAttrEntity entity = new Sys_RoleAttrEntity();
                entity.Id = ms.ReadInt();
                entity.ModelId = ms.ReadInt();
                entity.AttackRange = ms.ReadFloat();
                entity.AttackInterval = ms.ReadFloat();
                entity.Hp = ms.ReadInt();
                entity.Mp = ms.ReadInt();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}