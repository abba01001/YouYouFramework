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
    [HideInInspector]
    public bool goToCustomer;
    public bool notSpawnAuto;
    public float speed, jumpPower;
    private Transform targetPose;
    public string foodName;
    private BillingDesk _BillingDesk;
    [HideInInspector]
    public int maxFoodPlayerCarry;

    public FoodSpawner foodSpawner;
    private void Start()
    {
        _BillingDesk = FindObjectOfType<BillingDesk>();
    }
    
    public void OnEnable()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateFoodPlayerCarry,UpdateCarryEvent);
    }
    public void OnDisable()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateFoodPlayerCarry,UpdateCarryEvent);
    }

    public void Init(FoodSpawner spawner, string foodName)
    {
        this.foodSpawner = spawner;
        this.foodName = foodName;
    }

    private void UpdateCarryEvent(object userdata)
    {
        maxFoodPlayerCarry = (int) userdata;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }

    public void JumpFoodToPlayer(Collider other)
    {
        CharacterBase character = other.gameObject.GetComponent<CharacterBase>();
        if (character != null)
        {
            if(!character.CharacterData.collectFood.Contains(foodName))
            {
                return;
            }
            
            if (character.collectedFood.Count < character.CharacterData.maxFoodCarry)
            {
                if (notSpawnAuto /*foodName != "Egg" || foodName != "Sauce"*/)
                {
                    
                }
                else
                {
                    foodSpawner.ClearCollectFood(this);
                }

                Vibration.Vibrate(30);
                AudioManager.Instance.Play("FoodCollect");
                transform.parent = other.transform;
                character.AddCollectFood(this);
            }
            else
            {
                return;
            }
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

    }

    public void PlaceFood(Transform targetPos)
    {
        if(transform.parent)
        transform.parent = null;

        targetPose = targetPos;

        transform.DOJump(targetPose.position, 4, 1, .4f);

        foodCollectPos.position = new Vector3(foodCollectPos.transform.position.x, foodCollectPos.transform.position.y - foodCollectPlayerYVal, foodCollectPos.transform.position.z);
    }

    public void GotoCustomer(Transform target)
    {
        transform.DOJump(target.position, 4, 1, .4f)
        .OnComplete(delegate ()
        {
            goToCustomer = false;
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
