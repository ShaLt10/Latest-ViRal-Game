// ObjectiveManager.cs - FIXED for Simple DialogManager
// PELETAKAN: Scripts/Quest/ObjectiveManager.cs
// ATTACH KE: GameObject "ObjectiveManager" (atau DialogManager)
using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    // ========================================
    // SINGLETON
    // ========================================
    public static ObjectiveManager Instance { get; private set; }
    
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
    
    // ========================================
    // OBJECTIVE DATA
    // ========================================
    [System.Serializable]
    public class Objective
    {
        public string objectiveID;           // ID unik (misal: "GoToWarehouse")
        public string objectiveText;         // Text yang ditampilkan
        public ObjectiveType type;           // Tipe objective
        public ObjectiveStatus status;       // Status saat ini
        
        // Conditional requirements
        public string requiredLocation;      // Lokasi yang harus dikunjungi
        public string requiredNPC;           // NPC yang harus diajak bicara
        public string requiredItem;          // Item yang harus dikumpulkan
        public int requiredCount;            // Jumlah yang harus diselesaikan
        public int currentCount;             // Progress saat ini
        
        // Dialog integration
        public DialogSequence objectiveDialog;      // Dialog saat objective dimulai
        public DialogSequence completionDialog;     // Dialog saat objective selesai
        public DialogSequence failureDialog;        // Dialog jika objective gagal
        
        // Next chain
        public string nextObjectiveID;       // Objective berikutnya (auto-trigger)
        public bool autoProgressOnComplete;  // Auto lanjut ke next objective?
        
        public Objective(string id, string text, ObjectiveType objType)
        {
            objectiveID = id;
            objectiveText = text;
            type = objType;
            status = ObjectiveStatus.Inactive;
            currentCount = 0;
            requiredCount = 1;
            autoProgressOnComplete = true;
        }
    }
    
    public enum ObjectiveType
    {
        GoToLocation,    // Pergi ke lokasi tertentu
        TalkToNPC,       // Bicara dengan NPC
        CollectItem,     // Kumpulkan item
        DefeatEnemy,     // Kalahkan musuh
        CompletePuzzle,  // Selesaikan puzzle/quiz
        Custom           // Custom logic
    }
    
    public enum ObjectiveStatus
    {
        Inactive,    // Belum dimulai
        Active,      // Sedang berjalan
        Completed,   // Selesai
        Failed       // Gagal
    }
    
    // ========================================
    // DATA STORAGE
    // ========================================
    [Header("=== OBJECTIVES DATABASE ===")]
    [Tooltip("Setup objectives secara manual di Inspector")]
    public List<Objective> allObjectives = new List<Objective>();
    
    [Header("=== CURRENT STATE ===")]
    public Objective currentObjective;
    public bool isObjectiveActive = false;
    
    [Header("=== SETTINGS ===")]
    [Tooltip("Blokir interaksi sampai objective selesai?")]
    public bool strictMode = true;
    
    [Tooltip("Show UI notification saat objective update?")]
    public bool showNotifications = true;
    
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;
    
    // ========================================
    // EVENTS
    // ========================================
    public Action<Objective> OnObjectiveStarted;
    public Action<Objective> OnObjectiveCompleted;
    public Action<Objective> OnObjectiveFailed;
    public Action<Objective> OnObjectiveUpdated;
    
    // ========================================
    // START OBJECTIVE
    // ========================================
    public void StartObjective(string objectiveID)
    {
        var objective = GetObjective(objectiveID);
        
        if (objective == null)
        {
            Debug.LogError($"[ObjectiveManager] Objective tidak ditemukan: {objectiveID}");
            return;
        }
        
        if (isObjectiveActive && strictMode)
        {
            Debug.LogWarning($"[ObjectiveManager] Objective lain masih aktif! Selesaikan dulu: {currentObjective.objectiveID}");
            return;
        }
        
        // Set status
        objective.status = ObjectiveStatus.Active;
        currentObjective = objective;
        isObjectiveActive = true;
        
        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] ✓ Objective started: {objectiveID}");
        
        // Show objective dialog
        if (objective.objectiveDialog != null && DialogManager.Instance != null)
        {
            DialogManager.Instance.PlaySequence(objective.objectiveDialog);
        }
        
        // Trigger event
        OnObjectiveStarted?.Invoke(objective);
    }
    
    // ========================================
    // UPDATE PROGRESS
    // ========================================
    public void UpdateProgress(string objectiveID, int amount = 1)
    {
        var objective = GetObjective(objectiveID);
        
        if (objective == null || objective.status != ObjectiveStatus.Active)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[ObjectiveManager] Cannot update: {objectiveID} (not active)");
            return;
        }
        
        objective.currentCount += amount;
        
        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] Progress: {objectiveID} ({objective.currentCount}/{objective.requiredCount})");
        
        // Check completion
        if (objective.currentCount >= objective.requiredCount)
        {
            CompleteObjective(objectiveID);
        }
        else
        {
            OnObjectiveUpdated?.Invoke(objective);
        }
    }
    
    // ========================================
    // COMPLETE OBJECTIVE
    // ========================================
    public void CompleteObjective(string objectiveID)
    {
        var objective = GetObjective(objectiveID);
        
        if (objective == null)
        {
            Debug.LogError($"[ObjectiveManager] Objective tidak ditemukan: {objectiveID}");
            return;
        }
        
        if (objective.status == ObjectiveStatus.Completed)
        {
            if (showDebugLogs)
                Debug.Log($"[ObjectiveManager] Objective sudah complete: {objectiveID}");
            return;
        }
        
        // Set status
        objective.status = ObjectiveStatus.Completed;
        objective.currentCount = objective.requiredCount;
        
        if (currentObjective == objective)
        {
            isObjectiveActive = false;
            currentObjective = null;
        }
        
        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] ✓✓✓ OBJECTIVE COMPLETED: {objectiveID}");
        
        // Show completion dialog
        if (objective.completionDialog != null && DialogManager.Instance != null)
        {
            DialogManager.Instance.PlaySequence(objective.completionDialog);
        }
        
        // Trigger event
        OnObjectiveCompleted?.Invoke(objective);
        
        // Auto-progress ke next objective?
        if (objective.autoProgressOnComplete && !string.IsNullOrEmpty(objective.nextObjectiveID))
        {
            if (showDebugLogs)
                Debug.Log($"[ObjectiveManager] Auto-progressing to: {objective.nextObjectiveID}");
            
            StartObjective(objective.nextObjectiveID);
        }
    }
    
    // ========================================
    // FAIL OBJECTIVE
    // ========================================
    public void FailObjective(string objectiveID)
    {
        var objective = GetObjective(objectiveID);
        
        if (objective == null) return;
        
        objective.status = ObjectiveStatus.Failed;
        
        if (currentObjective == objective)
        {
            isObjectiveActive = false;
            currentObjective = null;
        }
        
        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] ✗ Objective failed: {objectiveID}");
        
        // Show failure dialog
        if (objective.failureDialog != null && DialogManager.Instance != null)
        {
            DialogManager.Instance.PlaySequence(objective.failureDialog);
        }
        
        OnObjectiveFailed?.Invoke(objective);
    }
    
    // ========================================
    // VALIDATION - GRANULAR CONTROL
    // ========================================
    
    /// <summary>Check apakah bisa lanjut ke STORY PROGRESSION (scripted dialog)</summary>
    public bool CanProgressStory()
    {
        if (!strictMode) return true;
        
        if (isObjectiveActive)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[ObjectiveManager] Cannot progress story - selesaikan objective dulu: {currentObjective.objectiveID}");
            return false;
        }
        
        return true;
    }
    
    /// <summary>Check apakah objective tertentu sudah selesai</summary>
    public bool IsObjectiveCompleted(string objectiveID)
    {
        var obj = GetObjective(objectiveID);
        return obj != null && obj.status == ObjectiveStatus.Completed;
    }
    
    /// <summary>Check apakah NPC ini adalah target objective saat ini</summary>
    public bool IsQuestTargetNPC(string npcName)
    {
        if (currentObjective == null) return false;
        
        return currentObjective.type == ObjectiveType.TalkToNPC &&
               currentObjective.requiredNPC == npcName;
    }
    
    /// <summary>Check apakah bisa casual chat dengan NPC (generated dialog)</summary>
    public bool CanCasualChatWithNPC(string npcName)
    {
        // Casual chat SELALU allowed, bahkan saat objective aktif
        // Karena ini cuma small talk, tidak advance plot
        return true;
    }
    
    /// <summary>Check apakah bisa trigger SCRIPTED dialog dengan NPC</summary>
    public bool CanTriggerScriptedDialog(string npcName)
    {
        if (!strictMode) return true;
        
        // 1. Kalau NPC ini adalah quest target, allowed
        if (IsQuestTargetNPC(npcName))
        {
            return true;
        }
        
        // 2. Kalau ada objective aktif, block scripted dialog NPC lain
        if (isObjectiveActive)
        {
            if (showDebugLogs)
                Debug.Log($"[ObjectiveManager] Scripted dialog BLOCKED untuk {npcName} - selesaikan objective: {currentObjective.objectiveID}");
            return false;
        }
        
        return true;
    }
    
    /// <summary>Get feedback message kenapa dialog di-block</summary>
    public string GetBlockedDialogMessage()
    {
        if (currentObjective != null)
        {
            return $"Selesaikan objective dulu: {currentObjective.objectiveText}";
        }
        return "Ada objective yang harus diselesaikan.";
    }
    
    // ========================================
    // GETTERS
    // ========================================
    public Objective GetObjective(string objectiveID)
    {
        return allObjectives.Find(o => o.objectiveID == objectiveID);
    }
    
    public bool HasObjective(string objectiveID)
    {
        return GetObjective(objectiveID) != null;
    }
    
    public ObjectiveStatus GetObjectiveStatus(string objectiveID)
    {
        var obj = GetObjective(objectiveID);
        return obj != null ? obj.status : ObjectiveStatus.Inactive;
    }
    
    // ========================================
    // RESET
    // ========================================
    public void ResetObjective(string objectiveID)
    {
        var obj = GetObjective(objectiveID);
        if (obj != null)
        {
            obj.status = ObjectiveStatus.Inactive;
            obj.currentCount = 0;
            
            if (currentObjective == obj)
            {
                currentObjective = null;
                isObjectiveActive = false;
            }
        }
    }
    
    public void ResetAllObjectives()
    {
        foreach (var obj in allObjectives)
        {
            obj.status = ObjectiveStatus.Inactive;
            obj.currentCount = 0;
        }
        currentObjective = null;
        isObjectiveActive = false;
        
        if (showDebugLogs)
            Debug.Log("[ObjectiveManager] All objectives reset");
    }
    
    // ========================================
    // DEBUG TOOLS
    // ========================================
    [ContextMenu("Print All Objectives")]
    public void PrintAllObjectives()
    {
        Debug.Log("=== OBJECTIVE MANAGER STATUS ===");
        Debug.Log($"Strict Mode: {strictMode}");
        Debug.Log($"Active Objective: {(currentObjective != null ? currentObjective.objectiveID : "None")}");
        Debug.Log($"Total Objectives: {allObjectives.Count}");
        
        foreach (var obj in allObjectives)
        {
            Debug.Log($"  [{obj.status}] {obj.objectiveID}: {obj.currentCount}/{obj.requiredCount}");
        }
    }
    
    [ContextMenu("Create Sample Objective")]
    private void CreateSampleObjective()
    {
        var sample = new Objective("GoToWarehouse", "Pergi ke Warehouse", ObjectiveType.GoToLocation);
        sample.requiredLocation = "Warehouse";
        allObjectives.Add(sample);
        Debug.Log("[ObjectiveManager] Sample objective created");
    }
}