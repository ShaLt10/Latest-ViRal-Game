using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DigitalForensicsQuiz;
using DG.Tweening;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TextMeshProUGUI _textLabel;
    [SerializeField] private Image _backgroundImage;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Vector3 _startPosition;
    private Transform _startParent;

    private MinigameManager _gameManager;
    public string ScenarioId { get; private set; }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _rectTransform = GetComponent<RectTransform>();
    }

    // Main Initialize method using DragScenario from MinigameQuestionData
    public void Initialize(DragScenario scenario, MinigameManager manager)
    {
        ScenarioId = scenario.id;
        _gameManager = manager;

        Debug.Log($"DragItem initialized - ID: {ScenarioId}");

        // Auto-find TextMeshProUGUI if not assigned (check children)
        if (_textLabel == null)
        {
            _textLabel = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (_textLabel != null)
        {
            _textLabel.text = scenario.description;
            _textLabel.color = Color.black;
            _textLabel.enableAutoSizing = true;
            _textLabel.fontSizeMin = 8f;
            _textLabel.fontSizeMax = 20f;
            Debug.Log($"TextLabel found and configured for DragItem {ScenarioId}");
        }
        else
        {
            Debug.LogWarning($"TextLabel is null for DragItem {ScenarioId} - no TextMeshProUGUI found in children");
        }

        // Auto-find Image component if not assigned
        if (_backgroundImage == null)
        {
            _backgroundImage = GetComponent<Image>();
        }
        if (_backgroundImage != null)
        {
            _backgroundImage.color = Color.white;
            _backgroundImage.raycastTarget = true;
        }
        else
        {
            Debug.LogWarning($"BackgroundImage is null for DragItem {ScenarioId}");
        }
    }

    // Legacy overload for compatibility with objects that have different structure
    public void Initialize(object legacyScenario, MinigameManager manager)
    {
        // Extract id and description using reflection for legacy compatibility
        var type = legacyScenario.GetType();
        
        // Try to get id value
        string scenarioId = "";
        var idProperty = type.GetProperty("id");
        var idField = type.GetField("id");
        
        if (idProperty != null)
        {
            scenarioId = idProperty.GetValue(legacyScenario)?.ToString() ?? "";
        }
        else if (idField != null)
        {
            scenarioId = idField.GetValue(legacyScenario)?.ToString() ?? "";
        }
        
        // Try to get description value
        string description = "";
        var descProperty = type.GetProperty("description");
        var descField = type.GetField("description");
        
        if (descProperty != null)
        {
            description = descProperty.GetValue(legacyScenario)?.ToString() ?? "";
        }
        else if (descField != null)
        {
            description = descField.GetValue(legacyScenario)?.ToString() ?? "";
        }

        if (!string.IsNullOrEmpty(scenarioId) && !string.IsNullOrEmpty(description))
        {
            ScenarioId = scenarioId;
            _gameManager = manager;

            Debug.Log($"DragItem initialized (legacy) - ID: {ScenarioId}");

            // Auto-find TextMeshProUGUI if not assigned (check children)
            if (_textLabel == null)
            {
                _textLabel = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (_textLabel != null)
            {
                _textLabel.text = description;
                _textLabel.color = Color.black;
                _textLabel.enableAutoSizing = true;
                _textLabel.fontSizeMin = 8f;
                _textLabel.fontSizeMax = 20f;
                Debug.Log($"TextLabel found and configured for DragItem {ScenarioId} (legacy)");
            }

            // Auto-find Image component if not assigned
            if (_backgroundImage == null)
            {
                _backgroundImage = GetComponent<Image>();
            }
            if (_backgroundImage != null)
            {
                _backgroundImage.color = Color.white;
                _backgroundImage.raycastTarget = true;
            }
        }
        else
        {
            Debug.LogError("Legacy scenario object doesn't have required id/description properties!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Begin drag: {ScenarioId}");
        
        _startPosition = _rectTransform.anchoredPosition;
        _startParent = transform.parent;

        // Move to drag container for proper layering
        Transform dragContainer = _gameManager.GetDragContainerRect();
        if (dragContainer != null)
        {
            transform.SetParent(dragContainer, true);
        }
        else
        {
            Debug.LogWarning($"Drag container is null for {ScenarioId}");
        }
        
        _canvasGroup.blocksRaycasts = false;
        
        // Visual feedback - slightly scale up and reduce alpha
        transform.DOScale(1.1f, 0.1f);
        _canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_gameManager != null)
        {
            _rectTransform.anchoredPosition += eventData.delta / _gameManager.GetCanvasScaleFactor();
        }
        else
        {
            _rectTransform.anchoredPosition += eventData.delta;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"End drag: {ScenarioId}");
        
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        // Reset scale
        transform.DOScale(1f, 0.1f);

        // Check if we're still in the drag container (not dropped on a valid zone)
        Transform dragContainer = _gameManager.GetDragContainerRect();
        if (transform.parent == dragContainer)
        {
            Debug.Log($"Item {ScenarioId} not dropped on valid zone, resetting position");
            ResetPosition();
        }
        else
        {
            Debug.Log($"Item {ScenarioId} successfully dropped on: {transform.parent.name}");
        }
    }

    public void ResetPosition()
    {
        Debug.Log($"Resetting position for {ScenarioId}");
        
        // Return to original parent
        if (_startParent != null)
        {
            transform.SetParent(_startParent, false);
        }
        else
        {
            // Fallback: return to drag item container
            Transform container = _gameManager.GetDragItemContainer();
            if (container != null)
            {
                transform.SetParent(container, false);
            }
        }
        
        // Animate back to position
        _rectTransform.DOKill();
        _rectTransform.DOAnchorPos(Vector2.zero, 0.2f)
                      .SetEase(Ease.OutQuad);
        _rectTransform.DOScale(Vector3.one, 0.2f);
        
        // Ensure proper layering and visual state
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    public string GetCurrentText()
    {
        return _textLabel != null ? _textLabel.text : "";
    }

    // Check if this item is in the correct zone using MinigameManager's data
    public bool IsInCorrectZone()
    {
        DropZone currentZone = GetComponentInParent<DropZone>();
        if (currentZone == null || _gameManager == null) return false;
        
        // Get the correct category for this scenario from the game manager
        string correctCategoryId = _gameManager.GetCorrectCategoryForScenario(ScenarioId);
        
        return !string.IsNullOrEmpty(correctCategoryId) && currentZone.CategoryId == correctCategoryId;
    }

    // Debugging methods
    public void LogCurrentState()
    {
        Debug.Log($"DragItem {ScenarioId} State:");
        Debug.Log($"  Parent: {(transform.parent != null ? transform.parent.name : "NULL")}");
        Debug.Log($"  Position: {_rectTransform.anchoredPosition}");
        Debug.Log($"  Scale: {transform.localScale}");
        Debug.Log($"  Alpha: {_canvasGroup.alpha}");
        Debug.Log($"  BlocksRaycasts: {_canvasGroup.blocksRaycasts}");
        
        if (_gameManager != null)
        {
            string correctCategory = _gameManager.GetCorrectCategoryForScenario(ScenarioId);
            Debug.Log($"  CorrectCategory: {correctCategory}");
            Debug.Log($"  IsInCorrectZone: {IsInCorrectZone()}");
        }
    }

    private void OnDestroy()
    {
        // Clean up any ongoing tweens
        _rectTransform?.DOKill();
    }
}