using System.Collections;
using System.Collections.Generic;
using System.Data;
using Assets.Scripts.Eventdata;
using UnityEngine;
using UnityEngine.UI;

public class DifferenceController : MonoBehaviour
{
    private int diffCount;

    private int allDiff = 6;

    [SerializeField]
    GameObject result;
    private void OnEnable()
    {
        EventManager.Subscribe<DifferenceSpottedData>(GameStart);
        EventManager.Subscribe<ResetGameData>(RestartGame);
        EventManager.Subscribe<SpotTheDifferenceStatusData>(GameEnd);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<DifferenceSpottedData>(GameStart);
        EventManager.Unsubscribe<ResetGameData>(RestartGame);
        EventManager.Unsubscribe<SpotTheDifferenceStatusData>(GameEnd);
    }

    private void GameStart(DifferenceSpottedData data)
    {
        diffCount++;
        CheckAllDiff();
    }

    private void CheckAllDiff()
    {
        if (diffCount >= allDiff)
        {
            result.SetActive(true);
            EventManager.Publish(new SpotTheDifferenceStatusData(false, true, true));
        }
    }

    private void RestartGame(ResetGameData data)
    {
            result.SetActive(false);
            diffCount = 0;
    }

    private void GameEnd(SpotTheDifferenceStatusData data)
    {
        if (!data.gameWin && gameObject.GetInstanceID() != data.InstanceId && data.gameFinished)
        {
            result.SetActive(true);
            EventManager.Publish(new SpotTheDifferenceStatusData(false, false, true,gameObject.GetInstanceID()));
        }
    }
}
