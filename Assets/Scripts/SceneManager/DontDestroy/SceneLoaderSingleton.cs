using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderSingleton : Singleton<SceneLoaderSingleton>
{
    public void LoadSceneMode(int scene)
    {
        SceneManager.LoadScene(scene);
    }
}
