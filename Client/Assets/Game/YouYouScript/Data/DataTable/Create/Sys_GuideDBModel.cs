
using System.Collections;
using System.Collections.Generic;
using System;

namespace YouYou
{
    /// <summary>
    /// Sys_Guide数据管理
    /// </summary>
    public partial class Sys_GuideDBModel : DataTableDBModelBase<Sys_GuideDBModel, Sys_GuideEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_Guide"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_GuideEntity entity = new Sys_GuideEntity();
                entity.Id = ms.ReadInt();
                entity.TaskId = ms.ReadInt();
                entity.Path = ms.ReadUTF8String();
                entity.CheckBtn = ms.ReadInt();
                entity.CheckToggle = ms.ReadInt();
                entity.CheckEvent = ms.ReadInt();
                entity.NextTaskId = ms.ReadInt();
                entity.Progress = ms.ReadUTF8String();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}