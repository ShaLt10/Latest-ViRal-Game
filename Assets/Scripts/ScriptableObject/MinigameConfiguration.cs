using UnityEngine;

namespace DigitalForensicsQuiz
{
    /// <summary>
    /// Configuration ScriptableObject that centralizes all minigame settings.
    /// Provides a designer-friendly interface for adjusting game behavior, UI appearance,
    /// and text content without requiring code changes.
    /// 
    /// Should be placed in Resources folder as "MinigameConfig" for automatic loading.
    /// </summary>
    [CreateAssetMenu(fileName = "MinigameConfig", menuName = "Digital Forensics Quiz/Minigame Configuration")]
    public class MinigameConfiguration : ScriptableObject
    {
        #region Visual Settings

        [Header("Animation & Timing")]
        [Tooltip("Duration for background color transition animations")]
        [Range(0.1f, 2f)]
        public float animationDuration = 0.3f;

        [Tooltip("Fade duration for panel transitions")]
        [Range(0.1f, 1f)]
        public float panelTransitionDuration = 0.25f;

        [Header("Feedback Colors")]
        [Tooltip("Background overlay color when answer is correct")]
        public Color correctColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);

        [Tooltip("Background overlay color when answer is incorrect")]
        public Color incorrectColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);

        [Tooltip("Background overlay color for successful completion")]
        public Color successColor = new Color(0.0f, 0.5f, 0.0f, 0.3f);

        [Tooltip("Background overlay color for failed completion")]
        public Color failureColor = new Color(0.5f, 0.0f, 0.0f, 0.3f);

        [Header("UI Element Colors")]
        [Tooltip("Color when a multiple choice button is selected")]
        public Color buttonSelectedColor = new Color(0.8f, 0.8f, 1f, 1f);

        [Tooltip("Default color for multiple choice buttons")]
        public Color buttonDefaultColor = Color.white;

        [Tooltip("Color for tracking circles when correct")]
        public Color trackingCorrectColor = Color.green;

        [Tooltip("Color for tracking circles when incorrect")]
        public Color trackingIncorrectColor = Color.red;

        [Tooltip("Default color for tracking circles")]
        public Color trackingDefaultColor = Color.gray;

        #endregion

        #region Layout Settings

        [Header("Drag & Drop Configuration")]
        [Tooltip("Minimum touch/click area size for drag items")]
        [Range(60f, 120f)]
        public float dragItemMinSize = 90f;

        [Tooltip("Minimum size for drop zones")]
        [Range(100f, 200f)]
        public float dropZoneMinSize = 120f;

        [Tooltip("Snap distance for drag and drop interactions")]
        [Range(10f, 50f)]
        public float snapDistance = 20f;

        #endregion

        #region Game Flow Settings

        [Header("Game Flow Control")]
        [Tooltip("Show introduction dialog panel at game start")]
        public bool showDialogPanel = true;

        [Tooltip("Show instruction panel before starting questions")]
        public bool showInstructionPanel = true;

        [Tooltip("Require perfect score (all correct) for success")]
        public bool requirePerfectScore = false;

        [Tooltip("Minimum score percentage required for success (if not perfect score)")]
        [Range(0f, 1f)]
        public float minimumPassingScore = 0.5f;

        [Tooltip("Allow players to skip questions (for testing purposes)")]
        public bool allowQuestionSkip = false;

        [Tooltip("Show detailed explanations for each answer")]
        public bool showDetailedExplanations = true;

        #endregion

        #region Audio Settings

        [Header("Audio Configuration")]
        [Tooltip("Enable sound effects")]
        public bool enableSFX = true;

        [Tooltip("Enable background music")]
        public bool enableMusic = true;

        [Tooltip("Master volume level")]
        [Range(0f, 1f)]
        public float masterVolume = 0.8f;

        [Tooltip("SFX volume level")]
        [Range(0f, 1f)]
        public float sfxVolume = 0.7f;

        [Tooltip("Music volume level")]
        [Range(0f, 1f)]
        public float musicVolume = 0.5f;

        #endregion

        #region Text Content

        [Header("Dialog & Character")]
        [Tooltip("Character name displayed in dialog")]
        public string characterName = "Gavi";

        [Tooltip("Opening dialog text")]
        [TextArea(3, 5)]
        public string openingDialogText = "Baiklah tim, saatnya investigasi. Kita akan trace bukti digital dan pahami cara kerja scam ini.";

        [Header("Feedback Messages")]
        [Tooltip("Text displayed when answer is correct")]
        public string correctFeedback = "Benar!";

        [Tooltip("Text displayed when answer is incorrect")]
        public string incorrectFeedback = "Salah!";

        [Tooltip("Additional text for correct answers")]
        public string correctAdditionalText = "Jawaban Anda tepat!";

        [Tooltip("Additional text for incorrect answers")]
        public string incorrectAdditionalText = "Mari pelajari lebih lanjut:";

        [Header("Completion Messages")]
        [Tooltip("Message displayed on successful completion")]
        public string successMessage = "Target Secured!";

        [Tooltip("Message displayed on failed completion")]
        public string failureMessage = "Proses Gagal, Coba Lagi!";

        [Tooltip("Result text for perfect success")]
        public string successResultText = "SYSTEM HACKED SUCCESSFULLY!";

        [Tooltip("Result text format for failure (use {0} for score, {1} for total)")]
        public string failureResultTextFormat = "GAGAL! Skor: {0}/{1}";

        [Tooltip("Result text format for partial success")]
        public string partialSuccessResultTextFormat = "BERHASIL! Skor: {0}/{1}";

        [Header("Button Labels")]
        [Tooltip("Text for continue/next buttons")]
        public string continueButtonText = "Lanjut";

        [Tooltip("Text for submit answer button")]
        public string submitButtonText = "Submit";

        [Tooltip("Text for view results button")]
        public string viewResultsButtonText = "Lihat Hasil";

        [Tooltip("Text for restart button")]
        public string restartButtonText = "Coba Lagi";

        [Tooltip("Text for quit button")]
        public string quitButtonText = "Keluar";

        [Tooltip("Text for skip button (if enabled)")]
        public string skipButtonText = "Lewati";

        [Header("Instruction Text")]
        [Tooltip("Instructions for multiple choice questions")]
        [TextArea(2, 4)]
        public string multipleChoiceInstructions = "Pilih jawaban yang paling tepat dari pilihan yang tersedia.";

        [Tooltip("Instructions for drag and drop questions")]
        [TextArea(2, 4)]
        public string dragDropInstructions = "Seret setiap skenario ke kategori yang sesuai.";

        #endregion

        #region Resource Configuration

        [Header("Resource Paths")]
        [Tooltip("Path to questions folder in Resources directory")]
        public string questionsResourcePath = "Minigame3Questions";

        [Tooltip("Path to audio files in Resources directory")]
        public string audioResourcePath = "Audio";

        [Tooltip("Path to sprite assets in Resources directory")]
        public string spritesResourcePath = "Sprites";

        #endregion

        #region Debug Settings

        [Header("Debug & Development")]
        [Tooltip("Enable debug logging")]
        public bool enableDebugLogging = false;

        [Tooltip("Show question IDs in UI (for development)")]
        public bool showQuestionIds = false;

        [Tooltip("Enable developer shortcuts (spacebar to skip, etc.)")]
        public bool enableDeveloperShortcuts = false;

        [Tooltip("Force specific question order (disable shuffling)")]
        public bool forceQuestionOrder = false;

        #endregion

        #region Singleton Implementation

        /// <summary>
        /// Singleton instance for global access to configuration settings.
        /// Automatically loads from Resources folder on first access.
        /// </summary>
        private static MinigameConfiguration _instance;

        public static MinigameConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<MinigameConfiguration>("MinigameConfig");

                    if (_instance == null)
                    {
                        Debug.LogWarning("MinigameConfiguration not found in Resources folder. Creating default configuration.");
                        _instance = CreateDefaultConfiguration();
                    }
                    else if (_instance.enableDebugLogging)
                    {
                        Debug.Log("MinigameConfiguration loaded successfully from Resources.");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Clears the cached instance. Useful for editor refresh or testing scenarios.
        /// </summary>
        public static void ClearInstance()
        {
            _instance = null;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Creates a default configuration with sensible fallback values.
        /// Used when no configuration asset is found in Resources.
        /// </summary>
        /// <returns>Default configuration instance</returns>
        private static MinigameConfiguration CreateDefaultConfiguration()
        {
            var config = CreateInstance<MinigameConfiguration>();

            // Set default values (values are already set in field declarations)
            Debug.Log("Using default MinigameConfiguration values.");

            return config;
        }

        /// <summary>
        /// Validates the configuration settings and logs warnings for potential issues.
        /// Called automatically in the editor when values change.
        /// </summary>
        public void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(questionsResourcePath))
            {
                Debug.LogWarning("MinigameConfiguration: Questions resource path is empty!");
            }

            if (minimumPassingScore < 0f || minimumPassingScore > 1f)
            {
                Debug.LogWarning("MinigameConfiguration: Minimum passing score should be between 0 and 1!");
                minimumPassingScore = Mathf.Clamp01(minimumPassingScore);
            }

            if (animationDuration <= 0f)
            {
                Debug.LogWarning("MinigameConfiguration: Animation duration should be greater than 0!");
                animationDuration = 0.3f;
            }
        }

        /// <summary>
        /// Gets the appropriate result text based on score and settings.
        /// </summary>
        /// <param name="correctCount">Number of correct answers</param>
        /// <param name="totalCount">Total number of questions</param>
        /// <returns>Formatted result text</returns>
        public string GetResultText(int correctCount, int totalCount)
        {
            if (totalCount == 0) return "No questions available";

            float scorePercentage = (float)correctCount / totalCount;
            bool isPerfect = correctCount == totalCount;
            bool isPassing = requirePerfectScore ? isPerfect : scorePercentage >= minimumPassingScore;

            if (isPerfect)
            {
                return successResultText;
            }
            else if (isPassing)
            {
                return string.Format(partialSuccessResultTextFormat, correctCount, totalCount);
            }
            else
            {
                return string.Format(failureResultTextFormat, correctCount, totalCount);
            }
        }

        /// <summary>
        /// Determines if the game should be considered successfully completed.
        /// </summary>
        /// <param name="correctCount">Number of correct answers</param>
        /// <param name="totalCount">Total number of questions</param>
        /// <returns>True if the completion criteria are met</returns>
        public bool IsGameSuccessful(int correctCount, int totalCount)
        {
            if (totalCount == 0) return false;

            if (requirePerfectScore)
            {
                return correctCount == totalCount;
            }
            else
            {
                float scorePercentage = (float)correctCount / totalCount;
                return scorePercentage >= minimumPassingScore;
            }
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only validation that runs when configuration values change in the inspector.
        /// Ensures configuration remains in a valid state during development.
        /// </summary>
        private void OnValidate()
        {
            ValidateConfiguration();
        }

        /// <summary>
        /// Creates a menu item for easy configuration validation in the editor.
        /// </summary>
        [UnityEditor.MenuItem("Digital Forensics Quiz/Validate Configuration")]
        public static void ValidateConfigurationMenuItem()
        {
            var config = Instance;
            config.ValidateConfiguration();
            Debug.Log("Configuration validation complete.");
        }

        /// <summary>
        /// Creates a menu item to reset configuration to defaults.
        /// </summary>
        [UnityEditor.MenuItem("Digital Forensics Quiz/Reset Configuration to Defaults")]
        public static void ResetToDefaults()
        {
            var config = Instance;
            UnityEditor.EditorUtility.SetDirty(config);

            // Reset to default values
            var defaultConfig = CreateDefaultConfiguration();
            UnityEditor.EditorUtility.CopySerialized(defaultConfig, config);

            Debug.Log("Configuration reset to default values.");
        }
#endif

        #endregion
    }
}