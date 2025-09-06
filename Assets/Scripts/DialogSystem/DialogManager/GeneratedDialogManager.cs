// GeneratedDialogManager.cs - FIXED VERSION
// PELETAKAN: Scripts/Dialog/GeneratedDialogManager.cs
// ATTACH KE: GameObject "DialogManager" (sama object dengan DialogManager.cs)
using UnityEngine;
using System.Collections.Generic;

public class GeneratedDialogManager : MonoBehaviour
{
    [System.Serializable]
    public class NPCDialogFile
    {
        public string npcName;
        public TextAsset jsonFile;
    }
    
    [Header("NPC Dialog Files")]
    public NPCDialogFile[] npcDialogFiles;
    
    // Cache untuk dialog yang sudah di-load
    private Dictionary<string, string[]> loadedDialogs = new Dictionary<string, string[]>();
    
    void Start()
    {
        LoadAllGeneratedDialogs();
    }
    
    void LoadAllGeneratedDialogs()
    {
        foreach (var npcFile in npcDialogFiles)
        {
            if (npcFile.jsonFile != null)
            {
                try
                {
                    // Parse JSON menggunakan NPCDialogData dari DialogSequence.cs
                    NPCDialogData dialogData = JsonUtility.FromJson<NPCDialogData>(npcFile.jsonFile.text);
                    loadedDialogs[npcFile.npcName] = dialogData.dialogLines;
                    
                    Debug.Log($"Loaded {dialogData.dialogLines.Length} dialogs for {npcFile.npcName}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load dialog for {npcFile.npcName}: {e.Message}");
                }
            }
        }
    }
    
    public string GetRandomDialog(string npcName)
    {
        if (loadedDialogs.ContainsKey(npcName))
        {
            string[] dialogs = loadedDialogs[npcName];
            if (dialogs != null && dialogs.Length > 0)
            {
                int randomIndex = Random.Range(0, dialogs.Length);
                return dialogs[randomIndex];
            }
        }
        
        // Fallback dialog
        return GetFallbackDialog(npcName);
    }
    
    string GetFallbackDialog(string npcName)
    {
        switch (npcName.ToLower())
        {
            case "jack":
                return "Halo, apa kabar?";
            case "omar":
                return "Bagaimana harimu?";
            case "pak satya":
            case "paksatya":
                return "Selamat siang.";
            case "ethan":
                return "Hey there!";
            case "kanaya":
                return "Hai teman!";
            default:
                return "...";
        }
    }
    
    public bool HasDialogFor(string npcName)
    {
        return loadedDialogs.ContainsKey(npcName);
    }
    
    public int GetDialogCount(string npcName)
    {
        if (loadedDialogs.ContainsKey(npcName))
        {
            return loadedDialogs[npcName].Length;
        }
        return 0;
    }
    
    [ContextMenu("Reload All Dialogs")]
    public void ReloadAllDialogs()
    {
        loadedDialogs.Clear();
        LoadAllGeneratedDialogs();
    }
}