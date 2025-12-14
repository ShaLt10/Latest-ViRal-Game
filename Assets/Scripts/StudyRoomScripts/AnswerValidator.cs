using System.Collections.Generic;
using System.Linq;

namespace DigitalForensicsQuiz
{
    public static class AnswerValidator
    {
        public static bool ValidateMultipleChoice(MinigameQuestionData question, int selectedAnswer, int shuffledCorrectIndex)
        {
            if (question.type != QuestionType.MultipleChoice) return false;
            if (selectedAnswer < 0 || shuffledCorrectIndex < 0) return false;
            return selectedAnswer == shuffledCorrectIndex;
        }

        public static bool ValidateDragAndDrop(MinigameQuestionData question, Dictionary<string, string> playerAnswers)
        {
            if (question.type != QuestionType.DragAndDrop) return false;
            if (question.correctMappings == null || playerAnswers == null) return false;
            if (playerAnswers.Count != question.correctMappings.Count) return false;

            foreach (var correctMapping in question.correctMappings)
            {
                if (!playerAnswers.ContainsKey(correctMapping.scenarioId) || 
                    playerAnswers[correctMapping.scenarioId] != correctMapping.categoryId)
                {
                    return false;
                }
            }
            return true;
        }

        public static float GetAnswerAccuracy(MinigameQuestionData question, Dictionary<string, string> playerAnswers)
        {
            if (question.type != QuestionType.DragAndDrop || 
                question.correctMappings == null || 
                playerAnswers == null || 
                question.correctMappings.Count == 0)
            {
                return 0f;
            }

            int correctMappings = 0;
            foreach (var correctMapping in question.correctMappings)
            {
                if (playerAnswers.ContainsKey(correctMapping.scenarioId) && 
                    playerAnswers[correctMapping.scenarioId] == correctMapping.categoryId)
                {
                    correctMappings++;
                }
            }

            return (float)correctMappings / question.correctMappings.Count;
        }

        public static bool ValidateAllAnswers(List<MinigameQuestionData> questions, List<bool> results)
        {
            if (questions == null || results == null) return false;
            if (questions.Count != results.Count) return false;

            return results.All(result => result);
        }

        public static int GetCorrectAnswersCount(List<bool> results)
        {
            if (results == null) return 0;
            return results.Count(result => result);
        }

        public static float GetOverallAccuracy(List<bool> results)
        {
            if (results == null || results.Count == 0) return 0f;
            return (float)GetCorrectAnswersCount(results) / results.Count;
        }
    }
}