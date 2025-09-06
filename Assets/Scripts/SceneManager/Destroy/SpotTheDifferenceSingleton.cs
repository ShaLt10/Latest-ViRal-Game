using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Eventdata;
using Game.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpotTheDifferenceSingleton : Singleton<SpotTheDifferenceSingleton>
{
    private int gameType = 0;
    
    [SerializeField]
    private GameObject game1;

    [SerializeField]
    private GameObject game2;

    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();

        EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.SpotTheDifference_Opening}", StartGame));
    }

    public void StartGame()
    {
        EventManager.Publish<SpotTheDifferenceStatusData>(new SpotTheDifferenceStatusData(true,false,false));
    }
    
    public void StopGame()
    {
        EventManager.Publish<SpotTheDifferenceStatusData>(new SpotTheDifferenceStatusData(false,false,false));
    }

    public void SetGame(int add)
    {
        gameType+= add ;
        DisableType();
        if (gameType == 0)
        {
            game1.SetActive(true);
            EventManager.Publish(new ResetGameData());
        }
        if (gameType == 1)
        {
            game2.SetActive(true);
            if (add != 0)
            {
                EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.SpotTheDifference_Stage2}",() => EventManager.Publish(new ResetGameData())));
            }
            else
            {
                EventManager.Publish(new ResetGameData());
            }
        }
        else if (gameType >1) 
        {
            var scene = SceneManager.GetActiveScene();
            
            // FIXED: Gunakan CharacterManager singleton yang baru
            if (CharacterManager.Instance != null)
            {
                if (CharacterManager.Instance.GetPlayerName() == "Gavi")
                {
                    EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.SpotTheDifference_win_Gavi}", () => SceneLoaderSingleton.Instance.LoadSceneMode(scene.buildIndex+1)));
                }
                else
                {
                    EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.SpotTheDifference_win_Raline}", () => SceneLoaderSingleton.Instance.LoadSceneMode(scene.buildIndex + 1)));
                }
            }
            else
            {
                // Fallback jika CharacterManager belum ready
                Debug.LogWarning("CharacterManager.Instance is null! Using default Raline win dialog.");
                EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.SpotTheDifference_win_Raline}", () => SceneLoaderSingleton.Instance.LoadSceneMode(scene.buildIndex + 1)));
            }
        }
    }

    public void DisableType()
    { 
        game1?.SetActive(false);
        game2?.SetActive(false);
    }
}