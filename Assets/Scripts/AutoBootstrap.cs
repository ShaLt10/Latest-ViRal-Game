// AutoBootstrap.cs - ENHANCED VERSION
// PELETAKAN: Assets/Scripts/Bootstrap/AutoBootstrap.cs
// FUNGSI: Auto-spawn persistent managers saat game start
using UnityEngine;

public static class AutoBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Debug.Log("=== AutoBootstrap Starting ===");
        
        // Spawn managers dari Resources/Managers/
        SpawnDDOL("Managers/DialogManager");
        SpawnDDOL("Managers/ObjectiveManager");
        
        // Optional: CharacterManager (kalau ada)
        SpawnDDOL("Managers/CharacterManager");
        
        Debug.Log("=== AutoBootstrap Complete ===");
    }

    private static void SpawnDDOL(string resourcesPath)
    {
        // Load prefab dari Resources folder
        var prefab = Resources.Load<GameObject>(resourcesPath);
        
        if (prefab == null)
        {
            Debug.LogWarning($"[AutoBootstrap] ⚠️ Prefab tidak ditemukan: Resources/{resourcesPath}");
            Debug.LogWarning($"[AutoBootstrap] Pastikan prefab ada di: Assets/Resources/{resourcesPath}.prefab");
            return;
        }

        // Check kalau sudah ada di scene (avoid duplicate)
        string managerName = prefab.name;
        var existing = GameObject.Find(managerName);
        
        if (existing != null)
        {
            Debug.Log($"[AutoBootstrap] ℹ️ {managerName} sudah ada di scene, skip spawn.");
            Object.DontDestroyOnLoad(existing);
            return;
        }

        // Instantiate prefab
        var go = Object.Instantiate(prefab);
        go.name = managerName; // Hapus "(Clone)" dari nama
        Object.DontDestroyOnLoad(go);
        
        Debug.Log($"[AutoBootstrap] ✓ Spawned: {go.name}");
    }
}