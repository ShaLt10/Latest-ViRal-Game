using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map1 : SingletonDestroy<Map1>
{
    // Start is called before the first frame update
    void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();

        // FIXED: Gunakan CharacterManager singleton yang baru
        if (CharacterManager.Instance != null)
        {
            string selectedCharacter = CharacterManager.Instance.GetPlayerName();
            string dialogKey = $"Area1-PlayerOpening-{selectedCharacter}";
            
            Debug.Log($"Triggering dialog: {dialogKey}");
            EventManager.Publish(new OnDialogueRequestData(dialogKey));
        }
        else
        {
            // Fallback jika CharacterManager belum ready
            Debug.LogWarning("CharacterManager.Instance is null! Using default dialog.");
            EventManager.Publish(new OnDialogueRequestData("Area1-PlayerOpening-Raline"));
        }
    }
}