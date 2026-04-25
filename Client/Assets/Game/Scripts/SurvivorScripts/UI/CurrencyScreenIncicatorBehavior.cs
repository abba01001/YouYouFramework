using Main;
using OctoberStudio.Save;
using OctoberStudio.UI;
using UnityEngine;

namespace OctoberStudio.Currency
{
    public class CurrencyScreenIncicatorBehavior : ScalingLabelBehavior
    {
        public CurrencySave Currency { get; private set; }

        private void Start()
        {
            Currency = GameController.SaveManager.GoldData;
            SetAmount(Currency.Amount);
            icon.sprite = GameController.CurrenciesManager.GetIcon(SaveKey.GoldData);
            Currency.onGoldAmountChanged += SetAmount;
        }

        private void OnDestroy()
        {
            Currency.onGoldAmountChanged -= SetAmount;
        }
    }
}