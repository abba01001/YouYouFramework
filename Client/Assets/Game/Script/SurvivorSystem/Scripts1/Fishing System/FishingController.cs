using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    public class FishingController : MonoBehaviour
    {
        private static FishingController _instance;
        public static FishingController Instance => _instance ??= new FishingController();
        float spawnPercent = 0.5f;
        public float SpawnPercent => spawnPercent;

        private  List<FishingPlaceBehavior> registeredFishingPlaces = new List<FishingPlaceBehavior>();

        public async UniTask Initialise()
        {
            await UniTask.NextFrame();
        }

        public void SpawnFishingPlaces()
        {
            if(!registeredFishingPlaces.IsNullOrEmpty())
            {
                // Shuffle fishing places list
                registeredFishingPlaces.Shuffle();

                int filteredPlacesCount = Mathf.Clamp(Mathf.RoundToInt(registeredFishingPlaces.Count * spawnPercent), 1, registeredFishingPlaces.Count);
                for(int i = 0; i < filteredPlacesCount; i++)
                {
                    registeredFishingPlaces[i].Spawn();
                }
            }
        }

        public  void SpawnRandomFishingPlace()
        {
            int fishingPlacesCount = registeredFishingPlaces.Count;
            int randomStartPoint = Random.Range(0, fishingPlacesCount);
            for (int i = 0; i < fishingPlacesCount; i++)
            {
                int currentIndex = (randomStartPoint + i) % fishingPlacesCount;
                if (registeredFishingPlaces[currentIndex].CanBeRespawn())
                {
                    registeredFishingPlaces[currentIndex].Spawn();

                    break;
                }
            }
        }

        public void AddFishingPlace(FishingPlaceBehavior fishingPlaceBehavior, bool spawnIfRequired = false)
        {
            registeredFishingPlaces.Add(fishingPlaceBehavior);

            if(spawnIfRequired)
            {
                int activePlacesCount = 0;
                foreach(var fishingPlace in registeredFishingPlaces)
                {
                    if(fishingPlace.IsActive)
                    {
                        activePlacesCount++;
                    }
                }

                int requiredPlacesCount = Mathf.Clamp(Mathf.RoundToInt(registeredFishingPlaces.Count * spawnPercent), 1, registeredFishingPlaces.Count);
                int countDiff = requiredPlacesCount - activePlacesCount;
                if(countDiff > 0)
                {
                    for(int i = 0; i < countDiff; i++)
                    {
                        SpawnRandomFishingPlace();
                    }
                }
            }
        }

        public  void RemoveFishingPlace(FishingPlaceBehavior fishingPlaceBehavior)
        {
            registeredFishingPlaces.Remove(fishingPlaceBehavior);
        }

        public  void Unload()
        {
            registeredFishingPlaces.Clear();
        }
    }
}