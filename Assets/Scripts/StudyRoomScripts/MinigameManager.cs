using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace DigitalForensicsQuiz
{
    public class MinigameManager : MonoBehaviour
    {
        #region === UI: Panels & Backgrounds ===
        public enum UIPanel { Dialog, Instruction, Minigame, Feedback, Result, Completion }

        [Header("Panel Objects (urut sesuai enum: Dialog, Instruction, Minigame, Feedback, Result, Completion)")]
        [SerializeField] private GameObject[] panelObjects; // assign urut sesuai enum
        private readonly Dictionary<UIPanel, GameObject> _panels = new();

        [Header("Panel Backgrounds")]
        [SerializeField] private GameObject feedbackBG;
        [SerializeField] private GameObject resultBG;
        [SerializeField] private GameObject completionBG;
        #endregion

        #region === Core UI ===
        [Header("Core UI")]
        [SerializeField] private Transform trackingBar;
        [SerializeField] private GameObject trackingCirclePrefab;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button submitButton;
        #endregion

        #region === Multiple Choice ===
        [Header("Multiple Choice")]
        [SerializeField] private Transform multipleChoiceContainer;
        [SerializeField] private Button optionButtonPrefab;
        #endregion

        #region === Drag & Drop ===
        [Header("Drag and Drop")]
        [SerializeField] private Transform dragItemContainer;
        [SerializeField] private Transform dropZoneContainer;
        [SerializeField] private RectTransform dragContainerRect;
        [SerializeField] private DragItem dragItemPrefab;
        [SerializeField] private DropZone dropZonePrefab;
        #endregion

        #region === Feedback & Result ===
        [Header("Feedback UI")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI explanationText;
        [SerializeField] private Button nextQuestionButton;

        [Header("Result UI")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button resultNextButton;

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
        #endregion

        #region === Dialog & Instruction ===
        [Header("Dialog UI")]
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Button dialogNextButton;

        [Header("Instruction UI")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Image glitchImage;
        [SerializeField] private Button confirmationButton;
        #endregion

        #region === Completion Screen (Root) ===
        [Header("Completion Root")]
        [SerializeField] private GameObject completionScreen;
        #endregion

        #region === Text & Colors & Anim ===
        [Header("Text Settings")]
        [SerializeField] private string characterName = "Gavi";
        [TextArea(2, 4)] [SerializeField] private string openingDialogText = "Baiklah tim, saatnya investigasi. Kita akan trace bukti digital dan pahami cara kerja scam ini.";
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

        [Header("Optional HUD")]
        [SerializeField] private TextMeshProUGUI scoreText; // boleh kosong
        #endregion

        #region === State ===
        private List<MinigameQuestionData> _allQuestions;
        private int _currentQuestionIndex = 0;
        private readonly List<bool> _questionResults = new();
        private readonly List<Image> _trackingCircles = new();

        // Multiple Choice
        private int _selectedAnswerIndex = -1;
        private int _shuffledCorrectIndex = -1;
        private readonly List<Button> _spawnedOptionButtons = new();

        // Drag & Drop (unified)
        private readonly Dictionary<string, string> _dragDropAnswers = new();
        private readonly Dictionary<string, string> _correctMappingsDict = new();
        private readonly List<DragItem> _spawnedDragItems = new();
        private readonly List<DropZone> _spawnedDropZones = new();
        private int _expectedPairingCount = 0;
        private readonly HashSet<string> _pairedScenarioIds = new();

        // Anim
        private Coroutine _currentTypewriterCoroutine;
        private Coroutine _currentGlitchCoroutine;
        private Vector3 _originalGlitchPosition;

        private MinigameAudioManager _audioManager;
        private bool _trackingBarInitialized = false;

        // Debug switch
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const bool VERBOSE = true;
#else
        private const bool VERBOSE = false;
#endif
        #endregion

        #region === Unity Lifecycle ===
        private void Awake()
        {
            // Map enum -> object & set inactive
            for (int i = 0; i < panelObjects.Length; i++)
            {
                var go = panelObjects[i];
                if (go != null)
                {
                    _panels[(UIPanel)i] = go;
                    go.SetActive(false);
                }
            }

            SetupButtonListeners();
            InitializeBackgrounds();
        }

        private void Start()
        {
            _audioManager = FindObjectOfType<MinigameAudioManager>();

            _allQuestions = QuestionProvider.GetAllQuestions();
            if (_allQuestions == null || _allQuestions.Count == 0)
            {
                Debug.LogError("No questions loaded! Pastikan QuestionProvider siap.");
                ShowPanel(UIPanel.Dialog);
                StartTypewriterEffect(dialogText, "Data soal tidak ditemukan.");
                return;
            }

            if (submitButton == null)
            {
                Debug.LogError("Submit button reference missing!");
                return;
            }

            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
            submitButton.gameObject.SetActive(false);

            ShowPanel(UIPanel.Dialog);
            SetupDialog();
        }
        #endregion

        #region === Public (Scene Buttons) ===
        public void ExitToMenu() => SceneManager.LoadScene("MainMenu");
        public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        #endregion

        #region === Flow ===
        private void StartMinigame()
        {
            _currentQuestionIndex = 0;
            _questionResults.Clear();
            _trackingBarInitialized = false;
            CreateTrackingBar();

            ShowPanel(UIPanel.Minigame);
            DisplayCurrentQuestion();
        }

        private void DisplayCurrentQuestion()
        {
            if (_currentQuestionIndex >= _allQuestions.Count) return;

            if (VERBOSE) Debug.Log($"=== DISPLAYING QUESTION #{_currentQuestionIndex} ===");
            StartCoroutine(DisplayQuestionAfterCleanup());
        }

        private IEnumerator DisplayQuestionAfterCleanup()
        {
            ForceCleanupPreviousQuestion();
            yield return null; // satu frame untuk bersih

            var question = _allQuestions[_currentQuestionIndex];
            if (questionText != null) questionText.text = question.questionText;

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

        private void GoToNextQuestion()
        {
            _currentQuestionIndex++;
            if (_currentQuestionIndex < _allQuestions.Count)
            {
                ShowPanel(UIPanel.Minigame);
                DisplayCurrentQuestion();
            }
            else
            {
                ShowResultPanel();
            }
        }

        private void ShowResultPanel()
        {
            ShowPanel(UIPanel.Result);
            if (resultBG != null) resultBG.SetActive(true);

            int correctCount = _questionResults.Count(b => b);
            bool allCorrect = correctCount == _allQuestions.Count;
            string msg = allCorrect ? successMessage : string.Format(failureMessage, correctCount, _allQuestions.Count);

            if (resultText != null)
            {
                StartTypewriterEffect(resultText, msg);
                resultText.color = allCorrect ? correctColor : incorrectColor;
            }

            if (scoreText != null) scoreText.text = $"Score: {correctCount}/{_allQuestions.Count}";

            _audioManager?.OnGameEnd(allCorrect);
        }

        private void ShowCompletionScreen()
        {
            // Root completion panel sudah diaktifkan via ShowPanel(UIPanel.Completion)
            if (completionBG != null) completionBG.SetActive(true);

            int correctCount = _questionResults.Count(b => b);
            bool allCorrect = correctCount == _allQuestions.Count;

            if (allCorrect) ShowSuccessCompletion();
            else ShowFailedCompletion();
        }

        private void ShowSuccessCompletion()
        {
            if (successElements) successElements.SetActive(true);
            if (failedElements) failedElements.SetActive(false);
            StartCoroutine(ShowSuccessElementsSequentially());
        }

        private void ShowFailedCompletion()
        {
            if (successElements) successElements.SetActive(false);
            if (failedElements) failedElements.SetActive(true);
            if (failedText) StartTypewriterEffect(failedText, failedMessage);
        }
        #endregion

        #region === Panel Ops ===
        private void ShowPanel(UIPanel target)
        {
            foreach (var kv in _panels) kv.Value.SetActive(false);

            // Hide all BGs
            if (feedbackBG) feedbackBG.SetActive(false);
            if (resultBG) resultBG.SetActive(false);
            if (completionBG) completionBG.SetActive(false);

            if (_panels.TryGetValue(target, out var panel))
            {
                panel.SetActive(true);
                switch (target)
                {
                    case UIPanel.Instruction:
                        SetupInstructionPanel();
                        break;
                    case UIPanel.Completion:
                        ShowCompletionScreen();
                        break;
                }
            }
        }

        private void InitializeBackgrounds()
        {
            if (feedbackBG) feedbackBG.SetActive(false);
            if (resultBG) resultBG.SetActive(false);
            if (completionBG) completionBG.SetActive(false);
            if (glitchImage) _originalGlitchPosition = glitchImage.transform.localPosition;

            // Matikan success/failed root
            if (successElements) successElements.SetActive(false);
            if (failedElements) failedElements.SetActive(false);

            // Matikan completion root (kalau dipakai terpisah)
            if (completionScreen) completionScreen.SetActive(false);
        }
        #endregion

        #region === Button Listeners ===
        private void SetupButtonListeners()
        {
            ClearAllButtonListeners();

            if (dialogNextButton) dialogNextButton.onClick.AddListener(() => ShowPanel(UIPanel.Instruction));

            if (confirmationButton) confirmationButton.onClick.AddListener(StartMinigame);

            if (nextQuestionButton) nextQuestionButton.onClick.AddListener(GoToNextQuestion);
            if (resultNextButton) resultNextButton.onClick.AddListener(() => ShowPanel(UIPanel.Completion));
            if (closeButton) closeButton.onClick.AddListener(QuitGame);
            if (restartButton) restartButton.onClick.AddListener(RestartGame);
        }

        private void ClearAllButtonListeners()
        {
            if (dialogNextButton) dialogNextButton.onClick.RemoveAllListeners();
            if (confirmationButton) confirmationButton.onClick.RemoveAllListeners();
            if (nextQuestionButton) nextQuestionButton.onClick.RemoveAllListeners();
            if (resultNextButton) resultNextButton.onClick.RemoveAllListeners();
            if (closeButton) closeButton.onClick.RemoveAllListeners();
            if (restartButton) restartButton.onClick.RemoveAllListeners();
        }
        #endregion

        #region === Dialog & Instruction ===
        private void SetupDialog()
        {
            if (dialogText) StartTypewriterEffect(dialogText, openingDialogText);
        }

        private void SetupInstructionPanel()
        {
            if (instructionText && !string.IsNullOrEmpty(instructionText.text))
            {
                string cur = instructionText.text;
                StartTypewriterEffect(instructionText, cur);
            }

            if (glitchImage) StartGlitchAnimation();
        }
        #endregion

        #region === Submit & Feedback ===
        private void ResetSubmitButton()
        {
            if (!submitButton) return;
            submitButton.gameObject.SetActive(false);
            submitButton.interactable = false;

            var img = submitButton.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
        }

        private void EnableSubmitButton(string reason)
        {
            if (!submitButton) { Debug.LogError("Cannot enable submit: no button ref"); return; }
            if (VERBOSE) Debug.Log($"Enable submit: {reason}");

            submitButton.gameObject.SetActive(true);
            submitButton.interactable = true;

            var img = submitButton.GetComponent<Image>();
            if (img)
            {
                img.raycastTarget = true;
                if (img.color.a < 0.95f)
                {
                    var c = img.color; c.a = 1f; img.color = c;
                }
            }

            foreach (var cg in submitButton.GetComponentsInParent<CanvasGroup>())
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.alpha = Mathf.Max(1f, cg.alpha);
            }

            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
            submitButton.transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
        }

        private void OnSubmitButtonClicked() => SubmitAnswer();

        private void SubmitAnswer()
        {
            if (_currentQuestionIndex >= _allQuestions.Count) return;

            var question = _allQuestions[_currentQuestionIndex];
            bool isCorrect = false;

            if (question.type == QuestionType.MultipleChoice)
            {
                isCorrect = AnswerValidator.ValidateMultipleChoice(question, _selectedAnswerIndex, _shuffledCorrectIndex);
            }
            else if (question.type == QuestionType.DragAndDrop)
            {
                isCorrect = ValidateUnifiedDragAndDrop(question, _dragDropAnswers);
            }

            _questionResults.Add(isCorrect);
            UpdateTrackingCircle(_currentQuestionIndex, isCorrect);

            string explanation = isCorrect ? question.correctExplanation : question.incorrectExplanation;
            ShowFeedbackPanel(isCorrect, explanation);
        }

        private void ShowFeedbackPanel(bool isCorrect, string explanation)
        {
            ShowPanel(UIPanel.Feedback);
            if (feedbackBG) feedbackBG.SetActive(true);

            if (feedbackText)
            {
                feedbackText.text = isCorrect ? correctFeedback : incorrectFeedback;
                feedbackText.color = isCorrect ? correctColor : incorrectColor;
            }

            if (explanationText) explanationText.text = explanation;

            if (nextQuestionButton)
            {
                var txt = nextQuestionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.text = (_currentQuestionIndex >= _allQuestions.Count - 1) ? "Lihat Hasil" : "Lanjut";
            }

            if (isCorrect) _audioManager?.OnQuestionCorrect();
            else _audioManager?.OnQuestionIncorrect();
        }
        #endregion

        #region === Tracking Bar ===
        private void CreateTrackingBar()
        {
            if (_trackingBarInitialized || trackingBar == null || trackingCirclePrefab == null) return;

            foreach (Transform child in trackingBar)
                SafeDestroy(child.gameObject);
            _trackingCircles.Clear();

            for (int i = 0; i < _allQuestions.Count; i++)
            {
                var go = Instantiate(trackingCirclePrefab, trackingBar);
                var img = go.GetComponent<Image>();
                if (img)
                {
                    img.color = trackingDefaultColor;
                    _trackingCircles.Add(img);
                }
            }
            _trackingBarInitialized = true;
            if (VERBOSE) Debug.Log($"Tracking bar created with {_trackingCircles.Count} circles");
        }

        private void UpdateTrackingCircle(int questionIndex, bool isCorrect)
        {
            if (questionIndex < 0 || questionIndex >= _trackingCircles.Count) return;
            _trackingCircles[questionIndex].color = isCorrect ? correctColor : incorrectColor;
        }
        #endregion

        #region === Multiple Choice Render ===
        private IEnumerator DisplayMultipleChoiceCoroutine(MinigameQuestionData question)
        {
            if (multipleChoiceContainer == null || optionButtonPrefab == null)
            {
                Debug.LogError("Multiple choice container/prefab is null!");
                yield break;
            }

            multipleChoiceContainer.gameObject.SetActive(true);

            var shuffledOptions = question.GetShuffledOptions(out _shuffledCorrectIndex);
            _selectedAnswerIndex = -1;

            for (int i = 0; i < shuffledOptions.Count; i++)
            {
                Button btn = Instantiate(optionButtonPrefab, multipleChoiceContainer);
                btn.gameObject.SetActive(true);
                btn.interactable = true;

                var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.text = shuffledOptions[i];

                int idx = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectOption(idx));

                _spawnedOptionButtons.Add(btn);
                yield return null; // give time per spawn
            }

            yield return new WaitForEndOfFrame();
            var containerRect = multipleChoiceContainer.GetComponent<RectTransform>();
            if (containerRect) LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }

        public void SelectOption(int index)
        {
            _selectedAnswerIndex = index;
            _audioManager?.PlayButtonClickSFX();

            for (int i = 0; i < _spawnedOptionButtons.Count; i++)
            {
                var img = _spawnedOptionButtons[i]?.GetComponent<Image>();
                if (img) img.color = (i == index) ? selectedButtonColor : defaultButtonColor;
            }
            EnableSubmitButton("Option selected");
        }
        #endregion

        #region === Drag & Drop Render & Validation ===
        private IEnumerator DisplayDragAndDropCoroutine(MinigameQuestionData question)
        {
            if (!ValidateDragDropComponents())
            {
                Debug.LogError("Drag & Drop components invalid!");
                yield break;
            }

            ForceCleanupDragDropElements();
            yield return new WaitForEndOfFrame();

            _spawnedDragItems.Clear();
            _spawnedDropZones.Clear();
            _dragDropAnswers.Clear();
            _pairedScenarioIds.Clear();
            _correctMappingsDict.Clear();
            _expectedPairingCount = 0;

            dragItemContainer.gameObject.SetActive(true);
            dropZoneContainer.gameObject.SetActive(true);

            var categories = question.categories;
            var scenarios = question.scenarios;

            if (scenarios == null || scenarios.Count == 0) { Debug.LogError("No scenarios in question"); yield break; }
            if (categories == null || categories.Count == 0) { Debug.LogError("No categories in question"); yield break; }

            foreach (var map in question.correctMappings)
                _correctMappingsDict[map.scenarioId] = map.categoryId;

            _expectedPairingCount = scenarios.Count;

            // Spawn Drop Zones
            foreach (var category in categories)
            {
                var zone = Instantiate(dropZonePrefab, dropZoneContainer);
                zone.SetupVisuals(category, this);
                zone.gameObject.SetActive(true);
                _spawnedDropZones.Add(zone);
                yield return null;
            }

            // Spawn Drag Items (shuffled)
            var shuffled = question.GetShuffledScenarios();
            foreach (var sc in shuffled)
            {
                var itemGO = Instantiate(dragItemPrefab.gameObject, dragItemContainer);
                itemGO.SetActive(true);

                var rt = itemGO.GetComponent<RectTransform>();
                if (rt)
                {
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition = Vector2.zero;
                    rt.localRotation = Quaternion.identity;
                    rt.sizeDelta = new Vector2(300, 60);
                }

                var dragItem = itemGO.GetComponent<DragItem>();
                if (dragItem) { dragItem.Initialize(sc, this); _spawnedDragItems.Add(dragItem); }
                else Debug.LogError("DragItem component missing on spawned prefab");

                yield return null;
            }

            // Rebuild layout once
            yield return ForceLayoutRebuildOnce();

            if (_expectedPairingCount == 0)
                EnableSubmitButton("No items to pair");
        }

        public void OnItemPaired(string scenarioId, string categoryId)
        {
            if (string.IsNullOrEmpty(scenarioId) || string.IsNullOrEmpty(categoryId)) return;

            _dragDropAnswers[scenarioId] = categoryId;
            _pairedScenarioIds.Add(scenarioId);
            _audioManager?.PlayDragDropSFX();

            if (IsAllItemsPaired())
                EnableSubmitButton("All drag-drop items paired");
        }

        private bool ValidateUnifiedDragAndDrop(MinigameQuestionData question, Dictionary<string, string> userAnswers)
        {
            if (_correctMappingsDict.Count == 0) return false;

            foreach (var exp in _correctMappingsDict)
            {
                if (!userAnswers.TryGetValue(exp.Key, out var got)) return false;
                if (got != exp.Value) return false;
            }
            return true;
        }

        private bool IsAllItemsPaired()
        {
            bool countComplete = _dragDropAnswers.Count >= _expectedPairingCount && _expectedPairingCount > 0;
            if (countComplete) return true;

            // Physical check
            if (_spawnedDragItems.Count > 0)
            {
                int inZones = 0;
                foreach (var it in _spawnedDragItems)
                {
                    if (!it) continue;
                    var p = it.transform.parent;
                    if (p && p.GetComponent<DropZone>()) inZones++;
                }
                if (inZones >= _expectedPairingCount) return true;
            }

            // Expected scenario IDs check
            int pairedExpected = 0;
            foreach (var id in _correctMappingsDict.Keys)
                if (_dragDropAnswers.ContainsKey(id)) pairedExpected++;
            return pairedExpected >= _correctMappingsDict.Count;
        }

        private bool ValidateDragDropComponents()
        {
            bool ok = true;

            if (dragItemContainer == null)
            {
                Debug.LogError("[MinigameManager] dragItemContainer is null!");
                ok = false;
            }
            if (dropZoneContainer == null)
            {
                Debug.LogError("[MinigameManager] dropZoneContainer is null!");
                ok = false;
            }
            if (dragItemPrefab == null)
            {
                Debug.LogError("[MinigameManager] dragItemPrefab is null!");
                ok = false;
            }
            if (dropZonePrefab == null)
            {
                Debug.LogError("[MinigameManager] dropZonePrefab is null!");
                ok = false;
            }

            return ok;
        }
        #endregion

        #region === Helpers for DragItem / DropZone ===
        public RectTransform GetDragContainerRect() => dragContainerRect ? dragContainerRect : (dragItemContainer ? dragItemContainer.GetComponent<RectTransform>() : null);
        public Transform GetDragItemContainer() => dragItemContainer;
        public float GetCanvasScaleFactor() => canvasScaler ? canvasScaler.scaleFactor : 1f;
        public string GetCorrectCategoryForScenario(string scenarioId) => _correctMappingsDict.TryGetValue(scenarioId, out var cat) ? cat : "";
        #endregion

        #region === Cleanup & Layout ===
        private void ForceCleanupPreviousQuestion()
        {
            // Multiple choice
            for (int i = _spawnedOptionButtons.Count - 1; i >= 0; i--)
                if (_spawnedOptionButtons[i]) SafeDestroy(_spawnedOptionButtons[i].gameObject);
            _spawnedOptionButtons.Clear();

            if (multipleChoiceContainer)
            {
                for (int i = multipleChoiceContainer.childCount - 1; i >= 0; i--)
                    SafeDestroy(multipleChoiceContainer.GetChild(i).gameObject);
                multipleChoiceContainer.gameObject.SetActive(false);
            }

            // Drag & drop
            ForceCleanupDragDropElements();

            _selectedAnswerIndex = -1;
            _shuffledCorrectIndex = -1;
        }

        private void ForceCleanupDragDropElements()
        {
            // spawned lists
            for (int i = _spawnedDragItems.Count - 1; i >= 0; i--)
                if (_spawnedDragItems[i]) SafeDestroy(_spawnedDragItems[i].gameObject);
            for (int i = _spawnedDropZones.Count - 1; i >= 0; i--)
                if (_spawnedDropZones[i]) SafeDestroy(_spawnedDropZones[i].gameObject);

            // containers
            if (dragItemContainer)
            {
                for (int i = dragItemContainer.childCount - 1; i >= 0; i--)
                    SafeDestroy(dragItemContainer.GetChild(i).gameObject);
                dragItemContainer.gameObject.SetActive(false);
            }
            if (dropZoneContainer)
            {
                for (int i = dropZoneContainer.childCount - 1; i >= 0; i--)
                    SafeDestroy(dropZoneContainer.GetChild(i).gameObject);
                dropZoneContainer.gameObject.SetActive(false);
            }

            _spawnedDragItems.Clear();
            _spawnedDropZones.Clear();
            _dragDropAnswers.Clear();
            _pairedScenarioIds.Clear();
            _correctMappingsDict.Clear();
            _expectedPairingCount = 0;
        }

        private IEnumerator ForceLayoutRebuildOnce()
        {
            yield return new WaitForEndOfFrame();

            if (dragItemContainer)
            {
                var lg = dragItemContainer.GetComponent<LayoutGroup>();
                if (lg) { lg.enabled = false; yield return null; lg.enabled = true; }
                var rt = dragItemContainer.GetComponent<RectTransform>();
                if (rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }

            if (dropZoneContainer)
            {
                var lg = dropZoneContainer.GetComponent<LayoutGroup>();
                if (lg) { lg.enabled = false; yield return null; lg.enabled = true; }
                var rt = dropZoneContainer.GetComponent<RectTransform>();
                if (rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }

            Canvas.ForceUpdateCanvases();
        }

        private void SafeDestroy(GameObject go)
        {
            if (!go) return;
#if UNITY_EDITOR
            if (!Application.isPlaying) { DestroyImmediate(go); return; }
#endif
            Destroy(go);
        }
        #endregion

        #region === Completion: Success Sequence ===
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
        #endregion

        #region === Typewriter & Glitch ===
        private void StartTypewriterEffect(TextMeshProUGUI textComponent, string fullText)
        {
            if (!textComponent || string.IsNullOrEmpty(fullText)) return;
            if (_currentTypewriterCoroutine != null) StopCoroutine(_currentTypewriterCoroutine);
            _currentTypewriterCoroutine = StartCoroutine(TypewriterCoroutine(textComponent, fullText));
        }

        private IEnumerator TypewriterCoroutine(TextMeshProUGUI textComponent, string fullText)
        {
            textComponent.text = "";
            for (int i = 0; i <= fullText.Length; i++)
            {
                if (!textComponent) yield break;
                textComponent.text = fullText.Substring(0, i);
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        private void StartGlitchAnimation()
        {
            if (_currentGlitchCoroutine != null) StopCoroutine(_currentGlitchCoroutine);
            _currentGlitchCoroutine = StartCoroutine(GlitchCoroutine());
        }

        private IEnumerator GlitchCoroutine()
        {
            while (glitchImage && glitchImage.gameObject.activeInHierarchy)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-glitchIntensity, glitchIntensity),
                    Random.Range(-glitchIntensity, glitchIntensity),
                    0f
                );
                glitchImage.transform.localPosition = _originalGlitchPosition + offset;
                yield return new WaitForSeconds(glitchInterval);
                glitchImage.transform.localPosition = _originalGlitchPosition;
                yield return new WaitForSeconds(glitchInterval * 0.1f);
            }
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}
