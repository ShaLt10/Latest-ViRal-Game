using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Eventdata;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Difference : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private Difference link;
    [SerializeField]
    private DifferenceController controller;
    public bool isClicked =false;

    private void OnEnable()
    {
        EventManager.Subscribe<ResetGameData>(ResetGame);
        EventManager.Subscribe<SpotTheDifferenceStatusData>(Finished);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<ResetGameData>(ResetGame);
        EventManager.Subscribe<SpotTheDifferenceStatusData>(Finished);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isClicked) return;
        OnClick();
        link.OnClick();
    }

    public void OnClick()
    { 
        isClicked = true;
        image.color = ChangeAlpha(image.color);
        EventManager.Publish(new DifferenceSpottedData(1));
    }

    public Color ChangeAlpha(Color color, float a =1)
    {
            color.a = a;
        return color;
    }

    public void ResetGame(ResetGameData data)
    {
            image.color =ChangeAlpha(image.color,4 / 255);
            isClicked = false;
    }
    public void Finished(SpotTheDifferenceStatusData data)
    {
        if (data.gameFinished)
        { 
        isClicked=true;
        }
    }
}
