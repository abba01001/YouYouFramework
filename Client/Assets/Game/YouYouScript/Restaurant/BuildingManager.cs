using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager
{
    private static BuildingManager _instance;
    private BuildingManager()
    {

    }

    public static BuildingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BuildingManager();
            }
            return _instance;
        }
    }

    public void Update()
    {

    }
}