using UnityEngine;
using UnityEngine.SceneManagement;

public class SpotTheDifferenceSingleton : MonoBehaviour
{
    public static SpotTheDifferenceSingleton Instance { get; private set; }

    [Header("Panels / Dialogs")]
    [SerializeField] private GameObject openingPanel;  // mis. intro stage 1
    [SerializeField] private GameObject stage1Group;   // container stage 1 (controller, timer, spots)
    [SerializeField] private GameObject stage2Group;   // container stage 2
    [SerializeField] private GameObject winPanel;      // final win
    [SerializeField] private GameObject loseDialog;    // dialog kalah (opsional)

    [Header("Flow")]
    [SerializeField] private string nextSceneOnFinish = ""; // kosong = tetap di scene
    private int currentStage = 1;
    private int maxStage = 2;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // uncomment kalau lintas-scene
    }

    private void Start()
    {
        // Awal: tampilkan opening → start stage 1
        SetAllGroups(false);
        if (openingPanel) openingPanel.SetActive(true);
    }

    public void StartStage1()
    {
        currentStage = 1;
        BeginStage(currentStage);
    }

    public void StartStage2()
    {
        currentStage = 2;
        BeginStage(currentStage);
    }

    private void BeginStage(int stage)
    {
        SetAllGroups(false);
        if (stage == 1 && stage1Group) stage1Group.SetActive(true);
        if (stage == 2 && stage2Group) stage2Group.SetActive(true);

        // publish START
        EventManager.Publish(SpotTheDifferenceStatusData.Start(stage));
        // reset komponen stage
        EventManager.Publish(new ResetGameData(stage));
    }

    public void OnStageCleared()
    {
        // publish WIN untuk stage sekarang
        EventManager.Publish(SpotTheDifferenceStatusData.Win(currentStage));

        if (currentStage < maxStage)
        {
            // lanjut ke stage berikutnya
            currentStage++;
            // bisa selipkan dialog transisi, lalu:
            BeginStage(currentStage);
        }
        else
        {
            // Final clear
            SetAllGroups(false);
            if (winPanel) winPanel.SetActive(true);

            if (!string.IsNullOrEmpty(nextSceneOnFinish))
            {
                SceneManager.LoadScene(nextSceneOnFinish);
            }
        }
    }

    public void OnStageFailed()
    {
        // publish LOSE untuk stage sekarang
        EventManager.Publish(SpotTheDifferenceStatusData.Lose(currentStage));

        // tampilkan dialog kalah (opsional), lalu reset stage yang sama
        if (loseDialog) loseDialog.SetActive(true);
        // kamu bisa kasih tombol "Retry" yang memanggil RetryStage()
        RetryStage();
    }

    public void RetryStage()
    {
        // dari lose → mulai ulang stage saat ini
        BeginStage(currentStage);
        if (loseDialog) loseDialog.SetActive(false);
    }

    public void CloseOpeningAndStart()
    {
        if (openingPanel) openingPanel.SetActive(false);
        StartStage1();
    }

    private void SetAllGroups(bool active)
    {
        if (openingPanel) openingPanel.SetActive(false);
        if (stage1Group) stage1Group.SetActive(active);
        if (stage2Group) stage2Group.SetActive(active);
        if (winPanel) winPanel.SetActive(false);
        if (loseDialog) loseDialog.SetActive(false);
    }
}
