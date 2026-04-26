using UnityEngine;

namespace OctoberStudio.Upgrades
{
    public class UpgradesManager : MonoBehaviour
    {
        private static UpgradesManager instance;

        [SerializeField] UpgradesDatabase database;

        private UpgradesSave save;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);

                return;
            }

            instance = this;

            DontDestroyOnLoad(this);

            save = GameController.SaveManager.UpgradesData;
            save.Init();

            for(int i = 0; i < database.UpgradesCount; i++)
            {
                var upgrade = database.GetUpgrade(i);

                if(GetUpgradeLevel(upgrade.UpgradeType) < upgrade.DevStartLevel)
                {
                    save.SetUpgradeLevel(upgrade.UpgradeType, upgrade.DevStartLevel);
                }
            }
        }

        public void IncrementUpgradeLevel(UpgradeType upgradeType)
        {
            var level = save.GetUpgradeLevel(upgradeType);
            save.SetUpgradeLevel(upgradeType, level + 1);
        }

        public int GetUpgradeLevel(UpgradeType upgradeType)
        {
            return save.GetUpgradeLevel(upgradeType);
        }

        public bool IsUpgradeAquired(UpgradeType upgradeType)
        {
            var level = save.GetUpgradeLevel(upgradeType);

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