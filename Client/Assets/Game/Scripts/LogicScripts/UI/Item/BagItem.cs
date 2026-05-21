
using UniRx;
using UnityEngine.UI;

namespace GameScripts
{
    public class BagItem : ScrollItem
    {
        private string TexName = null;
        private Image iconImage;
        private void Awake()
        {
            iconImage = transform.Get<Image>("ItemIcon");
            transform.GetComponent<Button>().SetButtonClick(() =>
            {
                GameEntry.UI.OpenUIForm<FormItemInfo>();
            });
        }
        
        protected override void OnRefreshUI(object data, int index)
        {
            iconImage.SetSpriteByAtlas(Constants.AtlasNamePath.PartsThumbnail, data as string);
        }
    }
}