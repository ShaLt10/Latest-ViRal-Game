using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreDetermindSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        // Check for existing instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Kill duplicate
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
}
