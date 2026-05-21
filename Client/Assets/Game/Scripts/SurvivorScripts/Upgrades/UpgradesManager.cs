using GameScripts;
using UnityEngine;

namespace OctoberStudio.Upgrades
{
    public class UpgradesManager : MonoBehaviour
    {
        private static UpgradesManager instance;

        [SerializeField] UpgradesDatabase database;


        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);

                return;
            }

            instance = this;

            DontDestroyOnLoad(this);

            for(int i = 0; i < database.UpgradesCount; i++)
            {
                var upgrade = database.GetUpgrade(i);

                if(GetUpgradeLevel(upgrade.UpgradeType) < upgrade.DevStartLevel)
                {
                    GameEntry.Data.PlayerRoleData.SetUpgradeLevel((int)upgrade.UpgradeType, upgrade.DevStartLevel);
                }
            }
        }

        public void IncrementUpgradeLevel(UpgradeType upgradeType)
        {
            int upType = (int)upgradeType;
            var level = GameEntry.Data.PlayerRoleData.GetUpgradeLevel(upType);
            GameEntry.Data.PlayerRoleData.SetUpgradeLevel(upType, level + 1);
        }

        public int GetUpgradeLevel(UpgradeType upgradeType)
        {
            int upType = (int)upgradeType;
            return GameEntry.Data.PlayerRoleData.GetUpgradeLevel(upType);
        }

        public bool IsUpgradeAquired(UpgradeType upgradeType)
        {
            int upType = (int)upgradeType;
            var level = GameEntry.Data.PlayerRoleData.GetUpgradeLevel(upType);
            return level != -1;
        }

        public UpgradeData GetUpgradeData(UpgradeType upgradeType)
        {
            return database.GetUpgrade(upgradeType);
        }

        public float GetUpgadeValue(UpgradeType upgradeType,ValueType valueType)
        {
            var data = GetUpgradeData(upgradeType);
            var level = GetUpgradeLevel(upgradeType);

            if(level >= 0)
            {
                switch (valueType)
                {
                    case ValueType.AddValue:
                        return data.GetLevel(level).AddValue;
                    case ValueType.MultiplyValue:
                        return data.GetLevel(level).MultiplyValue;
                }
            }
            return 0;
        }
    }
}