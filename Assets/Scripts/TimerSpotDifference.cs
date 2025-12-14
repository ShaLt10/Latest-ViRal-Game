using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerSpotDifference : MonoBehaviour
{
    public Text timerText; // Text UI untuk timer
    public float timeLimit = 30f; // Waktu dalam detik

    private float timeRemaining;

    // Start is called before the first frame update
    void Start()
    {
        timeRemaining = timeLimit;
    }

    // Update is called once per frame
    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            EndGame(); // Fungsi untuk mengakhiri game
        }
    }

    void UpdateTimerDisplay()
    {
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString() + "s";
    }

    void EndGame()
    {
        // Logika jika waktu habis, misalnya munculkan Game Over
        Debug.Log("Waktu habis!");
    }
}
