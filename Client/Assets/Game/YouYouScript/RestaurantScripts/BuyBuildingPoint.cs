using UnityEngine;
using RDG;
using TMPro;
using DG.Tweening;
using Main;
using YouYou;

public class BuyBuildingPoint : MonoBehaviour
{
    public Sys_BuildingsEntity Entity;
    
    
    public int srNo, purchaseAmount;
    private GameManager _GameManager;
    private float countAnimSpeed = 0.1f;
    private float animDuration = 0.5f;
    private TextMeshPro moneyAmountText;

    private void Awake()
    {
        OnUpdateBuildingSpend(null);
    }
    
    public void Init(Sys_BuildingsEntity entity)
    {
        Entity =  entity;
    }

    public void OnEnable()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateBuildingSpend,OnUpdateBuildingSpend);
    }
    public void OnDisable()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateBuildingSpend,OnUpdateBuildingSpend);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BuildingSystem.Instance.SpendCoinToUnlock(Entity.BuildingId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            BuildingSystem.Instance.StopSpend();
        }
    }
    
    private void OnUpdateBuildingSpend(object userdata)
    {
        if (userdata == null)
        {
            
            return;
        }
        UpdateBuildingSpendEvent e = (UpdateBuildingSpendEvent) userdata;
        if (e.BuildingId != Entity.BuildingId) return;
        // GameEntry.Data.PlayerRoleData.restaurantData.readyUnlocks[Entity.BuildingId]
        
        AudioManager.Instance.Play("BuyPoint");
        Vibration.Vibrate(30);
        purchaseAmount--;
        
        if (e.Unlock)
        {
            AudioManager.Instance.Play("Unlock");
            PoolObj obj = GameEntry.Pool.GameObjectPool.SpawnSynchronous($"Assets/Game/Download/Prefab/Regions/UnlockBuildingEf.prefab",transform);
            obj.gameObject.MSetActive(true);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.parent = null;
            ParticleSystem particle = obj.GetComponentInChildren<ParticleSystem>(true);
            particle.Play();
            Destroy(obj,2f);
            BuildingSystem.Instance.RemoveBuyBuildingPoint(this);
        }
        MainEntry.ClassObjectPool.Enqueue(e);
    }
}
