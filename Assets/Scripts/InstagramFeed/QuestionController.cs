using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestionController : MonoBehaviour
{
    [SerializeField]
    private Button fakta;

    [SerializeField]
    private Button opini;

    InstagramQuestionData questionData;

    [SerializeField]
    TMP_Text question;
    [SerializeField]
    Image potraitImage;

    int score = 0;

    int count = 0;

    private void OnEnable()
    {
        fakta.onClick.AddListener(() => Answer(1));
        opini.onClick.AddListener(() => Answer(0));
        EventManager.Subscribe<QuestionData>(GetQuestion);
    }
    private void OnDisable()
    {
        fakta.onClick.RemoveAllListeners();
        opini.onClick.RemoveAllListeners();
        EventManager.Unsubscribe<QuestionData>(GetQuestion);
    }


    private void Answer(int i)
    {
        count ++;
        switch (i)
        { 
            case 0:
                if (!questionData.Fact)
                {
                    score += 2000;
                }
                break;
            case 1:
                if (questionData.Fact)
                {
                    score += 2000;
                }
                break;
        }
        if (count < 6)
            EventManager.Publish(new GetQuestion(count));
        else
        {
            var scene = SceneManager.GetActiveScene();
            if (score >= 12000)
            {
                EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.InstagramFeed_Win}", () => SceneManager.LoadScene(scene.buildIndex + 1)));
            }
            else
            {
                EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.SpotTheDifference_Lose}", () => SceneManager.LoadScene(scene.buildIndex)));
            }
        }
    }

    private void GetQuestion(QuestionData data)
    {
        questionData = new InstagramQuestionData(data.data);
        question.SetText(questionData.Question);
        potraitImage.sprite = questionData.image;

    }
}
