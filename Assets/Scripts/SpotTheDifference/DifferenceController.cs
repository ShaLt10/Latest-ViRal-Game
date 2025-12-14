using UnityEngine;

public class DifferenceController : MonoBehaviour
{
    [Header("Stage")]
    [SerializeField] private int stageIndex = 1;
    [SerializeField] private int allDiff = 5;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private ResultButton resultButton;

    private int currentCount = 0;
    private bool finished = false;

    private void OnEnable()
    {
        EventManager.Subscribe<ResetGameData>(OnResetGame);
        EventManager.Subscribe<SpotTheDifferenceStatusData>(OnStatus);
        EventManager.Subscribe<DifferenceSpottedData>(OnSpotted);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<ResetGameData>(OnResetGame);
        EventManager.Unsubscribe<SpotTheDifferenceStatusData>(OnStatus);
        EventManager.Unsubscribe<DifferenceSpottedData>(OnSpotted);
    }

    private void OnResetGame(ResetGameData r)
    {
        if (r.stageIndex != stageIndex) return;

        currentCount = 0;
        finished = false;
        if (resultPanel) resultPanel.SetActive(false);
    }

    private void OnStatus(SpotTheDifferenceStatusData data)
    {
        if (data.stageIndex != stageIndex) return;
        if (finished) return;

        if (data.lose)
        {
            finished = true;
            ShowLose();
        }
    }

    private void OnSpotted(DifferenceSpottedData d)
    {
        if (d.stageIndex != stageIndex) return;
        if (finished) return;

        currentCount += Mathf.Max(1, d.addDifference);
        if (currentCount >= allDiff)
        {
            finished = true;
            ShowWin();
        }
    }

    private void ShowWin()
    {
        if (resultPanel) resultPanel.SetActive(true);
        if (resultButton) resultButton.SetResult(true, stageIndex);
        else SpotTheDifferenceSingleton.Instance.OnStageCleared();
    }

    private void ShowLose()
    {
        if (resultPanel) resultPanel.SetActive(true);
        if (resultButton) resultButton.SetResult(false, stageIndex);
        else SpotTheDifferenceSingleton.Instance.OnStageFailed();
    }
}
