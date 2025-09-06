using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstagramFeedManager : SingletonDestroy<InstagramFeedManager>
{
    [SerializeField]
    IntagramFeedQuestionData questionDatas;

    private void Awake()
    {
        ScreenRotateControl.Instance.SetPortrait();
    }

    private void OnEnable()
    {
        EventManager.Subscribe<GetQuestion>(GetQuestion);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<GetQuestion>(GetQuestion);
    }


    private void Start()
    {
        EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.InstagramFeed_Opening}",() => GetDataQuestion(0)));
    }

    public void GetDataQuestion(int question)
    { 
        EventManager.Publish(new QuestionData(questionDatas.GetQuestion(question)));
    }

    private void GetQuestion (GetQuestion question)
    {
        GetDataQuestion(question.index);
    }

}
