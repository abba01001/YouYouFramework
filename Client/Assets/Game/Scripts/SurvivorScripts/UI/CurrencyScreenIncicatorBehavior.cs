using GameScripts;
using Main;
using OctoberStudio.Save;
using OctoberStudio.UI;
using UnityEngine;

namespace OctoberStudio.Currency
{
    public class CurrencyScreenIncicatorBehavior : ScalingLabelBehavior
    {
        private void Start()
        {
            SetAmount(GameEntry.Data.GetProps((int)PropEnum.Coin));
            icon.sprite = GameController.CurrenciesManager.GetIcon(SaveKey.GoldData);
            GameEntry.Event.AddEventListener(Constants.EventName.PropsChangedEvent,HandleCoinAmountChanged);
        }

        private void HandleCoinAmountChanged(object userdata)
        {
            PropChangeModel model = (PropChangeModel)userdata;
            if (model.PropType == PropEnum.Coin)
            {
                SetAmount(model.PropValue);
            }
        }

        private void OnDestroy()
        {
            GameEntry.Event.RemoveEventListener(Constants.EventName.PropsChangedEvent,HandleCoinAmountChanged);
        }
    }
}