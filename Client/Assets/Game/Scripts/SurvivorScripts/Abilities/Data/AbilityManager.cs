using System.Collections.Generic;
using Main;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    using OctoberStudio.Easing;
    using OctoberStudio.Extensions;

    public class AbilityManager : MonoBehaviour
    {
        [SerializeField] protected AbilitiesDatabase abilitiesDatabase;

        [Space]
        [SerializeField, Range(0, 1)] protected float chestChanceTier5;
        [SerializeField, Range(0, 1)] protected float chestChanceTier3;

        protected List<IAbilityBehavior> aquiredAbilities = new List<IAbilityBehavior>();
        protected List<AbilityType> removedAbilities = new List<AbilityType>();
        protected AbilitiesSave save;
        protected StageSave stageSave;

        public int ActiveAbilitiesCapacity => abilitiesDatabase.ActiveAbilitiesCapacity;
        public int PassiveAbilitiesCapacity => abilitiesDatabase.PassiveAbilitiesCapacity;

        protected virtual void Awake()
        {
            save = GameController.SaveManager.AbilitiesData;
            save.Init();

            stageSave = GameController.SaveManager.StageData;

            // Usualy the data isn't getting reset only if the Player continues the game after they've closed it without dying
            if(stageSave.ResetStageData) save.Clear();
        }

        public virtual void Init(PresetData testingPreset, CharacterData characterData)
        {
            StageController.ExperienceManager.onXpLevelChanged += OnXpLevelChanged;

            if(testingPreset != null)
            {
                // If testing preset is assigned, loading starting abilities from it

                for (int i = 0; testingPreset.Abilities.Count > i; i++)
                {
                    AbilityType type = testingPreset.Abilities[i].abilityType;

                    AbilityData data = abilitiesDatabase.GetAbility(type);
                    AddAbility(data, testingPreset.Abilities[i].level);
                }
            } else if (!stageSave.ResetStageData)
            {
                // The game is being continued after the Player exited it without dying. Loading abilities from the save file

                var savedAbilities = save.GetSavedAbilities();
                for(int i = 0; i < savedAbilities.Count; i++)
                {
                    AbilityType type = savedAbilities[i];

                    AbilityData data = abilitiesDatabase.GetAbility(type);
                    AddAbility(data, save.GetAbilityLevel(type));
                }

                // If there are no abilities stored in the save file, than we're loading character's starting ability or showing weapon selection window if there are none

                if(savedAbilities.Count == 0)
                {
                    if (characterData.HasStartingAbility)
                    {
                        AbilityData data = abilitiesDatabase.GetAbility(characterData.StartingAbility);
                        AddAbility(data, 0);
                    } else
                    {
                        EasingManager.DoAfter(0.3f, ShowWeaponSelectScreen);
                    }
                    
                }
            } else if (characterData.HasStartingAbility)
            {
                // No testing presets or abilities from save file, loading character starting ability.

                AbilityData data = abilitiesDatabase.GetAbility(characterData.StartingAbility);
                AddAbility(data, 0);
            }
            else
            {
                // Just showing weapon selection window with some delay

                EasingManager.DoAfter(0.3f, ShowWeaponSelectScreen);
            }
        }

        public virtual void AddAbility(AbilityData abilityData, int level = 0)
        {
            IAbilityBehavior ability = Instantiate(abilityData.Prefab).GetComponent<IAbilityBehavior>();
            ability.Init(abilityData, level);

            if (abilityData.IsEvolution)
            {
                //为了进化能力，我们必须满足每一项要求，
                // 检查是否已满足条件，并移除所有正在进化的能力
                for (int i = 0; i < abilityData.EvolutionRequirements.Count; i++)
                {
                    var requirement = abilityData.EvolutionRequirements[i];

                    if (requirement.ShouldRemoveAfterEvolution)
                    {
                        var requiredAbility = GetAquiredAbility(requirement.AbilityType);

                        if (requiredAbility != null)
                        {
                            // Abilities' game object is being destroyed inside
                            requiredAbility.Clear();

                            aquiredAbilities.Remove(requiredAbility);
                            save.RemoveAbility(requiredAbility.AbilityType);

                            removedAbilities.Add(requiredAbility.AbilityType);
                        }
                    }
                }
            }

            save.SetAbilityLevel(abilityData.AbilityType, level);
            aquiredAbilities.Add(ability);
        }

        public virtual int GetActiveAbilitiesCount()
        {
            // Crude, but it's called rearly, so no harm
            int counter = 0;
            foreach(var ability in aquiredAbilities)
            {
                if (ability.AbilityData.IsActiveAbility) counter++;
            }
            return counter;
        }

        public virtual int GetPassiveAbilitiesCount()
        {
            // Crude, but it's called rearly, so no harm
            int counter = 0;
            foreach (var ability in aquiredAbilities)
            {
                if (!ability.AbilityData.IsActiveAbility) counter++;
            }
            return counter;
        }

        protected virtual void ShowWeaponSelectScreen()
        {
            var weaponAbilities = new List<AbilityData>();

            // 查找所有非进化型的武器技能
            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                var abilityData = abilitiesDatabase.GetAbility(i);
                if (CheckUnlockAbility(abilityData) && abilityData.IsWeaponAbility && !abilityData.IsEvolution)
                {
                    weaponAbilities.Add(abilityData);
                }
            }

            //随机选择最多三个
            var selectedAbilities = new List<AbilityData>();
            while (weaponAbilities.Count > 0 && selectedAbilities.Count < 3)
            {
                var abilityData = weaponAbilities.PopRandom();
                selectedAbilities.Add(abilityData);
            }

            // 仅作确认检查，若未选中任何能力，则显然出了问题。
            // 在这种情况下应检查武器能力是否已分配至能力数据库
            if (selectedAbilities.Count > 0)
            {
                StageController.GameScreen.ShowAbilitiesPanel(selectedAbilities, false);
            }
        }

        protected virtual void OnXpLevelChanged(int level)
        {
            var abilities = GetAvailableAbilities();
            var selectedAbilities = new List<AbilityData>();

            var weightedAbilities = new List<WeightedAbility>();

            bool firstLevels = level < 10;

            var activeCount = GetActiveAbilitiesCount();
            var passiveCount = GetPassiveAbilitiesCount();

            bool moreActive = activeCount > passiveCount;
            bool morePassive = passiveCount > activeCount;

            //在这里，我们用权重填充能力列表。
            //根据能力数据库中的乘数，一些能力将有更高的机会被选择
            //例如，通常进化能力应该在每次可用时进行选择
            foreach (var ability in abilities)
            {
                var weight = 1f;
                if (IsAbilityAquired(ability.AbilityType)) weight *= abilitiesDatabase.AquiredAbilityWeightMultiplier;

                if (ability.IsActiveAbility)
                {
                    if (firstLevels) weight *= abilitiesDatabase.FirstLevelsActiveAbilityWeightMultiplier;
                    if (morePassive) weight *= abilitiesDatabase.LessAbilitiesOfTypeWeightMultiplier;
                    if (ability.IsEvolution) weight *= abilitiesDatabase.EvolutionAbilityWeightMultiplier;
                } else
                {
                    if (IsRequiredForAquiredEvolution(ability.AbilityType)) weight *= abilitiesDatabase.RequiredForEvolutionWeightMultiplier;
                    if (moreActive) weight *= abilitiesDatabase.LessAbilitiesOfTypeWeightMultiplier;
                }

                weightedAbilities.Add(new WeightedAbility() { abilityData = ability, weight = weight });
            }

            while (abilities.Count > 0 && selectedAbilities.Count < 3)
            {
                // Here we're evening out weights so that their sum was exactly 1

                float weightSum = 0f;
                foreach(var container in weightedAbilities) weightSum += container.weight;

                foreach (var container in weightedAbilities) container.weight /= weightSum;

                // Getting random value between 0 and 1,
                // Iteraing abilities until the sum of ther weight is more than the random value
                // if the random value is 0, we'll select the first ability
                // if it is 1, we'll select the last one

                float random = Random.value;
                float progress = 0;

                AbilityData selectedAbility = null;

                foreach (var container in weightedAbilities)
                {
                    progress += container.weight;

                    if(random <= progress)
                    {
                        selectedAbility = container.abilityData;
                        break;
                    }
                }

                // If we've successfully selected an ability (as we should have been), were removing it from the pool of available abilities
                // If something gone wrong, we have a failsafe - just completely randomly selecting one of the available abilities
                if(selectedAbility != null)
                {
                    abilities.Remove(selectedAbility);
                } else
                {
                    selectedAbility = abilities.PopRandom();
                }

                // Removing selected ability from the weighted abilities list

                foreach (var container in weightedAbilities)
                {
                    if(container.abilityData == selectedAbility)
                    {
                        weightedAbilities.Remove(container);
                        break;
                    }
                }

                selectedAbilities.Add(selectedAbility);
            }

            if (!change)
            {
                change = true;
                selectedAbilities.RemoveAt(0);
                for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
                {
                    if (abilitiesDatabase.GetAbility(i).AbilityType == AbilityType.DestructiveAxe)
                    {
                        selectedAbilities.Add(abilitiesDatabase.GetAbility(i));
                        break;
                    }
                }
            }
            
            if(selectedAbilities.Count > 0)
            {
                StageController.GameScreen.ShowAbilitiesPanel(selectedAbilities, true);
            }
        }

        private bool change = false;

        private bool CheckUnlockAbility(AbilityData ability)
        {
            return GameController.SaveManager.StageData.MaxReachedStageId >= ability.UnlockLv;
        }

        protected virtual List<AbilityData> GetAvailableAbilities()
        {
            var result = new List<AbilityData>();
            //统计被动和主动获得的能力数量。
            //我们在能力数据库中指定了每种能力的最大数量
            int activeAbilitiesCount = 0;
            int passiveAbilitiesCount = 0;
            for(int i = 0; i < aquiredAbilities.Count; i++)
            {
                var abilityBehavior = aquiredAbilities[i];
                var abilityData = abilitiesDatabase.GetAbility(abilityBehavior.AbilityType);
                if (abilityData.IsActiveAbility)
                {
                    activeAbilitiesCount++;
                } else
                {
                    passiveAbilitiesCount++;
                }
            }

            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                var abilityData = abilitiesDatabase.GetAbility(i);
                
                //该关卡没有解锁这个能力
                if(!CheckUnlockAbility(abilityData)) continue;
                
                //这种能力只有在没有其他能力时才会出现。
                //通常是某种治愈或黄金
                if (abilityData.IsEndgameAbility) continue;

                //这种能力已经达到了极限。无法进一步升级
                if (save.GetAbilityLevel(abilityData.AbilityType) >= abilityData.LevelsCount - 1) continue;

                //能力已经进化
                if (removedAbilities.Contains(abilityData.AbilityType)) continue;

                if (abilityData.IsEvolution)
                {
                    // 如果能力是一种进化，我们会检查它的要求是否得到满足
                    bool fulfilled = true;
                    for (int j = 0; j < abilityData.EvolutionRequirements.Count; j++)
                    {
                        var evolutionRequirements = abilityData.EvolutionRequirements[j];
                        var isRequiredAbilityAquired = IsAbilityAquired(evolutionRequirements.AbilityType);
                        var requiredAbilityReachedLevel = save.GetAbilityLevel(evolutionRequirements.AbilityType) >= evolutionRequirements.RequiredAbilityLevel;

                        if (!isRequiredAbilityAquired || !requiredAbilityReachedLevel)
                        {
                            // Found a requirement that is not fulfilled

                            fulfilled = false;
                            break;
                        }
                    }

                    if (!fulfilled) continue;
                } else
                {
                    var isAbilityAquired = IsAbilityAquired(abilityData.AbilityType);

                    //玩家一次只能拥有一种武器技能
                    if (abilityData.IsWeaponAbility && !isAbilityAquired) continue;

                    //没有剩余的可用活动能力插槽
                    if (abilityData.IsActiveAbility && activeAbilitiesCount >= abilitiesDatabase.ActiveAbilitiesCapacity && !isAbilityAquired) continue;

                    //没有剩余的可用被动能力插槽
                    if (!abilityData.IsActiveAbility && passiveAbilitiesCount >= abilitiesDatabase.PassiveAbilitiesCapacity && !isAbilityAquired) continue;
                }

                result.Add(abilityData);
            }

            if (result.Count == 0)
            {
                // There are no more abilities left, time for endgame abilities :)

                for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
                {
                    var abilityData = abilitiesDatabase.GetAbility(i);

                    if (abilityData.IsEndgameAbility)
                    {
                        result.Add(abilityData);
                    }
                }
            }

            return result;
        }

        public virtual int GetAbilityLevel(AbilityType abilityType)
        {
            return save.GetAbilityLevel(abilityType);
        }

        public virtual IAbilityBehavior GetAquiredAbility(AbilityType abilityType)
        {
            for (int i = 0; i < aquiredAbilities.Count; i++)
            {
                if (aquiredAbilities[i].AbilityType == abilityType) return aquiredAbilities[i];
            }

            return null;
        }

        public virtual bool IsAbilityAquired(AbilityType ability)
        {
            for (int i = 0; i < aquiredAbilities.Count; i++)
            {
                if (aquiredAbilities[i].AbilityType == ability) return true;
            }

            return false;
        }

        public virtual bool IsRequiredForAquiredEvolution(AbilityType abilityType)
        {
            // Looking through every aquired ability to find an ability that can evolve and has this ability type in requirements
            foreach(var ability in aquiredAbilities)
            {
                if (!ability.AbilityData.IsActiveAbility || ability.AbilityData.IsEvolution) continue;

                foreach(var requirement in ability.AbilityData.EvolutionRequirements)
                {
                    if (requirement.AbilityType == abilityType) return true;
                }
            }

            return false;
        }

        public virtual bool HasEvolution(AbilityType abilityType, out AbilityType otherRequiredAbilityType)
        {
            // We wokring under the assumption that each evolution requires one active and one passive ability.
            // If this is not the case than the logic will break
            // There will be no errors but the ui ability cards won't show correct info regarding evolution

            otherRequiredAbilityType = abilityType;

            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                // Going through every active evolution ability in the database, checking for requirements

                var ability = abilitiesDatabase.GetAbility(i);

                if (!ability.IsEvolution) continue;

                for(int j = 0; j < ability.EvolutionRequirements.Count; j++)
                {
                    var requirement = ability.EvolutionRequirements[j];

                    if(requirement.AbilityType == abilityType)
                    {
                        for(int k = 0; k < ability.EvolutionRequirements.Count; k++)
                        {
                            if (k == j) continue;

                            otherRequiredAbilityType = ability.EvolutionRequirements[k].AbilityType;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual bool HasAvailableAbilities()
        {
            var abilities = GetAvailableAbilities();

            return abilities.Count > 0;
        }

        public virtual AbilityData GetAbilityData(AbilityType abilityType)
        {
            return abilitiesDatabase.GetAbility(abilityType);
        }

        public virtual List<AbilityType> GetAquiredAbilityTypes()
        {
            // Simply going through evert aquired abilities and getting their types

            var result = new List<AbilityType>();

            for(int i = 0; i < aquiredAbilities.Count; i++)
            {
                result.Add(aquiredAbilities[i].AbilityType);
            }

            return result;
        }

        public virtual void ShowChest()
        {
            Time.timeScale = 0;

            // Collecting every ability and numbers of its levels that are still available. 
            // For example, if ability has 5 levels, and it's on level 3 (counting from 0), the number will be (5 - 3 - 1) = 1
            // It works with not aquired abilities as well. (5 - -1 - 1) = 5

            var availableAbilities = GetAvailableAbilities();
            var dictionary = new Dictionary<AbilityData, int>();

            // Populating Dictionary <Ability, LevelsLeft>
            // Also we're counting how many upgrades to the abilities there still can be in the game

            var counter = 0;
            foreach (var ability in availableAbilities)
            {
                int levelsLeft = ability.LevelsCount - save.GetAbilityLevel(ability.AbilityType) - 1;
                dictionary.Add(ability, levelsLeft);

                counter += levelsLeft;
            }

            // We've got 3 tiers of chests, with 5, 3, or 1 abilities
            // We're randomly selecting the tier, but minding the counter from above
            // We won't be able to show the best tier chest with 5 abilities is we only have 4 upgrades left

            var selectedAbilitiesCount = 1;
            var tierId = 0;
            if(counter >= 5 && Random.value < chestChanceTier5)
            {
                selectedAbilitiesCount = 5;
                tierId = 2;
            } else if(counter >= 3 && Random.value < chestChanceTier3)
            {
                selectedAbilitiesCount = 3;
                tierId = 1;
            }

            int activeAbilitiesCount = GetActiveAbilitiesCount();
            int passiveAbilitiesCount = GetPassiveAbilitiesCount();

            // Randomly selecting abilities
            var selectedAbilities = new List<AbilityData>();
            for (int i = 0; i < selectedAbilitiesCount; i++)
            {
                // Getting random ability from dictionary
                var abilityPair = dictionary.Random();
                var ability = abilityPair.Key;

                dictionary[ability] -= 1;
                if (dictionary[ability] <= 0) dictionary.Remove(ability);

                // There is a possibility that we are reached the available capacity with this one ability that we have selected.
                // If the ability has been selected already or an evolution than it's ok 
                if (!selectedAbilities.Contains(ability) && !ability.IsEvolution)
                {
                    selectedAbilities.Add(ability);

                    // Only checking the new abilities
                    if (!IsAbilityAquired(ability.AbilityType))
                    {
                        var abilitiesToRemove = new List<AbilityData>();

                        if (ability.IsActiveAbility)
                        {
                            // There is a new active ability
                            activeAbilitiesCount++;

                            // We've reached the capacity for the active abilities
                            if(activeAbilitiesCount == ActiveAbilitiesCapacity)
                            {
                                foreach(var savedAbility in dictionary.Keys)
                                {
                                    // This one ability is no longer available for us to win from the chest
                                    if(savedAbility.IsActiveAbility && !IsAbilityAquired(savedAbility.AbilityType) && !selectedAbilities.Contains(savedAbility))
                                    {
                                        abilitiesToRemove.Add(savedAbility);
                                    }
                                }
                            }
                        } else
                        {
                            // There is a new passive ability
                            passiveAbilitiesCount++;

                            // We've reached the capacity for the active abilities
                            if (passiveAbilitiesCount == PassiveAbilitiesCapacity)
                            {
                                foreach (var savedAbility in dictionary.Keys)
                                {
                                    // This one ability is no longer available for us to win from the chest
                                    if (!savedAbility.IsActiveAbility && !IsAbilityAquired(savedAbility.AbilityType) && !selectedAbilities.Contains(savedAbility))
                                    {
                                        abilitiesToRemove.Add(savedAbility);
                                    }
                                }
                            }
                        }

                        foreach(var abilityToRemove in abilitiesToRemove)
                        {
                            dictionary.Remove(abilityToRemove);
                        }
                    }
                } else
                {
                    selectedAbilities.Add(ability);
                }

                if (dictionary.Count == 0) break;
            }

            // We might have removed some abilities in the previous step and there might not be enough abilities for selected chest tier
            while(selectedAbilities.Count < selectedAbilitiesCount)
            {
                tierId--;
                selectedAbilitiesCount -= 2;

                for(int i = selectedAbilitiesCount; i < selectedAbilities.Count; i++)
                {
                    selectedAbilities.RemoveAt(i);
                    i--;
                }
            }
            StageController.GameScreen.ShowChestWindow(tierId, availableAbilities, selectedAbilities);

            // Applying abilities
            foreach (var ability in selectedAbilities)
            {
                if (IsAbilityAquired(ability.AbilityType))
                {
                    var level = save.GetAbilityLevel(ability.AbilityType);

                    if (!ability.IsEndgameAbility) level++;

                    if (level < 0) level = 0;

                    save.SetAbilityLevel(ability.AbilityType, level);

                    ability.Upgrade(level);
                }
                else
                {
                    AddAbility(ability);
                }
            }
        }

#if UNITY_EDITOR

        public virtual List<AbilityData> GetAllAbilitiesDev()
        {
            var abilities = new List<AbilityData>();

            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                abilities.Add(abilitiesDatabase.GetAbility(i));
            }

            return abilities;
        }

        public virtual int GetAbilityLevelDev(AbilityType type)
        {
            return save.GetAbilityLevel(type);
        }

        public virtual void RemoveAbilityDev(AbilityData abilityData)
        {
            for (int i = 0; i < aquiredAbilities.Count; i++)
            {
                var ability = aquiredAbilities[i];

                if (ability.AbilityData == abilityData)
                {
                    ability.Clear();

                    aquiredAbilities.RemoveAt(i);

                    save.RemoveAbility(abilityData.AbilityType);

                    break;
                }
            }
        }

        public virtual void DecreaseAbilityLevelDev(AbilityData abilityData)
        {
            var level = save.GetAbilityLevel(abilityData.AbilityType);
            if (level > 0)
            {
                save.SetAbilityLevel(abilityData.AbilityType, level - 1);

                for (int i = 0; i < aquiredAbilities.Count; i++)
                {
                    var ability = aquiredAbilities[i];

                    if (ability.AbilityData == abilityData)
                    {
                        abilityData.Upgrade(level - 1);

                        break;
                    }
                }
            }
        }

        public virtual void IncreaseAbilityLevelDev(AbilityData abilityData)
        {
            var level = save.GetAbilityLevel(abilityData.AbilityType);
            if (level < abilityData.LevelsCount - 1)
            {
                save.SetAbilityLevel(abilityData.AbilityType, level + 1);

                for (int i = 0; i < aquiredAbilities.Count; i++)
                {
                    var ability = aquiredAbilities[i];

                    if (ability.AbilityData == abilityData)
                    {
                        abilityData.Upgrade(level + 1);

                        break;
                    }
                }
            }
        }

#endif
    }

    [System.Serializable]
    public class AbilityDev
    {
        public AbilityType abilityType;
        public int level;
    }

    public class WeightedAbility
    {
        public AbilityData abilityData;
        public float weight;
    }
}