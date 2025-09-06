// SimpleTrigger.cs
// PELETAKAN: GameObject NPC atau interactive objects
// FUNGSI: Simple click trigger untuk dialog, works on Windows & Android
using UnityEngine;

public class SimpleTrigger : MonoBehaviour
{
    [Header("Dialog Settings")]
    public DialogSequence dialogToTrigger;
    public string dialogSequenceName = ""; // Alternative: trigger by name
    
    [Header("Trigger Settings")]
    public bool triggerOnce = false;
    public LayerMask clickableLayer = -1;
    
    [Header("Visual Feedback")]
    public GameObject clickHint; // Optional UI hint
    
    private bool hasTriggered = false;
    private DialogManager dialogManager;
    
    void Start()
    {
        // Find DialogManager (DontDestroyOnLoad object)
        dialogManager = FindObjectOfType<DialogManager>();
        
        if (clickHint != null)
            clickHint.SetActive(false);
    }
    
    void Update()
    {
        HandleClickInput();
    }
    
    void HandleClickInput()
    {
        // Handle both mouse click (Windows) and touch (Android)
        bool inputDetected = false;
        Vector3 inputPosition = Vector3.zero;
        
        // Mouse input (Windows)
        if (Input.GetMouseButtonDown(0))
        {
            inputDetected = true;
            inputPosition = Input.mousePosition;
        }
        
        // Touch input (Android)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputDetected = true;
            inputPosition = Input.GetTouch(0).position;
        }
        
        if (inputDetected)
        {
            CheckClickOnObject(inputPosition);
        }
    }
    
    void CheckClickOnObject(Vector3 screenPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        Ray ray = cam.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
        {
            if (hit.collider.gameObject == gameObject)
            {
                TriggerDialog();
            }
        }
    }
    
    void TriggerDialog()
    {
        // Check conditions
        if (triggerOnce && hasTriggered)
        {
            Debug.Log($"{gameObject.name} dialog already triggered");
            return;
        }
        
        if (dialogManager == null)
        {
            Debug.LogError("DialogManager not found!");
            return;
        }
        
        // Check if dialog currently active
        if (dialogManager.IsDialogActive())
        {
            Debug.Log("Another dialog is currently active");
            return;
        }
        
        // Trigger dialog
        bool success = false;
        
        if (dialogToTrigger != null)
        {
            dialogManager.PlaySequence(dialogToTrigger);
            success = true;
        }
        else if (!string.IsNullOrEmpty(dialogSequenceName))
        {
            dialogManager.PlaySequenceByName(dialogSequenceName);
            success = true;
        }
        
        if (success)
        {
            hasTriggered = true;
            Debug.Log($"Dialog triggered: {gameObject.name}");
            
            if (clickHint != null)
                clickHint.SetActive(false);
        }
        else
        {
            Debug.LogError($"No valid dialog assigned to {gameObject.name}");
        }
    }
    
    // Show/hide click hint
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
    
    // PUBLIC METHODS
    public void ManualTrigger()
    {
        TriggerDialog();
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}

// AutoDialogTrigger.cs  
// PELETAKAN: GameObject "AutoTrigger" di scene yang perlu auto-start dialog
// FUNGSI: Auto trigger dialog saat scene load atau kondisi tertentu
public class AutoDialogTrigger : MonoBehaviour
{
    [Header("Auto Trigger Settings")]
    public DialogSequence autoSequence;
    public float triggerDelay = 1f;
    public bool triggerOnStart = true;
    
    [Header("Conditions")]
    public bool requireCharacterSelected = true;
    
    private DialogManager dialogManager;
    
    void Start()
    {
        dialogManager = FindObjectOfType<DialogManager>();
        
        if (triggerOnStart)
        {
            StartCoroutine(DelayedAutoTrigger());
        }
    }
    
    System.Collections.IEnumerator DelayedAutoTrigger()
    {
        yield return new WaitForSeconds(triggerDelay);
        
        // Check conditions
        if (requireCharacterSelected && CharacterManager.Instance != null)
        {
            if (!CharacterManager.Instance.HasSelectedCharacter())
            {
                Debug.Log("Waiting for character selection...");
                yield break;
            }
        }
        
        // Trigger dialog
        if (dialogManager != null && autoSequence != null)
        {
            dialogManager.PlaySequence(autoSequence);
        }
    }
    
    // Manual trigger method
    public void TriggerAutoDialog()
    {
        if (dialogManager != null && autoSequence != null)
        {
            dialogManager.PlaySequence(autoSequence);
        }
    }
}