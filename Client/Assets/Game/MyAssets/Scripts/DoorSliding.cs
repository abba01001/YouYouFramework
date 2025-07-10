using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSliding : MonoBehaviour
{
    public Transform doorR, doorL;
    public float duration;
    private float initialXPos;

    private void Start()
    {
        initialXPos = doorR.localPosition.x;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Customer") || other.CompareTag("Player"))
        {
            doorR.DOLocalMoveX(initialXPos*2, duration);
            doorL.DOLocalMoveX(-initialXPos*2, duration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Customer") || other.CompareTag("Player"))
        {
            doorR.DOLocalMoveX(initialXPos, duration);
            doorL.DOLocalMoveX(-initialXPos, duration);
        }
    }
}
