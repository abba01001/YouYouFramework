using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

public class Program : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var mc = new MyClass
        {
            Age = 99,
            FirstName = "fn",
            LastName = "ln",
        };

        DataService dataService = new DataService();
        dataService.Age = 20;
        byte[] bytes = MessagePackSerializer.Serialize(dataService);
        DataService mc2 = MessagePackSerializer.Deserialize<DataService>(bytes);
        var json = MessagePackSerializer.ConvertToJson(bytes);
        GameUtil.LogError(json);

    }
}