using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;
public interface IDataService
{
    public int Age { get; set; }
}

[MessagePackObject]
public class DataService : IDataService
{
    [Key(0)]
    public int Age { get; set; }
}
