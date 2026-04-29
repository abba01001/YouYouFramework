using System;
using Main;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class ToggleComponent : MonoBehaviour
    {
        [SerializeField] private Button onBtn;
        [SerializeField] private Button offBtn;

        public Action TriggerOnEvent;
        public Action TriggerOffEvent;

        private void OnEnable()
        {
            onBtn.SetButtonClick(() =>
            {
                TriggerOffEvent?.Invoke();
                SetState(false);
            });
            offBtn.SetButtonClick(() =>
            {
                TriggerOnEvent?.Invoke();
                SetState(true);
            });
        }
        
        public void SetState(bool isOn)
        {
            onBtn.transform.parent.gameObject.MSetActive(isOn);
            offBtn.transform.parent.gameObject.MSetActive(!isOn);
        }
    }
}