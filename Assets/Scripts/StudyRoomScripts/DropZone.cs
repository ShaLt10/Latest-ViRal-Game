using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DigitalForensicsQuiz;
using DG.Tweening;

public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _labelText;
    private string _categoryId;
    private MinigameManager _gameManager;
    private DragItem _currentItem; // Track current item in this zone

    private readonly Vector3 _insertScale = new Vector3(0.9f, 0.9f, 1f);

    public string CategoryId => _categoryId; // Public accessor for category ID

    public void SetupVisuals(DropCategory category, MinigameManager manager)
    {
        _categoryId = category.id;
        _gameManager = manager;

        Debug.Log($"DropZone setup - Category: {_categoryId}, Name: {category.categoryName}");

        // Auto-find Image component if not assigned
        if (_backgroundImage == null)
        {
            _backgroundImage = GetComponent<Image>();
        }

        if (_backgroundImage != null)
        {
            var col = category.zoneColor;
            if (col.a <= 0.01f) col.a = 1f;
            _backgroundImage.color = col;
            _backgroundImage.raycastTarget = true;
            _backgroundImage.enabled = true;
            if (_backgroundImage.sprite == null)
                _backgroundImage.type = Image.Type.Sliced;
            Debug.Log($"BackgroundImage found and configured for DropZone {_categoryId}");
        }
        else
        {
            Debug.LogWarning($"BackgroundImage is null for DropZone {_categoryId} - no Image component found");
        }

        SetupTextLabel(category);
    }

    private void SetupTextLabel(DropCategory category)
    {
        // Auto-find TextMeshProUGUI if not assigned (check children first, then self)
        if (_labelText == null)
        {
            _labelText = GetComponentInChildren<TextMeshProUGUI>();
            if (_labelText == null)
            {
                _labelText = GetComponent<TextMeshProUGUI>();
            }
        }

        if (_labelText != null)
        {
            _labelText.text = category.categoryName;
            _labelText.color = Color.black;
            _labelText.enableAutoSizing = true;
            _labelText.fontSizeMin = 8f;
            _labelText.fontSizeMax = 24f;
            _labelText.alignment = TextAlignmentOptions.Center;
            Debug.Log($"LabelText found and configured for DropZone {_categoryId}");
        }
        else
        {
            Debug.LogWarning($"LabelText is null for DropZone {_categoryId} - no TextMeshProUGUI found in children or self");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragItem = eventData.pointerDrag?.GetComponent<DragItem>();
        if (dragItem == null)
        {
            Debug.LogWarning("No DragItem component found on dropped object");
            return;
        }

        Debug.Log($"Item dropped on DropZone - Item: {dragItem.ScenarioId}, Zone: {_categoryId}");

        // Handle existing item in zone
        if (_currentItem != null && _currentItem != dragItem)
        {
            Debug.Log($"Zone {_categoryId} already has item {_currentItem.ScenarioId}, returning it to container");
            
            // Return existing item to drag container
            _currentItem.transform.SetParent(_gameManager.GetDragItemContainer(), false);
            _currentItem.ResetPosition();
            
            // Clear the reference
            _currentItem = null;
        }

        // Set new current item
        _currentItem = dragItem;
        
        // Parent the drag item to this drop zone
        dragItem.transform.SetParent(transform, false);
        
        // Animate the insertion
        AnimateInsert(dragItem);

        // Notify game manager about the pairing
        if (_gameManager != null)
        {
            _gameManager.OnItemPaired(dragItem.ScenarioId, _categoryId);
        }
        else
        {
            Debug.LogError("GameManager reference is null in DropZone!");
        }
    }

    private void AnimateInsert(DragItem item)
    {
        RectTransform rt = item.GetComponent<RectTransform>();
        if (rt == null) return;

        Debug.Log($"Animating insert for item {item.ScenarioId} in zone {_categoryId}");

        rt.DOKill(); // Stop any ongoing animations
        
        // Animate to center position
        rt.DOAnchorPos(Vector2.zero, 0.15f)
          .SetEase(Ease.OutQuad);
          
        // Scale animation with bounce effect
        rt.DOScale(_insertScale, 0.15f)
          .SetEase(Ease.OutQuad)
          .OnComplete(() =>
          {
              // Return to normal scale with bounce
              rt.DOScale(Vector3.one, 0.15f)
                .SetEase(Ease.OutBack);
          });
    }

    public string GetLabelText()
    {
        return _labelText != null ? _labelText.text : "";
    }

    // Get current item in this zone
    public DragItem GetCurrentItem()
    {
        return _currentItem;
    }

    // Check if zone has an item
    public bool HasItem()
    {
        return _currentItem != null;
    }

    // Clear current item reference (for cleanup)
    public void ClearCurrentItem()
    {
        if (_currentItem != null)
        {
            Debug.Log($"Clearing current item {_currentItem.ScenarioId} from zone {_categoryId}");
            _currentItem = null;
        }
    }

    // Force remove item from zone
    public void ForceRemoveItem()
    {
        if (_currentItem != null)
        {
            Debug.Log($"Force removing item {_currentItem.ScenarioId} from zone {_categoryId}");
            
            _currentItem.transform.SetParent(_gameManager.GetDragItemContainer(), false);
            _currentItem.ResetPosition();
            _currentItem = null;
        }
    }

    // Validate if correct item is in zone using MinigameManager's data
    public bool HasCorrectItem()
    {
        if (_currentItem == null || _gameManager == null) return false;
        
        // Get correct category for the current item from game manager
        string correctCategoryId = _gameManager.GetCorrectCategoryForScenario(_currentItem.ScenarioId);
        
        // Check if the correct category matches this zone's category
        return !string.IsNullOrEmpty(correctCategoryId) && correctCategoryId == _categoryId;
    }

    // Debugging methods
    public void LogCurrentState()
    {
        Debug.Log($"DropZone {_categoryId} State:");
        Debug.Log($"  Category Name: {GetLabelText()}");
        Debug.Log($"  Has Item: {HasItem()}");
        Debug.Log($"  Current Item: {(_currentItem != null ? _currentItem.ScenarioId : "None")}");
        Debug.Log($"  Children Count: {transform.childCount}");
        Debug.Log($"  Has Correct Item: {HasCorrectItem()}");
        
        // Log all children
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var dragItem = child.GetComponent<DragItem>();
            if (dragItem != null)
            {
                Debug.Log($"    Child {i}: DragItem {dragItem.ScenarioId}");
            }
            else
            {
                Debug.Log($"    Child {i}: {child.name} (not a DragItem)");
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up any ongoing animations
        if (_currentItem != null)
        {
            var rt = _currentItem.GetComponent<RectTransform>();
            rt?.DOKill();
        }
    }
}