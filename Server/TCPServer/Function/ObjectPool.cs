using System.Collections.Concurrent;

public class ObjectPool<T> where T : new()
{
    private readonly ConcurrentBag<T> _objects = new ConcurrentBag<T>();

    // 获取一个对象
    public T Rent()
    {
        if (_objects.TryTake(out T item))
        {
            return item;
        }
        return new T();
    }

    // 归还一个对象
    public void Return(T item)
    {
        _objects.Add(item);
    }
}
