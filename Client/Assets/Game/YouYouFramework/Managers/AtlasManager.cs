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

        public SpriteAtlas GetAtlas(string atlasPath)
        {
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(atlasPath);
            if (referenceEntity != null)
            {
                // SpriteAtlas obj = UnityEngine.Object.Instantiate(referenceEntity.Target as SpriteAtlas, parent);
                // AutoReleaseHandle.Add(referenceEntity, obj);
                // return obj;
                referenceEntity.ReferenceAdd();
            }
            return referenceEntity.Target as SpriteAtlas;
        }
        
    }
}