// DialogUI.cs - FIXED VERSION
// PELETAKAN: Scripts/Dialog/DialogUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogUI : MonoBehaviour
{
    [Header("Roots")]
    public GameObject dialogPanel;
    public GameObject characterRoot;
    public GameObject narrationRoot;

    [Header("Character UI")]
    public TMP_Text characterSpeakerText;
    public TMP_Text characterDialogText;
    public Image characterPortraitImage;
    public TMP_Text characterActionText;

    [Header("Narration UI")]
    public TMP_Text narrationSpeakerText;
    public TMP_Text narrationDialogText;
    public TMP_Text narrationActionText;

    [Header("Controls")]
    public Button nextButton;

    [Header("Settings")]
    public float typeSpeed = 0.02f;
    public bool canSkipTypewriter = true;

    [Header("Narration Rules")]
    public bool treatEmptySpeakerAsNarration = true;
    public string[] narrationTokens = new string[] { "Narrator", "[Narrator]", "System" };
    public string narrationDisplayName = "Narrator";
    [Tooltip("Jika true, narrator name tidak ditampilkan (lebih clean)")]
    public bool hideNarratorName = true;

    [Header("Objective/Result Display")]
    [Tooltip("Warna text untuk objective")]
    public Color objectiveColor = new Color(0.2f, 0.8f, 1f);
    [Tooltip("Warna text untuk win result")]
    public Color winColor = new Color(0.2f, 1f, 0.3f);
    [Tooltip("Warna text untuk lose result")]
    public Color loseColor = new Color(1f, 0.3f, 0.2f);

    // State
    private DialogSequence _currentSequence;
    private int _currentLineIndex = 0;
    private bool _dialogActive = false;
    private bool _isTyping = false;
    private bool _activeIsNarration = false;
    
    // Active refs
    private TMP_Text _activeSpeakerText;
    private TMP_Text _activeDialogText;
    private TMP_Text _activeActionText;
    private Image _activePortraitImage;
    
    // Original colors (for reset)
    private Color _originalCharacterDialogColor;
    private Color _originalNarrationDialogColor;

    public System.Action OnSequenceComplete;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (characterRoot != null) characterRoot.SetActive(false);
        if (narrationRoot != null) narrationRoot.SetActive(false);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextLine);

        // Store original colors
        if (characterDialogText != null)
            _originalCharacterDialogColor = characterDialogText.color;
        if (narrationDialogText != null)
            _originalNarrationDialogColor = narrationDialogText.color;
    }

    public void PlaySequence(DialogSequence sequence)
    {
        if (sequence == null || sequence.lines == null)
        {
            Debug.LogError("[DialogUI] Invalid sequence!");
            return;
        }

        _currentSequence = sequence;
        _currentLineIndex = 0;
        _dialogActive = true;

        if (dialogPanel != null) dialogPanel.SetActive(true);

        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        if (_currentLineIndex >= _currentSequence.lines.Length)
        {
            EndSequence();
            return;
        }

        DialogLine line = _currentSequence.lines[_currentLineIndex];

        // 1) Determine narration or character
        bool isNarration = IsNarration(line.speaker);
        _activeIsNarration = isNarration;

        // 2) Setup active refs
        SetupActiveRefs(isNarration);

        // 3) Process speaker & text
        string processedSpeaker = ProcessPlaceholders(line.speaker);
        if (isNarration && string.IsNullOrWhiteSpace(processedSpeaker))
            processedSpeaker = narrationDisplayName;

        string dialog = GetDialogContent(line);

        // 4) Apply color based on dialog type
        ApplyDialogTypeColor(line.dialogType);

        // 5) Speaker name (hanya untuk character, bukan narration)
        if (_activeSpeakerText != null && !isNarration)
        {
            _activeSpeakerText.text = processedSpeaker;
            _activeSpeakerText.gameObject.SetActive(true);
        }

        // 6) Portrait (character only)
        if (!isNarration)
        {
            UpdatePortrait(processedSpeaker, line.portraitExpression);
        }
        else if (_activePortraitImage != null)
        {
            _activePortraitImage.gameObject.SetActive(false);
        }

        // 7) Action text
        UpdateActionText(line);

        // 8) Typewriter
        StartCoroutine(TypewriterEffect(dialog, typeSpeed));
    }

    // ✨ FIXED: Proper generated dialog handling
    string GetDialogContent(DialogLine line)
    {
        string content;

        // Check if this line should use generated dialog
        if (line.dialogType == DialogType.NPCRandom || line.useGeneratedDialog)
        {
            GeneratedDialogManager genManager = FindObjectOfType<GeneratedDialogManager>();
            
            if (genManager != null)
            {
                // ✅ FIX: Use speaker name, not npcJsonFile
                string npcName = !string.IsNullOrEmpty(line.npcJsonFile) 
                    ? line.npcJsonFile  // If explicitly set, use it
                    : line.speaker;     // Otherwise, use speaker name
                
                // Get dialog from GeneratedDialogManager
                content = genManager.GetRandomDialog(npcName);
                
                // Fallback to line.text if no dialog found
                if (string.IsNullOrEmpty(content))
                {
                    Debug.LogWarning($"[DialogUI] No generated dialog found for '{npcName}', using fallback text");
                    content = line.text;
                }
            }
            else
            {
                Debug.LogWarning("[DialogUI] GeneratedDialogManager not found, using line.text");
                content = line.text;
            }
        }
        else
        {
            // Normal dialog - use text directly
            content = line.text;
        }

        return ProcessPlaceholders(content);
    }

    void ApplyDialogTypeColor(DialogType type)
    {
        if (_activeDialogText == null) return;

        Color targetColor = type switch
        {
            DialogType.Objective => objectiveColor,
            DialogType.WinResult => winColor,
            DialogType.LoseResult => loseColor,
            _ => _activeIsNarration ? _originalNarrationDialogColor : _originalCharacterDialogColor
        };

        _activeDialogText.color = targetColor;
    }

    string ProcessPlaceholders(string original)
    {
        if (string.IsNullOrEmpty(original)) return original;
        
        string result = original;
        
        // CharacterManager placeholders
        if (CharacterManager.Instance != null)
        {
            result = result
                .Replace("[Player]", CharacterManager.Instance.GetPlayerName())
                .Replace("{player}", CharacterManager.Instance.GetPlayerName())
                .Replace("[Supporting]", CharacterManager.Instance.GetSupportingName())
                .Replace("[Supporting Character]", CharacterManager.Instance.GetSupportingName());
        }
        
        return result;
    }

    bool IsNarration(string rawSpeaker)
    {
        // Check via CharacterManager first
        if (CharacterManager.Instance != null && CharacterManager.Instance.IsNarrator(rawSpeaker))
            return true;

        // Empty speaker = narration (if enabled)
        if (treatEmptySpeakerAsNarration && string.IsNullOrWhiteSpace(rawSpeaker))
            return true;

        string s = (rawSpeaker ?? string.Empty).Trim();
        
        // Check against narration tokens
        foreach (var token in narrationTokens)
        {
            if (s.Equals(token, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Check after placeholder processing
        string processed = ProcessPlaceholders(s);
        foreach (var token in narrationTokens)
        {
            if (processed.Equals(token, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        
        return false;
    }

    void SetupActiveRefs(bool isNarration)
    {
        if (characterRoot != null) characterRoot.SetActive(!isNarration);
        if (narrationRoot != null) narrationRoot.SetActive(isNarration);

        if (isNarration)
        {
            _activeSpeakerText = narrationSpeakerText;
            _activeDialogText = narrationDialogText;
            _activeActionText = narrationActionText;
            _activePortraitImage = null;
        }
        else
        {
            _activeSpeakerText = characterSpeakerText;
            _activeDialogText = characterDialogText;
            _activeActionText = characterActionText;
            _activePortraitImage = characterPortraitImage;
        }

        if (_activeDialogText != null) _activeDialogText.text = "";
        if (_activeActionText != null) _activeActionText.gameObject.SetActive(false);
        
        // Hide speaker name untuk narration (lebih clean)
        if (_activeSpeakerText != null)
        {
            if (isNarration && hideNarratorName)
                _activeSpeakerText.gameObject.SetActive(false);
            else
                _activeSpeakerText.gameObject.SetActive(true);
        }
    }

    void UpdatePortrait(string speakerName, string expression)
    {
        if (_activePortraitImage == null)
            return;
        
        if (CharacterManager.Instance == null)
        {
            _activePortraitImage.gameObject.SetActive(false);
            return;
        }

        Sprite portrait = CharacterManager.Instance.GetPortrait(speakerName, expression);

        if (portrait != null)
        {
            _activePortraitImage.sprite = portrait;
            _activePortraitImage.gameObject.SetActive(true);
        }
        else
        {
            _activePortraitImage.gameObject.SetActive(false);
        }
    }

    void UpdateActionText(DialogLine line)
    {
        if (_activeActionText == null) return;

        if (line.hasAction && !string.IsNullOrEmpty(line.actionText))
        {
            _activeActionText.text = $"*{line.actionText}*";
            _activeActionText.gameObject.SetActive(true);
        }
        else
        {
            _activeActionText.gameObject.SetActive(false);
        }
    }

    IEnumerator TypewriterEffect(string text, float speed)
    {
        _isTyping = true;

        if (_activeDialogText != null)
        {
            _activeDialogText.text = "";
            foreach (char letter in text)
            {
                _activeDialogText.text += letter;
                yield return new WaitForSeconds(speed);
            }
        }

        _isTyping = false;
    }

    public void NextLine()
    {
        if (!_dialogActive) return;

        // Skip typewriter if still typing
        if (_isTyping && canSkipTypewriter)
        {
            StopAllCoroutines();
            DialogLine line = _currentSequence.lines[_currentLineIndex];
            string content = GetDialogContent(line);
            if (_activeDialogText != null)
                _activeDialogText.text = ProcessPlaceholders(content);
            _isTyping = false;
            return;
        }

        // Move to next line
        _currentLineIndex++;
        DisplayCurrentLine();
    }

    void EndSequence()
    {
        _dialogActive = false;

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        Debug.Log("[DialogUI] Sequence completed");
        OnSequenceComplete?.Invoke();
    }

    // ========================================
    // PUBLIC API
    // ========================================
    
    public bool IsActive() => _dialogActive;
    
    public DialogSequence GetCurrentSequence() => _currentSequence;
    
    /// <summary>Force close dialog (emergency use)</summary>
    public void ForceClose()
    {
        StopAllCoroutines();
        _dialogActive = false;
        _isTyping = false;
        
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }
    
    /// <summary>Check if currently typing</summary>
    public bool IsTyping() => _isTyping;
    
    #if UNITY_EDITOR
    // ========================================
    // DEBUG TOOLS (Editor Only)
    // ========================================
    
    [ContextMenu("Test Dialog Panel")]
    private void TestDialogPanel()
    {
        if (dialogPanel != null)
        {
            bool newState = !dialogPanel.activeSelf;
            dialogPanel.SetActive(newState);
            Debug.Log($"[DialogUI] Dialog panel: {(newState ? "SHOWN" : "HIDDEN")}");
        }
    }
    
    [ContextMenu("Print UI Status")]
    private void PrintUIStatus()
    {
        Debug.Log("=== DialogUI Status ===");
        Debug.Log($"Active: {_dialogActive}");
        Debug.Log($"Typing: {_isTyping}");
        Debug.Log($"Current Line: {_currentLineIndex}");
        Debug.Log($"Current Sequence: {(_currentSequence != null ? _currentSequence.name : "None")}");
        Debug.Log($"Dialog Panel: {(dialogPanel != null ? dialogPanel.activeSelf.ToString() : "NULL")}");
    }
    #endif
}