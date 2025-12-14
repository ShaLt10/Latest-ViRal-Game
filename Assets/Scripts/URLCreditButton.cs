using UnityEngine;

public class OpenURLButton : MonoBehaviour
{
    [SerializeField] string url;

    [Header("Options")]
    [SerializeField] bool addHttpsIfMissing = true;   // auto tambah https:// kalau kamu isi "instagram.com/..."
    [SerializeField] bool logWhenOpened = true;

    public void OpenURL()
    {
        var u = url?.Trim();

        if (string.IsNullOrEmpty(u))
        {
            Debug.LogWarning("[OpenURLButton] URL kosong di " + name);
            return;
        }

        // tambahkan skema kalau belum ada
        if (addHttpsIfMissing && !u.Contains("://"))
            u = "https://" + u;

#if UNITY_WEBGL
        Application.ExternalEval($"window.open('{u}', '_blank')");
#else
        Application.OpenURL(u);
#endif

        if (logWhenOpened) Debug.Log("[OpenURLButton] Open " + u);
    }
}
