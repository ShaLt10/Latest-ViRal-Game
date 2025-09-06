// CharacterSelection.cs — Simple version: self-start DialogSequence showcase + select reticle
// Requires: DialogManager + CharacterManager in the scene.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CharacterSelection : MonoBehaviour
{
    [Header("UI References")]
    public Button ralineButton;
    public Button gaviButton;
    public Button confirmButton;
    public GameObject selectionPanel;   // panel container pemilihan

    [Header("Preview UI")]
    public Image previewImage;
    public TMP_Text previewName;
    public TMP_Text previewDescription;
    public Sprite fallbackPortrait;     // opsional

    [Header("Character Descriptions")]
    [TextArea(2, 4)] public string ralineDescription = "Detective yang detail-oriented. Suka investigasi mendalam sampai tuntas!";
    [TextArea(2, 4)] public string gaviDescription   = "IT enthusiast yang benci manipulasi digital. Tech-savvy problem solver!";

    [Header("Select Reticle (optional)")]
    public RectTransform reticle;       // assign ikon kotak-sudut
    public float reticleScaleMin = 0.95f;
    public float reticleScaleMax = 1.05f;
    public float reticlePulseSpeed = 3f;

    [Header("Showcase (DialogSequence)")]
    public bool playShowcaseOnStart = true;            // putar cutscene kalau true
    public string showcaseAreaName = "CharacterSelect";// samakan dengan Area Name asset sequence-mu
    public bool hideSelectionWhileShowcase = true;     // sembunyikan panel saat showcase
    public bool skipShowcaseIfSeen = true;             // tidak diputar lagi setelah sekali
    const string ShowcaseSeenKey = "cs_showcase_seen";

    [Header("Setelah Confirm → lanjut cerita")]
    public bool useAfterPickSequences = false;         // kalau mau sequence beda per karakter
    public string afterPickRaline = "Opening_AfterPick_Raline";
    public string afterPickGavi  = "Opening_AfterPick_Gavi";
    public string openingAreaName = "Opening";         // fallback: mulai area Opening

    [Header("Keyboard (opsional)")]
    public bool enableKeyboardSwitch = true;           // ←/A = Raline, →/D = Gavi, Enter/Space = Confirm

    private int selectedIndex = -1; // 0=Raline, 1=Gavi
    private Coroutine pulseCo;

    void OnEnable()
    {
        if (DialogManager.Instance != null)
            DialogManager.Instance.OnAreaComplete += HandleAreaComplete;
    }

    void OnDisable()
    {
        if (DialogManager.Instance != null)
            DialogManager.Instance.OnAreaComplete -= HandleAreaComplete;
    }

    void Start()
    {
        SetupButtons();

        bool seen = PlayerPrefs.GetInt(ShowcaseSeenKey, 0) == 1;

        if (playShowcaseOnStart && (!skipShowcaseIfSeen || !seen))
        {
            if (hideSelectionWhileShowcase && selectionPanel) selectionPanel.SetActive(false);
            StartCoroutine(StartShowcaseAfterDelay(0.1f)); // jeda kecil biar DialogManager siap
        }
        else
        {
            if (selectionPanel) selectionPanel.SetActive(true);
            PreviewCharacter(0);                          // tampilkan default
            if (confirmButton) confirmButton.gameObject.SetActive(false);
            if (reticle) reticle.gameObject.SetActive(true);
        }
    }

    IEnumerator StartShowcaseAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        var dm = DialogManager.Instance;
        if (dm == null) { Debug.LogError("DialogManager not found"); yield break; }

        // Putar DialogSequence berdasarkan AREA "CharacterSelect"
        dm.PlaySequenceByArea(showcaseAreaName);
    }

    void SetupButtons()
    {
        if (ralineButton)
        {
            ralineButton.onClick.RemoveAllListeners();
            ralineButton.onClick.AddListener(() => PreviewCharacter(0));
        }
        if (gaviButton)
        {
            gaviButton.onClick.RemoveAllListeners();
            gaviButton.onClick.AddListener(() => PreviewCharacter(1));
        }
        if (confirmButton)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmSelection);
            confirmButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!enableKeyboardSwitch) return;
        if (!selectionPanel || !selectionPanel.activeInHierarchy) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) PreviewCharacter(0);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) PreviewCharacter(1);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (confirmButton && confirmButton.gameObject.activeInHierarchy) ConfirmSelection();
        }
    }

    public void PreviewCharacter(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, 1);
        string name = selectedIndex == 0 ? "Raline" : "Gavi";
        string desc = selectedIndex == 0 ? ralineDescription : gaviDescription;

        // ambil portrait dari CharacterManager
        Sprite portrait = null;
        if (CharacterManager.Instance != null)
            portrait = CharacterManager.Instance.GetPortrait(name, "Default");
        if (portrait == null) portrait = fallbackPortrait;

        if (previewImage) previewImage.sprite = portrait;
        if (previewName)  previewName.text = name;
        if (previewDescription) previewDescription.text = desc;

        // pindahkan reticle ke tombol yang aktif
        if (reticle)
        {
            Button target = selectedIndex == 0 ? ralineButton : gaviButton;
            if (target)
            {
                reticle.SetParent(target.transform, worldPositionStays: false);
                reticle.anchoredPosition = Vector2.zero;
                reticle.SetAsLastSibling();
                StartPulse();
            }
        }

        if (confirmButton && !confirmButton.gameObject.activeSelf)
            confirmButton.gameObject.SetActive(true);
    }

    void StartPulse()
    {
        if (!reticle) return;
        if (pulseCo != null) StopCoroutine(pulseCo);
        pulseCo = StartCoroutine(PulseReticle());
    }

    IEnumerator PulseReticle()
    {
        while (reticle)
        {
            float t = (Mathf.Sin(Time.unscaledTime * reticlePulseSpeed) + 1f) * 0.5f; // 0..1
            float s = Mathf.Lerp(reticleScaleMin, reticleScaleMax, t);
            reticle.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
    }

    public void ConfirmSelection()
    {
        if (selectedIndex < 0) return;

        if (CharacterManager.Instance != null)
            CharacterManager.Instance.SelectCharacter(selectedIndex);

        string picked = selectedIndex == 0 ? "Raline" : "Gavi";
        Debug.Log($"Player confirmed selection: {picked}");

        if (selectionPanel) selectionPanel.SetActive(false);
        if (reticle) reticle.gameObject.SetActive(false);

        StartGameDialog();
    }

    void StartGameDialog()
    {
        var dm = DialogManager.Instance;
        if (dm == null) return;

        if (useAfterPickSequences)
        {
            string seq = (selectedIndex == 0) ? afterPickRaline : afterPickGavi;
            if (!string.IsNullOrEmpty(seq))
            {
                dm.PlaySequenceByName(seq);
                return;
            }
        }

        // default: mulai area Opening
        if (!string.IsNullOrEmpty(openingAreaName))
            dm.PlaySequenceByArea(openingAreaName);
    }

    // === Dipanggil saat sebuah AREA selesai (dari DialogManager) ===
    void HandleAreaComplete(string justFinishedArea)
    {
        if (string.IsNullOrEmpty(justFinishedArea)) return;

        // jika yang selesai adalah showcase "CharacterSelect", munculkan panel pilih
        if (string.Equals(justFinishedArea, showcaseAreaName, System.StringComparison.OrdinalIgnoreCase))
        {
            PlayerPrefs.SetInt(ShowcaseSeenKey, 1);
            PlayerPrefs.Save();

            if (selectionPanel) selectionPanel.SetActive(true);
            if (reticle) { reticle.gameObject.SetActive(true); reticle.localScale = Vector3.one; }

            PreviewCharacter(0);
            if (confirmButton) confirmButton.gameObject.SetActive(false); // tunggu user pilih dulu
        }
    }

    // util opsional buat reset dari UI
    public void ResetSelection()
    {
        if (CharacterManager.Instance != null) CharacterManager.Instance.ResetCharacter();

        selectedIndex = -1;
        if (selectionPanel) selectionPanel.SetActive(true);
        if (confirmButton) confirmButton.gameObject.SetActive(false);
        if (reticle) reticle.gameObject.SetActive(false);
        PreviewCharacter(0);
    }
}
