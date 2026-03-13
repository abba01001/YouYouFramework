using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public partial class Sys_LevelDBModel
    {
        public Dictionary<int, LevelModel> IdByDic;

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            IdByDic = new Dictionary<int, LevelModel>();
            for (int i = 0; i < m_List.Count; i++)
            {
                Sys_LevelEntity entity = m_List[i];
                LevelModel model = new LevelModel();
                model.levelId = entity.LevelId;
                model.mapLevel = entity.MapLevel;
                model.levelNum = entity.LevelNumber;
                IdByDic.Add(entity.LevelId, model);
            }
        }

        public LevelModel GetEntity(int levelId)
        {
            if (IdByDic.ContainsKey(levelId))
            {
                return IdByDic[levelId];
            }
            GameEntry.LogError(LogCategory.Framework, "没有找到资源, levelId==" + levelId);
            return null;
        }
    }
