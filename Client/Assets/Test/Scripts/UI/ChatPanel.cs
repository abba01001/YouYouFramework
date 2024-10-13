using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{
    public Button sendBtn;
    public InputField inputField;
    public ScrollRect sr;

    // Start is called before the first frame update
    void Start()
    {
        sendBtn.onClick.AddListener(SendMsg);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendMsg();
        }
    }

    void SendMsg()
    {
        if (inputField.text != "")
        {
            ChatMsg chatMsg = new ChatMsg();
            chatMsg.playerInfo = GameData.playerInfo;
            chatMsg.chatStr = inputField.text;
            NetManager.Instance.Send<ChatMsg>(chatMsg);
            UpdateChatInfo(chatMsg);
            inputField.text = "";
        }
    }

    public void UpdateChatInfo(ChatMsg msgInfo)
    {
        Text chatInfoText = Instantiate(Resources.Load<Text>("UI/MsgInfoText"), sr.content);
        chatInfoText.text = msgInfo.playerInfo.name + "<" + msgInfo.playerInfo.level + "级>: " + msgInfo.chatStr;
    }
}
