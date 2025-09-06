using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;

namespace DigitalForensicsQuiz
{
    // REMOVED: Duplicate class definitions for DropCategory and DragScenario
    // These classes are already defined in MinigameQuestionData.cs
    // Use: DigitalForensicsQuiz.DropCategory and DigitalForensicsQuiz.DragScenario instead

    public class MinigameManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private GameObject instructionPanel;
        [SerializeField] private GameObject minigamePanel;
        [SerializeField] private GameObject gamePanel; // Keep for compatibility
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private GameObject completionScreen;

        [Header("Panel Backgrounds")]
        [SerializeField] private GameObject feedbackBG;
        [SerializeField] private GameObject resultBG;
        [SerializeField] private GameObject completionBG;

        [Header("Core UI")]
        [SerializeField] private Transform trackingBar;
        [SerializeField] private GameObject trackingCirclePrefab;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button submitButton;

        [Header("Multiple Choice")]
        [SerializeField] private Transform multipleChoiceContainer;
        [SerializeField] private Button optionButtonPrefab;

        [Header("Drag and Drop")]
        [SerializeField] private Transform dragItemContainer;
        [SerializeField] private Transform dropZoneContainer;
        [SerializeField] private RectTransform dragContainerRect;
        [SerializeField] private DragItem dragItemPrefab;
        [SerializeField] private DropZone dropZonePrefab;

        [Header("Feedback UI")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI explanationText;
        [SerializeField] private Button nextQuestionButton;

        [Header("Result UI")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button resultNextButton;
        [SerializeField] private Button retryButton; // Keep for compatibility
        [SerializeField] private Button exitButton; // Keep for compatibility

        [Header("Dialog UI")]
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Button dialogNextButton;

        [Header("Instruction UI")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Image glitchImage;
        [SerializeField] private Button confirmationButton;

        [Header("Completion UI - Success")]
        [SerializeField] private GameObject successElements;
        [SerializeField] private TextMeshProUGUI successHeaderText;
        [SerializeField] private TextMeshProUGUI trackingText;
        [SerializeField] private TextMeshProUGUI ipAddressText;
        [SerializeField] private TextMeshProUGUI locationText;
        [SerializeField] private Button closeButton;

        [Header("Completion UI - Failed")]
        [SerializeField] private GameObject failedElements;
        [SerializeField] private TextMeshProUGUI failedText;
        [SerializeField] private Button restartButton;

        [Header("Text Settings")]
        [SerializeField] private string characterName = "Gavi";
        [TextArea(2, 4)]
        [SerializeField] private string openingDialogText = "Baiklah tim, saatnya investigasi. Kita akan trace bukti digital dan pahami cara kerja scam ini.";
        [SerializeField] private string correctFeedback = "Benar!";
        [SerializeField] private string incorrectFeedback = "Salah!";
        [SerializeField] private string successMessage = "PERFECT! Semua jawaban benar!";
        [SerializeField] private string failureMessage = "GAGAL! Skor: {0}/{1}";

        [Header("Completion Text Settings")]
        [SerializeField] private string successHeader = "INVESTIGASI BERHASIL!";
        [SerializeField] private string trackingMessage = "Jejak digital berhasil dilacak...";
        [SerializeField] private string ipAddress = "IP Address: 192.168.1.45";
        [SerializeField] private string location = "Location: Jakarta, Indonesia";
        [SerializeField] private string failedMessage = "Investigasi gagal! Tim forensik harus mengulang analisis.";

        [Header("Color Settings")]
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color incorrectColor = Color.red;
        [SerializeField] private Color selectedButtonColor = Color.cyan;
        [SerializeField] private Color defaultButtonColor = Color.white;
        [SerializeField] private Color trackingDefaultColor = Color.gray;

        [Header("Animation Settings")]
        [SerializeField] private float typewriterSpeed = 0.05f;
        [SerializeField] private float glitchInterval = 0.1f;
        [SerializeField] private float glitchIntensity = 5f;

        [Header("Canvas")]
        [SerializeField] private CanvasScaler canvasScaler;

        [Header("Text Display")]
        [SerializeField] private TextMeshProUGUI scoreText;

        // FIXED: State variables using proper class references
        private List<MinigameQuestionData> _allQuestions;
        private int _currentQuestionIndex = 0;
        private List<bool> _questionResults = new List<bool>();
        private List<Image> _trackingCircles = new List<Image>();

        // Multiple Choice State
        private int _selectedAnswerIndex = -1;
        private int _shuffledCorrectIndex = -1;
        private List<Button> _spawnedOptionButtons = new List<Button>();

        // UNIFIED Drag & Drop State - Now using classes from MinigameQuestionData.cs
        private Dictionary<string, string> _dragDropAnswers = new Dictionary<string, string>();
        private Dictionary<string, string> _correctMappingsDict = new Dictionary<string, string>();
        private List<DragItem> _spawnedDragItems = new List<DragItem>();
        private List<DropZone> _spawnedDropZones = new List<DropZone>();
        private int _expectedPairingCount = 0;
        private HashSet<string> _pairedScenarioIds = new HashSet<string>();

        // Legacy compatibility lists - DEPRECATED, use only for fallback
        private List<DropZone> dropZones = new List<DropZone>();
        private List<DragItem> dragItems = new List<DragItem>();

        // Animation State
        private Coroutine _currentTypewriterCoroutine;
        private Coroutine _currentGlitchCoroutine;
        private Vector3 _originalGlitchPosition;

        private MinigameAudioManager _audioManager;
        private bool _trackingBarInitialized = false;

        private void Start()
        {
            Debug.Log("=== MINIGAME MANAGER START METHOD CALLED ===");
            ForceInitializePanels();
            InitializeGame();
        }

        private void InitializeGame()
        {
            Debug.Log("=== INITIALIZE GAME START ===");

            _audioManager = FindObjectOfType<MinigameAudioManager>();

            _allQuestions = QuestionProvider.GetAllQuestions();
            if (_allQuestions == null || _allQuestions.Count == 0)
            {
                Debug.LogError("No questions loaded!");
                SetupLegacyGame(); // Fallback to legacy setup
                return;
            }

            // Enhanced submit button initialization
            if (submitButton != null)
            {
                Debug.Log($"Submit button reference found: {submitButton.name}");
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(OnSubmitButtonClicked);
                submitButton.gameObject.SetActive(false); // Initially hidden
            }
            else
            {
                Debug.LogError("Submit button reference is missing! Check inspector assignment.");
                return;
            }

            SetupButtonListeners();
            InitializeBackgrounds();
            ShowPanel(dialogPanel);
            SetupDialog();
        }

        private void SetupLegacyGame()
        {
            Debug.Log("Setting up legacy game mode...");
            
            // Legacy button setup - REDIRECT to new logic
            if (submitButton != null)
            {
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(OnSubmitButtonClicked); // Use unified logic
            }

            if (retryButton != null)
                retryButton.onClick.AddListener(RestartGame);

            if (exitButton != null)
                exitButton.onClick.AddListener(ExitToMenu);

            SetupLegacyDragDrop();
        }

        private void SetupLegacyDragDrop()
        {
            ClearChildren(dragItemContainer.GetComponent<RectTransform>());
            ClearChildren(dropZoneContainer.GetComponent<RectTransform>());
            
            // Clear ALL lists for consistency
            ClearAllDragDropLists();

            // Create legacy dummy data for testing - FIXED: Using proper class references
            for (int i = 0; i < 3; i++)
            {
                var category = new DropCategory
                {
                    id = $"cat_{i}",
                    categoryName = $"Category {i + 1}",
                    zoneColor = Color.Lerp(Color.yellow, Color.red, i / 2f)
                };

                var dropZone = Instantiate(dropZonePrefab, dropZoneContainer);
                dropZone.SetupVisuals(category, this);
                
                // Use UNIFIED lists only
                _spawnedDropZones.Add(dropZone);
                dropZones.Add(dropZone); // Keep for legacy compatibility
            }

            for (int i = 0; i < 5; i++)
            {
                var correctCategoryId = $"cat_{Random.Range(0, _spawnedDropZones.Count)}";
                
                // FIXED: Create DragScenario using the proper class from MinigameQuestionData.cs
                var scenario = new DragScenario
                {
                    id = $"sc_{i}",
                    description = $"Scenario {i + 1}"
                    // Note: correctCategoryId is no longer part of DragScenario in the new structure
                };

                var dragItem = Instantiate(dragItemPrefab, dragItemContainer);
                
                // Use the legacy Initialize overload that handles objects without correctCategoryId
                dragItem.Initialize(scenario, this);
                
                // Use UNIFIED lists only
                _spawnedDragItems.Add(dragItem);
                dragItems.Add(dragItem); // Keep for legacy compatibility
                
                // Build correct mappings
                _correctMappingsDict[scenario.id] = correctCategoryId;
            }

            _expectedPairingCount = _spawnedDragItems.Count;

            if (submitButton != null)
                submitButton.interactable = true;
        }

        private void ClearAllDragDropLists()
        {
            _spawnedDragItems.Clear();
            _spawnedDropZones.Clear();
            _dragDropAnswers.Clear();
            _pairedScenarioIds.Clear();
            _correctMappingsDict.Clear();
            _expectedPairingCount = 0;
            
            // Legacy lists
            dropZones.Clear();
            dragItems.Clear();
        }

        private void ClearChildren(RectTransform container)
        {
            if (container == null) return;
            
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.GetChild(i).gameObject);
            }
        }

        // UNIFIED SUBMIT LOGIC - Single source of truth
        public void OnSubmit()
        {
            Debug.Log("Legacy OnSubmit called - redirecting to unified logic");
            OnSubmitButtonClicked(); // Redirect to unified logic
        }

        private void OnSubmitButtonClicked()
        {
            Debug.Log("=== UNIFIED SUBMIT BUTTON CLICKED ===");
            SubmitAnswer();
        }

        public void ExitToMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void ForceCleanupPreviousQuestion()
        {
            Debug.Log("=== FORCE CLEANUP PREVIOUS QUESTION (UNIFIED) ===");

            // Clear multiple choice elements
            for (int i = _spawnedOptionButtons.Count - 1; i >= 0; i--)
            {
                if (_spawnedOptionButtons[i] != null)
                {
                    DestroyImmediate(_spawnedOptionButtons[i].gameObject);
                }
            }
            _spawnedOptionButtons.Clear();

            // Clear drag & drop elements using UNIFIED method
            ForceCleanupDragDropElements();

            if (multipleChoiceContainer != null)
            {
                while (multipleChoiceContainer.childCount > 0)
                {
                    DestroyImmediate(multipleChoiceContainer.GetChild(0).gameObject);
                }
                multipleChoiceContainer.gameObject.SetActive(false);
            }

            _selectedAnswerIndex = -1;
            _shuffledCorrectIndex = -1;
        }

        private void StartMinigame()
        {
            _currentQuestionIndex = 0;
            _questionResults.Clear();

            // Reset tracking bar initialization flag and recreate it
            _trackingBarInitialized = false;
            CreateTrackingBar();

            ShowPanel(minigamePanel ?? gamePanel); // Use gamePanel as fallback for legacy compatibility
            DisplayCurrentQuestion();
        }

        private void CreateTrackingBar()
        {
            if (_trackingBarInitialized || trackingBar == null || trackingCirclePrefab == null)
                return;

            // Clear existing circles
            foreach (Transform child in trackingBar)
            {
                Destroy(child.gameObject);
            }
            _trackingCircles.Clear();

            // Create circles for each question
            for (int i = 0; i < _allQuestions.Count; i++)
            {
                GameObject circleObj = Instantiate(trackingCirclePrefab, trackingBar);
                Image circleImage = circleObj.GetComponent<Image>();
                if (circleImage != null)
                {
                    circleImage.color = trackingDefaultColor;
                    _trackingCircles.Add(circleImage);
                }
            }

            _trackingBarInitialized = true;
            Debug.Log($"Tracking bar created with {_trackingCircles.Count} circles");
        }

        // UNIFIED SubmitAnswer with single validation path
        private void SubmitAnswer()
        {
            Debug.Log("=== UNIFIED SUBMIT ANSWER ===");

            if (_currentQuestionIndex >= _allQuestions.Count)
            {
                Debug.LogError("Current question index out of bounds!");
                return;
            }

            MinigameQuestionData question = _allQuestions[_currentQuestionIndex];
            bool isCorrect = false;

            if (question.type == QuestionType.MultipleChoice)
            {
                Debug.Log($"Validating Multiple Choice - Selected: {_selectedAnswerIndex}, Correct: {_shuffledCorrectIndex}");
                isCorrect = AnswerValidator.ValidateMultipleChoice(question, _selectedAnswerIndex, _shuffledCorrectIndex);
            }
            else if (question.type == QuestionType.DragAndDrop)
            {
                Debug.Log($"Validating Drag and Drop with unified data:");
                Debug.Log($"  Answers: {string.Join(", ", _dragDropAnswers.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
                Debug.Log($"  Expected: {string.Join(", ", _correctMappingsDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
                
                // Use UNIFIED validation with correct mappings
                isCorrect = ValidateUnifiedDragAndDrop(question, _dragDropAnswers);
            }

            Debug.Log($"Answer is correct: {isCorrect}");

            _questionResults.Add(isCorrect);
            UpdateTrackingCircle(_currentQuestionIndex, isCorrect);

            string explanationToShow = isCorrect ? question.correctExplanation : question.incorrectExplanation;
            ShowFeedbackPanel(isCorrect, explanationToShow);
        }

        // UNIFIED drag and drop validation
        private bool ValidateUnifiedDragAndDrop(MinigameQuestionData question, Dictionary<string, string> userAnswers)
        {
            // Use the pre-built correct mappings dictionary
            if (_correctMappingsDict.Count == 0)
            {
                Debug.LogError("No correct mappings available for validation!");
                return false;
            }

            Debug.Log($"Validating {userAnswers.Count} user answers against {_correctMappingsDict.Count} expected mappings");

            // Check if all expected scenarios are answered
            foreach (var expectedMapping in _correctMappingsDict)
            {
                if (!userAnswers.ContainsKey(expectedMapping.Key))
                {
                    Debug.Log($"Missing answer for scenario: {expectedMapping.Key}");
                    return false;
                }

                if (userAnswers[expectedMapping.Key] != expectedMapping.Value)
                {
                    Debug.Log($"Incorrect mapping for {expectedMapping.Key}: got {userAnswers[expectedMapping.Key]}, expected {expectedMapping.Value}");
                    return false;
                }
            }

            Debug.Log("All drag and drop answers are correct!");
            return true;
        }

        private void ShowPanel(GameObject panelToShow)
        {
            if (panelToShow == null) return;

            // Hide all panels first
            if (dialogPanel != null) dialogPanel.SetActive(false);
            if (instructionPanel != null) instructionPanel.SetActive(false);
            if (minigamePanel != null) minigamePanel.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(false); // Legacy compatibility
            if (feedbackPanel != null) feedbackPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
            if (completionScreen != null) completionScreen.SetActive(false);

            // Hide all backgrounds
            if (feedbackBG != null) feedbackBG.SetActive(false);
            if (resultBG != null) resultBG.SetActive(false);
            if (completionBG != null) completionBG.SetActive(false);

            // Show target panel
            panelToShow.SetActive(true);

            // Handle special cases
            if (panelToShow == instructionPanel)
            {
                SetupInstructionPanel();
            }
            else if (panelToShow == completionScreen)
            {
                ShowCompletionScreen();
                return;
            }
        }

        private IEnumerator DisplayMultipleChoiceCoroutine(MinigameQuestionData question)
        {
            Debug.Log("=== DISPLAYING MULTIPLE CHOICE ===");

            if (multipleChoiceContainer == null || optionButtonPrefab == null)
            {
                Debug.LogError("Multiple choice container or prefab is null!");
                yield break;
            }

            multipleChoiceContainer.gameObject.SetActive(true);

            var shuffledOptions = question.GetShuffledOptions(out _shuffledCorrectIndex);
            _selectedAnswerIndex = -1;

            for (int i = 0; i < shuffledOptions.Count; i++)
            {
                Button optionButton = Instantiate(optionButtonPrefab, multipleChoiceContainer);
                optionButton.gameObject.SetActive(true);
                optionButton.interactable = true;

                TextMeshProUGUI buttonText = optionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = shuffledOptions[i];
                }

                int buttonIndex = i;
                optionButton.onClick.RemoveAllListeners();
                optionButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Option button {buttonIndex} clicked");
                    SelectOption(buttonIndex);
                });

                _spawnedOptionButtons.Add(optionButton);
                yield return null;
            }

            yield return StartCoroutine(RefreshMultipleChoiceLayout());
        }

        private IEnumerator RefreshMultipleChoiceLayout()
        {
            yield return new WaitForEndOfFrame();

            if (multipleChoiceContainer != null)
            {
                RectTransform containerRect = multipleChoiceContainer.GetComponent<RectTransform>();
                if (containerRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
                }
            }
        }

        public void SelectOption(int index)
        {
            Debug.Log($"Option {index} selected");

            _selectedAnswerIndex = index;
            _audioManager?.PlayButtonClickSFX();

            for (int i = 0; i < _spawnedOptionButtons.Count; i++)
            {
                if (_spawnedOptionButtons[i] != null)
                {
                    Image buttonImage = _spawnedOptionButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = (i == index) ? selectedButtonColor : defaultButtonColor;
                    }
                }
            }

            EnableSubmitButton("Option selected");
        }

        private void UpdateTrackingCircle(int questionIndex, bool isCorrect)
        {
            if (questionIndex < 0 || questionIndex >= _trackingCircles.Count) return;
            _trackingCircles[questionIndex].color = isCorrect ? correctColor : incorrectColor;
        }

        private void ShowFeedbackPanel(bool isCorrect, string explanation)
        {
            ShowPanel(feedbackPanel);
            if (feedbackBG != null) feedbackBG.SetActive(true);

            if (feedbackText != null)
            {
                feedbackText.text = isCorrect ? correctFeedback : incorrectFeedback;
                feedbackText.color = isCorrect ? correctColor : incorrectColor;
            }

            if (explanationText != null)
                explanationText.text = explanation;

            if (nextQuestionButton != null)
            {
                TextMeshProUGUI buttonText = nextQuestionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = (_currentQuestionIndex >= _allQuestions.Count - 1) ? "Lihat Hasil" : "Lanjut";
            }

            _audioManager?.OnQuestionCorrect();
            if (!isCorrect) _audioManager?.OnQuestionIncorrect();
        }

        private void GoToNextQuestion()
        {
            Debug.Log($"=== GOING TO NEXT QUESTION ===");

            _currentQuestionIndex++;
            if (_currentQuestionIndex < _allQuestions.Count)
            {
                ShowPanel(minigamePanel ?? gamePanel);
                DisplayCurrentQuestion();
            }
            else
            {
                ShowResultPanel();
            }
        }

        private void ShowResultPanel()
        {
            ShowPanel(resultPanel);
            if (resultBG != null) resultBG.SetActive(true);

            int correctCount = 0;
            foreach (bool result in _questionResults)
            {
                if (result) correctCount++;
            }

            bool allCorrect = correctCount == _allQuestions.Count;
            string resultMessage = allCorrect ?
                successMessage :
                string.Format(failureMessage, correctCount, _allQuestions.Count);

            if (resultText != null)
            {
                StartTypewriterEffect(resultText, resultMessage);
                resultText.color = allCorrect ? correctColor : incorrectColor;
            }

            // Legacy compatibility - update scoreText if available
            if (scoreText != null)
                scoreText.text = $"Score: {correctCount}/{_allQuestions.Count}";

            _audioManager?.OnGameEnd(allCorrect);
        }

        private void ShowCompletionScreen()
        {
            ShowPanel(completionScreen);
            if (completionBG != null) completionBG.SetActive(true);

            int correctCount = 0;
            foreach (bool result in _questionResults)
            {
                if (result) correctCount++;
            }

            bool allCorrect = correctCount == _allQuestions.Count;

            if (allCorrect)
            {
                ShowSuccessCompletion();
            }
            else
            {
                ShowFailedCompletion();
            }
        }

        private void ShowSuccessCompletion()
        {
            if (successElements != null) successElements.SetActive(true);
            if (failedElements != null) failedElements.SetActive(false);
            StartCoroutine(ShowSuccessElementsSequentially());
        }

        private void ShowFailedCompletion()
        {
            if (successElements != null) successElements.SetActive(false);
            if (failedElements != null) failedElements.SetActive(true);

            if (failedText != null)
            {
                StartTypewriterEffect(failedText, failedMessage);
            }
        }

        private IEnumerator ShowSuccessElementsSequentially()
        {
            if (successHeaderText != null)
            {
                yield return StartCoroutine(TypewriterCoroutine(successHeaderText, successHeader));
                yield return new WaitForSeconds(0.5f);
            }

            if (trackingText != null)
            {
                yield return StartCoroutine(TypewriterCoroutine(trackingText, trackingMessage));
                yield return new WaitForSeconds(0.5f);
            }

            if (ipAddressText != null)
            {
                yield return StartCoroutine(TypewriterCoroutine(ipAddressText, ipAddress));
                yield return new WaitForSeconds(0.5f);
            }

            if (locationText != null)
            {
                yield return StartCoroutine(TypewriterCoroutine(locationText, location));
            }
        }

        private void SetupDialog()
        {
            if (dialogText != null)
            {
                StartTypewriterEffect(dialogText, openingDialogText);
            }
        }

        private void InitializeBackgrounds()
        {
            if (feedbackBG != null) feedbackBG.SetActive(false);
            if (resultBG != null) resultBG.SetActive(false);
            if (completionBG != null) completionBG.SetActive(false);

            if (glitchImage != null)
            {
                _originalGlitchPosition = glitchImage.transform.localPosition;
            }
        }

        private void SetupInstructionPanel()
        {
            if (instructionText != null && !string.IsNullOrEmpty(instructionText.text))
            {
                string currentInstructionContent = instructionText.text;
                StartTypewriterEffect(instructionText, currentInstructionContent);
            }

            if (glitchImage != null)
            {
                StartGlitchAnimation();
            }
        }

        private void StartGlitchAnimation()
        {
            if (_currentGlitchCoroutine != null)
            {
                StopCoroutine(_currentGlitchCoroutine);
            }
            _currentGlitchCoroutine = StartCoroutine(GlitchCoroutine());
        }

        private IEnumerator GlitchCoroutine()
        {
            while (glitchImage != null && glitchImage.gameObject.activeInHierarchy)
            {
                Vector3 glitchOffset = new Vector3(
                    Random.Range(-glitchIntensity, glitchIntensity),
                    Random.Range(-glitchIntensity, glitchIntensity),
                    0
                );

                glitchImage.transform.localPosition = _originalGlitchPosition + glitchOffset;
                yield return new WaitForSeconds(glitchInterval);
                glitchImage.transform.localPosition = _originalGlitchPosition;
                yield return new WaitForSeconds(glitchInterval * 0.1f);
            }
        }

        private void StartTypewriterEffect(TextMeshProUGUI textComponent, string fullText)
        {
            if (textComponent == null || string.IsNullOrEmpty(fullText)) return;

            if (_currentTypewriterCoroutine != null)
            {
                StopCoroutine(_currentTypewriterCoroutine);
            }
            _currentTypewriterCoroutine = StartCoroutine(TypewriterCoroutine(textComponent, fullText));
        }

        private IEnumerator TypewriterCoroutine(TextMeshProUGUI textComponent, string fullText)
        {
            if (textComponent == null) yield break;

            textComponent.text = "";

            for (int i = 0; i <= fullText.Length; i++)
            {
                if (textComponent == null) yield break;
                textComponent.text = fullText.Substring(0, i);
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        public void RestartGame()
        {
            Debug.Log("Restarting game...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void QuitGame()
        {
            Debug.Log("Quitting game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ENHANCED DisplayDragAndDropCoroutine with unified data flow
        private IEnumerator DisplayDragAndDropCoroutine(MinigameQuestionData question)
        {
            Debug.Log("=== DISPLAYING DRAG AND DROP (UNIFIED) ===");

            // Validate all required components first
            if (!ValidateDragDropComponents())
            {
                Debug.LogError("Drag drop components validation failed!");
                yield break;
            }

            // COMPLETE cleanup of all data structures
            ForceCleanupDragDropElements();
            yield return new WaitForEndOfFrame();

            // Reset ALL tracking variables
            ClearAllDragDropLists();

            // Activate containers
            dragItemContainer.gameObject.SetActive(true);
            dropZoneContainer.gameObject.SetActive(true);

            // Get question data
            var categories = question.categories;
            var scenarios = question.scenarios;

            Debug.Log($"Question has {scenarios?.Count ?? 0} scenarios and {categories?.Count ?? 0} categories");

            if (scenarios == null || scenarios.Count == 0)
            {
                Debug.LogError("Question has no scenarios for drag and drop!");
                yield break;
            }

            if (categories == null || categories.Count == 0)
            {
                Debug.LogError("Question has no categories for drag and drop!");
                yield break;
            }

            // BUILD correct mappings dictionary from question data
            _correctMappingsDict.Clear();
            foreach (var mapping in question.correctMappings)
            {
                _correctMappingsDict[mapping.scenarioId] = mapping.categoryId;
                Debug.Log($"Correct mapping: {mapping.scenarioId} -> {mapping.categoryId}");
            }

            // Set expected pairing count
            _expectedPairingCount = scenarios.Count;
            Debug.Log($"Expected pairing count set to: {_expectedPairingCount}");

            // Create drop zones first
            Debug.Log("Creating drop zones...");
            foreach (var category in categories)
            {
                if (dropZonePrefab == null)
                {
                    Debug.LogError("DropZone prefab is null!");
                    yield break;
                }

                var zone = Instantiate(dropZonePrefab, dropZoneContainer);
                zone.SetupVisuals(category, this);
                zone.gameObject.SetActive(true);
                
                // UNIFIED list management
                _spawnedDropZones.Add(zone);

                Debug.Log($"Drop zone created: {category.id}");
                yield return null;
            }

            // Create drag items with ENHANCED setup and correct mapping
            Debug.Log("Creating drag items...");
            var shuffledScenarios = question.GetShuffledScenarios();

            for (int i = 0; i < shuffledScenarios.Count; i++)
            {
                var scenario = shuffledScenarios[i];
                
                if (dragItemPrefab == null)
                {
                    Debug.LogError("DragItem prefab is null!");
                    yield break;
                }

                // Create the drag item GameObject
                var dragItemGO = Instantiate(dragItemPrefab.gameObject, dragItemContainer);
                
                // Ensure proper setup
                dragItemGO.SetActive(true);
                
                // Configure RectTransform
                var rt = dragItemGO.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition = Vector2.zero;
                    rt.localRotation = Quaternion.identity;
                    rt.sizeDelta = new Vector2(300, 60);
                }

                // Get and initialize DragItem component
                var dragItem = dragItemGO.GetComponent<DragItem>();
                if (dragItem != null)
                {
                    dragItem.Initialize(scenario, this);
                    
                    // UNIFIED list management
                    _spawnedDragItems.Add(dragItem);
                    
                    Debug.Log($"Drag item created: {scenario.description} (ID: {scenario.id})");
                }
                else
                {
                    Debug.LogError($"DragItem component not found on {dragItemGO.name}!");
                }

                yield return null;
            }

            // Force layout rebuild
            yield return StartCoroutine(ForceLayoutRebuild());

            // Verify creation success
            Debug.Log($"Unified Drag and Drop setup complete - Items: {_spawnedDragItems.Count}, Zones: {_spawnedDropZones.Count}");
            Debug.Log($"Correct mappings loaded: {_correctMappingsDict.Count}");

            // Enable submit immediately if no items to pair (edge case)
            if (_expectedPairingCount == 0)
            {
                Debug.LogWarning("No items expected to be paired - enabling submit button");
                EnableSubmitButton("No items to pair");
            }
        }

        // UNIFIED OnItemPaired with single validation path
        public void OnItemPaired(string scenarioId, string categoryId)
        {
            Debug.Log($"UNIFIED OnItemPaired - Scenario: '{scenarioId}', Category: '{categoryId}'");

            if (string.IsNullOrEmpty(scenarioId) || string.IsNullOrEmpty(categoryId))
            {
                Debug.LogError($"Invalid pairing data: scenarioId='{scenarioId}', categoryId='{categoryId}'");
                return;
            }

            // Update UNIFIED pairing data
            _dragDropAnswers[scenarioId] = categoryId;
            _pairedScenarioIds.Add(scenarioId);
            
            // Play audio feedback if available
            if (_audioManager != null)
            {
                _audioManager.PlayDragDropSFX();
            }

            Debug.Log($"Item paired successfully. Current pairs: {_dragDropAnswers.Count}/{_expectedPairingCount}");
            
            // Log all current pairings for debugging
            foreach (var kvp in _dragDropAnswers)
            {
                Debug.Log($"Pairing - {kvp.Key} -> {kvp.Value}");
            }

            // UNIFIED completion check - single path
            if (IsAllItemsPaired())
            {
                Debug.Log("All items paired - enabling submit button!");
                EnableSubmitButton("All drag-drop items paired (unified validation)");
            }
            else
            {
                Debug.Log($"Still need more pairings... ({_dragDropAnswers.Count}/{_expectedPairingCount})");
            }
        }

        // UNIFIED completion check - single source of truth
        private bool IsAllItemsPaired()
        {
            // Primary check: count-based
            bool countComplete = _dragDropAnswers.Count >= _expectedPairingCount && _expectedPairingCount > 0;
            
            // Secondary check: physical verification
            bool physicalComplete = CheckPhysicalCompletion();
            
            // Tertiary check: expected scenarios
            bool scenarioComplete = CheckExpectedScenariosCompletion();

            Debug.Log($"Unified completion check:");
            Debug.Log($"  Count complete: {countComplete} ({_dragDropAnswers.Count}/{_expectedPairingCount})");
            Debug.Log($"  Physical complete: {physicalComplete}");
            Debug.Log($"  Scenario complete: {scenarioComplete}");

            // Return true if ANY method confirms completion (failsafe)
            return countComplete || physicalComplete || scenarioComplete;
        }

        // Physical completion check - verify all items are in drop zones
        private bool CheckPhysicalCompletion()
        {
            if (_spawnedDragItems.Count == 0) return false;

            int itemsInDropZones = 0;
            foreach (var dragItem in _spawnedDragItems)
            {
                if (dragItem == null) continue;
                
                // Check if this drag item is child of a drop zone
                Transform parent = dragItem.transform.parent;
                if (parent != null && parent.GetComponent<DropZone>() != null)
                {
                    itemsInDropZones++;
                }
            }

            Debug.Log($"Physical check - {itemsInDropZones}/{_spawnedDragItems.Count} items in drop zones");
            return itemsInDropZones >= _expectedPairingCount;
        }

        // Expected scenarios completion check
        private bool CheckExpectedScenariosCompletion()
        {
            if (_correctMappingsDict.Count == 0) return false;

            int pairedExpectedScenarios = 0;
            foreach (string expectedScenarioId in _correctMappingsDict.Keys)
            {
                if (_dragDropAnswers.ContainsKey(expectedScenarioId))
                {
                    pairedExpectedScenarios++;
                }
            }

            Debug.Log($"Expected scenario check - {pairedExpectedScenarios}/{_correctMappingsDict.Count} expected scenarios paired");
            return pairedExpectedScenarios >= _correctMappingsDict.Count;
        }

        // Enhanced EnableSubmitButton with forced activation
        private void EnableSubmitButton(string reason)
        {
            if (submitButton == null)
            {
                Debug.LogError("Cannot enable submit button - reference is null!");
                return;
            }

            Debug.Log($"Enabling submit button - Reason: {reason}");

            // FORCE activation sequence
            submitButton.gameObject.SetActive(true);
            submitButton.interactable = true;

            // Ensure button is visible and clickable
            Image buttonImage = submitButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
                buttonImage.enabled = true;
                
                // Force visible alpha
                var color = buttonImage.color;
                if (color.a < 0.8f)
                {
                    color.a = 1f;
                    buttonImage.color = color;
                }
            }

            // Fix all parent CanvasGroups
            CanvasGroup[] canvasGroups = submitButton.GetComponentsInParent<CanvasGroup>();
            foreach (CanvasGroup cg in canvasGroups)
            {
                if (!cg.interactable || !cg.blocksRaycasts || cg.alpha < 0.8f)
                {
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    cg.alpha = 1f;
                }
            }

            // Re-setup listener with additional safety
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitButtonClicked);

            // Move to front and force layout
            submitButton.transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();

            Debug.Log("Submit button enabled and configured successfully");
        }

        private bool ValidateDragDropComponents()
        {
            bool isValid = true;

            if (dragItemContainer == null)
            {
                Debug.LogError("dragItemContainer is null!");
                isValid = false;
            }

            if (dropZoneContainer == null)
            {
                Debug.LogError("dropZoneContainer is null!");
                isValid = false;
            }

            if (dragItemPrefab == null)
            {
                Debug.LogError("dragItemPrefab is null!");
                isValid = false;
            }

            if (dropZonePrefab == null)
            {
                Debug.LogError("dropZonePrefab is null!");
                isValid = false;
            }

            return isValid;
        }

        private void ForceCleanupDragDropElements()
        {
            Debug.Log("Force cleaning drag drop elements...");

            // Destroy objects in UNIFIED lists
            for (int i = _spawnedDragItems.Count - 1; i >= 0; i--)
            {
                if (_spawnedDragItems[i] != null)
                {
                    DestroyImmediate(_spawnedDragItems[i].gameObject);
                }
            }

            for (int i = _spawnedDropZones.Count - 1; i >= 0; i--)
            {
                if (_spawnedDropZones[i] != null)
                {
                    DestroyImmediate(_spawnedDropZones[i].gameObject);
                }
            }

            // Force clear container children
            if (dragItemContainer != null)
            {
                while (dragItemContainer.childCount > 0)
                {
                    DestroyImmediate(dragItemContainer.GetChild(0).gameObject);
                }
                dragItemContainer.gameObject.SetActive(false);
            }

            if (dropZoneContainer != null)
            {
                while (dropZoneContainer.childCount > 0)
                {
                    DestroyImmediate(dropZoneContainer.GetChild(0).gameObject);
                }
                dropZoneContainer.gameObject.SetActive(false);
            }

            // Clear all data structures
            ClearAllDragDropLists();
        }

        private IEnumerator ForceLayoutRebuild()
        {
            yield return new WaitForEndOfFrame();

            if (dragItemContainer != null)
            {
                var layoutGroup = dragItemContainer.GetComponent<LayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.enabled = false;
                    yield return null;
                    layoutGroup.enabled = true;
                }

                RectTransform dragContainerRect = dragItemContainer.GetComponent<RectTransform>();
                if (dragContainerRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(dragContainerRect);
                }
            }

            if (dropZoneContainer != null)
            {
                var layoutGroup = dropZoneContainer.GetComponent<LayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.enabled = false;
                    yield return null;
                    layoutGroup.enabled = true;
                }

                RectTransform dropContainerRect = dropZoneContainer.GetComponent<RectTransform>();
                if (dropContainerRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(dropContainerRect);
                }
            }

            Canvas.ForceUpdateCanvases();
        }

        private void ForceInitializePanels()
        {
            if (dialogPanel != null) dialogPanel.SetActive(false);
            if (instructionPanel != null) instructionPanel.SetActive(false);
            if (minigamePanel != null) minigamePanel.SetActive(false);
            if (gamePanel != null) gamePanel.SetActive(false); // Legacy compatibility
            if (feedbackPanel != null) feedbackPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
            if (completionScreen != null) completionScreen.SetActive(false);

            if (feedbackBG != null) feedbackBG.SetActive(false);
            if (resultBG != null) resultBG.SetActive(false);
            if (completionBG != null) completionBG.SetActive(false);

            if (successElements != null) successElements.SetActive(false);
            if (failedElements != null) failedElements.SetActive(false);
        }

        private void SetupButtonListeners()
        {
            Debug.Log("Setting up button listeners...");
            ClearAllButtonListeners();

            if (dialogNextButton != null)
                dialogNextButton.onClick.AddListener(() => ShowPanel(instructionPanel));

            if (confirmationButton != null)
            {
                confirmationButton.onClick.AddListener(StartMinigame);
            }
            else if (instructionPanel != null)
            {
                Button instructionButton = instructionPanel.GetComponentInChildren<Button>();
                if (instructionButton != null)
                    instructionButton.onClick.AddListener(StartMinigame);
            }

            if (nextQuestionButton != null)
                nextQuestionButton.onClick.AddListener(GoToNextQuestion);

            if (resultNextButton != null)
                resultNextButton.onClick.AddListener(() => ShowPanel(completionScreen));

            if (closeButton != null)
                closeButton.onClick.AddListener(QuitGame);

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            // Legacy compatibility - redirect to unified logic
            if (retryButton != null)
                retryButton.onClick.AddListener(RestartGame);

            if (exitButton != null)
                exitButton.onClick.AddListener(ExitToMenu);
        }

        private void ClearAllButtonListeners()
        {
            if (dialogNextButton != null) dialogNextButton.onClick.RemoveAllListeners();
            if (confirmationButton != null) confirmationButton.onClick.RemoveAllListeners();
            if (nextQuestionButton != null) nextQuestionButton.onClick.RemoveAllListeners();
            if (resultNextButton != null) resultNextButton.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
            if (restartButton != null) restartButton.onClick.RemoveAllListeners();
            if (retryButton != null) retryButton.onClick.RemoveAllListeners();
            if (exitButton != null) exitButton.onClick.RemoveAllListeners();

            foreach (Button btn in _spawnedOptionButtons)
            {
                if (btn != null) btn.onClick.RemoveAllListeners();
            }
        }

        private void DisplayCurrentQuestion()
        {
            if (_currentQuestionIndex >= _allQuestions.Count) return;

            Debug.Log($"=== DISPLAYING QUESTION {_currentQuestionIndex} ===");
            StartCoroutine(DisplayQuestionAfterCleanup());
        }

        private IEnumerator DisplayQuestionAfterCleanup()
        {
            ForceCleanupPreviousQuestion();
            yield return null;
            yield return null;

            MinigameQuestionData question = _allQuestions[_currentQuestionIndex];

            if (questionText != null)
                questionText.text = question.questionText;

            ResetSubmitButton();

            if (question.type == QuestionType.MultipleChoice)
            {
                yield return StartCoroutine(DisplayMultipleChoiceCoroutine(question));
            }
            else if (question.type == QuestionType.DragAndDrop)
            {
                yield return StartCoroutine(DisplayDragAndDropCoroutine(question));
            }
        }

        private void ResetSubmitButton()
        {
            if (submitButton == null) return;

            Debug.Log("Resetting submit button state...");
            submitButton.gameObject.SetActive(false);
            submitButton.interactable = false;

            Image buttonImage = submitButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
            }
        }

        // === Helpers untuk DragItem & DropZone (Required by DragItem.cs and DropZone.cs) ===
        public RectTransform GetDragContainerRect()
        {
            if (dragContainerRect != null) return dragContainerRect;
            return dragItemContainer != null ? dragItemContainer.GetComponent<RectTransform>() : null;
        }

        public Transform GetDragItemContainer()
        {
            return dragItemContainer;
        }

        public float GetCanvasScaleFactor()
        {
            return canvasScaler != null ? canvasScaler.scaleFactor : 1f;
        }

        // Helper method for DragItem to get correct category
        public string GetCorrectCategoryForScenario(string scenarioId)
        {
            if (_correctMappingsDict.ContainsKey(scenarioId))
            {
                return _correctMappingsDict[scenarioId];
            }
            return "";
        }

        // DEBUGGING AND VALIDATION METHODS
        [System.Serializable]
        public class DebugInfo
        {
            public bool submitButtonExists;
            public bool submitButtonActive;
            public bool submitButtonInteractable;
            public bool hasImageComponent;
            public bool raycastTarget;
            public int listenerCount;
            public string buttonName;
            public int dragItemCount;
            public int dropZoneCount;
            public int dragItemContainerChildren;
            public int dropZoneContainerChildren;
            public int expectedPairings;
            public int currentPairings;
            public int pairedScenarioIds;
            public int correctMappings;
        }

        [Header("Debug Info")]
        [SerializeField] private DebugInfo debugInfo = new DebugInfo();

        private void UpdateDebugInfo()
        {
            if (submitButton != null)
            {
                debugInfo.submitButtonExists = true;
                debugInfo.submitButtonActive = submitButton.gameObject.activeInHierarchy;
                debugInfo.submitButtonInteractable = submitButton.interactable;
                debugInfo.buttonName = submitButton.gameObject.name;

                Image img = submitButton.GetComponent<Image>();
                debugInfo.hasImageComponent = img != null;
                debugInfo.raycastTarget = img != null ? img.raycastTarget : false;
                debugInfo.listenerCount = submitButton.onClick.GetPersistentEventCount();
            }
            else
            {
                debugInfo.submitButtonExists = false;
                debugInfo.submitButtonActive = false;
                debugInfo.submitButtonInteractable = false;
                debugInfo.hasImageComponent = false;
                debugInfo.raycastTarget = false;
                debugInfo.listenerCount = 0;
                debugInfo.buttonName = "NULL";
            }

            debugInfo.dragItemCount = _spawnedDragItems.Count;
            debugInfo.dropZoneCount = _spawnedDropZones.Count;
            debugInfo.dragItemContainerChildren = dragItemContainer != null ? dragItemContainer.childCount : 0;
            debugInfo.dropZoneContainerChildren = dropZoneContainer != null ? dropZoneContainer.childCount : 0;
            debugInfo.expectedPairings = _expectedPairingCount;
            debugInfo.currentPairings = _dragDropAnswers.Count;
            debugInfo.pairedScenarioIds = _pairedScenarioIds.Count;
            debugInfo.correctMappings = _correctMappingsDict.Count;
        }

        // Context menu methods for debugging
        [ContextMenu("Force Enable Submit Button")]
        private void ForceEnableSubmitButtonManually()
        {
            Debug.Log("Manually forcing submit button enable...");
            EnableSubmitButton("Manual force enable for testing");
        }

        [ContextMenu("Check Unified Drag Drop State")]
        private void CheckUnifiedDragDropState()
        {
            Debug.Log("=== UNIFIED DRAG DROP STATE CHECK ===");
            Debug.Log($"Expected pairings: {_expectedPairingCount}");
            Debug.Log($"Current pairings: {_dragDropAnswers.Count}");
            Debug.Log($"Paired scenario IDs: {_pairedScenarioIds.Count}");
            Debug.Log($"Spawned drag items: {_spawnedDragItems.Count}");
            Debug.Log($"Spawned drop zones: {_spawnedDropZones.Count}");
            Debug.Log($"Correct mappings: {_correctMappingsDict.Count}");
            
            Debug.Log("Current pairings:");
            foreach (var kvp in _dragDropAnswers)
            {
                Debug.Log($"  User: {kvp.Key} -> {kvp.Value}");
            }
            
            Debug.Log("Expected mappings:");
            foreach (var kvp in _correctMappingsDict)
            {
                Debug.Log($"  Correct: {kvp.Key} -> {kvp.Value}");
            }

            Debug.Log($"All items paired check: {IsAllItemsPaired()}");
            Debug.Log($"Physical completion: {CheckPhysicalCompletion()}");
            Debug.Log($"Expected scenarios completion: {CheckExpectedScenariosCompletion()}");
        }

        [ContextMenu("Test Unified Validation")]
        private void TestUnifiedValidation()
        {
            if (_currentQuestionIndex >= _allQuestions.Count) return;
            
            var question = _allQuestions[_currentQuestionIndex];
            if (question.type != QuestionType.DragAndDrop) return;
            
            bool result = ValidateUnifiedDragAndDrop(question, _dragDropAnswers);
            Debug.Log($"Unified validation result: {result}");
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                UpdateDebugInfo();
            }
#endif
        }
    }
}