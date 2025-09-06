using UnityEngine;

public class OpenURLButton : MonoBehaviour
{
    [SerializeField] private string url = "https://yourlink.com";

    public void OpenURL()
    {
        Application.OpenURL(url);
    }
}
