using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SkinsHandler
    {
        [SerializeField] AbstractSkinDatabase[] skinProviders;
        public AbstractSkinDatabase[] SkinsProviders => skinProviders;

        public int ProvidersCount => skinProviders.Length;

        public AbstractSkinDatabase GetSkinsProvider(int index)
        {
            return skinProviders[index];
        }

        public async UniTask RefreshSkinProviderCache()
        {
            AbstractSkinDatabase t = await GameEntry.Loader.LoadMainAssetAsync<AbstractSkinDatabase>("Assets/Game/Download/ProjectFiles/Data/Player Skins Database.asset", GameEntry.Instance.gameObject);
            skinProviders = new[] {t};
        }
        
        public AbstractSkinDatabase GetSkinsProvider(System.Type providerType)
        {
            if (!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinDatabase skinProvider in skinProviders)
                {
                    if (skinProvider.GetType() == providerType)
                        return skinProvider;
                }
            }

            return null;
        }

        public bool HasSkinsProvider(System.Type providerType)
        {
            if (!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinDatabase skinProvider in skinProviders)
                {
                    if (skinProvider.GetType() == providerType)
                        return true;
                }
            }

            return false;
        }

        public bool HasSkinsProvider(AbstractSkinDatabase provider)
        {
            if(!skinProviders.IsNullOrEmpty())
            {
                foreach (AbstractSkinDatabase skinProvider in skinProviders)
                {
                    if (skinProvider == provider)
                        return true;
                }
            }

            return false;
        }
    }
}
