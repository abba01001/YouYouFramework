using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BillingDesk : MonoBehaviour
{
    private bool customer, player, isCounterEmpty = true;
    private Customer currentCustomer;
    public Transform packageBoxPos;
    public Transform moneyPosParent;
    public Transform cashierPos;
    public GameObject packageBoxPrefab;
    private GameObject packageBox;
    [HideInInspector]
    public List<Customer> customersForBilling;
    public Transform[] billingQue;
    [HideInInspector]
    public List<GameObject> money;
    public List<Transform> moneyPos;
    [HideInInspector]
    public int moneyPosCount;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Customer") && isCounterEmpty)
        {
            currentCustomer = other.gameObject.GetComponent<Customer>();

            if (currentCustomer.collectedFoods.Count > 0)
            {
                isCounterEmpty = false;
                customer = true;
                CheckPackaging();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !player)
        {
            player = true;
            CheckPackaging();
        }
    }

    private void CheckPackaging()
    {
        if (customer && player)
        {
            if (packageBox == null)
                packageBox = Instantiate(packageBoxPrefab, packageBoxPos.position, packageBoxPos.rotation);

            CollectFoodFromCustomer();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Customer"))
            customer = false;

        if (other.CompareTag("Player")) 
            player = false;
    }

    public void CollectFoodFromCustomer()
    {
        int foodCount = currentCustomer.collectedFoods.Count;
        Food food;

        if (foodCount > 0)
        {
            food = currentCustomer.collectedFoods[foodCount - 1];
            currentCustomer.collectedFoods.Remove(food);
            food.GotoBillingCounterBox(packageBoxPos);
        }
        else
        {
            packageBox.GetComponent<Animator>().SetTrigger("StartProduction");

            Invoke("DeliverBox", .6f);          
        }
    }

    private void DeliverBox()
    {
        if (currentCustomer != null)
        {
            Destroy(currentCustomer.trolly);

            if (packageBox)
            {

                print(packageBox.gameObject.name);
                print(currentCustomer.gameObject.name);
                print(currentCustomer.handPos.gameObject.name);


                packageBox.transform.DOJump(currentCustomer.handPos.position, 4, 1, .3f)
                .OnComplete(delegate ()
                {
                    packageBox.transform.position = currentCustomer.handPos.position;
                    packageBox.transform.rotation = currentCustomer.handPos.rotation;
                    packageBox.transform.parent = currentCustomer.transform;

                    packageBox = null;
                    currentCustomer.PayMoney();
                    GetComponent<AudioSource>().Play();
                    customer = false;
                    Invoke("GotoMyExit", .4f);
                });
            }
        }
    }

    private void GotoMyExit()
    {
        currentCustomer.GoToExit();
        currentCustomer = null;
        isCounterEmpty = true;
    }

    public void ArrangeCustomersInQue()
    {
        for(int i = 0; i< customersForBilling.Count; i++)
        {
            if (customersForBilling[i].target == null || customersForBilling[i].target != billingQue[i])
            {
                customersForBilling[i].agent.SetDestination(billingQue[i].position);
                customersForBilling[i].target = billingQue[i];
                customersForBilling[i].counterLook = true;
            }
        }
    }
}
