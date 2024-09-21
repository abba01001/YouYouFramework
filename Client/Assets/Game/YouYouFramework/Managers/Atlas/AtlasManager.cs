using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace YouYou
{
    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AtlasManager
    {

        public void Init()
        {

        }
        public void OnUpdate()
        {

        }

        public SpriteAtlas GetAtlas(string atlasName)
        {
            Sys_AtlasEntity sys_Atlas = GameEntry.DataTable.Sys_AtlasDBModel.GetEntity(atlasName);
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(sys_Atlas.AssetFullPath);
            return referenceEntity.Target as SpriteAtlas;
        }
        
    }
}