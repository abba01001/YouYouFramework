using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts
{
    public partial class Sys_GuideDBModel

        {

            public Dictionary<int, Sys_GuideEntity> IdByDic;

    

            protected override void OnLoadListComple()

            {

                base.OnLoadListComple();

                IdByDic = new Dictionary<int, Sys_GuideEntity>();

                for (int i = 0; i < m_List.Count; i++)

                {

                    Sys_GuideEntity entity = m_List[i];

                    if (!IdByDic.ContainsKey(entity.GuideId))

                    {

                        IdByDic.Add(entity.GuideId, entity);

                    }

                    else

                    {

                        GameEntry.LogError(LogCategory.Framework, "Guide���ñ����! TaskId==" + entity.GuideId);

                    }

                }

            }

    

            public Sys_GuideEntity GetEntity(int task_id)

            {

                if (IdByDic.ContainsKey(task_id))

                {

                    return IdByDic[task_id];

                }

                GameEntry.LogError(LogCategory.Framework, "û���ҵ�����, TaskId==" + task_id);

                return null;

            }

        }
}