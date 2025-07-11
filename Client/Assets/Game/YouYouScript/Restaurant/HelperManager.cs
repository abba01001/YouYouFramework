using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperManager : MonoBehaviour
{
    private static HelperManager _instance;
    private HelperManager()
    {

    }

    public static HelperManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new HelperManager();
            }
            return _instance;
        }
    }
}
