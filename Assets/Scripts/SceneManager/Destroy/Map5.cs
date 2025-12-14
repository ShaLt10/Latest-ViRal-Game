using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map5 : SingletonDestroy<Map5>
{
    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();
        
        // FIXED: Gunakan CharacterManager singleton yang baru
        if (CharacterManager.Instance != null)
        {
            if (CharacterManager.Instance.GetPlayerName() == "Gavi")
            {
                EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.MapEpilogue_Gavi}", () => SceneManager.LoadScene(0)));
            }
            else
            {
                EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.MapEpilogue_Raline}", () => SceneManager.LoadScene(0)));
            }
        }
        else
        {
            // Fallback jika CharacterManager belum ready
            Debug.LogWarning("CharacterManager.Instance is null! Using default Raline epilogue.");
            EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.MapEpilogue_Raline}", () => SceneManager.LoadScene(0)));
        }
    }
}