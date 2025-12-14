using UnityEngine;
using UnityEngine.UI;

public class ResultButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject resultPanel;

    private bool isWin = false;
    private int stageIndex = 1;

    private void Awake()
    {
        if (!button) button = GetComponent<Button>();
        if (button) button.onClick.AddListener(OnClick);
    }

    public void SetResult(bool win, int stageIndex)
    {
        isWin = win;
        this.stageIndex = stageIndex;
    }

    private void OnClick()
    {
        if (resultPanel) resultPanel.SetActive(false);

        if (isWin)
            SpotTheDifferenceSingleton.Instance.OnStageCleared();
        else
            SpotTheDifferenceSingleton.Instance.OnStageFailed();
    }
}
