using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameObject HighscorePanel;
    public TMP_Text ScoreText;
    public void ExitGame()
    {
        Application.Quit();
    }

    public void PlayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ShowHighscore(bool toggle)
    {
        int score = PlayerPrefs.GetInt("Highscore", 0);
        ScoreText.text = score.ToString();
        HighscorePanel.SetActive(toggle);
    }
}
