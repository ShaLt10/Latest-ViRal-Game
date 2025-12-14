using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

[DisallowMultipleComponent]
public class CharacterSelection : MonoBehaviour
{
    [Header("UI Panels (GameObject biasa)")]
    [SerializeField] private GameObject selectionRoot;   // tombol Raline/Gavi + Reticle
    [SerializeField] private GameObject narratorPanel;   // panel narrator (posisi bawah, biarkan layout di Scene)
    [SerializeField] private GameObject charIntroPanel;  // panel intro (posisi bawah)
    [SerializeField] private GameObject confirmPanel;    // panel confirm (posisi bawah)

    [Header("Selection Buttons")]
    [SerializeField] private Button ralineButton;
    [SerializeField] private Button gaviButton;

    [Header("Reticle")]
    [SerializeField] private RectTransform reticle;              // pastikan Image.raycastTarget = OFF
    [SerializeField] private RectTransform ralineReticleAnchor;
    [SerializeField] private RectTransform gaviReticleAnchor;
    [SerializeField] private float reticleScaleMin = 0.95f;
    [SerializeField] private float reticleScaleMax = 1.05f;
    [SerializeField] private float reticlePulseSpeed = 3f;

    [Header("Narrator")]
    [SerializeField] private TMP_Text narratorText;
    [SerializeField] private Button   skipButton;
    [SerializeField] private float    narratorDisplayTime = 3f;
    [TextArea] [SerializeField] private string narratorMessage = "Pilih karaktermu dulu ya!";

    [Header("Character Intro (panel bawah)")]
    [SerializeField] private TMP_Text charIntroText;
    [SerializeField] private Image    charIntroPortrait;
    [SerializeField] private TMP_Text charIntroName;
    [SerializeField] private Button   charIntroChooseButton;
    [TextArea] [SerializeField] private string ralineIntro = "Hai! Aku Raline, siap bantuin!";
    [TextArea] [SerializeField] private string gaviIntro   = "Halo, aku Gavi. Yuk mulai!";

    [Header("Portrait Assets")]
    [SerializeField] private Sprite ralinePortrait;
    [SerializeField] private Sprite gaviPortrait;
    [SerializeField] private string ralineDisplayName = "Raline";
    [SerializeField] private string gaviDisplayName   = "Gavi";

    [Header("Confirm Panel")]
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button   confirmYesButton;
    [SerializeField] private Button   confirmNoButton;

    [Header("Result")]
    [SerializeField] private string gameScene = "Map1";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // ---- state ----
    private string  selectedCharacter = ""; // "Raline"/"Gavi"
    private int     selectedIndex     = -1; // 0 Raline / 1 Gavi
    private Coroutine narratorAutoCo;
    private Coroutine reticlePulseCo;
    private CanvasGroup selectionGroup;

    void Awake() => InitializeComponent();

    public void InitializeComponent()
    {
        if (!selectionRoot) { Debug.LogError("[CS] selectionRoot belum di-assign"); return; }

        // CanvasGroup untuk lock interaksi saat narrator tampil
        selectionGroup = selectionRoot.GetComponent<CanvasGroup>();
        if (!selectionGroup) selectionGroup = selectionRoot.AddComponent<CanvasGroup>();
        selectionGroup.alpha = 1f;

        // Reticle non-blocking
        if (reticle)
        {
            var cg = reticle.GetComponent<CanvasGroup>();
            if (!cg) cg = reticle.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable   = false;
        }

        // Bind buttons
        if (ralineButton) { ralineButton.onClick.RemoveAllListeners(); ralineButton.onClick.AddListener(() => OnCharacterSelected("Raline", 0)); }
        if (gaviButton)   { gaviButton.onClick.RemoveAllListeners();   gaviButton.onClick.AddListener(() => OnCharacterSelected("Gavi",   1)); }
        if (skipButton)   { skipButton.onClick.RemoveAllListeners();   skipButton.onClick.AddListener(OnSkipNarrator);                      }
        if (charIntroChooseButton)
        {
            charIntroChooseButton.onClick.RemoveAllListeners();
            charIntroChooseButton.onClick.AddListener(OnChooseThis);
        }
        if (confirmYesButton) { confirmYesButton.onClick.RemoveAllListeners(); confirmYesButton.onClick.AddListener(OnConfirmYes); }
        if (confirmNoButton)  { confirmNoButton.onClick.RemoveAllListeners();  confirmNoButton.onClick.AddListener(OnConfirmNo);  }

        // Default OFF (biar nggak pindah posisi/anchor buatanmu)
        SafeSetActive(selectionRoot,  false);
        SafeSetActive(narratorPanel,  false);
        SafeSetActive(charIntroPanel, false);
        SafeSetActive(confirmPanel,   false);

        Log("Initialized");
    }

    void OnDisable()
    {
        if (reticlePulseCo != null) { StopCoroutine(reticlePulseCo); reticlePulseCo = null; }
        if (narratorAutoCo != null) { StopCoroutine(narratorAutoCo); narratorAutoCo = null; }
    }

    // === Entry dari MainMenu ===
    public void EnterFromMenu(bool showNarratorFirst)
    {
        // reset local state setiap masuk
        selectedCharacter = "";
        selectedIndex     = -1;
        if (reticlePulseCo != null) { StopCoroutine(reticlePulseCo); reticlePulseCo = null; }

        SafeSetActive(selectionRoot,  true);
        SafeSetActive(charIntroPanel, false);
        SafeSetActive(confirmPanel,   false);

        SetSelectionInteractable(!showNarratorFirst);

        if (showNarratorFirst) ShowNarrator();
        else                   ShowCharacterSelection();

        // focus awal (tanpa spam)
        if (!showNarratorFirst && ralineButton) Focus(ralineButton);

        Log($"EnterFromMenu narratorFirst={showNarratorFirst}");
    }

    // === Narrator ===
    private void ShowNarrator()
    {
        SafeSetActive(narratorPanel, true);
        if (narratorText) narratorText.text = narratorMessage;

        if (narratorAutoCo != null) StopCoroutine(narratorAutoCo);
        if (narratorDisplayTime > 0f) narratorAutoCo = StartCoroutine(AutoAdvance());

        Log("ShowNarrator");
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(narratorDisplayTime);
        if (narratorPanel && narratorPanel.activeInHierarchy) OnSkipNarrator();
    }

    private void OnSkipNarrator()
    {
        SafeSetActive(narratorPanel, false);
        SetSelectionInteractable(true);
        Focus(ralineButton);
        Log("SkipNarrator → Selection interactable");
    }

    // === Selection ===
    private void ShowCharacterSelection()
    {
        SafeSetActive(narratorPanel,  false);
        SafeSetActive(charIntroPanel, false);
        SafeSetActive(confirmPanel,   false);
        SafeSetActive(selectionRoot,  true);
        Log("ShowCharacterSelection");
    }

    private void OnCharacterSelected(string who, int index)
    {
        selectedCharacter = who;
        selectedIndex     = index;

        MoveReticle(index == 0 ? ralineReticleAnchor : gaviReticleAnchor);
        ShowCharacterIntro(who);
    }

    // === Intro (panel bawah) ===
    private void ShowCharacterIntro(string who)
    {
        SafeSetActive(confirmPanel,   false);
        SafeSetActive(charIntroPanel, true);

        if (charIntroText)     charIntroText.text   = (who == "Raline") ? ralineIntro     : gaviIntro;
        if (charIntroPortrait) charIntroPortrait.sprite = (who == "Raline") ? ralinePortrait : gaviPortrait;
        if (charIntroName)     charIntroName.text   = (who == "Raline") ? ralineDisplayName : gaviDisplayName;

        if (charIntroChooseButton) Focus(charIntroChooseButton);
        Log($"ShowCharacterIntro: {who}");
    }

    private void OnChooseThis()
    {
        if (selectedIndex < 0) { Log("OnChooseThis ignored (no selection)"); return; }
        ShowConfirmPanel();
    }

    // === Confirm (panel bawah) ===
    private void ShowConfirmPanel()
    {
        SafeSetActive(charIntroPanel, false);
        SafeSetActive(confirmPanel,   true);

        if (confirmText) confirmText.text = $"Mulai sebagai {selectedCharacter}?";

        // Lock selection saat confirm terbuka
        SetSelectionInteractable(false);

        if (confirmYesButton) Focus(confirmYesButton);
        Log("ShowConfirmPanel");
    }

    private void OnConfirmYes()
    {
        // FIX utama: sumber kebenaran → CharacterManager (bukan PlayerPrefs string)
        CharacterManager.Instance?.SelectCharacter(selectedIndex);

        if (!string.IsNullOrEmpty(gameScene)) SceneManager.LoadScene(gameScene);
        else Debug.LogError("[CS] gameScene kosong");
    }

    private void OnConfirmNo()
    {
        SafeSetActive(confirmPanel, false);
        SetSelectionInteractable(true);
        if (!string.IsNullOrEmpty(selectedCharacter)) ShowCharacterIntro(selectedCharacter);
        else ShowCharacterSelection();
    }

    // === Reticle ===
    private void MoveReticle(RectTransform anchor)
    {
        if (!reticle || !anchor) return;

        // Pindah parent/posisi lokal biar pas di atas tombol
        reticle.SetParent(anchor, false);
        reticle.anchoredPosition = Vector2.zero;

        if (reticlePulseCo != null) StopCoroutine(reticlePulseCo);
        reticlePulseCo = StartCoroutine(ReticlePulse());
    }

    private IEnumerator ReticlePulse()
    {
        float t = 0f;
        while (reticle)
        {
            t += Time.unscaledDeltaTime * reticlePulseSpeed;
            float s = Mathf.Lerp(reticleScaleMin, reticleScaleMax, (Mathf.Sin(t) + 1f) * 0.5f);
            reticle.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
    }

    // === Helpers ===
    private void SetSelectionInteractable(bool on)
    {
        if (!selectionGroup) return;
        selectionGroup.interactable   = on;
        selectionGroup.blocksRaycasts = on;
    }

    private void SafeSetActive(GameObject go, bool active)
    {
        if (!go) return;
        if (go.activeSelf != active) go.SetActive(active);
        Log($"Panel {go.name} => {(active ? "ON" : "off")}");
    }

    private void Focus(Selectable s)
    {
        if (!s || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        s.Select();
    }

    // Dipanggil MainMenu ketika tombol Back dari CharacterSelect (kalau kamu pakai)
    public void ResetForMenu()
    {
        if (reticlePulseCo != null) { StopCoroutine(reticlePulseCo); reticlePulseCo = null; }
        if (narratorAutoCo != null) { StopCoroutine(narratorAutoCo); narratorAutoCo = null; }

        selectedCharacter = "";
        selectedIndex     = -1;

        SafeSetActive(confirmPanel,   false);
        SafeSetActive(charIntroPanel, false);
        SafeSetActive(narratorPanel,  false);
        SafeSetActive(selectionRoot,  false);
    }

    private void Log(string s) { if (enableDebugLogs) Debug.Log($"[CS] {s}"); }
}
