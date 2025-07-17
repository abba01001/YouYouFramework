using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class MoneySystem
{
    private List<Helper> helpers = new List<Helper>();
    private static MoneySystem _instance;

    private MoneySystem() { }

    public static MoneySystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MoneySystem();
            }
            return _instance;
        }
    }

}
