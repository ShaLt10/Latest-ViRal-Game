using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Analog : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background; // The outer circle
    public RectTransform handle;     // The inner knob
    public float handleLimit = 1f;   // Range of the knob [0–1]

    private Vector2 inputVector = Vector2.zero;

    public Vector2 Direction => inputVector;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos /= background.sizeDelta;
            pos = pos * 2; // Normalize based on half-size
            inputVector = Vector2.ClampMagnitude(pos, 1f);
            handle.anchoredPosition = inputVector * (background.sizeDelta.x / 2) * handleLimit;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
