using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using YouYou;

public class FoodSpawner : MonoBehaviour
{
    public Food food;
    public Animator anim;
    public bool spawnAtStart;
    public List<Food> matureFoodList = new List<Food>();
    private void Awake()
    {
        if(spawnAtStart)
            SpawnFood();
    }

    public void JumpFoodToPlayer(Collider other)
    {
        foreach (var f in matureFoodList.ToList())
        {
            f.JumpFoodToPlayer(other);
        }
    }
    
    public void SpawnFood()
    {
        if (food.foodName == "Tomato")
        {
            GameObject obj = Instantiate(food.gameObject, transform.position, transform.rotation);
            obj.GetComponent<Food>().Init(this, food.foodName);
            obj.transform.parent = this.transform;
            obj.transform.localScale = Vector3.zero;
            obj.transform.DOScale(Vector3.one, 10).OnComplete(() =>
            {
                matureFoodList.Add(obj.GetComponent<Food>());
                if (anim)
                {
                    anim.SetTrigger("Play");
                }
            });
        }
        else
        {
            Invoke("Spawn", 1);
        }
    }

    public void Spawn()
    {
        if (anim)
        {
            anim.SetTrigger("Play");
        }

        GameObject foodObj = Instantiate(food.gameObject, transform.position, transform.rotation);
        foodObj.GetComponent<Food>().Init(this, food.foodName);
        foodObj.transform.parent = this.transform;
        matureFoodList.Add(foodObj.GetComponent<Food>());
    }

    public void RefreshFood(Food discardFood)
    {
        matureFoodList.Remove(discardFood);
        SpawnFood();
    }
    
    public bool HasSpawnedFood()
    {
        return matureFoodList.Count > 0;
    }
}
