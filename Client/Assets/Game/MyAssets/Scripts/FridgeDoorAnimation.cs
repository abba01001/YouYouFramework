using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeDoorAnimation : MonoBehaviour
{
    public Animator anim;

    private void OnTriggerEnter(Collider other)
    {
     
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Customer"))
        {
            if (anim)
            {
                anim.SetBool("Open", true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Customer"))
        {
            if (anim)
            {
                anim.SetBool("Open", false);
            }
        }
    }
}
