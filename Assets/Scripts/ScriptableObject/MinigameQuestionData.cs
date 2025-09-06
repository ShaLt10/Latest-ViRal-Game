using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DigitalForensicsQuiz
{
    public enum QuestionType
    {
        MultipleChoice = 0,
        DragAndDrop = 1
    }

    [CreateAssetMenu(fileName = "NewQuestion", menuName = "Digital Forensics Quiz/Minigame Question")]
    public class MinigameQuestionData : ScriptableObject
    {
        [Header("Question Identity")]
        public string id;
        public QuestionType type;
        
        [TextArea(3, 6)]
        public string questionText;
        
        [Header("Explanations")]
        [TextArea(2, 4)]
        [Tooltip("Explanation shown when answer is correct")]
        public string correctExplanation;
        
        [TextArea(2, 4)]
        [Tooltip("Explanation shown when answer is incorrect")]
        public string incorrectExplanation;

        [Header("Multiple Choice (if type = MultipleChoice)")]
        public List<string> options = new List<string>();
        public int correctAnswerIndex;

        [Header("Drag and Drop (if type = DragAndDrop)")]
        public List<DragScenario> scenarios = new List<DragScenario>();
        public List<DropCategory> categories = new List<DropCategory>();
        public List<DragDropMapping> correctMappings = new List<DragDropMapping>();

        private System.Random _randomGenerator;
        private List<string> _cachedShuffledOptions;
        private int _cachedCorrectIndex = -1;
        private List<DragScenario> _cachedShuffledScenarios;

        private void OnValidate()
        {
            // Ensure ID is not empty
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }

            // Validate multiple choice settings
            if (type == QuestionType.MultipleChoice)
            {
                if (correctAnswerIndex < 0 || correctAnswerIndex >= options.Count)
                {
                    Debug.LogWarning($"Question {name}: Correct answer index is out of range!");
                }
            }

            // Validate drag and drop settings
            if (type == QuestionType.DragAndDrop)
            {
                ValidateDragDropData();
            }
        }

        private void ValidateDragDropData()
        {
            // Check for duplicate scenario IDs
            var scenarioIds = scenarios.Select(s => s.id).ToList();
            if (scenarioIds.Count != scenarioIds.Distinct().Count())
            {
                Debug.LogWarning($"Question {name}: Duplicate scenario IDs found!");
            }

            // Check for duplicate category IDs
            var categoryIds = categories.Select(c => c.id).ToList();
            if (categoryIds.Count != categoryIds.Distinct().Count())
            {
                Debug.LogWarning($"Question {name}: Duplicate category IDs found!");
            }

            // Check mappings
            foreach (var mapping in correctMappings)
            {
                if (!scenarios.Any(s => s.id == mapping.scenarioId))
                {
                    Debug.LogWarning($"Question {name}: Mapping references unknown scenario '{mapping.scenarioId}'!");
                }

                if (!categories.Any(c => c.id == mapping.categoryId))
                {
                    Debug.LogWarning($"Question {name}: Mapping references unknown category '{mapping.categoryId}'!");
                }
            }

            // ENHANCED: Check if all scenarios have mappings
            foreach (var scenario in scenarios)
            {
                if (!correctMappings.Any(m => m.scenarioId == scenario.id))
                {
                    Debug.LogWarning($"Question {name}: Scenario '{scenario.id}' has no correct mapping defined!");
                }
            }
        }

        public List<string> GetShuffledOptions(out int newCorrectIndex)
        {
            newCorrectIndex = correctAnswerIndex;

            if (options == null || options.Count <= 1)
            {
                return new List<string>(options ?? new List<string>());
            }

            // Return cached result if available
            if (_cachedShuffledOptions != null && _cachedCorrectIndex != -1)
            {
                newCorrectIndex = _cachedCorrectIndex;
                return new List<string>(_cachedShuffledOptions);
            }

            InitializeRandom();
            var indexedOptions = options.Select((option, index) => new { option, index }).ToList();
            
            // Shuffle using Fisher-Yates algorithm
            for (int i = indexedOptions.Count - 1; i > 0; i--)
            {
                int j = _randomGenerator.Next(0, i + 1);
                (indexedOptions[i], indexedOptions[j]) = (indexedOptions[j], indexedOptions[i]);
            }

            // Find new correct index and cache results
            _cachedCorrectIndex = indexedOptions.FindIndex(item => item.index == correctAnswerIndex);
            _cachedShuffledOptions = indexedOptions.Select(x => x.option).ToList();
            
            newCorrectIndex = _cachedCorrectIndex;
            return new List<string>(_cachedShuffledOptions);
        }

        public List<DragScenario> GetShuffledScenarios()
        {
            if (scenarios == null || scenarios.Count <= 1)
            {
                return new List<DragScenario>(scenarios ?? new List<DragScenario>());
            }

            // Return cached result if available
            if (_cachedShuffledScenarios != null)
            {
                return new List<DragScenario>(_cachedShuffledScenarios);
            }

            InitializeRandom();
            _cachedShuffledScenarios = new List<DragScenario>(scenarios);
            
            // Shuffle using Fisher-Yates algorithm
            for (int i = _cachedShuffledScenarios.Count - 1; i > 0; i--)
            {
                int j = _randomGenerator.Next(0, i + 1);
                (_cachedShuffledScenarios[i], _cachedShuffledScenarios[j]) = (_cachedShuffledScenarios[j], _cachedShuffledScenarios[i]);
            }
            
            return new List<DragScenario>(_cachedShuffledScenarios);
        }

        public List<DropCategory> GetShuffledCategories()
        {
            if (categories == null || categories.Count <= 1)
            {
                return new List<DropCategory>(categories ?? new List<DropCategory>());
            }

            InitializeRandom();
            var shuffledCategories = new List<DropCategory>(categories);
            
            // Shuffle using Fisher-Yates algorithm
            for (int i = shuffledCategories.Count - 1; i > 0; i--)
            {
                int j = _randomGenerator.Next(0, i + 1);
                (shuffledCategories[i], shuffledCategories[j]) = (shuffledCategories[j], shuffledCategories[i]);
            }
            
            return shuffledCategories;
        }

        // ENHANCED: Get correct mappings as dictionary for easy lookup
        public Dictionary<string, string> GetCorrectMappingsDictionary()
        {
            var mappingsDict = new Dictionary<string, string>();
            
            foreach (var mapping in correctMappings)
            {
                if (mapping.IsValid())
                {
                    mappingsDict[mapping.scenarioId] = mapping.categoryId;
                }
                else
                {
                    Debug.LogWarning($"Invalid mapping found in question {name}: {mapping.scenarioId} -> {mapping.categoryId}");
                }
            }
            
            return mappingsDict;
        }

        // ENHANCED: Validate all mappings are complete
        public bool HasCompleteMappings()
        {
            if (scenarios == null || correctMappings == null) return false;
            
            // Check if every scenario has a mapping
            foreach (var scenario in scenarios)
            {
                if (!correctMappings.Any(m => m.scenarioId == scenario.id))
                {
                    Debug.LogWarning($"Scenario '{scenario.id}' missing correct mapping in question {name}");
                    return false;
                }
            }
            
            return true;
        }

        private void InitializeRandom()
        {
            if (_randomGenerator == null)
            {
                int seed = string.IsNullOrEmpty(id) ? Random.Range(0, int.MaxValue) : id.GetHashCode();
                _randomGenerator = new System.Random(seed);
            }
        }

        public bool IsValidQuestion()
        {
            if (string.IsNullOrEmpty(questionText)) return false;
            if (string.IsNullOrEmpty(correctExplanation) || string.IsNullOrEmpty(incorrectExplanation)) return false;
            
            if (type == QuestionType.MultipleChoice)
            {
                return options != null && options.Count >= 2 && 
                       correctAnswerIndex >= 0 && correctAnswerIndex < options.Count;
            }
            else if (type == QuestionType.DragAndDrop)
            {
                return scenarios != null && scenarios.Count > 0 &&
                       categories != null && categories.Count > 0 &&
                       correctMappings != null && correctMappings.Count > 0 &&
                       HasCompleteMappings(); // ENHANCED validation
            }
            
            return false;
        }

        public int GetEstimatedTimeToComplete()
        {
            // Simple estimate based on question type and complexity
            int baseTime = type == QuestionType.MultipleChoice ? 30 : 45;
            int complexityMultiplier = type == QuestionType.MultipleChoice ? options.Count : scenarios.Count;
            
            return baseTime + (complexityMultiplier * 5);
        }

        public void ClearCache()
        {
            _randomGenerator = null;
            _cachedShuffledOptions = null;
            _cachedCorrectIndex = -1;
            _cachedShuffledScenarios = null;
        }

        public void RegenerateShuffles()
        {
            ClearCache();
            // Force regeneration by calling the shuffle methods
            if (type == QuestionType.MultipleChoice)
            {
                GetShuffledOptions(out _);
            }
            else if (type == QuestionType.DragAndDrop)
            {
                GetShuffledScenarios();
            }
        }

        // Helper methods for editor tools
        #if UNITY_EDITOR
        [ContextMenu("Validate Question")]
        public void ValidateQuestionData()
        {
            if (IsValidQuestion())
            {
                Debug.Log($"Question '{name}' is valid!");
            }
            else
            {
                Debug.LogError($"Question '{name}' has validation errors!");
            }
        }

        [ContextMenu("Clear Shuffle Cache")]
        public void ClearShuffleCache()
        {
            ClearCache();
            Debug.Log($"Shuffle cache cleared for question '{name}'");
        }

        [ContextMenu("Test Shuffle")]
        public void TestShuffle()
        {
            ClearCache();
            
            if (type == QuestionType.MultipleChoice)
            {
                var shuffled = GetShuffledOptions(out int newIndex);
                Debug.Log($"Original correct index: {correctAnswerIndex}, New index: {newIndex}");
                Debug.Log($"Shuffled options: {string.Join(", ", shuffled)}");
            }
            else if (type == QuestionType.DragAndDrop)
            {
                var shuffledScenarios = GetShuffledScenarios();
                Debug.Log($"Shuffled scenarios: {string.Join(", ", shuffledScenarios.Select(s => s.description))}");
                
                var mappingsDict = GetCorrectMappingsDictionary();
                Debug.Log($"Correct mappings: {string.Join(", ", mappingsDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
            }
        }

        [ContextMenu("Auto-Generate Missing Mappings")]
        public void AutoGenerateMissingMappings()
        {
            if (type != QuestionType.DragAndDrop || scenarios == null || categories == null) return;
            
            foreach (var scenario in scenarios)
            {
                if (!correctMappings.Any(m => m.scenarioId == scenario.id))
                {
                    // Auto-assign to first category (you should manually fix this)
                    var newMapping = new DragDropMapping
                    {
                        scenarioId = scenario.id,
                        categoryId = categories.Count > 0 ? categories[0].id : "unknown"
                    };
                    correctMappings.Add(newMapping);
                    Debug.Log($"Auto-generated mapping: {scenario.id} -> {newMapping.categoryId} (PLEASE REVIEW!)");
                }
            }
        }
        #endif
    }

    [System.Serializable]
    public class DragScenario
    {
        public string id;
        [TextArea(2, 3)]
        public string description;
        
        // REMOVED: correctCategoryId - now handled by correctMappings in MinigameQuestionData
        // This keeps the data structure clean and centralized
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(description);
        }
    }

    [System.Serializable]
    public class DropCategory
    {
        public string id;
        public string categoryName;
        public Color zoneColor = Color.white;
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(categoryName);
        }
    }

    [System.Serializable]
    public class DragDropMapping
    {
        [Tooltip("ID of the drag scenario")]
        public string scenarioId;
        
        [Tooltip("ID of the correct drop category")]
        public string categoryId;
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(scenarioId) && !string.IsNullOrEmpty(categoryId);
        }
        
        public override string ToString()
        {
            return $"Mapping: {scenarioId} -> {categoryId}";
        }
    }
}