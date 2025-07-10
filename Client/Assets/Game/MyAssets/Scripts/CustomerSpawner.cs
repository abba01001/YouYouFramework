using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab;
    public Transform exitTransform;

    void Start()
    {
        int shelfsCount = GameObject.FindGameObjectsWithTag("Shelf").Length;

        for(int i = 0; i< shelfsCount; i++)
        {
            int spawnTime = Random.Range(1, 4);

            Invoke("SpawnCustomer", spawnTime);
        }
    }

    public void SpawnCustomer()
    {
        GameObject customer = Instantiate(customerPrefab, transform.position, transform.rotation);

        customer.GetComponent<Customer>().exitTransform = exitTransform;
    }
}
