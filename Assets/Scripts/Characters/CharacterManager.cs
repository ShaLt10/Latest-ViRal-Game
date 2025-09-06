// CharacterManager.cs - FINAL (portrait hub + IsNarrator)
// PELETAKAN: Assets/Scripts/DialogSystem/Characters/CharacterManager.cs
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public enum SelectedMC { None, Raline, Gavi }

    [Header("Character Selection")]
    public SelectedMC selectedCharacter = SelectedMC.None;

    [System.Serializable]
    public class CharacterPortrait
    {
        public string portraitName; // "Raline_Happy", "Raline_Default", "Gavi_Angry", ...
        public Sprite sprite;
    }

    [Header("Character Portraits")]
    public CharacterPortrait[] allPortraits;

    // Singleton
    public static CharacterManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSelectedCharacter();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectCharacter(int index)
    {
        selectedCharacter = index == 0 ? SelectedMC.Raline : SelectedMC.Gavi;
        SaveSelectedCharacter();
        Debug.Log($"Character selected: {GetPlayerName()}");
    }

    public string GetPlayerName()
    {
        return selectedCharacter.ToString(); // "Raline" atau "Gavi" atau "None"
    }

    public string GetSupportingName()
    {
        return selectedCharacter == SelectedMC.Raline ? "Gavi" : "Raline";
    }

    // Main: cari portrait berdasar nama & expression
    public Sprite GetPortrait(string characterName, string expression = "Default")
    {
        if (string.IsNullOrEmpty(characterName)) return null;

        string fullName = $"{characterName}_{expression}";

        // exact match
        foreach (var portrait in allPortraits)
        {
            if (portrait.portraitName == fullName)
                return portrait.sprite;
        }

        // fallback ke Default
        if (expression != "Default")
            return GetPortrait(characterName, "Default");

        // fallback ke portrait pertama yang match nama karakter
        foreach (var portrait in allPortraits)
        {
            if (portrait.portraitName.StartsWith(characterName + "_"))
                return portrait.sprite;
        }

        Debug.LogWarning($"Portrait not found for: {characterName}_{expression}");
        return null;
    }

    // Overload (menerima "Name_Expression")
    public Sprite GetPortrait(string characterName)
    {
        if (characterName.Contains("_"))
        {
            foreach (var portrait in allPortraits)
            {
                if (portrait.portraitName == characterName)
                    return portrait.sprite;
            }
        }
        return GetPortrait(characterName, "Default");
    }

    public Sprite GetPortraitWithExpression(string characterName, string expression)
    {
        string fullName = $"{characterName}_{expression}";
        return GetPortrait(fullName);
    }

    public bool IsMainCharacter(string characterName)
    {
        return characterName == "Raline" || characterName == "Gavi";
    }

    // Narrator/system check (dipakai DialogUI)
    public bool IsNarrator(string characterName)
    {
        string s = (characterName ?? string.Empty).Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(s) || s == "narrator" || s == "[narrator]" || s == "system";
    }

    public bool HasSelectedCharacter()
    {
        return selectedCharacter != SelectedMC.None;
    }

    void SaveSelectedCharacter()
    {
        PlayerPrefs.SetInt("SelectedCharacter", (int)selectedCharacter);
        PlayerPrefs.Save();
    }

    void LoadSelectedCharacter()
    {
        if (PlayerPrefs.HasKey("SelectedCharacter"))
            selectedCharacter = (SelectedMC)PlayerPrefs.GetInt("SelectedCharacter");
    }

    public void ResetCharacter()
    {
        selectedCharacter = SelectedMC.None;
        PlayerPrefs.DeleteKey("SelectedCharacter");
    }
}
