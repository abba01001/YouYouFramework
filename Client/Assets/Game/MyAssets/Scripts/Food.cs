using UnityEngine;
using RDG;
using DG.Tweening;
using System.Collections.Generic;

public class Food : MonoBehaviour
{
    private Transform foodCollectPos;
    private float foodCollectPlayerYVal = 1f;
    private bool goToPlayer = true;
    [HideInInspector]
    public bool goToCustomer;
    public bool notSpawnAuto;
    public float speed, jumpPower;
    private Transform targetPose;
    public string foodName;
    private BillingDesk _BillingDesk;
    [HideInInspector]
    public int maxFoodPlayerCarry;

    private void Start()
    {
        _BillingDesk = FindObjectOfType<BillingDesk>();
        maxFoodPlayerCarry = FindObjectOfType<PlayerManager>().maxFoodPlayerCarry;
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
        if(transform.parent)
        transform.parent = null;

        targetPose = targetPos;

        transform.DOJump(targetPose.position, 4, 1, .4f);

        foodCollectPos.position = new Vector3(foodCollectPos.transform.position.x, foodCollectPos.transform.position.y - foodCollectPlayerYVal, foodCollectPos.transform.position.z);
    }

    public void GotoCustomer(Transform target, Customer customer)
    {
        goToPlayer = false;

        transform.DOJump(target.position, 4, 1, .4f)
        .OnComplete(delegate ()
        {
            goToCustomer = false;
            transform.parent = target;
            transform.position = target.position;
            customer.FoodColected();

        });
    }

    public void GotoBillingCounterBox(Transform target)
    {
        transform.DOJump(target.position, 4, 1, .4f)
        .OnComplete(delegate ()
        {
            _BillingDesk.CollectFoodFromCustomer();
            Destroy(this.gameObject);
        });
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
