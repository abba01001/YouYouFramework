using System;
using Cysharp.Threading.Tasks;
using GameScripts;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace GameScripts
{
    public static class SpriteUtils
    {
        public static async UniTask SetSpriteByAtlas(this Component component, string atlas_key, string sprite_name, bool needSetNative = true, Action action = null)
        {
            SpriteAtlas atlas = await GameEntry.Loader.LoadAtlasAsync(atlas_key);
            SetSpriteAsyncDetail(atlas, component, sprite_name, needSetNative, action);
        }

        static void SetSpriteAsyncDetail(SpriteAtlas atlas, Component component, string sprite_name, bool needSetNative = true, Action action = null)
        {
            if (component == null || component.gameObject == null)
            {
                return;
            }
            
            switch (component)
            {
                case Image image:
                    image.sprite = atlas.GetSprite(sprite_name);
                    image.enabled = true;
                    if (needSetNative) image.SetNativeSize();
                    break;
                case SpriteRenderer spriteRenderer:
                    spriteRenderer.sprite = atlas.GetSprite(sprite_name);
                    spriteRenderer.enabled = true;
                    break;
                case RawImage rawImage:
                    rawImage.texture = atlas.GetSprite(sprite_name).texture;
                    rawImage.enabled = true;
                    break;
                default:
                    break;
            }
            action?.Invoke();
        }
    }
}