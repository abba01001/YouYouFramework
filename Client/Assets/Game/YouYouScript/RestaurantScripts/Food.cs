using System;
using UnityEngine;
using RDG;
using DG.Tweening;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using YouYou;

public class Food : MonoBehaviour
{
    private Transform foodCollectPos;
    private float foodCollectPlayerYVal = 1f;
    private bool goToPlayer = true;
    [HideInInspector]
    public bool notSpawnAuto;
    public float speed, jumpPower;
    private Transform targetPose;
    public string foodName;
    // [HideInInspector]
    // public int maxFoodPlayerCarry;

    
    public void OnEnable()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateFoodPlayerCarry,UpdateCarryEvent);
    }
    public void OnDisable()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateFoodPlayerCarry,UpdateCarryEvent);
    }

    private void UpdateCarryEvent(object userdata)
    {
        // maxFoodPlayerCarry = (int) userdata;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (goToPlayer)
            {
                if (other.gameObject.GetComponent<PlayerManager>())
                {
                    PlayerManager _PlayerManager = other.gameObject.GetComponent<PlayerManager>();

                    if (other.gameObject.GetComponent<Helper>())
                    {
                        if(foodName != _PlayerManager.currentFoodName)
                        {
                            return;
                        }
                    }

                    if (_PlayerManager.collectedFood.Count < _PlayerManager.maxFoodPlayerCarry)
                    {
                        if(notSpawnAuto/*foodName != "Egg" || foodName != "Sauce"*/)
                            transform.GetComponentInParent<FoodSpawner>().foodObj = null;
                        else
                            transform.GetComponentInParent<FoodSpawner>().SpawnFood();


                        Vibration.Vibrate(30);

                        if(other.gameObject.layer == 7)
                           AudioManager.Instance.Play("FoodCollect");

                        transform.parent = other.transform;
                        _PlayerManager.collectedFood.Add(this);
                    }
                    else
                        return;
                }

                foodCollectPos = other.transform.GetChild(1).transform;
                targetPose = foodCollectPos;

                transform.DOLocalJump(targetPose.localPosition, jumpPower, 1, speed)
                .OnComplete(delegate ()
                {
                    this.transform.localPosition = foodCollectPos.localPosition;
                    this.transform.localEulerAngles = Vector3.zero;

                    foodCollectPos.position = new Vector3(foodCollectPos.transform.position.x, foodCollectPos.transform.position.y + foodCollectPlayerYVal, foodCollectPos.transform.position.z);
                });

                goToPlayer = false;
            }
        }
    }

    public void PlaceFood(Transform targetPos)
    {
        if (transform.parent)
            transform.parent = null;

        targetPose = targetPos;

        transform.DOJump(targetPose.position, 4, 1, .4f);

        foodCollectPos.position = new Vector3(foodCollectPos.transform.position.x,
            foodCollectPos.transform.position.y - foodCollectPlayerYVal, foodCollectPos.transform.position.z);
    }

    public void GotoCustomer(Transform target)
    {
        goToPlayer = false;

        transform.DOJump(target.position, 4, 1, .4f)
        .OnComplete(delegate ()
        {
            transform.parent = target;
            transform.position = target.position;
        });
    }

    public async UniTask GotoBillingCounterBox(Transform target)
    {
        transform.DOJump(target.position, 4, 1, .4f);
        await UniTask.Delay(400);
    }

    public void GotoTrashBin(Transform target)
    {
        transform.DOJump(target.position, 4, 1, .4f)
        .OnComplete(delegate ()
        {
            Destroy(this.gameObject);
        });
    }
}
