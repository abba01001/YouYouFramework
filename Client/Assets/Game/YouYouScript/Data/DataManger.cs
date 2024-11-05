using System.Collections;
using System.Collections.Generic;
using Main;
using UnityEngine;
using MessagePack;

public interface IDataManger
{
    int Age { get; set; }
    string UserId { get; set; }
    void SaveData(bool upload = false);
}

[MessagePackObject(keyAsPropertyName: true)]
public class DataManger : Observable<DataManger>, IDataManger
{
    public int Age { get; set; }
    private string _user_id;

    public string UserId
    {
        get => _user_id;
        set
        {
            _user_id = value;
            SaveData();
        }
    }

    public void SaveData(bool upload = false)
    {
        throw new System.NotImplementedException();
    }


    public void OnUpdate()
    {
    }

    public void Init()
    {
    }
}