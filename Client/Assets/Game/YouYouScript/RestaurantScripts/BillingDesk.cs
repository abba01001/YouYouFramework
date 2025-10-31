using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class BillingDesk : MonoBehaviour
{
    private bool customer, player, isIdling = true;
    private Customer currentCustomer;
    public Transform packageBoxPos;
    public Transform moneyPosParent;
    public Transform cashierPos;
    private GameObject packageBox;
    [HideInInspector]
    public List<Customer> customersForBilling;
    public Transform[] billingQue;
    private Vector3 initIdlePos = new Vector3(8, 0f, 1.5f);
    [HideInInspector]
    public List<GameObject> money;
    public List<Transform> moneyPos;
    [HideInInspector]
    public int moneyPosCount;
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // 缓存 AudioSource
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Customer") && isIdling)
        {
            currentCustomer = other.gameObject.GetComponent<Customer>();
            if (CustomerSystem.Instance.CheckIsFullCollect(currentCustomer.CharacterData))
            {
                isIdling = false;
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

    public async UniTask CollectFoodFromCustomer()
    {
        if (currentCustomer.IsNullFood()) return;
        await UniTask.Delay(1000);
        await currentCustomer.CheckPackageFood();
        customer = false;
        audioSource.Play();
        await UniTask.Delay(400);
        GotoMyExit();
    }

    private void GotoMyExit()
    {
        currentCustomer.GoToExit();
        currentCustomer = null;
        isIdling = true;
    }

    public void ArrangeCustomersInQue()
    {
        for(int i = 0; i< customersForBilling.Count; i++)
        {
            int index = i;
            customersForBilling[index].CheckGotoBillingDesk(initIdlePos + new Vector3(index * 8, 0, 0)); // //billingQue[i].position);
        }
    }

    public void RemoveCustomer(Customer customer)
    {
        customersForBilling.Remove(customer);
        ArrangeCustomersInQue();
    }

    public void AddCustomer(Customer customer)
    {
        customersForBilling.Add(customer);
        ArrangeCustomersInQue();
    }
}
