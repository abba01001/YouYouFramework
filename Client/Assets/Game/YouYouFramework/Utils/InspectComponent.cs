using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class InspectComponent : MonoBehaviour
{
    
    [SerializeField]
    private UnityEvent onButtonClick; // 事件，可以在 Inspector 中配置
    
    [Button("触发事件", ButtonSizes.Large)]
    public void MyButtonAction1()
    {
        onButtonClick?.Invoke(); // 调用事件
    }

}
