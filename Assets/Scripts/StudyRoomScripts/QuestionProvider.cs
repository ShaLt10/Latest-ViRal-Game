using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DigitalForensicsQuiz
{
    public static class QuestionProvider
    {
        private static List<MinigameQuestionData> _cachedQuestions;
        private static bool _isDataLoaded = false;
        private const string QUESTIONS_PATH = "Minigame3QuestionData"; // Put questions in Resources/Questions/

        public static List<MinigameQuestionData> GetAllQuestions()
        {
            if (_isDataLoaded && _cachedQuestions != null)
            {
                return new List<MinigameQuestionData>(_cachedQuestions);
            }

            LoadQuestionsFromResources();
            return new List<MinigameQuestionData>(_cachedQuestions ?? new List<MinigameQuestionData>());
        }

        public static List<MinigameQuestionData> GetQuestionsByType(QuestionType type)
        {
            var allQuestions = GetAllQuestions();
            return allQuestions.Where(q => q.type == type).ToList();
        }

        public static MinigameQuestionData GetQuestionById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            
            var allQuestions = GetAllQuestions();
            return allQuestions.FirstOrDefault(q => q.id == id);
        }

        public static int GetTotalQuestionsCount()
        {
            return GetAllQuestions().Count;
        }

        public static int GetQuestionsCountByType(QuestionType type)
        {
            return GetQuestionsByType(type).Count;
        }

        private static void LoadQuestionsFromResources()
        {
            try
            {
                var loadedQuestions = Resources.LoadAll<MinigameQuestionData>(QUESTIONS_PATH);

                if (loadedQuestions.Length == 0)
                {
                    Debug.LogWarning($"No questions found in 'Resources/{QUESTIONS_PATH}'. Create question assets there.");
                    _cachedQuestions = new List<MinigameQuestionData>();
                    _isDataLoaded = true;
                    return;
                }

                // Sort by type (MultipleChoice first, then DragAndDrop)
                _cachedQuestions = loadedQuestions
                    .Where(q => q != null) // Filter out null questions
                    .OrderBy(q => q.type)
                    .ToList();
                    
                _isDataLoaded = true;
                
                Debug.Log($"Loaded {_cachedQuestions.Count} questions successfully!");
                LogQuestionsBreakdown();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load questions: {e.Message}");
                _cachedQuestions = new List<MinigameQuestionData>();
                _isDataLoaded = true;
            }
        }

        private static void LogQuestionsBreakdown()
        {
            if (_cachedQuestions == null || _cachedQuestions.Count == 0) return;

            int multipleChoiceCount = _cachedQuestions.Count(q => q.type == QuestionType.MultipleChoice);
            int dragDropCount = _cachedQuestions.Count(q => q.type == QuestionType.DragAndDrop);

            Debug.Log($"Questions loaded - Multiple Choice: {multipleChoiceCount}, Drag & Drop: {dragDropCount}");
        }

        public static bool ValidateAllQuestions()
        {
            var questions = GetAllQuestions();
            bool allValid = true;

            foreach (var question in questions)
            {
                if (!ValidateQuestion(question))
                {
                    allValid = false;
                }
            }

            return allValid;
        }

        private static bool ValidateQuestion(MinigameQuestionData question)
        {
            if (question == null)
            {
                Debug.LogError("Found null question in resources!");
                return false;
            }

            if (string.IsNullOrEmpty(question.questionText))
            {
                Debug.LogError($"Question {question.name} has empty question text!");
                return false;
            }

            if (question.type == QuestionType.MultipleChoice)
            {
                if (question.options == null || question.options.Count < 2)
                {
                    Debug.LogError($"Question {question.name} has insufficient options for multiple choice!");
                    return false;
                }

                if (question.correctAnswerIndex < 0 || question.correctAnswerIndex >= question.options.Count)
                {
                    Debug.LogError($"Question {question.name} has invalid correct answer index!");
                    return false;
                }
            }
            else if (question.type == QuestionType.DragAndDrop)
            {
                if (question.scenarios == null || question.scenarios.Count == 0)
                {
                    Debug.LogError($"Question {question.name} has no scenarios for drag and drop!");
                    return false;
                }

                if (question.categories == null || question.categories.Count == 0)
                {
                    Debug.LogError($"Question {question.name} has no categories for drag and drop!");
                    return false;
                }

                if (question.correctMappings == null || question.correctMappings.Count == 0)
                {
                    Debug.LogError($"Question {question.name} has no correct mappings for drag and drop!");
                    return false;
                }

                // Validate that all scenarios have mappings
                foreach (var scenario in question.scenarios)
                {
                    if (!question.correctMappings.Any(m => m.scenarioId == scenario.id))
                    {
                        Debug.LogError($"Question {question.name} - scenario '{scenario.id}' has no mapping!");
                        return false;
                    }
                }

                // Validate that all mappings reference valid scenarios and categories
                foreach (var mapping in question.correctMappings)
                {
                    if (!question.scenarios.Any(s => s.id == mapping.scenarioId))
                    {
                        Debug.LogError($"Question {question.name} - mapping references invalid scenario '{mapping.scenarioId}'!");
                        return false;
                    }

                    if (!question.categories.Any(c => c.id == mapping.categoryId))
                    {
                        Debug.LogError($"Question {question.name} - mapping references invalid category '{mapping.categoryId}'!");
                        return false;
                    }
                }
            }

            return true;
        }

        public static void ClearCache()
        {
            if (_cachedQuestions != null)
            {
                foreach (var question in _cachedQuestions)
                {
                    question?.ClearCache();
                }
            }
            
            _cachedQuestions = null;
            _isDataLoaded = false;
        }

        public static void ReloadQuestions()
        {
            ClearCache();
            GetAllQuestions();
        }
    }
}