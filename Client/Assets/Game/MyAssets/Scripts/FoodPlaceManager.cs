using System.Collections.Generic;
using UnityEngine;

public class FoodPlaceManager : MonoBehaviour
{
    public Transform HelperPos;
    public int collectFoodCapacity;

    public string shelfFoodName;
    public Transform shelfTopTransform;
    public List<CustomerPoints> customerPoints;

    [HideInInspector]
    public List<Food> collectedFoods;
    public List<Transform> shelfPos;
    public FoodSpawner[] availableFoodSpawners;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Unlocked"))
            gameObject.SetActive(true);
    }

    public void MoveShelfTopTransform()
    {
        if(collectedFoods.Count < collectFoodCapacity)
        shelfTopTransform.position = shelfPos[collectedFoods.Count].position;
    }
}
