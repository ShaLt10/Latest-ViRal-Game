using UnityEngine;

public class Map1 : SingletonDestroy<Map1>
{
    [Header("Keys (fallback kalau CharacterManager null/unknown)")]
    [Tooltip("Key default kalau player belum terdeteksi")]
    public string defaultOpeningKey = "Area1-PlayerOpening-Raline";

    private void Start()
    {
        // Pastikan orientasi
        ScreenRotateControl.Instance.SetLandscape();

        // Pastikan DialogManager ada (DDOL via AutoBootstrap atau sudah ada di scene)
        if (DialogManager.Instance == null)
        {
            Debug.LogError("[Map1] DialogManager.Instance null. Pastikan AutoBootstrap/Prefab Managers sudah jalan.");
            return;
        }

        // Ambil nama karakter player dari CharacterManager (Gavi/Raline)
        string selected = CharacterManager.Instance != null 
            ? CharacterManager.Instance.GetPlayerName()
            : null;

        // Rakit key: "Area1-PlayerOpening-Gavi" / "Area1-PlayerOpening-Raline"
        string key = !string.IsNullOrEmpty(selected) 
            ? $"Area1-PlayerOpening-{selected}" 
            : defaultOpeningKey;

        Debug.Log($"[Map1] Triggering dialog: {key}");

        // Mainkan dialog pembuka; setelah selesai kamu bisa lanjut apa pun (opsional)
        DialogManager.Instance.PlaySequenceThen(key, onDone: () =>
        {
            Debug.Log("[Map1] Opening dialog finished.");
            // TODO: taruh logic lanjutan map di sini (enable input, spawn NPC marker, dsb) 
        });
    }
}
