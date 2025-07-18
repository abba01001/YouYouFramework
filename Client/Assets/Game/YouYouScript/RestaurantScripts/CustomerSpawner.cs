using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YouYou;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab;
    public Transform exitTransform;

    void Start()
    {
        InitCustomer();
    }

    public async UniTask InitCustomer()
    {
        int shelfsCount = GameObject.FindGameObjectsWithTag("Shelf").Length;

        for(int i = 0; i< shelfsCount; i++)
        {
            int spawnTime = Random.Range(1, 4) + 3;

            Invoke("SpawnCustomer", spawnTime);
            GameEntry.Time.CreateTimer(this, spawnTime, () =>
            {
                SpawnCustomer();
            });
        }
    }

    public async UniTask SpawnCustomer()
    {
        PoolObj customer = await GameEntry.Pool.GameObjectPool.SpawnAsync($"Assets/Game/Download/Prefab/RestaurantPrefab/Customer.prefab");
        customer.gameObject.MSetActive(true);
        customer.transform.position = transform.position;
        customer.transform.rotation = transform.rotation;
        customer.transform.SetParent(this.transform);
    }
}
