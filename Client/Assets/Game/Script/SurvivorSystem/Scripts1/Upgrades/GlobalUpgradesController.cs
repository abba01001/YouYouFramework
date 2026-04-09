using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Watermelon
{
    using GlobalUpgrades;

    public class GlobalUpgradesController : MonoBehaviour
    {
        private static GlobalUpgradesController _instance;
        public static GlobalUpgradesController Instance => _instance ??= new GlobalUpgradesController();
        private const string SAVE_IDENTIFIER = "upgrades:{0}";

        GlobalUpgradesDatabase upgradesDatabase;

        private  List<AbstactGlobalUpgrade> activeUpgrades;
        public  List<AbstactGlobalUpgrade> ActiveUpgrades => activeUpgrades;

        private  Dictionary<GlobalUpgradeType, AbstactGlobalUpgrade> activeUpgradesLink;

        private  List<IUpgrade> globalSimpleUpgrades = new List<IUpgrade>();

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

        public async UniTask OpenMainUpgradesPage()
        {
            FormUpgrade form = await GameEntry.UI.OpenUIForm<FormUpgrade>();
            form.ResetUpgrades();
            form.RegisterUpgrades(ActiveUpgrades.ConvertAll(upgrade => (IUpgrade)upgrade));
            form.RegisterUpgrades(globalSimpleUpgrades);
            form.PlayShowAnimation();
        }

        public async UniTask OpenUpgradesPage(List<IUpgrade> upgradesToOpen)
        {
            FormUpgrade form = await GameEntry.UI.OpenUIForm<FormUpgrade>();
            form.ResetUpgrades();
            form.RegisterUpgrades(upgradesToOpen);
            form.PlayShowAnimation();
        }
    }
}