using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPanel : MonoBehaviour
{
    public Button button;
    public InputField inputField;
    public PlayInfoPanel playInfoPanel;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            string name = inputField.text;
            if(name == null || name == "")
            {
                return;
            }
            else
            {
                GameData.playerInfo.name = name;
                playInfoPanel.UpdateNameText(name);
                inputField.text = "";
                gameObject.SetActive(false);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
