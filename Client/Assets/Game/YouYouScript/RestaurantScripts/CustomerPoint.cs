using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerPoint : MonoBehaviour
{
    public bool fill;
    public string customerName;

    public void Clean()
    {
        fill = false;
        customerName = "";
    }
}
