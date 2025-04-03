using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using YouYou;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bloodValue;
    [SerializeField] private SpriteRenderer defenseValue;

    private int maxHp;
    private EnemyBase enemyParent = null;
    private EnemyData enemyData = null;
    public void Init(EnemyData data,EnemyBase parent)
    {
        transform.SetParent(parent.transform,false);
        transform.localPosition = new Vector3(0, 5, 0);
        enemyData = data;
        enemyParent = parent;

        maxHp = enemyData.hp;
        bloodValue.size = Vector2.one;
        defenseValue.size = Vector2.one;
    }

    public void OnUpdateHealth()
    {
        var value = (float) enemyParent.healthValue / maxHp;
        //StartCoroutine(UpdateProgressBar(bloodValue,value, 0.7f));
        bloodValue.size = new Vector2(value, progressHeight);
    }
    
        
    public float duration = 0.5f;
    private float progressMin = 0f;
    private float progressMax = 1f;
    private float progressHeight =1f;
    // 更新进度条的动画
    private IEnumerator UpdateProgressBar(SpriteRenderer progress,float targetSize,float time)
    {
        Vector2 currentSize = progress.size;
        float startSize = currentSize.x;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / time;
            float newSize = Mathf.Lerp(startSize, targetSize, t);
            progress.size = new Vector2(newSize, progressHeight);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        progress.size = new Vector2(targetSize, progressHeight);
    }
    
}
