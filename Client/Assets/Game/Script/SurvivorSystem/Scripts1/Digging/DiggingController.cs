using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    public class DiggingController : MonoBehaviour
    {
        private static DiggingController _instance;
        public static DiggingController Instance => _instance ??= new DiggingController();

        private bool isActive;
        private DiggingSpawnSettings settings;

        private Coroutine spawnCoroutine;

        private  List<DiggingSpotBehavior> activeDiggingPoints = new List<DiggingSpotBehavior>();
        private  List<DiggingSpawnPoint> registeredSpawnPoints = new List<DiggingSpawnPoint>();

        public async UniTask Initialise()
        {
            await UniTask.NextFrame();
        }

        private IEnumerator SpawnCoroutine()
        {
            WaitForSeconds waitForSeconds;
            int startPoints = settings.StartActivePoints;
            for(int i = 0; i < startPoints; i++)
            {
                SpawnPoint();
            }

            while(isActive)
            {
                waitForSeconds = new WaitForSeconds(settings.SpawnDelay.Random());

                yield return waitForSeconds;

                if(activeDiggingPoints.Count <= settings.MaxActivePoints)
                {
                    SpawnPoint();
                }
            }
        }

        private void SpawnPoint()
        {
            if (registeredSpawnPoints.IsNullOrEmpty()) return;

            GameObject diggingPointPrefab = settings.Prefabs.GetRandomItem();

            if(diggingPointPrefab == null)
            {
                Debug.LogError("There are no linked digging point prefabs for the current world!");

                return;
            }

            IEnumerable<DiggingSpawnPoint> filteredPoints = registeredSpawnPoints.Where(x => x.IsActive).OrderBy(x => Random.value);

            int totalWeight = filteredPoints.Sum(x => x.SpawnPriorityWeight);
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            DiggingSpawnPoint selectedSpawnPoint = null;
            foreach(DiggingSpawnPoint point in filteredPoints)
            {
                currentWeight += point.SpawnPriorityWeight;

                if(currentWeight >= randomValue)
                {
                    selectedSpawnPoint = point;

                    break;
                }
            }

            if (selectedSpawnPoint == null) return;

            DiggingSpotBehavior diggingPointBehavior = selectedSpawnPoint.Spawn(diggingPointPrefab);
            if(diggingPointBehavior != null)
            {
                activeDiggingPoints.Add(diggingPointBehavior);
            }
        }

        public void Activate(DiggingSpawnSettings settings)
        {
            this.settings = settings;

            isActive = true;

            spawnCoroutine = GameEntry.Instance.StartCoroutine(SpawnCoroutine());
        }

        public void Disable()
        {
            isActive = false;

            if (spawnCoroutine != null)
                GameEntry.Instance.StopCoroutine(spawnCoroutine);
        }

        public void Unload()
        {
            registeredSpawnPoints.Clear();
            activeDiggingPoints.Clear();
        }

        public  void OnDiggingPointCollected(DiggingSpotBehavior diggingPointBehavior)
        {
            activeDiggingPoints.Remove(diggingPointBehavior);
        }

        public void OverrideSpawnSettings(DiggingSpawnSettings _settings)
        {
            if (settings == null) return;

            settings = _settings;
        }

        public  void RegisterSpawnPoint(DiggingSpawnPoint spawnPoint)
        {
            registeredSpawnPoints.Add(spawnPoint);
        }
    }
}