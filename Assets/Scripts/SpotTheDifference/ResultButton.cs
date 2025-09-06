using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultButton : MonoBehaviour
{
    [SerializeField]
    Button button;

    [SerializeField]
    TMP_Text title;

    [SerializeField]
    TMP_Text description;


    public int win = 0;

    private void OnEnable()
    {
        button.onClick.AddListener(OnClickButton);
        EventManager.Subscribe<SpotTheDifferenceStatusData>(OnWin);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClickButton);
        EventManager.Unsubscribe<SpotTheDifferenceStatusData>(OnWin);
    }

    private void OnClickButton()
    {
        SpotTheDifferenceSingleton.Instance.SetGame(win);
    }

    private void OnWin(SpotTheDifferenceStatusData data)
    {
        if (data.gameWin)
        {
            win = 1;
            title.SetText($"Semua Perbedaan Didapatkan");
            description.SetText($"Selamat kamu mendapatkan semua perbedaan yang ada pada kedua gambar sebelumnya");
        }
        else
        {
            win = 0;
            title.SetText($"Tidak Semua Didapatkan");
            description.SetText($"kamu belum mendapatkan semua perbedaan yang ada pada kedua gambar");
            EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.SpotTheDifference_Lose}"));
        }
    }
}
