public class BaseMsg : BaseData
{
    public override int GetDataBytesLength()
    {
        throw new System.NotImplementedException();
    }

    public override int ReadingData(byte[] bytes, int beginIndex = 0)
    {
        throw new System.NotImplementedException();
    }

    public override byte[] SerializeData()
    {
        throw new System.NotImplementedException();
    }

    public virtual int GetId()
    {
        return 0;
    }
}
