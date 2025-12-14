using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject openingPanel;
    [SerializeField] private GameObject characterSelectPanel;

    [Header("First Selected (optional)")]
    [SerializeField] private Selectable firstOnTitle;
    [SerializeField] private Selectable firstOnLoad;
    [SerializeField] private Selectable firstOnCredits;
    [SerializeField] private Selectable firstOnSettings;
    [SerializeField] private Selectable firstOnSelect;

    [Header("Components")]
    [SerializeField] private OpeningAnimator openingAnimator; // kalau ada
    [SerializeField] private CharacterSelection characterSelection;

    [Header("Flow")]
    [SerializeField] private bool useOpening = true;
    [SerializeField] private bool narratorOnFirstEnter = true;
    [SerializeField] private bool resetSelectionOnEnter = true; // FIX “pilih Gavi → back → pilih Raline masih Gavi”
    [SerializeField] private string fallbackScene = "Map1";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    void Start()
    {
        ShowOnly(titleScreen);
        SelectFirst(firstOnTitle);

        if (openingAnimator)
        {
            openingAnimator.OnOpeningComplete.RemoveAllListeners();
            openingAnimator.OnOpeningComplete.AddListener(OnOpeningComplete);
        }
    }

    // === Title Buttons ===
    public void OnPlay()
    {
        if (useOpening && openingPanel && openingAnimator)
        {
            ShowOnly(openingPanel);
            openingAnimator.BeginOpening();
            Log("Play → Opening");
        }
        else
        {
            GoToCharacterSelectOrScene();
        }
    }

    public void OnOpenLoad()       { ShowOnly(loadPanel);       SelectFirst(firstOnLoad); }
    public void OnBackFromLoad()   { ShowOnly(titleScreen);     SelectFirst(firstOnTitle); }
    public void OnOpenCredits()    { ShowOnly(creditsPanel);    SelectFirst(firstOnCredits); }
    public void OnBackFromCredits(){ ShowOnly(titleScreen);     SelectFirst(firstOnTitle); }
    public void OnOpenSettings()   { ShowOnly(settingsScreen);  SelectFirst(firstOnSettings); }
    public void OnBackFromSettings(){ShowOnly(titleScreen);     SelectFirst(firstOnTitle); }

    // Kalau kamu punya tombol Back di CharacterSelect, panggil ini:
    public void OnBackFromCharacterSelect()
    {
        characterSelection?.ResetForMenu();
        ShowOnly(titleScreen);
        SelectFirst(firstOnTitle);
    }

    // === Opening → selesai ===
    private void OnOpeningComplete()
    {
        Log("OpeningComplete → CharacterSelect");
        GoToCharacterSelectOrScene();
    }

    private void GoToCharacterSelectOrScene()
    {
        if (characterSelectPanel && characterSelection)
        {
            if (resetSelectionOnEnter) CharacterManager.Instance?.ResetCharacter(); // BERSIHKAN pilihan lama
            ShowOnly(characterSelectPanel);

            characterSelection.EnterFromMenu(narratorOnFirstEnter);
            SelectFirst(firstOnSelect);
        }
        else
        {
            if (!string.IsNullOrEmpty(fallbackScene))
                SceneManager.LoadScene(fallbackScene);
        }
    }

    // === Helpers ===
    private void ShowOnly(GameObject target)
    {
        SetActive(titleScreen,         target == titleScreen);
        SetActive(loadPanel,           target == loadPanel);
        SetActive(creditsPanel,        target == creditsPanel);
        SetActive(settingsScreen,      target == settingsScreen);
        SetActive(openingPanel,        target == openingPanel);
        SetActive(characterSelectPanel,target == characterSelectPanel);
    }

    private void SetActive(GameObject go, bool on)
    {
        if (!go || go.activeSelf == on) return;
        go.SetActive(on);
        Log($"[Menu] {(on ? "ON " : "off")} {go.name}");
    }

    private void SelectFirst(Selectable s)
    {
        if (!s) return;
        EventSystem.current?.SetSelectedGameObject(null);
        s.Select();
    }

    private void Log(string s){ if (enableDebugLogs) Debug.Log(s); }
}
