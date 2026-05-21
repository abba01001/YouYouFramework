using System;
using GameScripts;
using OctoberStudio.Abilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AudioManager = OctoberStudio.Audio.AudioManager;

namespace OctoberStudio.UI
{
    public class CharacterItemBehavior : MonoBehaviour
    {
        [SerializeField] RectTransform rect;
        public RectTransform Rect => rect;

        [Header("Info")]
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] GameObject startingAbilityObject;
        [SerializeField] Image startingAbilityImage;

        [Header("Button")]
        [SerializeField] Button upgradeButton;
        [SerializeField] Sprite enabledButtonSprite;
        [SerializeField] Sprite disabledButtonSprite;
        [SerializeField] Sprite selectedButtonSprite;

        [Header("Stats")]
        [SerializeField] TMP_Text hpText;
        [SerializeField] TMP_Text damageText;

        [Space]
        [SerializeField] ScalingLabelBehavior costLabel;
        [SerializeField] TMP_Text buttonText;

        public Selectable Selectable => upgradeButton;

        public CharacterData Data { get; private set; }
        public int CharacterId { get; private set; }

        public bool IsSelected { get; private set; }

        public UnityAction<CharacterItemBehavior> onNavigationSelected;

        private void Start()
        {
            upgradeButton.onClick.AddListener(SelectButtonClick);
        }

        public void Init(int id, CharacterData characterData, AbilitiesDatabase database)
        {
            GameEntry.Event.AddEventListener(Constants.EventName.SelectedCharacterEvent,RedrawVisuals);
            GameEntry.Event.AddEventListener(Constants.EventName.PropsChangedEvent,HandleCoinAmountChanged);
            startingAbilityObject.SetActive(characterData.HasStartingAbility);

            if(characterData.HasStartingAbility)
            {
                var abilityData = database.GetAbility(characterData.StartingAbility);
                startingAbilityImage.sprite = abilityData.Icon;
            }

            Data = characterData;
            CharacterId = id;

            RedrawVisuals(null);
        }

        private void RedrawVisuals(object userdata)
        {
            titleLabel.text = Data.Name;
            iconImage.sprite = Data.Icon;

            hpText.text = Data.BaseHP.ToString();
            damageText.text = Data.BaseDamage.ToString();

            RedrawButton();
        }
        
        private void HandleCoinAmountChanged(object userdata)
        {
            RedrawButton();
        }

        private void RedrawButton()
        {
            if (GameEntry.Data.PlayerRoleData.HasCharacterBeenBought(CharacterId))
            {
                costLabel.gameObject.SetActive(false);
                buttonText.gameObject.SetActive(true);

                if(GameEntry.Data.PlayerRoleData.selectedCharacterId == CharacterId)
                {
                    upgradeButton.interactable = false;
                    upgradeButton.image.sprite = selectedButtonSprite;

                    buttonText.text = "SELECTED";

                } else
                {
                    upgradeButton.interactable = true;
                    upgradeButton.image.sprite = enabledButtonSprite;

                    buttonText.text = "SELECT";
                }
            }
            else
            {
                costLabel.gameObject.SetActive(true);
                buttonText.gameObject.SetActive(false);

                costLabel.SetAmount(Data.Cost);
                if (GameEntry.Data.PlayerRoleData.GetProps((int)PropEnum.Coin) >= Data.Cost)
                {
                    upgradeButton.interactable = true;
                    upgradeButton.image.sprite = enabledButtonSprite;
                }
                else
                {
                    upgradeButton.interactable = false;
                    upgradeButton.image.sprite = disabledButtonSprite;
                }
            }
        }

        private void SelectButtonClick()
        {
            if (!GameEntry.Data.PlayerRoleData.HasCharacterBeenBought(CharacterId))
            {
                GameEntry.Data.PlayerRoleData.DelProp((int)PropEnum.Coin,Data.Cost);
                GameEntry.Data.PlayerRoleData.AddBoughtCharacter(CharacterId);
            }

            GameEntry.Data.PlayerRoleData.SetSelectedCharacterId(CharacterId);

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            EventSystem.current.SetSelectedGameObject(upgradeButton.gameObject);
        }

        private void OnGoldAmountChanged(int amount)
        {
            RedrawButton();
        }

        public void Select()
        {
            EventSystem.current.SetSelectedGameObject(upgradeButton.gameObject);
        }

        public void Unselect()
        {
            IsSelected = false;
        }

        private void Update()
        {
            if(!IsSelected && EventSystem.current.currentSelectedGameObject == upgradeButton.gameObject)
            {
                IsSelected = true;

                onNavigationSelected?.Invoke(this);
            } 
            else if(IsSelected && EventSystem.current.currentSelectedGameObject != upgradeButton.gameObject)
            {
                IsSelected = false;
            } 
        }

        public void Clear()
        {
            GameEntry.Event.RemoveEventListener(Constants.EventName.PropsChangedEvent,HandleCoinAmountChanged);
            GameEntry.Event.RemoveEventListener(Constants.EventName.SelectedCharacterEvent,RedrawVisuals);
        }
    }
}