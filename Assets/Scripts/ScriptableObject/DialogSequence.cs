// DialogSequence.cs - FIXED (tanpa dependency runtime & Header di field)
using UnityEngine;
using System;

[System.Serializable]
public class DialogLine
{
    public string speaker;

    [TextArea(3, 6)]
    public string text;

    [Header("Portrait Settings (for characters only)")]
    public string portraitExpression = "Default"; // kept for future use

    [Header("Generated Dialog")]
    public bool useGeneratedDialog = false;
    public string npcJsonFile = "";

    [Header("Action Text")]
    public bool hasAction = false;
    [TextArea(1, 2)]
    public string actionText;

    [Header("Dialog Type Helper")]
    [Tooltip("Opsional: 'narrator', 'character', atau 'npc'. Kosong = auto-detect.")]
    public string dialogType = "";
}

[CreateAssetMenu(fileName = "DialogSequence", menuName = "Dialog/DialogSequence")]
public class DialogSequence : ScriptableObject
{
    public string areaName;
    public DialogLine[] lines;

    [Header("Next Dialog Sequence (Inspector chaining)")]
    public DialogSequence nextDialogSequence;

    public enum SequenceType { Mixed, Character, Narrator }

    [Header("Sequence Type")]
    [Tooltip("Mixed: campuran narrator & character. Character: hanya character. Narrator: hanya narrator.")]
    public SequenceType sequenceType = SequenceType.Mixed;

    public int GetLineCount() => (lines != null ? lines.Length : 0);

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(areaName) &&
               lines != null && lines.Length > 0;
    }

    public DialogSequence GetNextSequence() => nextDialogSequence;

    public bool IsPureNarrator()
    {
        if (sequenceType == SequenceType.Narrator) return true;
        if (sequenceType == SequenceType.Character) return false;
        if (lines == null || lines.Length == 0) return false;

        foreach (var line in lines)
        {
            if (!IsLineNarration(line))
                return false;
        }
        return true;
    }

    // Auto-detect sederhana (tanpa akses CharacterManager)
    private static bool IsLineNarration(DialogLine line)
    {
        // 1) Hormati override manual
        if (!string.IsNullOrEmpty(line.dialogType))
        {
            string t = line.dialogType.Trim().ToLowerInvariant();
            if (t == "narrator") return true;
            if (t == "character" || t == "npc") return false;
        }

        // 2) Auto: kosong atau token narrator/system
        string s = (line.speaker ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(s)) return true;

        string lowered = s.ToLowerInvariant();
        return lowered == "narrator" || lowered == "[narrator]" || lowered == "system";
    }
}

[System.Serializable]
public class NPCDialogData
{
    public string[] dialogLines;
}
