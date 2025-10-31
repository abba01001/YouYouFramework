using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    private bool canEat = true;
    public Animator anim;
    public FoodPlaceManager shelf;
    //public GameObject[] eggs;
    public FoodSpawner[] foodSpawners;
    //public Food eggPrefab;
    private FoodSpawner currentFoodSpawner;

    private void Start()
    {

    }

    private void Update()
    {
        if (shelf.collectedFoods.Count > 0 && canEat)
        {
            for(int i = 0; i< foodSpawners.Length; i++)
            {
                if (foodSpawners[i].HasSpawnedFood())
                {
                    canEat = false;
                    Invoke("Eat", 2);
                    currentFoodSpawner = foodSpawners[i];
                    break;
                }
            }
        }   
    }

    private void Eat()
    { 
        if(anim)
        anim.SetTrigger("Play");

        Food food = shelf.collectedFoods[shelf.collectedFoods.Count - 1];
        shelf.collectedFoods.Remove(food);

        Destroy(food.gameObject, 1);

        Invoke("SpawnEgg", 1);
    }

    private void SpawnEgg()
    {
        print("Eat");

        currentFoodSpawner.Spawn();
        canEat = true;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        if (shelf.collectedFoods.Count > 0 && canEat)
    //        {
                
    //            canEat = false;
    //            Invoke("Wait", 4);
    //        }
    //    }
    //}
}
