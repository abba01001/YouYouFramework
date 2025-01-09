
using System.Collections;
using System.Collections.Generic;
using System;

namespace YouYou
{
    /// <summary>
    /// Sys_Model数据管理
    /// </summary>
    public partial class Sys_ModelDBModel : DataTableDBModelBase<Sys_ModelDBModel, Sys_ModelEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Model"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_ModelEntity entity = new Sys_ModelEntity();
                entity.Id = ms.ReadInt();
                entity.ModelId = ms.ReadInt();
                entity.Type = ms.ReadInt();
                entity.AttackRange = ms.ReadFloat();
                entity.GridContain = ms.ReadInt();
                entity.AttackInterval = ms.ReadFloat();
                entity.Hp = ms.ReadInt();
                entity.Mp = ms.ReadInt();
                entity.InHeroPanel = ms.ReadInt();
                entity.HeroPanelIcon = ms.ReadUTF8String();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}