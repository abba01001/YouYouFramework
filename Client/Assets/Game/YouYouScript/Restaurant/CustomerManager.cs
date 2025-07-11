using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    private static CustomerManager _instance;
    private CustomerManager()
    {

    }

    public static CustomerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CustomerManager();
            }
            return _instance;
        }
    }
}
