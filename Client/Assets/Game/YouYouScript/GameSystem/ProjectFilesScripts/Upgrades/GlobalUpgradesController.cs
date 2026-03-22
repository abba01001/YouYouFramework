using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Watermelon
{
    using GlobalUpgrades;

    public class GlobalUpgradesController
    {
        private static GlobalUpgradesController _instance;
        public static GlobalUpgradesController Instance => _instance ??= new GlobalUpgradesController();
        private const string SAVE_IDENTIFIER = "upgrades:{0}";

        GlobalUpgradesDatabase upgradesDatabase;

        private  List<AbstactGlobalUpgrade> activeUpgrades;
        public  List<AbstactGlobalUpgrade> ActiveUpgrades => activeUpgrades;

        private  Dictionary<GlobalUpgradeType, AbstactGlobalUpgrade> activeUpgradesLink;

        private  List<IUpgrade> globalSimpleUpgrades = new List<IUpgrade>();

        private  UIUpgrades uiUpgrades;

        public async UniTask Initialise()
        {
            upgradesDatabase = await GameEntry.Loader.LoadMainAssetAsync<GlobalUpgradesDatabase>("Assets/Game/Download/ProjectFiles/Data/Upgrades/Global Upgrades Database.asset", GameEntry.Instance.gameObject);
            activeUpgrades = new List<AbstactGlobalUpgrade>(upgradesDatabase.Upgrades);
            
            activeUpgradesLink = new Dictionary<GlobalUpgradeType, AbstactGlobalUpgrade>();
            for (int i = 0; i < activeUpgrades.Count; i++)
            {
                var upgrade = activeUpgrades[i];

                var hash = string.Format(SAVE_IDENTIFIER, upgrade.GlobalUpgradeType.ToString()).GetHashCode();

                UpgradeSavableObject save = SaveController.GetSaveObject<UpgradeSavableObject>(hash); ;

                upgrade.SetSave(save);

                if (!activeUpgradesLink.ContainsKey(upgrade.GlobalUpgradeType))
                {
                    upgrade.Initialise();

                    activeUpgradesLink.Add(upgrade.GlobalUpgradeType, activeUpgrades[i]);
                }
            }

            uiUpgrades = UIController.GetPage<UIUpgrades>();

            await UniTask.NextFrame();
        }

        [System.Obsolete]
        public  AbstactGlobalUpgrade GetUpgradeByType(GlobalUpgradeType perkType)
        {
            if (activeUpgradesLink.ContainsKey(perkType))
                return activeUpgradesLink[perkType];

            Debug.LogError($"[Perks]: Upgrade with type {perkType} isn't registered!");

            return null;
        }

        public  T GetUpgrade<T>(GlobalUpgradeType type) where T : AbstactGlobalUpgrade
        {
            if (activeUpgradesLink.ContainsKey(type))
                return activeUpgradesLink[type] as T;

            Debug.LogError($"[Perks]: Upgrade with type {type} isn't registered!");

            return null;
        }

        public  void RegisterSimpleUpgrade(IUpgrade upgrade)
        {
            globalSimpleUpgrades.Add(upgrade);
        }

        public  void OpenMainUpgradesPage()
        {
            uiUpgrades.ResetUpgrades();
            uiUpgrades.RegisterUpgrades(ActiveUpgrades.ConvertAll(upgrade => (IUpgrade)upgrade));
            uiUpgrades.RegisterUpgrades(globalSimpleUpgrades);

            UIController.ShowPage<UIUpgrades>();
        }

        public  void OpenUpgradesPage(List<IUpgrade> upgradesToOpen)
        {
            uiUpgrades.ResetUpgrades();
            uiUpgrades.RegisterUpgrades(upgradesToOpen);

            UIController.ShowPage<UIUpgrades>();
        }
    }
}