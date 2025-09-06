using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalForensicsQuiz;

public class PrefabValidator : MonoBehaviour
{
    [Header("Drag Drop References")]
    [SerializeField] private MinigameManager minigameManager;
    [SerializeField] private Transform dragItemContainer;
    [SerializeField] private Transform dropZoneContainer;
    [SerializeField] private DragItem dragItemPrefab;
    [SerializeField] private DropZone dropZonePrefab;

    private void Start()
    {
        if (minigameManager == null)
            minigameManager = FindObjectOfType<MinigameManager>();
    }

    [ContextMenu("Test Create Drag Item")]
    public void TestCreateDragItem()
    {
        if (dragItemPrefab == null || dragItemContainer == null)
        {
            Debug.LogError("Missing prefab or container references!");
            return;
        }

        var testItem = Instantiate(dragItemPrefab, dragItemContainer);

        var testScenario = new DragScenario
        {
            id = "test_scenario",
            description = "Test Scenario Description"
        };

        testItem.Initialize(testScenario, minigameManager);
    }

    [ContextMenu("Test Create Drop Zone")]
    public void TestCreateDropZone()
    {
        if (dropZonePrefab == null || dropZoneContainer == null)
        {
            Debug.LogError("Missing prefab or container references!");
            return;
        }

        var testZone = Instantiate(dropZonePrefab, dropZoneContainer);

        var testCategory = new DropCategory
        {
            id = "test_category",
            categoryName = "Test Category Label",
            zoneColor = Color.yellow
        };

        testZone.SetupVisuals(testCategory, minigameManager);
    }
}
