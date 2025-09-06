using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Eventdata;
using UnityEngine;
using UnityEngine.UI;

public class SpotTheDifferenceTime : MonoBehaviour
{
    [SerializeField]
    private Image image;

    private bool isGameFinished = true;
    
    [SerializeField]
    private float time;

    private float timeLeft = 1;

    private void OnEnable()
    {
        EventManager.Subscribe<SpotTheDifferenceStatusData>(GameStart);
        EventManager.Subscribe<ResetGameData>(ResetGame);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<SpotTheDifferenceStatusData>(GameStart);
        EventManager.Unsubscribe<ResetGameData>(ResetGame);
    }

    public void Update()
    {
        if (!isGameFinished)
        {
            timeLeft -= Time.deltaTime / time;
            //Debug.Log($"time {timeLeft}");
            image.fillAmount = timeLeft;
            if (timeLeft <= 0)
            {
                EventManager.Publish(new SpotTheDifferenceStatusData(false,false,true));

                Debug.Log($"time {timeLeft}");
            }
        }
    }


    public void GameStart(SpotTheDifferenceStatusData data)
    {
        if (data.gameFinished == true)
        {
            Debug.Log("kalah");
            isGameFinished = data.gameFinished;
        }
        else if (data.gameStart == true)
        {
            isGameFinished = false;
        }
    }

    public void ResetGame(ResetGameData data)
    {
        timeLeft = 1;
        isGameFinished = false;
    }
}
