// DialogUI.cs - FINAL DUAL PANEL (Character & Narrator split)
// PELETAKAN: Assets/Scripts/DialogSystem/UI/DialogUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class DialogUI : MonoBehaviour
{
    [Header("Roots")]
    public GameObject dialogPanel;

    [Tooltip("Root untuk line karakter (ada portrait)")]
    public GameObject characterRoot;
    [Tooltip("Root untuk line narrator (tanpa portrait)")]
    public GameObject narrationRoot;

    [Header("Character UI")]
    public TMP_Text characterSpeakerText;
    public TMP_Text characterDialogText;
    public Image characterPortraitImage;
    public TMP_Text characterActionText;

    [Header("Narration UI")]
    public TMP_Text narrationSpeakerText;   // tetap tampil nama (mis. "Narrator" / "System")
    public TMP_Text narrationDialogText;
    public TMP_Text narrationActionText;

    [Header("Controls")]
    public Button nextButton;

    [Header("Settings")]
    [Tooltip("Kecepatan typewriter untuk semua line")]
    public float typeSpeed = 0.02f;
    [Tooltip("Boleh klik untuk skip ketikan")]
    public bool canSkipTypewriter = true;

    [Header("Narration Rules")]
    [Tooltip("Speaker kosong dianggap narration")]
    public bool treatEmptySpeakerAsNarration = true;
    [Tooltip("Token yang dianggap narration (case-insensitive)")]
    public string[] narrationTokens = new string[] { "Narrator", "[Narrator]", "System" };
    [Tooltip("Nama default jika speaker narration kosong")]
    public string narrationDisplayName = "Narrator";

    // State
    private DialogSequence currentSequence;
    private int currentLineIndex = 0;
    private bool dialogActive = false;
    private bool isTyping = false;
    private bool activeIsNarration = false;

    // Active refs (switch sesuai mode)
    private TMP_Text activeSpeakerText;
    private TMP_Text activeDialogText;
    private TMP_Text activeActionText;
    private Image activePortraitImage;

    // Events
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
    }

    public void PlaySequence(DialogSequence sequence)
    {
        if (sequence == null || sequence.lines == null)
        {
            Debug.LogError("Invalid dialog sequence!");
            return;
        }

        currentSequence = sequence;
        currentLineIndex = 0;
        dialogActive = true;

        if (dialogPanel != null) dialogPanel.SetActive(true);

        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        if (currentLineIndex >= currentSequence.lines.Length)
        {
            EndSequence();
            return;
        }

        DialogLine line = currentSequence.lines[currentLineIndex];

        // 1) Tentukan narration atau character
        bool isNarration = IsNarration(line.speaker);
        activeIsNarration = isNarration;

        // 2) Siapkan refs aktif
        SetupActiveRefs(isNarration);

        // 3) Proses speaker & text
        string processedSpeaker = ProcessPlaceholders(line.speaker);
        if (isNarration && string.IsNullOrWhiteSpace(processedSpeaker))
            processedSpeaker = narrationDisplayName;

        string dialog = GetDialogContent(line);

        // 4) Speaker name (kedua mode: tampil)
        if (activeSpeakerText != null)
        {
            activeSpeakerText.text = processedSpeaker;
            activeSpeakerText.gameObject.SetActive(true);
        }

        // 5) Portrait hanya untuk character
        if (!isNarration)
        {
            UpdatePortrait(processedSpeaker, line.portraitExpression);
        }
        else if (activePortraitImage != null)
        {
            activePortraitImage.gameObject.SetActive(false);
        }

        // 6) Action text (opsional)
        UpdateActionText(line);

        // 7) Typewriter
        StartCoroutine(TypewriterEffect(dialog, typeSpeed));
    }

    string GetDialogContent(DialogLine line)
    {
        string content;

        if (line.useGeneratedDialog && !string.IsNullOrEmpty(line.npcJsonFile))
        {
            GeneratedDialogManager genManager = FindObjectOfType<GeneratedDialogManager>();
            content = genManager != null ? genManager.GetRandomDialog(line.npcJsonFile) : line.text;
        }
        else
        {
            content = line.text;
        }

        return ProcessPlaceholders(content);
    }

    string ProcessPlaceholders(string original)
    {
        if (CharacterManager.Instance == null) return original;

        return original
            .Replace("[Player]", CharacterManager.Instance.GetPlayerName())
            .Replace("[Supporting]", CharacterManager.Instance.GetSupportingName())
            .Replace("[Supporting Character]", CharacterManager.Instance.GetSupportingName());
    }

    bool IsNarration(string rawSpeaker)
    {
        // Pakai rule CharacterManager kalau ada
        if (CharacterManager.Instance != null && CharacterManager.Instance.IsNarrator(rawSpeaker))
            return true;

        if (treatEmptySpeakerAsNarration && string.IsNullOrWhiteSpace(rawSpeaker))
            return true;

        string s = (rawSpeaker ?? string.Empty).Trim();
        foreach (var token in narrationTokens)
        {
            if (s.Equals(token, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Placeholder pass
        string processed = ProcessPlaceholders(s);
        foreach (var token in narrationTokens)
        {
            if (processed.Equals(token, StringComparison.OrdinalIgnoreCase))
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
            activeSpeakerText = narrationSpeakerText;
            activeDialogText  = narrationDialogText;
            activeActionText  = narrationActionText;
            activePortraitImage = null;
        }
        else
        {
            activeSpeakerText = characterSpeakerText;
            activeDialogText  = characterDialogText;
            activeActionText  = characterActionText;
            activePortraitImage = characterPortraitImage;
        }

        if (activeDialogText != null) activeDialogText.text = "";
        if (activeActionText != null) activeActionText.gameObject.SetActive(false);
    }

    void UpdatePortrait(string speakerName, string expression)
    {
        if (activePortraitImage == null || CharacterManager.Instance == null)
            return;

        Sprite portrait = CharacterManager.Instance.GetPortrait(speakerName, expression);

        if (portrait != null)
        {
            activePortraitImage.sprite = portrait;
            activePortraitImage.gameObject.SetActive(true);
        }
        else
        {
            activePortraitImage.gameObject.SetActive(false);
        }
    }

    void UpdateActionText(DialogLine line)
    {
        if (activeActionText == null) return;

        if (line.hasAction && !string.IsNullOrEmpty(line.actionText))
        {
            activeActionText.text = $"*{line.actionText}*";
            activeActionText.gameObject.SetActive(true);
        }
        else
        {
            activeActionText.gameObject.SetActive(false);
        }
    }

    IEnumerator TypewriterEffect(string text, float speed)
    {
        isTyping = true;

        if (activeDialogText != null)
        {
            activeDialogText.text = "";
            foreach (char letter in text)
            {
                activeDialogText.text += letter;
                yield return new WaitForSeconds(speed);
            }
        }

        isTyping = false;
    }

    public void NextLine()
    {
        if (!dialogActive) return;

        if (isTyping && canSkipTypewriter)
        {
            StopAllCoroutines();
            DialogLine line = currentSequence.lines[currentLineIndex];
            string content = GetDialogContent(line);
            if (activeDialogText != null)
                activeDialogText.text = content;
            isTyping = false;
            return;
        }

        currentLineIndex++;
        DisplayCurrentLine();
    }

    void EndSequence()
    {
        dialogActive = false;

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        Debug.Log("Dialog sequence completed");
        OnSequenceComplete?.Invoke();
    }

    public bool IsActive() => dialogActive;
    public DialogSequence GetCurrentSequence() => currentSequence;
}
