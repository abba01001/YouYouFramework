using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using YouYou;


public class BuildingBase : MonoBehaviour
{
    public Sys_BuildingsEntity Entity;
    public void Init(Sys_BuildingsEntity entity)
    {
        Entity =  entity;
    }

    public void PlayUnlockAnim()
    {
        transform.DOPunchScale(new Vector3(0.1f, 1, 0.1f), 0.5f, 7);
    }
    
}
