using UnityEngine;


[DefaultExecutionOrder(-100)]
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance = null;
    protected static object _lock = new object();
    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError($"[Singleton] Multiple instances of {typeof(T)} detected!");
                        return _instance;
                    }

                    if (_instance == null)
                    {

                        Debug.Log($"[Singleton] move to dont destroy");
                        GameObject singletonObj = new GameObject(typeof(T).Name);
                        _instance = singletonObj.AddComponent<T>();
                        DontDestroyOnLoad(singletonObj);
                    }
                }


                return _instance;
            }
        }
    }

    protected static bool applicationIsQuitting = false;

    protected virtual void OnDestroy()
    {
        applicationIsQuitting = true;
    }

    protected virtual void OnEnable()
    {
        if (_instance != null)
        {
            DontDestroyOnLoad(_instance);
        }
    }
}
