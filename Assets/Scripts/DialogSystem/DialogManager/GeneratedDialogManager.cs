// GeneratedDialogManager.cs - ENHANCED VERSION (Optional Upgrades)
// PELETAKAN: Scripts/Dialog/GeneratedDialogManager.cs
// ATTACH KE: GameObject "DialogManager" (sama object dengan DialogManager.cs)
using UnityEngine;
using System.Collections.Generic;

public class GeneratedDialogManager : MonoBehaviour
{
    // ========================================
    // SINGLETON (Optional Enhancement)
    // ========================================
    public static GeneratedDialogManager Instance { get; private set; }
    
    [System.Serializable]
    public class NPCDialogFile
    {
        public string npcName;
        public TextAsset jsonFile;
        [Tooltip("Karakter hybrid (main di gameplay tapi juga NPC)")]
        public bool isHybridCharacter = false;
    }
    
    [Header("NPC Dialog Files")]
    public NPCDialogFile[] npcDialogFiles;
    
    [Header("Hybrid Character Settings")]
    [Tooltip("Nama karakter hybrid (contoh: Kanaya)")]
    public string[] hybridCharacterNames = new string[] { "Kanaya" };
    
    [Header("Debug Settings")]
    [Tooltip("Show detailed logs for dialog loading")]
    public bool verboseLogging = false;
    
    // Cache untuk dialog yang sudah di-load
    private Dictionary<string, string[]> loadedDialogs = new Dictionary<string, string[]>();
    
    // Track index terakhir untuk sequential mode
    private Dictionary<string, int> lastUsedIndex = new Dictionary<string, int>();
    
    // Track hybrid status
    private Dictionary<string, bool> isHybrid = new Dictionary<string, bool>();
    
    // ========================================
    // INITIALIZATION
    // ========================================
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[GeneratedDialogManager] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        LoadAllGeneratedDialogs();
    }
    
    void LoadAllGeneratedDialogs()
    {
        int successCount = 0;
        int failCount = 0;
        
        if (npcDialogFiles == null || npcDialogFiles.Length == 0)
        {
            Debug.LogWarning("[GeneratedDialogManager] No NPC dialog files assigned!");
            return;
        }
        
        foreach (var npcFile in npcDialogFiles)
        {
            if (npcFile.jsonFile != null && !string.IsNullOrEmpty(npcFile.npcName))
            {
                try
                {
                    // Parse JSON menggunakan NPCDialogData dari DialogSequence.cs
                    NPCDialogData dialogData = JsonUtility.FromJson<NPCDialogData>(npcFile.jsonFile.text);
                    
                    if (dialogData.dialogLines != null && dialogData.dialogLines.Length > 0)
                    {
                        loadedDialogs[npcFile.npcName] = dialogData.dialogLines;
                        isHybrid[npcFile.npcName] = npcFile.isHybridCharacter;
                        
                        string hybridTag = npcFile.isHybridCharacter ? " (HYBRID)" : "";
                        
                        if (verboseLogging)
                        {
                            Debug.Log($"[GeneratedDialogManager] ✓ Loaded {dialogData.dialogLines.Length} dialogs for {npcFile.npcName}{hybridTag}");
                        }
                        
                        successCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[GeneratedDialogManager] {npcFile.npcName}: JSON has no dialog lines!");
                        failCount++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[GeneratedDialogManager] Failed to load dialog for {npcFile.npcName}: {e.Message}");
                    failCount++;
                }
            }
            else
            {
                Debug.LogWarning($"[GeneratedDialogManager] Skipped incomplete entry (missing name or JSON file)");
                failCount++;
            }
        }
        
        Debug.Log($"[GeneratedDialogManager] Loaded {successCount} NPC dialog files. {(failCount > 0 ? $"Failed: {failCount}" : "")}");
    }
    
    // ========================================
    // PUBLIC API - CORE METHODS
    // ========================================
    
    /// <summary>Get random dialog (bisa repeat)</summary>
    public string GetRandomDialog(string npcName)
    {
        if (string.IsNullOrEmpty(npcName))
        {
            Debug.LogWarning("[GeneratedDialogManager] GetRandomDialog called with empty npcName!");
            return "...";
        }
        
        if (loadedDialogs.ContainsKey(npcName))
        {
            string[] dialogs = loadedDialogs[npcName];
            if (dialogs != null && dialogs.Length > 0)
            {
                int randomIndex = Random.Range(0, dialogs.Length);
                return dialogs[randomIndex];
            }
        }
        
        if (verboseLogging)
        {
            Debug.Log($"[GeneratedDialogManager] No dialog found for '{npcName}', using fallback");
        }
        
        // Fallback dialog
        return GetFallbackDialog(npcName);
    }
    
    /// <summary>Get sequential dialog (tidak repeat sampai semua terpakai)</summary>
    public string GetSequentialDialog(string npcName)
    {
        if (string.IsNullOrEmpty(npcName))
        {
            Debug.LogWarning("[GeneratedDialogManager] GetSequentialDialog called with empty npcName!");
            return "...";
        }
        
        if (!loadedDialogs.ContainsKey(npcName))
        {
            if (verboseLogging)
            {
                Debug.Log($"[GeneratedDialogManager] No dialog found for '{npcName}', using fallback");
            }
            return GetFallbackDialog(npcName);
        }
        
        string[] dialogs = loadedDialogs[npcName];
        if (dialogs == null || dialogs.Length == 0)
            return GetFallbackDialog(npcName);
        
        // Get last used index
        if (!lastUsedIndex.ContainsKey(npcName))
            lastUsedIndex[npcName] = -1;
        
        // Move to next index (loop back to 0 after reaching end)
        int nextIndex = (lastUsedIndex[npcName] + 1) % dialogs.Length;
        lastUsedIndex[npcName] = nextIndex;
        
        if (verboseLogging)
        {
            Debug.Log($"[GeneratedDialogManager] Sequential dialog for {npcName}: index {nextIndex}/{dialogs.Length - 1}");
        }
        
        return dialogs[nextIndex];
    }
    
    /// <summary>Check if character is hybrid (ada di gameplay & NPC pool)</summary>
    public bool IsHybridCharacter(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
            return false;
        
        // Check dari dictionary cache
        if (isHybrid.ContainsKey(characterName))
            return isHybrid[characterName];
        
        // Check dari array fallback
        foreach (var name in hybridCharacterNames)
        {
            if (name.Equals(characterName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        
        return false;
    }
    
    // ========================================
    // PUBLIC API - UTILITY METHODS
    // ========================================
    
    public bool HasDialogFor(string npcName)
    {
        return !string.IsNullOrEmpty(npcName) && loadedDialogs.ContainsKey(npcName);
    }
    
    public int GetDialogCount(string npcName)
    {
        if (loadedDialogs.ContainsKey(npcName))
        {
            return loadedDialogs[npcName].Length;
        }
        return 0;
    }
    
    /// <summary>Reset sequential counter untuk NPC tertentu</summary>
    public void ResetSequence(string npcName)
    {
        if (lastUsedIndex.ContainsKey(npcName))
        {
            lastUsedIndex[npcName] = -1;
            if (verboseLogging)
            {
                Debug.Log($"[GeneratedDialogManager] Reset sequence for {npcName}");
            }
        }
    }
    
    /// <summary>Reset semua sequential counters</summary>
    public void ResetAllSequences()
    {
        lastUsedIndex.Clear();
        Debug.Log("[GeneratedDialogManager] All sequences reset");
    }
    
    /// <summary>Get list of all loaded NPC names</summary>
    public string[] GetLoadedNPCNames()
    {
        string[] names = new string[loadedDialogs.Count];
        loadedDialogs.Keys.CopyTo(names, 0);
        return names;
    }
    
    // ========================================
    // FALLBACK DIALOG
    // ========================================
    
    string GetFallbackDialog(string npcName)
    {
        string lowerName = npcName.ToLower();
        
        switch (lowerName)
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
    
    // ========================================
    // DEBUG TOOLS (Context Menu)
    // ========================================
    
    [ContextMenu("Reload All Dialogs")]
    public void ReloadAllDialogs()
    {
        loadedDialogs.Clear();
        lastUsedIndex.Clear();
        isHybrid.Clear();
        LoadAllGeneratedDialogs();
    }
    
    [ContextMenu("Print Dialog Status")]
    void PrintDialogStatus()
    {
        Debug.Log("=== Generated Dialog Manager Status ===");
        Debug.Log($"Total NPCs loaded: {loadedDialogs.Count}");
        
        foreach (var kvp in loadedDialogs)
        {
            string hybridTag = isHybrid.ContainsKey(kvp.Key) && isHybrid[kvp.Key] ? " [HYBRID]" : "";
            int currentIndex = lastUsedIndex.ContainsKey(kvp.Key) ? lastUsedIndex[kvp.Key] : -1;
            Debug.Log($"  • {kvp.Key}: {kvp.Value.Length} dialogs{hybridTag} (current index: {currentIndex})");
        }
    }
    
    [ContextMenu("Validate Setup")]
    private void ValidateSetup()
    {
        Debug.Log("=== Validating GeneratedDialogManager Setup ===");
        
        int issues = 0;
        
        // Check if any files assigned
        if (npcDialogFiles == null || npcDialogFiles.Length == 0)
        {
            Debug.LogWarning("⚠️ No NPC dialog files assigned!");
            issues++;
        }
        else
        {
            // Check each entry
            foreach (var npcFile in npcDialogFiles)
            {
                if (string.IsNullOrEmpty(npcFile.npcName))
                {
                    Debug.LogWarning("⚠️ NPC Dialog File with empty name!");
                    issues++;
                }
                
                if (npcFile.jsonFile == null)
                {
                    Debug.LogWarning($"⚠️ {npcFile.npcName}: No JSON file assigned!");
                    issues++;
                }
            }
        }
        
        if (issues == 0)
        {
            Debug.Log("✅ All checks passed!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Found {issues} potential issues.");
        }
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Preview Random Dialog (All NPCs)")]
    private void PreviewAllNPCs()
    {
        if (!Application.isPlaying)
        {
            LoadAllGeneratedDialogs();
        }
        
        Debug.Log("=== Dialog Preview ===");
        foreach (var npcName in loadedDialogs.Keys)
        {
            string dialog = GetRandomDialog(npcName);
            Debug.Log($"[{npcName}] \"{dialog}\"");
        }
    }
    #endif
}