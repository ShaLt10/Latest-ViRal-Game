using UnityEngine;
using UnityEngine.EventSystems;

public class WrongClickArea : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float boostPerWrongClick = 0.2f;
    [SerializeField] private int stageIndex = 1;
    [SerializeField] private bool respectUIBlocking = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        // opsional: cegah klik kalau di atas UI lain
        if (respectUIBlocking && eventData.pointerEnter != null)
        {
            // kalau ada button/image lain di atas area ini, biarkan mereka yang handle
        }

        EventManager.Publish(new WrongClickData(boostPerWrongClick, stageIndex));
    }
}
