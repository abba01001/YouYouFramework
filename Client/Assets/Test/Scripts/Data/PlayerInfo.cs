using System.Text;

public class PlayerInfo : BaseData
{
    public long playerID;
    public string name;
    public int level;

    public override int GetDataBytesLength()
    {
        return 8 + 4 + Encoding.UTF8.GetBytes(name).Length + 4;
    }

    public override int ReadingData(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        playerID = ReadingLong(bytes, ref index);
        name = ReadingString(bytes, ref index);
        level = ReadingInt(bytes, ref index);
        return index - beginIndex;
    }

    public override byte[] SerializeData()
    {
        byte[] bytes = new byte[GetDataBytesLength()];
        int index = 0;
        SerializeLong(bytes, playerID, ref index);
        SerializeString(bytes, name, ref index);
        SerializeInt(bytes, level, ref index);
        return bytes;
    }
}
