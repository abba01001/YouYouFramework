using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    public class EnvironmentController
    {
        private static EnvironmentController _instance;
        public static EnvironmentController Instance => _instance ??= new EnvironmentController();
        
        private  readonly int _WeatherWindMultiplier = Shader.PropertyToID("_WeatherWindMultiplier");

        private  readonly int _CloudsInfluence1 = Shader.PropertyToID("_CloudsInfluence1");
        private  readonly int _CloudsInfluence2 = Shader.PropertyToID("_CloudsInfluence2");

        private  readonly int _ShadowsIntensity = Shader.PropertyToID("_ShadowsIntensity");
        private  readonly int _RimIntensity = Shader.PropertyToID("_RimIntensity");


        private  List<WeatherContainer> weather = new List<WeatherContainer>();

        private  List<Material> cachedMaterials = new List<Material>();

        public  EnvironmentPresetsDatabase Database;

        public  EnvironmentPreset CurrentPreset { get; private set; }
        public  List<PartOfDayPreset> PartsOfDayPresets { get; private set; }
        public  PartOfDayPreset CurrentPartOfDay { get; private set; }

        private  Light Light { get; set; }

        private  Coroutine daynightCoroutine;
        private  Coroutine weatherCoroutine;

        bool dayNightEnabled = true;
        public bool DayNightEnabled { get => dayNightEnabled; set => dayNightEnabled = value; }

        bool weatherEnabled = true;
        public bool WeatherEnabled { get => weatherEnabled; set => weatherEnabled = value; }

        [SerializeField, Tooltip("Updates preset parameters every frame")] bool debug = false;
        public bool IsDebug { get => debug; set => debug = value; }

        private  EnvironmentSkyModule skyModule;
        private  EnvironmentWeatherModule weatherModule;

        public  bool TransitionInProgress { get; private set; }

        private bool Init = false;
        public async UniTask Initialise()
        {
            Database = await GameEntry.Loader.LoadMainAssetAsync<EnvironmentPresetsDatabase>("Assets/Game/Download/ProjectFiles/Data/Environment/Environment Presets Database.asset", GameEntry.Instance.gameObject);

            weatherModule = new EnvironmentWeatherModule();
            skyModule = new EnvironmentSkyModule(weatherModule);

            await UniTask.NextFrame();
            Init = true;
        }

        private void Update()
        {
            if (!Init) return;
            // Update for debug or weather transition
            if (!TransitionInProgress && (IsDebug || weatherModule.IsTransitioning))
            {
                ApplyDayPartPreset(CurrentPartOfDay);
            }
        }

        private IEnumerator DayNightCoroutine()
        {
            int i = 0;
            CurrentPartOfDay = PartsOfDayPresets[i];

            var pause = new WaitUntil(() => DayNightEnabled);

            while (true)
            {
                yield return new WaitForSeconds(CurrentPartOfDay.PartOfDayDuration);
                if (!DayNightEnabled) yield return pause;

                var nextPartOfDay = PartsOfDayPresets[++i % PartsOfDayPresets.Count];
                yield return PartsOfDayTransition(CurrentPartOfDay, nextPartOfDay);

                CurrentPartOfDay = nextPartOfDay;
            }
        }

        public IEnumerator PartsOfDayTransition(PartOfDayPreset from, PartOfDayPreset to)
        {
            TransitionInProgress = true;

            var time = 0f;
            var duration = from.PartOfDayDuration;

            var pause = new WaitUntil(() => DayNightEnabled);

            while (time < duration)
            {
                time += Time.deltaTime;
                var t = time / duration;

                // LIGHT COLOR
                var environmentLightColor = Color.Lerp(from.LightColor, to.LightColor, t);
               if(Light != null) Light.color = weatherModule.GetLightColor(environmentLightColor);

                // SKY
                skyModule.LerpDayPartPresets(from, to, t);

                // WIND
                var environmentWind = Mathf.Lerp(from.WindMultiplier, to.WindMultiplier, t);
                var weatherWinds = weatherModule.GetWindMultiplier(environmentWind);
                Shader.SetGlobalFloat(_WeatherWindMultiplier, weatherWinds);

                // SHADOWS
                var environmentShadows = Mathf.Lerp(from.ShadowsIntensity, to.ShadowsIntensity, t);
                var weatherShadows = weatherModule.GetShadowsMultiplier(environmentShadows);
                Shader.SetGlobalFloat(_ShadowsIntensity, weatherShadows);

                // RIM
                var environmentRim = Mathf.Lerp(from.RimIntensity, to.RimIntensity, t);
                var weatherRim = weatherModule.GetShadowsMultiplier(environmentRim);
                Shader.SetGlobalFloat(_RimIntensity, weatherRim);

                // CLOUDS
                if (Light != null && Light.cookie != null && Light.cookie is CustomRenderTexture cloudsTexture)
                {
                    var environmentClouds1 = Mathf.Lerp(from.CloudsInfluence1, to.CloudsInfluence1, t);
                    var environmentClouds2 = Mathf.Lerp(from.CloudsInfluence2, to.CloudsInfluence2, t);

                    var environmentCloudsInfluence = new DuoFloat(environmentClouds1, environmentClouds2);
                    var weatherCloudsInfluence = weatherModule.GetCloudsInfluence(environmentCloudsInfluence);

                    cloudsTexture.material.SetFloat(_CloudsInfluence1, weatherCloudsInfluence.firstValue);
                    cloudsTexture.material.SetFloat(_CloudsInfluence2, weatherCloudsInfluence.secondValue);
                }

                weatherModule.ApplyFog();

                yield return null;
                if (!DayNightEnabled) yield return pause;
            }

            ApplyDayPartPreset(to);

            TransitionInProgress = false;
        }

        private void ApplyDayPartPreset(PartOfDayPreset preset)
        {
            if (Light != null) Light.color = weatherModule.GetLightColor(preset.LightColor);

            skyModule.ApplyDayPartPreset(preset);

            Shader.SetGlobalFloat(_WeatherWindMultiplier, weatherModule.GetWindMultiplier(preset.WindMultiplier));
            Shader.SetGlobalFloat(_ShadowsIntensity, weatherModule.GetShadowsMultiplier(preset.ShadowsIntensity));
            Shader.SetGlobalFloat(_RimIntensity, weatherModule.GetShadowsMultiplier(preset.RimIntensity));

            if (Light != null && Light.cookie != null && Light.cookie is CustomRenderTexture cloudsTexture)
            {
                DuoFloat cloudsInfluence = new DuoFloat(preset.CloudsInfluence1, preset.CloudsInfluence2);
                cloudsInfluence = weatherModule.GetCloudsInfluence(cloudsInfluence);

                cloudsTexture.material.SetFloat(_CloudsInfluence1, cloudsInfluence.firstValue);
                cloudsTexture.material.SetFloat(_CloudsInfluence2, cloudsInfluence.secondValue);
            }

            weatherModule.ApplyFog();
        }

        public void SetPreset(EnvironmentPresetType type)
        {
            Light = GameEntry.FindFirstObjectByType<Light>();

            if(Light != null && Light.cookie != null && Light.cookie is CustomRenderTexture cloudsTexture)
            {
                if(!cachedMaterials.Contains(cloudsTexture.material)) cachedMaterials.Add(cloudsTexture.material);
            }

            CurrentPreset = Database.GetPreset(type);

            PartsOfDayPresets = new List<PartOfDayPreset>();

            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Day) == PartOfDay.Day) PartsOfDayPresets.Add(CurrentPreset.DayPreset);
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Evening) == PartOfDay.Evening) PartsOfDayPresets.Add(CurrentPreset.EveningPreset);
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Night) == PartOfDay.Night) PartsOfDayPresets.Add(CurrentPreset.NightPreset);
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Morning) == PartOfDay.Morning) PartsOfDayPresets.Add(CurrentPreset.MorningPreset);

            CurrentPartOfDay = PartsOfDayPresets[0];

            ApplyDayPartPreset(CurrentPartOfDay);

            if (daynightCoroutine != null) GameEntry.Instance.StopCoroutine(daynightCoroutine);

            if (PartsOfDayPresets.Count > 1)
            {
                daynightCoroutine = GameEntry.Instance.StartCoroutine(DayNightCoroutine());
            }

            weather.Clear();

            if (!CurrentPreset.Weather.IsNullOrEmpty())
            {
                foreach (var weatherContainer in CurrentPreset.Weather)
                {
                    weather.Add(weatherContainer);
                }
            }

            if (weatherCoroutine != null) GameEntry.Instance.StopCoroutine(weatherCoroutine);

            if (weather.Count > 1)
            {
                weatherCoroutine = GameEntry.Instance.StartCoroutine(WeatherCoroutine());
            }
            else if (weather.Count == 1)
            {
                weatherModule.SetWeatherPreset(weather[0].WeatherPreset, CurrentPreset.WeatherTransitionDuration);
            }
            else
            {
                weatherModule.RemoveWeatherPreset(0);
            }
        }

        public void OnWorldUnloaded()
        {
            if (daynightCoroutine != null) GameEntry.Instance.StopCoroutine(daynightCoroutine);
            if (weatherCoroutine != null) GameEntry.Instance.StopCoroutine(weatherCoroutine);
        }

        private IEnumerator WeatherCoroutine()
        {
            var pause = new WaitUntil(() => WeatherEnabled);

            while (true)
            {
                var weather = GetNextWeather();

                weatherModule.SetWeatherPreset(weather.WeatherPreset, CurrentPreset.WeatherTransitionDuration);

                yield return new WaitForSeconds(weather.Duration.Random());

                if (!WeatherEnabled) yield return pause;
            }
        }

        public WeatherContainer GetNextWeather()
        {
            var chanceSum = 0f;
            for (int i = 0; i < weather.Count; i++)
            {
                chanceSum += weather[i].Chance;
            }

            var random = Random.value * chanceSum;

            var counter = 0f;
            for (int i = 0; i < weather.Count; i++)
            {
                var weatherContainer = weather[i];
                counter += weather[i].Chance;

                if (random <= counter) return weather[i];
            }

            return null;
        }

        public void StartDayPreset()
        {
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Day) != PartOfDay.Day) return;

            DayNightEnabled = false;

            IsDebug = true;

            CurrentPartOfDay = CurrentPreset.DayPreset;

            ApplyDayPartPreset(CurrentPreset.DayPreset);
        }

        public void StartEveningPreset()
        {
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Evening) != PartOfDay.Evening) return;

            DayNightEnabled = false;

            IsDebug = true;

            CurrentPartOfDay = CurrentPreset.EveningPreset;

            ApplyDayPartPreset(CurrentPreset.EveningPreset);
        }

        public void StartNightPreset()
        {
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Night) != PartOfDay.Night) return;

            DayNightEnabled = false;

            IsDebug = true;

            CurrentPartOfDay = CurrentPreset.NightPreset;

            ApplyDayPartPreset(CurrentPreset.NightPreset);
        }

        public void StartMorningPreset()
        {
            if ((CurrentPreset.EnabledPartsOfDay & PartOfDay.Morning) != PartOfDay.Morning) return;

            DayNightEnabled = false;

            IsDebug = true;

            CurrentPartOfDay = CurrentPreset.MorningPreset;

            ApplyDayPartPreset(CurrentPreset.MorningPreset);
        }

        public void ApplyPreset(EnvironmentPreset preset, PartOfDay partOfDay)
        {
            if(CurrentPreset != preset)
            {
                SetPreset(preset.Type);
            }

            DayNightEnabled = false;
            WeatherEnabled = false;

            if (partOfDay == PartOfDay.Day) CurrentPartOfDay = CurrentPreset.DayPreset;
            if (partOfDay == PartOfDay.Evening) CurrentPartOfDay = CurrentPreset.EveningPreset;
            if (partOfDay == PartOfDay.Night) CurrentPartOfDay = CurrentPreset.NightPreset;
            if (partOfDay == PartOfDay.Morning) CurrentPartOfDay = CurrentPreset.MorningPreset;

            weatherModule.RemoveWeatherPreset(0);

            ApplyDayPartPreset(CurrentPartOfDay);
        }

        public void ApplyWeather(EnvironmentWeatherPreset preset)
        {
            WeatherEnabled = false;
            weatherModule.SetWeatherPreset(preset, 0);

            ApplyDayPartPreset(CurrentPartOfDay);
        }

        private void OnDestroy()
        {
            for(int i = 0; i < cachedMaterials.Count; i++)
            {
                var material = cachedMaterials[i];

                material.SetFloat(_CloudsInfluence1, 0);
                material.SetFloat(_CloudsInfluence2, 0);
            }
        }
    }
}