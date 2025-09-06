// DialogManager.cs - FINAL (Inspector Next Dialog, no sequenceName)
// PELETAKAN: Assets/Scripts/DialogSystem/DialogManager/DialogManager.cs
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [Header("References")]
    public DialogUI dialogUI;
    public GeneratedDialogManager generatedManager;

    [Header("Prefab (Optional)")]
    [Tooltip("Kalau DialogUI belum ada di scene, akan di-instantiate dari prefab ini.")]
    public GameObject dialogUIPrefab;

    [Header("Dialog Sequences (isi semua asset sequence yang dipakai)")]
    public DialogSequence[] allSequences;

    [Header("Current State")]
    public int currentAreaIndex = 0;

    // Events
    public System.Action<string> OnAreaComplete;

    // Singleton
    public static DialogManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupReferences();
    }

    void SetupReferences()
    {
        EnsureDialogUIExists();

        if (generatedManager == null)
            generatedManager = FindObjectOfType<GeneratedDialogManager>();

        if (dialogUI != null)
            dialogUI.OnSequenceComplete += OnSequenceComplete;
    }

    void EnsureDialogUIExists()
    {
        if (dialogUI == null)
            dialogUI = FindObjectOfType<DialogUI>();

        if (dialogUI == null && dialogUIPrefab != null)
        {
            GameObject dialogUIObj = Instantiate(dialogUIPrefab);
            dialogUIObj.name = "DialogUI";
            dialogUI = dialogUIObj.GetComponent<DialogUI>();
            Debug.Log("DialogUI instantiated from prefab");
        }

        if (dialogUI == null)
            Debug.LogError("DialogManager: No DialogUI found and no prefab assigned! Dialog system will not work.");
    }

    // MAIN PUBLIC METHODS
    public void PlaySequence(DialogSequence sequence)
    {
        if (dialogUI != null && sequence != null)
        {
            dialogUI.PlaySequence(sequence);
        }
        else
        {
            Debug.LogError("DialogUI or sequence is null!");
        }
    }

    public void PlaySequenceByName(string sequenceName)
    {
        DialogSequence sequence = FindSequenceByName(sequenceName);
        if (sequence != null)
        {
            PlaySequence(sequence);
        }
        else
        {
            Debug.LogError($"Sequence not found: {sequenceName}");
        }
    }

    public void PlaySequenceByArea(string areaName)
    {
        DialogSequence sequence = FindSequenceByArea(areaName);
        if (sequence != null)
        {
            PlaySequence(sequence);
        }
        else
        {
            Debug.LogError($"No sequence found for area: {areaName}");
        }
    }

    // AREA MANAGEMENT
    public void MoveToArea(int areaIndex)
    {
        currentAreaIndex = areaIndex;
        string areaName = GetAreaName(areaIndex);

        if (!string.IsNullOrEmpty(areaName))
            PlaySequenceByArea(areaName);
    }

    string GetAreaName(int index)
    {
        switch (index)
        {
            case 0: return "Opening";
            case 1: return "Home";
            case 2: return "TownHall";
            case 3: return "StudyRoom";
            case 4: return "Warehouse";
            default: return "";
        }
    }

    void OnSequenceComplete()
    {
        // Check next via Inspector reference
        DialogSequence currentSequence = dialogUI.GetCurrentSequence();

        if (currentSequence != null)
        {
            DialogSequence nextSequence = currentSequence.GetNextSequence();
            if (nextSequence != null)
            {
                Debug.Log($"Auto-playing next sequence: {nextSequence.name}");
                PlaySequence(nextSequence);
                return; // continue chaining, jangan trigger OnAreaComplete
            }
        }

        // Area completion (fallback)
        string currentArea = GetAreaName(currentAreaIndex);
        Debug.Log($"Area {currentArea} completed");
        OnAreaComplete?.Invoke(currentArea);
    }

    // UTILITY METHODS
    public DialogSequence FindSequenceByName(string sequenceName)
    {
        if (allSequences == null || string.IsNullOrEmpty(sequenceName)) return null;

        foreach (DialogSequence sequence in allSequences)
        {
            if (sequence == null) continue;

            // Cocokkan nama asset atau areaName (case-insensitive)
            if (string.Equals(sequence.name, sequenceName, System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(sequence.areaName, sequenceName, System.StringComparison.OrdinalIgnoreCase))
            {
                return sequence;
            }
        }
        return null;
    }

    DialogSequence FindSequenceByArea(string areaName)
    {
        if (allSequences == null || string.IsNullOrEmpty(areaName)) return null;

        foreach (DialogSequence sequence in allSequences)
        {
            if (sequence == null) continue;

            if (string.Equals(sequence.areaName, areaName, System.StringComparison.OrdinalIgnoreCase))
                return sequence;
        }
        return null;
    }

    public bool IsDialogActive()
    {
        return dialogUI != null && dialogUI.IsActive();
    }

    // DEBUG METHODS
    [ContextMenu("Test Opening")]
    public void TestOpening()
    {
        PlaySequenceByArea("Opening");
    }

    void OnDestroy()
    {
        if (dialogUI != null)
            dialogUI.OnSequenceComplete -= OnSequenceComplete;
    }
}
