using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEditor;
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
        
        public async UniTask SetSpriteAsyncByPath(string assetPath, SpriteRenderer spriteRenderer)
        {
            AssetReferenceEntity s =  await GameEntry.Loader.LoadMainAssetAsync(assetPath);
            Texture2D texture = s.Target as Texture2D;
            Sprite sprite = Sprite.Create(
                texture, 
                new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f)  // 锚点设为纹理的中心
            );
            spriteRenderer.sprite = sprite ;
        }
        
    }
}