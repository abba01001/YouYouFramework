using UnityEngine;
using UnityEngine.UI;
using YouYou;

public interface IDamageable
{
    void TakeNormalDamage(int damage);
    void TakeSkillDamage(int damage);
}

public class CharacterBase : MonoBehaviour
{
    [HideInInspector] protected Vector3 initScale;
    [HideInInspector] protected Animator animator;
    [HideInInspector] protected Transform modelRoot;
    
    [HideInInspector] protected GameObject modelDi;
    [HideInInspector] protected GameObject blood;
    [HideInInspector] protected GameObject defense;
    
    [HideInInspector] private Image bloodValue;
    [HideInInspector] protected RangeDetection rangeDetection;
    
    public Sys_ModelEntity config { get; set; }
    public virtual void Awake()
    {
        animator = transform.GetComponentInChildren<Animator>();
        modelRoot = transform.Find("Root");
        initScale = modelRoot.localScale;
        // modelDi = transform.Find("Content/Di").gameObject;
        // blood = transform.Find("Content/Blood").gameObject;
        // bloodValue = blood.transform.Find("Value").GetComponent<Image>();
        // defense = transform.Find("Content/Defense").gameObject;
        //
        // modelDi.gameObject.SetActive(false);
        // blood.gameObject.SetActive(false);
        // defense.gameObject.SetActive(false);
    }

    public void SetBloodValue(float value,bool needAnim = false)
    {
        //bloodValue.fillAmount = value;
    }

    public void UpdateInitScale()
    {
        initScale = modelRoot.localScale;
    }

    public void SetFace(int face)
    {
        modelRoot.localScale = new Vector3(face * initScale.x, initScale.y, initScale.z);
    }
}