using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using OctoberStudio;
using UnityEngine;

namespace GameScripts
{
    [Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class StageSaveData
    {
        #region Fields with Interceptors
        private int _maxReachedStageId;
        public int maxReachedStageId { get => _maxReachedStageId; set { if (_maxReachedStageId != value) { _maxReachedStageId = value; SetDirty(); } } }

        private int _selectedStageId;
        public int selectedStageId { get => _selectedStageId; set { if (_selectedStageId != value) { _selectedStageId = value; SetDirty(); } } }

        private bool _isPlaying;
        public bool isPlaying { get => _isPlaying; set { if (_isPlaying != value) { _isPlaying = value; SetDirty(); } } }

        private float _time;
        public float time { get => _time; set { if (!Mathf.Approximately(_time, value)) { _time = value; SetDirty(); } } }

        private bool _resetAbilities;
        public bool resetAbilities { get => _resetAbilities; set { if (_resetAbilities != value) { _resetAbilities = value; SetDirty(); } } }

        private int _xpLevel;
        public int xpLevel { get => _xpLevel; set { if (_xpLevel != value) { _xpLevel = value; SetDirty(); } } }

        private float _xp;
        public float xp { get => _xp; set { if (!Mathf.Approximately(_xp, value)) { _xp = value; SetDirty(); } } }

        private int _enemiesKilled;
        public int enemiesKilled { get => _enemiesKilled; set { if (_enemiesKilled != value) { _enemiesKilled = value; SetDirty(); } } }

        public Dictionary<int, int> abilitiesLevels;
        #endregion

        public StageSaveData()
        {
            abilitiesLevels = new Dictionary<int, int>();
        }

        private void SetDirty() => DataManager.IsDataDirty = true;

        #region Original Methods
        public void ClearAbilitiesLevels()
        {
            if (abilitiesLevels.Count > 0)
            {
                abilitiesLevels.Clear();
                SetDirty();
            }
        }
        
        public List<int> GetSavedAbilities()
        {
            return abilitiesLevels.Keys.ToList();
        }

        public int GetAbilityLevel(int abilityType)
        {
            return abilitiesLevels.GetValueOrDefault(abilityType, -1);
        }
        
        public int GetAbilityLevel(AbilityType abilityType)
        {
            int type = (int)abilityType;
            return abilitiesLevels.GetValueOrDefault(type, -1);
        }
              
        public void SetAbilityLevel(AbilityType abilityType, int level)
        {
            int type = (int)abilityType;
            if (abilitiesLevels.GetValueOrDefault(type, -1) != level)
            {
                abilitiesLevels[type] = level;
                SetDirty();
            }
        }
        
        public void SetAbilityLevel(int abilityType, int level)
        {
            if (abilitiesLevels.GetValueOrDefault(abilityType, -1) != level)
            {
                abilitiesLevels[abilityType] = level;
                SetDirty();
            }
        }
  
        public void RemoveAbility(AbilityType abilityType)
        {
            int type = (int)abilityType;
            if (abilitiesLevels.Remove(type)) SetDirty();
        }
        
        public void RemoveAbility(int abilityType)
        {
            if (abilitiesLevels.Remove(abilityType)) SetDirty();
        }
        
        public void SetTime(float time)
        {
            this.time = time; 
        }
        
        public void SetResetAbilities(bool resetAbilities)
        {
            this.resetAbilities = resetAbilities; 
        }
        
        public void SetXpLevel(int xpLevel)
        {
            this.xpLevel = xpLevel; 
        }

        public void SetXp(float xp)
        {
            this.xp = xp; 
        }
        
        public void SetSelectedStageId(int selectedStageId)
        {
            this.selectedStageId = selectedStageId; 
            GameEntry.Event.Dispatch(Constants.EventName.SelectedStageEvent);
        }

        public void SetMaxReachedStageId(int maxReachedStageId)
        {
            this.maxReachedStageId = maxReachedStageId; 
        }
        
        public void SetStageIsPlaying(bool isPlaying)
        {
            this.isPlaying = isPlaying; 
        }
        
        public void SetStageEnemiesKilled(int enemiesKilled)
        {
            this.enemiesKilled = enemiesKilled; 
            GameEntry.Event.Dispatch(Constants.EventName.RefreshKilledEnemiesCount, enemiesKilled);
        }
        #endregion
    }
    
    [Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class PlayerRoleData
    {
        #region Fields with Interceptors
        private string _name;
        public string name { get => _name; set { if (_name != value) { _name = value; SetDirty(); } } }

        private int _selectedCharacterId;
        public int selectedCharacterId { get => _selectedCharacterId; set { if (_selectedCharacterId != value) { _selectedCharacterId = value; SetDirty(); } } }

        private float _soundVolume;
        public float soundVolume { get => _soundVolume; set { if (!Mathf.Approximately(_soundVolume, value)) { _soundVolume = value; SetDirty(); } } }

        private float _musicVolume;
        public float musicVolume { get => _musicVolume; set { if (!Mathf.Approximately(_musicVolume, value)) { _musicVolume = value; SetDirty(); } } }

        private int _curGuide;
        public int curGuide { get => _curGuide; set { if (_curGuide != value) { _curGuide = value; SetDirty(); } } }

        private bool _isVibrationEnabled;
        public bool isVibrationEnabled { get => _isVibrationEnabled; set { if (_isVibrationEnabled != value) { _isVibrationEnabled = value; SetDirty(); } } }

        public int activeInput; // 输入类型
        public Dictionary<int, int> upgradeSaves; // 升级数据
        public Dictionary<int, int> propDic; // 货币数据
        public List<int> boughtCharacterIds; // 购买的角色id
        public int totalOnlineDuration;
        public int todayOnlineDuration;
        public List<int> dialogueIds;
        public List<int> guideIds;
        public List<string> guideEvent;
        #endregion

        public PlayerRoleData()
        {
            boughtCharacterIds = new List<int>() { 0 };
            upgradeSaves = new Dictionary<int, int>();
            propDic = new Dictionary<int, int>()
            {
                {(int)PropEnum.Coin, 100},
                {(int)PropEnum.Energy, 5},
            };
            guideIds = new List<int>();
            guideEvent = new List<string>();
            dialogueIds = new List<int>();
            totalOnlineDuration = 0;
            todayOnlineDuration = 0;
            soundVolume = 0.5f;
            musicVolume = 0.5f;
            name = "";
        }

        private void SetDirty() => DataManager.IsDataDirty = true;

        #region Original Methods
        public int GetUpgradeLevel(int upgradeType)
        {
            return this.upgradeSaves.GetValueOrDefault(upgradeType, -1);
        }

        public void SetUpgradeLevel(int upgradeType, int level)
        {
            if (this.upgradeSaves.GetValueOrDefault(upgradeType, -1) != level)
            {
                this.upgradeSaves[upgradeType] = level;
                SetDirty();
            }
        }

        public void RemoveUpgrade(int upgradeType)
        {
            if (this.upgradeSaves.Remove(upgradeType)) SetDirty();
        }

        public bool HasCharacterBeenBought(int id)
        {
            return this.boughtCharacterIds.Contains(id);
        }

        public void AddBoughtCharacter(int id)
        {
            if (!this.boughtCharacterIds.Contains(id))
            {
                this.boughtCharacterIds.Add(id);
                SetDirty();
            }
        }

        public void SetSelectedCharacterId(int id)
        {
            this.selectedCharacterId = id; 
            GameEntry.Event.Dispatch(Constants.EventName.SelectedCharacterEvent);
        }
        
        public int GetProps(int prop_id)
        {
            this.propDic.TryGetValue(prop_id, out var value);
            return value;
        }

        public bool CanAfford(int prop_id, int value)
        {
            int hasValue = GetProps(prop_id);
            return hasValue >= value;
        }

        public void AddProp(int prop_id, int value)
        {
            if (value == 0) return;
            if (!this.propDic.TryAdd(prop_id, value))
            {
                this.propDic[prop_id] += value;
            }
            SetDirty();
            OnPropChanged(prop_id);
        }

        public void DelProp(int prop_id, int value)
        {
            if (this.propDic.ContainsKey(prop_id))
            {
                this.propDic[prop_id] -= value;
                SetDirty();
                OnPropChanged(prop_id);
            }
        }
        
        public void DelPropAll(int prop_id)
        {
            if (this.propDic.ContainsKey(prop_id))
            {
                this.propDic[prop_id] = 0;
                SetDirty();
                OnPropChanged(prop_id);
            }
        }

        private void OnPropChanged(int prop_id)
        {
            PropChangeModel model = GameEntry.Pool.ClassObjectPool.Dequeue<PropChangeModel>();
            model.PropType = (PropEnum)prop_id;
            model.PropValue = GetProps(prop_id);
            GameEntry.Event.Dispatch(Constants.EventName.PropsChangedEvent, model);
            GameEntry.Pool.ClassObjectPool.Enqueue(model);
        }
        #endregion
    }
}