using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

namespace Watermelon
{
    public class EnvironmentSkyModule
    {
        private static FormGame _formGame;

        private Texture2D texture;
        private Color[] pixels;

        private EnvironmentWeatherModule weatherModule;

        public EnvironmentSkyModule(EnvironmentWeatherModule weatherModule)
        {
            // _formGame = UIController.GetPage<FormGame>();

            texture = new Texture2D(1, 100);
            texture.wrapMode = TextureWrapMode.Clamp;

            pixels = new Color[100];
            InitBackgroundTexture();
            // _formGame.SetBackgroundTexture(texture);
            this.weatherModule = weatherModule;
        }

        async UniTask InitBackgroundTexture()
        {
            var form = GameEntry.UI.GetUIForm<FormGame>();
            while (form == null)
            {
                form = GameEntry.UI.GetUIForm<FormGame>();
                if(form != null) break;
                await UniTask.Delay(50);
            }
            GameEntry.UI.GetUIForm<FormGame>().SetBackgroundTexture(texture);
        }
        
        public void ApplyDayPartPreset(PartOfDayPreset preset)
        {
            for (int i = 0; i < 100; i++)
            {
                var t = 1 - i / 99f;
                var color = preset.SkyGradient.Evaluate(t);
                pixels[i] = weatherModule.GetSkyColor(color, t);
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }

        public void LerpDayPartPresets(PartOfDayPreset from, PartOfDayPreset to, float t)
        {
            for (int i = 0; i < 100; i++)
            {
                var gradientT = 1 - i / 99f;
                var colorFrom = from.SkyGradient.Evaluate(gradientT);
                var colorTo = to.SkyGradient.Evaluate(gradientT);

                var color = Color.Lerp(colorFrom, colorTo, t);

                pixels[i] = weatherModule.GetSkyColor(color, gradientT);
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }
    }
}