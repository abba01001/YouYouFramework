using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public Food food;
    public Animator anim;
    public GameObject foodObj;
    public bool spawnAtStart;

    private void Awake()
    {
        if(spawnAtStart)
            SpawnFood();
    }

    public void SpawnFood()
    {
        Invoke("Spawn", 1);
    }

    public void Spawn()
    {
        if (anim)
            anim.SetTrigger("Play");

        foodObj = Instantiate(food.gameObject, transform.position, transform.rotation);
        foodObj.GetComponent<Food>().foodName = food.foodName;
        foodObj.transform.parent = this.transform;
    }
}
