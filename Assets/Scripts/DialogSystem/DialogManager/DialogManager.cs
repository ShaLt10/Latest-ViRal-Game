using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    // === Singleton ===
    public static DialogManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("UI handler untuk menampilkan dialog (typewriter, portrait, objective, dsb).")]
    [SerializeField] private DialogUI dialogUI;

    [Header("Registered Sequences (opsional, lookup by name)")]
    [Tooltip("Daftar DialogSequence yang bisa dipanggil via string key (pakai seq.name sebagai key).")]
    [SerializeField] private DialogSequence[] registeredSequences;

    [Header("Validation Settings")]
    [Tooltip("Apakah perlu validasi objective sebelum play scripted dialog?")]
    [SerializeField] private bool enableObjectiveValidation = true;

    // Cache untuk lookup cepat
    private readonly Dictionary<string, DialogSequence> _sequenceLookup =
        new Dictionary<string, DialogSequence>();

    // Callback yang nunggu dari DialogUI
    private System.Action _pendingOnComplete;

    // === Lifecycle ===
    private void Awake()
    {
        // Simple singleton (karena AutoBootstrap sudah jamin cuma satu prefab yang di-spawn)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Cari DialogUI di diri sendiri atau anak-anak kalau belum di-assign
        if (dialogUI == null)
        {
            dialogUI = GetComponent<DialogUI>();
            if (dialogUI == null)
                dialogUI = GetComponentInChildren<DialogUI>(true);
        }

        if (dialogUI == null)
        {
            Debug.LogError("[DialogManager] DialogUI reference belum di-assign!");
        }
        else
        {
            // Daftarkan callback ketika sequence selesai
            dialogUI.OnSequenceComplete += HandleUISequenceComplete;
        }

        BuildSequenceLookup();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (dialogUI != null)
            dialogUI.OnSequenceComplete -= HandleUISequenceComplete;
    }

    public bool IsDialogActive()
    {
        return dialogUI != null && dialogUI.IsActive();
    }
    public void PlaySequence(DialogSequence sequence, System.Action onDone = null, bool bypassValidation = false)
    {
        if (dialogUI == null)
        {
            Debug.LogError("[DialogManager] dialogUI null, nggak bisa PlaySequence.");
            onDone?.Invoke();
            return;
        }

        if (sequence == null)
        {
            Debug.LogError("[DialogManager] Sequence null di PlaySequence.");
            onDone?.Invoke();
            return;
        }
        if (!bypassValidation && enableObjectiveValidation)
        {
            if (!CanPlayScriptedDialog(sequence))
            {
                Debug.LogWarning($"[DialogManager] Scripted dialog '{sequence.name}' blocked by objective validation.");
                onDone?.Invoke();
                return;
            }
        }

        _pendingOnComplete = onDone;
        dialogUI.PlaySequence(sequence);
    }

    public void PlaySequenceByName(string key)
    {
        PlaySequenceByName(key, null);
    }

    public void PlaySequenceByName(string key, System.Action onDone)
    {
        DialogSequence seq = FindSequence(key);
        if (seq == null)
        {
            Debug.LogError($"[DialogManager] Nggak nemu DialogSequence dengan key: {key}");
            onDone?.Invoke();
            return;
        }

        PlaySequence(seq, onDone);
    }
    public void PlaySequenceThen(string key, System.Action onDone)
    {
        PlaySequenceByName(key, onDone);
    }

    public void PlayCasualDialog(DialogSequence sequence, System.Action onDone = null)
    {
        PlaySequence(sequence, onDone, bypassValidation: true);
    }

    // === Validation (Integration dengan ObjectiveManager) ===

    private bool CanPlayScriptedDialog(DialogSequence sequence)
    {
        // Kalau nggak ada ObjectiveManager, allowed
        if (ObjectiveManager.Instance == null)
            return true;

        // Check tipe dialog - kalau generated/casual, skip validation
        if (IsGeneratedDialog(sequence))
            return true;

        // Check dengan ObjectiveManager
        // Kita anggap scripted dialog itu "story progression"
        return ObjectiveManager.Instance.CanProgressStory();
    }
    private bool IsGeneratedDialog(DialogSequence sequence)
    {
        if (sequence == null) return false;

        // Check 1: Kalau sequence bukan dari asset (runtime instance)
        if (!IsAssetSequence(sequence))
            return true;

        // Check 2: Kalau area name punya prefix "Generated_" atau "Casual_"
        if (sequence.areaName.StartsWith("Generated_") || 
            sequence.areaName.StartsWith("Casual_"))
            return true;

        // Check 3: Kalau cuma 1 line dan pakai generated dialog flag
        if (sequence.lines != null && sequence.lines.Length == 1 && 
            sequence.lines[0].useGeneratedDialog)
            return true;

        return false;
    }
    private bool IsAssetSequence(DialogSequence sequence)
    {
        // Kalau ada di registered sequences, berarti asset
        foreach (var seq in registeredSequences)
        {
            if (seq == sequence)
                return true;
        }

        // Alternatif: check via asset path
        #if UNITY_EDITOR
        string path = UnityEditor.AssetDatabase.GetAssetPath(sequence);
        return !string.IsNullOrEmpty(path);
        #else
        return false;
        #endif
    }

    // === Internal ===

    private void HandleUISequenceComplete()
    {
        var cb = _pendingOnComplete;
        _pendingOnComplete = null;

        cb?.Invoke();
    }

    private void BuildSequenceLookup()
    {
        _sequenceLookup.Clear();

        if (registeredSequences == null) return;

        foreach (var seq in registeredSequences)
        {
            if (seq == null) continue;

            string key = seq.name.Trim();
            if (string.IsNullOrEmpty(key)) continue;

            if (_sequenceLookup.ContainsKey(key))
            {
                Debug.LogWarning($"[DialogManager] Duplicate sequence name: {key}. Pakai yang pertama saja.");
                continue;
            }

            _sequenceLookup.Add(key, seq);
        }
    }

    private DialogSequence FindSequence(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        key = key.Trim();

        if (_sequenceLookup.TryGetValue(key, out var seq))
            return seq;

        // Bisa tambah fallback di sini kalau mau Resources.Load, dsb.
        Debug.LogWarning($"[DialogManager] Sequence dengan nama '{key}' belum terdaftar di registeredSequences.");
        return null;
    }

    // === Debug Tools ===

    [ContextMenu("Print Registered Sequences")]
    private void PrintRegisteredSequences()
    {
        Debug.Log($"=== DialogManager - Registered Sequences ({_sequenceLookup.Count}) ===");
        foreach (var kvp in _sequenceLookup)
        {
            Debug.Log($"  [{kvp.Key}] → {kvp.Value.name} (Lines: {kvp.Value.lines?.Length ?? 0})");
        }
    }

    [ContextMenu("Test Dialog System")]
    private void TestDialogSystem()
    {
        Debug.Log("=== Dialog System Status ===");
        Debug.Log($"DialogUI: {(dialogUI != null ? "OK" : "NULL")}");
        Debug.Log($"Objective Validation: {(enableObjectiveValidation ? "ENABLED" : "DISABLED")}");
        Debug.Log($"ObjectiveManager: {(ObjectiveManager.Instance != null ? "FOUND" : "NOT FOUND")}");
        Debug.Log($"Registered Sequences: {registeredSequences?.Length ?? 0}");
    }
}