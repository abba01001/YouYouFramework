using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YouYou;

public class Joystick : MonoBehaviour
{
    public RectTransform center;
    public RectTransform knob;
    public float range;
    public bool fixedJoystick;

    [HideInInspector] public Vector2 direction;

    Vector2 start;

    void Start()
    {
        ShowHide(false);
    }

    void Update()
    {
        // 判断鼠标是否点击了UI元素，如果点击了UI元素，则不响应摇杆的操作
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            ShowHide(true);
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                center.parent as RectTransform,
                Input.mousePosition,
                GameEntry.Instance.UICamera, // Screen Space Overlay 模式
                out localPos
            );
            start = localPos;
            center.anchoredPosition = localPos;
            knob.anchoredPosition = localPos;
        }
        else if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                center.parent as RectTransform,
                Input.mousePosition,
                GameEntry.Instance.UICamera,
                out localPos
            );

            Vector2 offset = localPos - center.anchoredPosition;
            offset = Vector2.ClampMagnitude(offset, center.sizeDelta.x * range);
            knob.anchoredPosition = center.anchoredPosition + offset;

            direction = offset.normalized;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ShowHide(false);
            direction = Vector2.zero;
        }
    }


    void ShowHide(bool state)
    {
        center.gameObject.SetActive(state);
        knob.gameObject.SetActive(state);
    }
}