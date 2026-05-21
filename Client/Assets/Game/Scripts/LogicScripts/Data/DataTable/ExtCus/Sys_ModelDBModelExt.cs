using System.Collections;
using System.Collections.Generic;
using Main;
using OctoberStudio;
using UnityEngine;

namespace GameScripts
{
    public partial class Sys_ModelDBModel
    {
        // 缓存：EnemyType → Sys_ModelEntity
        private Dictionary<EnemyType, Sys_ModelEntity> _enemyModelCache = new Dictionary<EnemyType, Sys_ModelEntity>();
        public Dictionary<int, Sys_ModelEntity> IdByDic;
        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();

            IdByDic = new Dictionary<int, Sys_ModelEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_ModelEntity entity = m_List[i];
                IdByDic.TryAdd(entity.Id, entity);
                if (System.Enum.TryParse(entity.EnemyType, out EnemyType type))
                {
                    _enemyModelCache[type] = entity;
                }
            }
        }


        public Sys_ModelEntity GetEntity(int task_id)
        {
            if (IdByDic.TryGetValue(task_id, out var entity))
            {
                return entity;
            }
            return null;
        }
        
        public Sys_ModelEntity GetEntity(EnemyType enemyType)
        {
            _enemyModelCache.TryGetValue(enemyType, out var entity);
            return entity;
        }
    }
}