using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
public class SceneLoader : MonoBehaviour
{
    public Transform[] Characters;
    public Transform[] CafeSpawnPoints;
    public MMF_Player AfterMoveSceneFeedback;
    public void MoveToCafe()
    {
        Invoke("MovingToCafe", 15f);
    }

    public void MoveToMenu()
    {
        Invoke("MovingToMenu", 18f);
    }

    private void MovingToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void InstantMove()
    {
        MovingToCafe();
    }

    void MovingToCafe()
    {
        Characters[0].transform.position = CafeSpawnPoints[0].position;
        if(Characters.Length>1)
            Characters[1].transform.position = CafeSpawnPoints[1].position;
        AfterMoveSceneFeedback?.PlayFeedbacks();
    }
}
