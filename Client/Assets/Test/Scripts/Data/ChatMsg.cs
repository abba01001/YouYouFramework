using System.Text;
using System;

public class ChatMsg : BaseMsg
{
    public PlayerInfo playerInfo;
    public string chatStr;

    public override int GetDataBytesLength()
    {
        return 4 + playerInfo.GetDataBytesLength() + 4 + Encoding.UTF8.GetBytes(chatStr).Length;
    }

    public override int ReadingData(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        playerInfo = ReadingCustomClass<PlayerInfo>(bytes, ref index);
        chatStr = ReadingString(bytes, ref index);
        return index - beginIndex;
    }

    public override byte[] SerializeData()
    {
        byte[] bytes = new byte[GetDataBytesLength()];

        int index = 0;

        BitConverter.GetBytes(GetId()).CopyTo(bytes, index);
        index += 4;
        
        SerializeCustomClass(bytes, playerInfo, ref index);
        SerializeString(bytes, chatStr, ref index);
        return bytes;
    }

    public override int GetId()
    {
        return 1001;
    }
}
