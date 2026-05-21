using System.Collections;
using System.Collections.Generic;
using LayerLab.ArtMaker;
using Main;
using OctoberStudio;
using Sirenix.Utilities;
using UnityEngine;

namespace GameScripts
{
    public partial class Sys_EquipmentDBModel
    {
        
        private Dictionary<PartsType, List<string>> _partsTexCache = new Dictionary<PartsType, List<string>>();
        public Dictionary<int, Sys_EquipmentEntity> IdByDic;
        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();

            IdByDic = new Dictionary<int, Sys_EquipmentEntity>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_EquipmentEntity entity = m_List[i];
                IdByDic.TryAdd(entity.Id, entity);
                if (System.Enum.TryParse(entity.EquipType, out PartsType type))
                {
                    if (!_partsTexCache.ContainsKey(type))
                    {
                        _partsTexCache.Add(type,new List<string>());
                    }
                    _partsTexCache[type].Add(entity.TexName);
                }
            }
        }

        public Dictionary<PartsType, List<string>> GetPartsInfo()
        {
            return _partsTexCache;
        }
    }
}