using OctoberStudio.Abilities;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Input;
using System.Collections.Generic;
using GameScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class CharactersWindowBehavior : PanelBase
    {
        [SerializeField] CharactersDatabase database;
        [SerializeField] AbilitiesDatabase abilitiesDatabase;
        [SerializeField] GameObject itemPrefab;
        [SerializeField] RectTransform itemsParent;
        [SerializeField] ScrollRect scrollView;
        [SerializeField] Button backButton;

        private List<CharacterItemBehavior> items = null;
        protected override void OnAwake()
        {
            base.OnAwake();
            backButton.SetButtonClick(() =>
            {
                FormMain.Instance.ShowPanel(MainPanelType.lobbyWindow);
            });
        }
        
        public void Init()
        {
            if (items == null)
            {
                items = new List<CharacterItemBehavior>();
                for (int i = 0; i < database.CharactersCount; i++)
                {
                    var item = Instantiate(itemPrefab, itemsParent).GetComponent<CharacterItemBehavior>();
                    item.transform.ResetLocal();

                    item.Init(i, database.GetCharacterData(i), abilitiesDatabase);
                    item.onNavigationSelected += OnItemSelected;

                    items.Add(item);
                }
            }
            if (items.Count > 0)
            {
                items[0].Select();
            }
        }
        
        protected override void OnShow()
        {
            base.OnShow();
            Init();
        }


        public void OnItemSelected(CharacterItemBehavior selectedItem)
        {
            var objPosition = (Vector2)scrollView.transform.InverseTransformPoint(selectedItem.Rect.position);
            var scrollHeight = scrollView.GetComponent<RectTransform>().rect.height;
            var objHeight = selectedItem.Rect.rect.height;

            if (objPosition.y > scrollHeight / 2)
            {
                scrollView.content.localPosition = new Vector2(scrollView.content.localPosition.x,
                    scrollView.content.localPosition.y - objHeight - 37);
            }

            if (objPosition.y < -scrollHeight / 2)
            {
                scrollView.content.localPosition = new Vector2(scrollView.content.localPosition.x,
                    scrollView.content.localPosition.y + objHeight + 37);
            }
        }

        public void Clear()
        {
            if (items == null) return;
            foreach (var t in items)
            {
                t.Clear();
            }
        }
    }
}