// ScreenController.cs - CLEAN VERSION (No Pixel Crushers)
// PELETAKAN: Scripts/Game/ScreenController.cs
// FUNGSI: Global game state controller (win/lose, screen transitions, etc)
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenController : MonoBehaviour
{
    // ========================================
    // SINGLETON
    // ========================================
    public static ScreenController Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this; // ✅ FIX: Set instance
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("[ScreenController] Initialized");
    }

    // ========================================
    // GAME STATE METHODS
    // ========================================
    
    /// <summary>
    /// Called when player wins (minigame, quest, etc)
    /// </summary>
    public void GameWin()
    {
        Debug.Log("[ScreenController] Game Win!");
        
        // Trigger win dialog (if using DialogManager)
        if (DialogManager.Instance != null)
        {
            // Play win sequence (customize sequence name)
            DialogManager.Instance.PlaySequenceByName("WinDialog", OnWinDialogComplete);
        }
        else
        {
            // No dialog, proceed directly
            OnWinDialogComplete();
        }
    }
    
    /// <summary>
    /// Called after win dialog completes
    /// </summary>
    void OnWinDialogComplete()
    {
        Debug.Log("[ScreenController] Win dialog complete");
        
        // Example actions after win:
        // - Load next scene
        // - Show reward screen
        // - Update player progress
        // - etc
        
        // Uncomment to load next scene:
        // LoadNextScene();
    }
    
    /// <summary>
    /// Called when player loses (minigame, quest failed, etc)
    /// </summary>
    public void GameLose()
    {
        Debug.Log("[ScreenController] Game Lose!");
        
        // Trigger lose dialog
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.PlaySequenceByName("LoseDialog", OnLoseDialogComplete);
        }
        else
        {
            OnLoseDialogComplete();
        }
    }
    
    void OnLoseDialogComplete()
    {
        Debug.Log("[ScreenController] Lose dialog complete");
        
        // Example actions after lose:
        // - Retry option
        // - Return to checkpoint
        // - etc
    }
    
    // ========================================
    // SCENE MANAGEMENT
    // ========================================
    
    /// <summary>
    /// Load scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[ScreenController] Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Load next scene in build index
    /// </summary>
    public void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("[ScreenController] No next scene in build settings");
        }
    }
    
    /// <summary>
    /// Reload current scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[ScreenController] Reloading scene: {currentScene}");
        SceneManager.LoadScene(currentScene);
    }
    
    // ========================================
    // SCREEN TRANSITIONS (Optional)
    // ========================================
    
    /// <summary>
    /// Load scene with fade transition (requires FadeController)
    /// </summary>
    public void LoadSceneWithFade(string sceneName, float fadeDuration = 1f)
    {
        StartCoroutine(LoadSceneWithFadeCoroutine(sceneName, fadeDuration));
    }
    
    IEnumerator LoadSceneWithFadeCoroutine(string sceneName, float fadeDuration)
    {
        // TODO: Add fade out effect here
        // Example: FadeController.Instance.FadeOut(fadeDuration);
        
        yield return new WaitForSeconds(fadeDuration);
        
        SceneManager.LoadScene(sceneName);
        
        // TODO: Add fade in effect here
        // Example: FadeController.Instance.FadeIn(fadeDuration);
    }
    
    // ========================================
    // PUBLIC API
    // ========================================
    
    /// <summary>
    /// Get current scene name
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// Check if scene is loaded
    /// </summary>
    public bool IsSceneLoaded(string sceneName)
    {
        return SceneManager.GetActiveScene().name == sceneName;
    }
    
    // ========================================
    // DEBUG TOOLS
    // ========================================
    
    #if UNITY_EDITOR
    [ContextMenu("Test Game Win")]
    void TestGameWin()
    {
        GameWin();
    }
    
    [ContextMenu("Test Game Lose")]
    void TestGameLose()
    {
        GameLose();
    }
    
    [ContextMenu("Print Current Scene")]
    void PrintCurrentScene()
    {
        Debug.Log($"Current Scene: {GetCurrentSceneName()}");
    }
    #endif
}