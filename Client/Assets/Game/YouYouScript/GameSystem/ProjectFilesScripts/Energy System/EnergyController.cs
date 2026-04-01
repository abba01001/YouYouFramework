using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon
{
    public class EnergyController : SingletonMonoInstance<EnergyController>
    {
        bool isEnergyEnabled = true;
        public  EnergySystemDatabase Data;

        public  float LowEnergySpeedCoef => IsEnergySystemEnabled ? EnergyPoints == 0 ? Data.LowEnergySpeedMult : 1f : 1f;
        public  float LowEnergyHittingSpeedCoef => IsEnergySystemEnabled ? EnergyPoints == 0 ? Data.LowEnergyHarvestSpeedMult : 1f : 1f;

        public  float EnergyPoints
        {
            get => save.energyPoints;
            private set
            {
                save.energyPoints = Mathf.Clamp(value, 0, Data.MaxEnergyPoints);
                SaveController.MarkAsSaveIsRequired();
                GameEntry.Event.Dispatch(Constants.EventName.EnergyChangedEvent);
            }
        }

        private  readonly int FLOATING_TEXT_HASH = "Floating Text".GetHashCode();
        private  float notAccountedEnergyPoints;

        private  EnergySave save;

        public  bool IsEnergySystemEnabled => isEnergyEnabled;

        private  List<CurrencyType> foodResourceTypes = new List<CurrencyType>();
        public  bool IsFoorResource(CurrencyType resource) => foodResourceTypes.Contains(resource);

        private  FloatingTextBaseBehavior foodConsumedText;

        private  int lastDisplayedFoodPoints;
        private  readonly float FULL_ENERGY_THRESHOLD = 0.9f;

        public async UniTask Initialise()
        {
            save = SaveController.GetSaveObject<EnergySave>("Energy");
            Data = await GameEntry.Loader.LoadMainAssetAsync<EnergySystemDatabase>("Assets/Game/Download/ProjectFiles/Data/Energy System Database.asset", GameEntry.Instance.gameObject);

            // works only in editor
            if (EnergyActionMenu.IsEnergyDisabled())
            {
                EnergyPoints = Data.MaxEnergyPoints;
            }

            Data.Initialise();

            if (!isEnergyEnabled)
                return;

            for (int i = 0; i < Data.FoodItems.Count; i++)
            {
                CurrencyController.GetCurrency(Data.FoodItems[i].type).OnCurrencyChanged += OnFoodCurrencyChanged;

                foodResourceTypes.Add(Data.FoodItems[i].type);
            }

            ResourceSourceBehavior.OnFirstTimeHit += OnFirstResourceHit;
            PlayerBehavior.OnResourceWillBeReceived += OnResorceWillBeReceivedReceived;

            Tween.DelayedCall(0.1f, DisableFoodItemsCurrencyUI);

            await UniTask.NextFrame();
        }

        private void DisableFoodItemsCurrencyUI()
        {
            for (int i = 0; i < Data.FoodItems.Count; i++)
            {
                CurrencyUIController.Instance.DisableCurrency(Data.FoodItems[i].type);
            }
        }

        private void OnResorceWillBeReceivedReceived(FlyingResourceBehavior resource)
        {
            if (Data.IsFoodItem(resource.ResourceType))
            {
                FoodItemData itemData = Data.GetItemData(resource.ResourceType);

                notAccountedEnergyPoints += itemData.energyPointsRestoring;

                // restricting players ability to consume more food items
                if (EnergyPoints + notAccountedEnergyPoints >= Data.MaxEnergyPoints * FULL_ENERGY_THRESHOLD)
                {
                    UpdatePlayerFoodReceiveAbility();
                }
            }
        }

        private void OnDisable()
        {
            CurrencyController.UnsubscribeGlobalCallback(OnFoodCurrencyChanged);

            ResourceSourceBehavior.OnFirstTimeHit -= OnFirstResourceHit;
            PlayerBehavior.OnResourceWillBeReceived -= OnResorceWillBeReceivedReceived;
        }

        public  void Unload()
        {
            if (!IsEnergySystemEnabled)
                return;

            ResourceSourceBehavior.OnFirstTimeHit -= OnFirstResourceHit;
            PlayerBehavior.OnResourceWillBeReceived -= OnResorceWillBeReceivedReceived;
        }

        private void OnFoodCurrencyChanged(Currency currency, int difference)
        {
            if (difference > 0)
            {
                int newPoints = Data.GetItemData(currency.CurrencyType).energyPointsRestoring;

                notAccountedEnergyPoints -= newPoints;

                RestoreEnergyPoints(newPoints);

                if (foodConsumedText == null)
                {
                    lastDisplayedFoodPoints = newPoints;
                    foodConsumedText = FloatingTextController.Instance.SpawnFloatingText(FLOATING_TEXT_HASH, $"+{newPoints} ENERGY <sprite name={currency.CurrencyType}>", PlayerBehavior.InstanceTransform.position.AddToY(2f), Quaternion.identity, 1.0f, Color.white).GetComponent<FloatingTextBaseBehavior>();
                    foodConsumedText.OnAnimationCompleted += OnFoodConsumedTextAnimationCompleted;
                }
                else
                {
                    lastDisplayedFoodPoints += newPoints;
                    foodConsumedText.Activate($"+{lastDisplayedFoodPoints} ENERGY <sprite name={currency.CurrencyType}>", 1.0f, Color.white);
                }

                CurrencyController.Substract(currency.CurrencyType, difference);

#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

                AudioController.PlaySound(AudioController.AudioClips.boost);
            }
        }

        private void OnFoodConsumedTextAnimationCompleted()
        {
            if (foodConsumedText != null)
            {
                foodConsumedText.OnAnimationCompleted -= OnFoodConsumedTextAnimationCompleted;
                foodConsumedText = null;
            }
        }

        private  void UpdatePlayerFoodReceiveAbility()
        {
            if (EnergyPoints + notAccountedEnergyPoints >= Data.MaxEnergyPoints * FULL_ENERGY_THRESHOLD)
            {
                PlayerBehavior.GetBehavior().RemoveAcceptableResoruces(foodResourceTypes);
            }
            else
            {
                PlayerBehavior.GetBehavior().AddAcceptableResoruces(foodResourceTypes);
            }
        }

        private void OnFirstResourceHit()
        {
            RemoveEnergyPoints(Data.EnergyCostForHarvesting);
        }

        public  void RestoreEnergyPoints(float poinsAmount)
        {
            EnergyPoints += poinsAmount;
        }

        public  void RemoveEnergyPoints(float pointsAmount)
        {
            // works only in editor
            if (EnergyActionMenu.IsEnergyDisabled())
                return;

            bool wasFullEnergyPointsBefore = EnergyPoints >= Data.MaxEnergyPoints * FULL_ENERGY_THRESHOLD;

            EnergyPoints -= pointsAmount;

            // restoring players ability to consume energy items
            if (wasFullEnergyPointsBefore)
            {
                UpdatePlayerFoodReceiveAbility();
            }
        }

        public  void OnConstructionHit()
        {
            RemoveEnergyPoints(Data.EnergyCostForBuilding);
        }


        [System.Serializable]
        private class EnergySave : ISaveObject
        {
            public float energyPoints;

            public void Flush()
            {
            }
        }
    }
}