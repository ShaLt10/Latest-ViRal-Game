// PELETAKAN: Scripts/Dialog/DialogSequence.cs
using UnityEngine;

[System.Serializable]
public class NPCDialogData
{
    public string[] dialogLines;
}

public enum DialogType
{
    Normal,      // Dialog biasa
    Objective,   // Arahan/objective untuk player
    WinResult,   // Response win condition
    LoseResult,  // Response lose condition
    NPCRandom    // Dialog random dari NPC
}

public enum SequenceType
{
    Normal,      // Default - tidak auto-lanjut (standalone)
    Linear,      // Play otomatis ke next
    Conditional, // Tunggu condition (win/lose)
    Standalone   // Tidak auto-lanjut (deprecated, use Normal)
}

[System.Serializable]
public class DialogLine
{
    [Header("Speaker & Content")]
    public string speaker = "";
    [TextArea(2, 6)] public string text = "";
    public string portraitExpression = "Default";
    
    [Header("Type & Behavior")]
    public DialogType dialogType = DialogType.Normal;
    
    [Header("Generated Dialog (NPC Random)")]
    [Tooltip("Use dialog dari JSON GeneratedDialogManager?")]
    public bool useGeneratedDialog = false;
    [Tooltip("Nama file JSON (optional, kalau berbeda dari speaker name)")]
    public string npcJsonFile = "";
    
    [Header("Action Text (Optional)")]
    [Tooltip("Tampilkan action text? (misal: *membuka pintu*)")]
    public bool hasAction = false;
    [TextArea(1, 2)] public string actionText = "";
}

[CreateAssetMenu(fileName = "DialogSequence", menuName = "Dialog/Dialog Sequence")]
public class DialogSequence : ScriptableObject
{
    [Header("Sequence Info")]
    [Tooltip("Nama area/identifier untuk sequence ini")]
    public string areaName = "";
    
    [Tooltip("Tipe sequence - Normal: standalone, Linear: auto-next, Conditional: win/lose")]
    public SequenceType sequenceType = SequenceType.Normal;
    
    [Header("Dialog Content")]
    [Tooltip("Array of dialog lines untuk sequence ini")]
    public DialogLine[] lines;
    
    [Header("Branching (untuk Conditional)")]
    [Tooltip("Sequence yang dimainkan jika WIN condition")]
    public DialogSequence winSequence;
    
    [Tooltip("Sequence yang dimainkan jika LOSE condition")]
    public DialogSequence loseSequence;
    
    [Header("Linear Flow (untuk Linear type)")]
    [Tooltip("Next sequence yang auto-play setelah sequence ini selesai")]
    public DialogSequence nextDialogSequence;
    
    // Public getters
    public DialogSequence GetNextSequence() => nextDialogSequence;
    public DialogSequence GetWinSequence() => winSequence;
    public DialogSequence GetLoseSequence() => loseSequence;
    
    // Helper methods
    public bool HasNextSequence() => nextDialogSequence != null;
    public bool HasBranching() => winSequence != null || loseSequence != null;
    public bool IsConditional() => sequenceType == SequenceType.Conditional;
    public bool IsLinear() => sequenceType == SequenceType.Linear;
    
    // Validation
    public bool IsValid()
    {
        if (lines == null || lines.Length == 0)
            return false;
        
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.text))
                return false;
        }
        
        return true;
    }
    
    #if UNITY_EDITOR
    // Editor helper untuk quick preview
    [ContextMenu("Print Sequence Info")]
    private void PrintInfo()
    {
        Debug.Log($"=== {name} ===");
        Debug.Log($"Area: {areaName}");
        Debug.Log($"Type: {sequenceType}");
        Debug.Log($"Lines: {lines?.Length ?? 0}");
        Debug.Log($"Has Next: {HasNextSequence()}");
        Debug.Log($"Has Branching: {HasBranching()}");
    }
    #endif
}