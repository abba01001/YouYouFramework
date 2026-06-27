using System.Collections;
using System.Collections.Generic;
using Main;
using OctoberStudio;
using Sirenix.Utilities;
using UnityEngine;

namespace GameScripts
{
    public partial class Sys_EnemyDataDBModel
    {
        private Dictionary<EnemyType, List<EnemyDropData>> _enemyDropCache =
            new Dictionary<EnemyType, List<EnemyDropData>>();

        public Dictionary<int, Sys_EnemyDataEntity> IdByDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();

            IdByDic = new Dictionary<int, Sys_EnemyDataEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_EnemyDataEntity entity = m_List[i];
                IdByDic.TryAdd(entity.Id, entity);
                if (System.Enum.TryParse(entity.EnemyType, out EnemyType type))
                {
                    Dictionary<DropType, int> c = new Dictionary<DropType, int>();
                    List<EnemyDropData> list = new List<EnemyDropData>();
                    HandleDropInfo(c, entity.DropInfo);
                    foreach (var kv in c)
                    {
                        list.Add(new EnemyDropData()
                        {
                            dropType = kv.Key,
                            chance = kv.Value
                        });
                    }

                    _enemyDropCache[type] = list;
                }
            }
        }

        private void HandleDropInfo(Dictionary<DropType, int> c, string dropInfo)
        {
            foreach (var s in dropInfo.Split(Converter.FourthSeparator))
            {
                string[] t = s.Split(Converter.FifthSeparator);
                int type = Converter.ToInt(t[0]);
                int value = Converter.ToInt(t[1]);
                c.Add((DropType)type, value);
            }
        }

        public List<EnemyDropData> GetDropInfo(EnemyType enemyType)
        {
            _enemyDropCache.TryGetValue(enemyType, out var data);
            return data;
        }
    }
}