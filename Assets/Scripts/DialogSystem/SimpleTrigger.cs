// SimpleTrigger.cs - FIXED FOR 2D GAME
// PELETAKAN: Attach ke GameObject NPC atau interactive objects
// FUNGSI: Click interaction untuk 2D sprites
using UnityEngine;

public class SimpleTrigger : MonoBehaviour
{
    public enum TriggerMode
    {
        StorySequence,  // Dialog berurutan (pakai DialogSequence)
        NPCRandom       // Dialog random 1 line (langsung ke UI)
    }
    
    [Header("=== TRIGGER MODE ===")]
    public TriggerMode mode = TriggerMode.NPCRandom;
    
    [Header("=== STORY SEQUENCE (mode: StorySequence) ===")]
    [Tooltip("Drag DialogSequence asset untuk story mode")]
    public DialogSequence dialogToTrigger;
    
    [Tooltip("Atau isi nama sequence (lookup by name)")]
    public string dialogSequenceName = "";
    
    [Header("=== NPC RANDOM (mode: NPCRandom) ===")]
    [Tooltip("Nama NPC untuk ambil dialog dari JSON")]
    public string npcName = "";
    
    [Tooltip("Sequential = berurutan tanpa repeat. Random = acak bisa repeat")]
    public bool useSequentialDialog = true;
    
    [Header("=== TRIGGER SETTINGS ===")]
    [Tooltip("Trigger hanya sekali?")]
    public bool triggerOnce = false;
    
    [Header("=== VISUAL FEEDBACK ===")]
    [Tooltip("Icon yang muncul saat hover (optional)")]
    public GameObject clickHint;
    
    [Header("=== DEBUG ===")]
    public bool showDebugLogs = true;
    
    // Internal state
    private bool hasTriggered = false;
    private DialogManager dialogManager;
    private GeneratedDialogManager generatedManager;
    
    // =============================================
    // INITIALIZATION
    // =============================================
    void Start()
    {
        // Find managers
        dialogManager = DialogManager.Instance;
        if (dialogManager == null)
            dialogManager = FindObjectOfType<DialogManager>();
        
        generatedManager = FindObjectOfType<GeneratedDialogManager>();
        
        // Hide click hint initially
        if (clickHint != null)
            clickHint.SetActive(false);
        
        // Validate setup
        ValidateSetup();
    }
    
    void ValidateSetup()
    {
        if (mode == TriggerMode.StorySequence)
        {
            if (dialogToTrigger == null && string.IsNullOrEmpty(dialogSequenceName))
            {
                Debug.LogWarning($"[SimpleTrigger] {gameObject.name}: Story mode tapi dialog tidak di-assign!");
            }
        }
        else if (mode == TriggerMode.NPCRandom)
        {
            if (string.IsNullOrEmpty(npcName))
            {
                Debug.LogWarning($"[SimpleTrigger] {gameObject.name}: NPC mode tapi npcName kosong!");
            }
            
            if (generatedManager != null && !generatedManager.HasDialogFor(npcName))
            {
                Debug.LogWarning($"[SimpleTrigger] {gameObject.name}: Tidak ada dialog JSON untuk '{npcName}'!");
            }
        }
    }
    
    // =============================================
    // CLICK DETECTION (2D - OnMouseDown)
    // =============================================
    void OnMouseDown()
    {
        // ✅ Method ini otomatis dipanggil saat 2D collider di-click
        // Work untuk PC mouse DAN Android touch!
        TriggerDialog();
    }
    
    // =============================================
    // TRIGGER DIALOG
    // =============================================
    void TriggerDialog()
    {
        // Check if already triggered
        if (triggerOnce && hasTriggered)
        {
            if (showDebugLogs)
                Debug.Log($"[SimpleTrigger] {gameObject.name} sudah triggered (triggerOnce)");
            return;
        }
        
        // Check if dialog currently active
        if (dialogManager != null && dialogManager.IsDialogActive())
        {
            if (showDebugLogs)
                Debug.Log("[SimpleTrigger] Dialog lain sedang aktif");
            return;
        }
        
        // ✨ NEW: Check objective validation (optional)
        if (!CanTriggerDialog())
        {
            if (showDebugLogs)
                Debug.Log($"[SimpleTrigger] {gameObject.name} blocked by objective");
            return;
        }
        
        // Execute based on mode
        bool success = false;
        
        switch (mode)
        {
            case TriggerMode.StorySequence:
                success = TriggerStorySequence();
                break;
                
            case TriggerMode.NPCRandom:
                success = TriggerNPCRandom();
                break;
        }
        
        if (success)
        {
            hasTriggered = true;
            
            if (showDebugLogs)
                Debug.Log($"[SimpleTrigger] ✓ Dialog triggered: {gameObject.name} (mode: {mode})");
            
            if (clickHint != null)
                clickHint.SetActive(false);
        }
    }
    
    // =============================================
    // VALIDATION (Integration dengan ObjectiveManager)
    // =============================================
    bool CanTriggerDialog()
    {
        // Kalau nggak ada ObjectiveManager, always allowed
        if (ObjectiveManager.Instance == null)
            return true;
        
        // Story sequence harus di-validate
        if (mode == TriggerMode.StorySequence)
        {
            return ObjectiveManager.Instance.CanProgressStory();
        }
        
        // NPC Random (casual dialog) always allowed
        return true;
    }
    
    // =============================================
    // STORY SEQUENCE MODE
    // =============================================
    bool TriggerStorySequence()
    {
        if (dialogManager == null)
        {
            Debug.LogError("[SimpleTrigger] DialogManager tidak ditemukan!");
            return false;
        }
        
        // Priority: direct reference > name lookup
        if (dialogToTrigger != null)
        {
            dialogManager.PlaySequence(dialogToTrigger);
            return true;
        }
        else if (!string.IsNullOrEmpty(dialogSequenceName))
        {
            dialogManager.PlaySequenceByName(dialogSequenceName);
            return true;
        }
        
        Debug.LogError($"[SimpleTrigger] {gameObject.name}: Tidak ada DialogSequence yang valid!");
        return false;
    }
    
    // =============================================
    // NPC RANDOM MODE
    // =============================================
    bool TriggerNPCRandom()
    {
        if (generatedManager == null)
        {
            Debug.LogError("[SimpleTrigger] GeneratedDialogManager tidak ditemukan!");
            return false;
        }
        
        if (dialogManager == null)
        {
            Debug.LogError("[SimpleTrigger] DialogManager tidak ditemukan!");
            return false;
        }
        
        if (string.IsNullOrEmpty(npcName))
        {
            Debug.LogError($"[SimpleTrigger] {gameObject.name}: NPC name tidak di-set!");
            return false;
        }
        
        // Get dialog text from GeneratedDialogManager
        string dialogText = useSequentialDialog 
            ? generatedManager.GetSequentialDialog(npcName)
            : generatedManager.GetRandomDialog(npcName);
        
        if (showDebugLogs)
            Debug.Log($"[SimpleTrigger] NPC dialog: {dialogText}");
        
        // Create runtime sequence
        CreateAndPlayNPCDialog(npcName, dialogText);
        
        return true;
    }
    
    void CreateAndPlayNPCDialog(string speaker, string text)
    {
        // Create temporary DialogSequence untuk 1 line
        DialogSequence tempSeq = ScriptableObject.CreateInstance<DialogSequence>();
        
        tempSeq.areaName = $"Generated_{speaker}";
        tempSeq.sequenceType = SequenceType.Normal;
        
        tempSeq.lines = new DialogLine[1];
        tempSeq.lines[0] = new DialogLine
        {
            speaker = speaker,
            text = text,
            portraitExpression = "Default",
            dialogType = DialogType.Normal,
            useGeneratedDialog = false // sudah resolved
        };
        
        // Play via DialogManager (dengan bypass validation)
        if (dialogManager != null)
        {
            // Use PlayCasualDialog jika ada (dari enhanced DialogManager)
            // Fallback ke PlaySequence dengan bypass flag
            try
            {
                // Try reflection untuk call PlayCasualDialog jika ada
                var method = dialogManager.GetType().GetMethod("PlayCasualDialog");
                if (method != null)
                {
                    method.Invoke(dialogManager, new object[] { tempSeq, null });
                }
                else
                {
                    // Fallback
                    dialogManager.PlaySequence(tempSeq);
                }
            }
            catch
            {
                // Final fallback
                dialogManager.PlaySequence(tempSeq);
            }
        }
    }
    
    // =============================================
    // VISUAL FEEDBACK (Mouse Hover)
    // =============================================
    void OnMouseEnter()
    {
        if (clickHint != null && !hasTriggered)
            clickHint.SetActive(true);
    }
    
    void OnMouseExit()
    {
        if (clickHint != null)
            clickHint.SetActive(false);
    }
    
    // =============================================
    // PUBLIC API
    // =============================================
    
    /// <summary>Trigger dialog secara manual dari script lain</summary>
    public void ManualTrigger()
    {
        TriggerDialog();
    }
    
    /// <summary>Reset trigger (bisa trigger lagi)</summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        
        // Reset sequential counter jika NPC mode
        if (mode == TriggerMode.NPCRandom && generatedManager != null)
        {
            generatedManager.ResetSequence(npcName);
        }
    }
    
    /// <summary>Switch mode secara runtime</summary>
    public void SwitchMode(TriggerMode newMode)
    {
        mode = newMode;
        hasTriggered = false;
        
        if (showDebugLogs)
            Debug.Log($"[SimpleTrigger] {gameObject.name} switched to {newMode} mode");
    }
    
    // =============================================
    // GIZMOS (Visual Debug)
    // =============================================
    void OnDrawGizmos()
    {
        Color gizmoColor = mode == TriggerMode.StorySequence ? Color.magenta : Color.cyan;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        Color gizmoColor = mode == TriggerMode.StorySequence ? Color.magenta : Color.cyan;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(1.2f, 1.2f, 0f));
        
        string label = mode == TriggerMode.StorySequence 
            ? $"STORY\n{dialogSequenceName}" 
            : $"NPC\n{npcName}";
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, label);
        #endif
    }
}