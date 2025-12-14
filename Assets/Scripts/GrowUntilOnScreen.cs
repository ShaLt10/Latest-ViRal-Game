using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowUntilOnScreen  : MonoBehaviour
{
    public RectTransform target;      // The UI element to grow
    public float step = 5f;           // How much to increase width per step
    public float maxWidth = 2000f;    // Safety limit

    void Start()
    {
        GrowUntilOnScreenEdge();
    }

    void GrowUntilOnScreenEdge()
    {
        float currentWidth = target.sizeDelta.x;

        while (currentWidth < maxWidth)
        {
            Vector3 worldTopRight = GetWorldTopRightCorner();
            Vector3 screenPoint = worldTopRight; // In Overlay, world == screen point

            Debug.Log($"{screenPoint.x} dan screen {Screen.width}");
            if (screenPoint.x >= Screen.width)
                break;

            currentWidth += step;
            Vector2 size = target.sizeDelta;
            size.x = currentWidth;
            target.sizeDelta = size;
        }
    }

    // Returns the world position of the top-right corner of the RectTransform
    Vector3 GetWorldTopRightCorner()
    {
        Vector3 localTopRight = new Vector3(target.rect.xMax, target.rect.yMax, 0);
        return target.TransformPoint(localTopRight);
    }
}

