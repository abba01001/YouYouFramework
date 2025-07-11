using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    public List<Food> collectedFood;
    public int maxFoodPlayerCarry;
    public int minimumFoodPlayerCarry;
    [HideInInspector]
    public Vector3 initialFoodCollectPos;
    public Transform foodCollectPos;
    [HideInInspector]
    public string currentFoodName;

    private void Start()
    {
        initialFoodCollectPos = foodCollectPos.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TrashBin"))
        {
            for (int i = collectedFood.Count - 1; i >= 0; i--)
            {
                AudioManager.Instance.Play("FoodPlace");

                collectedFood[i].GotoTrashBin(other.transform);
                collectedFood.Remove(collectedFood[i]);
            }

            foodCollectPos.localPosition = initialFoodCollectPos;
        }
    }
}
