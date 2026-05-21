using System.Collections.Generic;
using DG.Tweening;
using LayerLab.ArtMaker;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class BagPanel : PanelBase
    {
        [SerializeField] private GameObject scrollItem;
        [SerializeField] private EfficientScrollRect scrollRect;
        [SerializeField] private List<Button> btns;
        [SerializeField] private RectTransform focusLine;
        protected override void OnShow()
        {
            base.OnShow();
            StartInitItem();
            for (int i = 0; i < btns.Count; i++)
            {
                int index = i;
                btns[index].GetComponent<Button>().SetButtonClick(() =>
                {
                    ChangeBtn(index);
                });
            }
        }

        private void ChangeBtn(int index)
        {
            var targetX = index * btns[0].GetComponent<RectTransform>().sizeDelta.x;
            focusLine.anchoredPosition = new Vector2(targetX, focusLine.anchoredPosition.y);
            scrollRect.UpdateDatas(GetData(index),false);
        }
        
        void StartInitItem()
        {
            scrollItem.MSetActive(false);
            scrollRect.SetSpace(14,25);
            scrollRect.Init(scrollItem, GetData(0));
        }
        
        object[] GetData(int type)
        {
            List<string> list = new List<string>();

            if (type == 0)
            {
                Dictionary<PartsType, List<string>> dic = GameEntry.DataTable.Sys_EquipmentDBModel.GetPartsInfo();
                foreach (var kv in dic)
                {
                    foreach (var s in kv.Value)
                    {
                        list.Add(s);
                    }
                }
            }
            
            object[] data = new object[list.Count];
            int index = 0;
            foreach (var VARIABLE in list)
            {
                data[index] = VARIABLE;
                index++;
            }
            return data;
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}