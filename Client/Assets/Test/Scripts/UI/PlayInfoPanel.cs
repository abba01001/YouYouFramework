using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayInfoPanel : MonoBehaviour
{
    public SetPanel setPanel;
    public Text nameText;
    public Text levelText;

    // Start is called before the first frame update
    void Start()
    {
        if (GameData.playerInfo == null)
        {
            GameData.playerInfo = new PlayerInfo();
            GameData.playerInfo.level = 1;
            GameData.playerInfo.playerID = 100000;//默认ID
            GameData.playerInfo.name = "游客";
            levelText.text = GameData.playerInfo.level.ToString();
            nameText.text = GameData.playerInfo.name;
            setPanel.gameObject.SetActive(true);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateNameText(string name)
    {
        nameText.text = name;
    }
}
