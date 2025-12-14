using UnityEngine;
using UnityEngine.UI;

public class SpotTheDifferenceTime : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image image; // radial fill or bar
    [Header("Time & Speed")]
    [SerializeField] private float baseDecayPerSecond = 0.1f;
    [SerializeField] private float boostDecayMultiplier = 1.6f;
    [SerializeField] private float boostDuration = 2.0f;

    [Header("Stage")]
    [SerializeField] private int stageIndex = 1;

    private float timeLeft = 1f;          // 1 = full bar, 0 = habis
    private float currentSpeed = 1f;
    private float boostTimer = 0f;
    private bool isGameFinished = true;   // default: belum start → jangan jalan

    private void OnEnable()
    {
        EventManager.Subscribe<ResetGameData>(OnReset);
        EventManager.Subscribe<WrongClickData>(OnWrongClick);
        EventManager.Subscribe<SpotTheDifferenceStatusData>(OnStatus);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<ResetGameData>(OnReset);
        EventManager.Unsubscribe<WrongClickData>(OnWrongClick);
        EventManager.Unsubscribe<SpotTheDifferenceStatusData>(OnStatus);
    }

    private void Update()
    {
        if (isGameFinished) return;

        // decay waktu
        float speed = baseDecayPerSecond * currentSpeed;
        timeLeft = Mathf.Clamp01(timeLeft - speed * Time.deltaTime);

        // apply UI
        if (image) image.fillAmount = timeLeft;

        // boost decay timeout
        if (boostTimer > 0f)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                currentSpeed = 1f;
            }
        }

        if (timeLeft <= 0f)
        {
            // waktu habis -> kalah
            isGameFinished = true;
            EventManager.Publish(SpotTheDifferenceStatusData.Lose(stageIndex));
        }
    }

    private void OnReset(ResetGameData r)
    {
        if (r.stageIndex != stageIndex) return;
        timeLeft = 1f;
        currentSpeed = 1f;
        boostTimer = 0f;
        if (image) image.fillAmount = 1f;
        isGameFinished = false;
    }

    private void OnWrongClick(WrongClickData w)
    {
        if (w.stageIndex != stageIndex) return;
        if (isGameFinished) return;

        // sementara boost speed
        currentSpeed = Mathf.Max(currentSpeed, boostDecayMultiplier);
        boostTimer = Mathf.Max(boostTimer, boostDuration * Mathf.Clamp01(w.boostAmount));
    }

    private void OnStatus(SpotTheDifferenceStatusData s)
    {
        if (s.stageIndex != stageIndex) return;

        // game start/stop ditentukan oleh .started dan .gameFinished
        isGameFinished = !s.started || s.gameFinished;

        if (s.started && !s.gameFinished)
        {
            // start stage
            timeLeft = 1f;
            currentSpeed = 1f;
            boostTimer = 0f;
            if (image) image.fillAmount = 1f;
        }
    }
}
