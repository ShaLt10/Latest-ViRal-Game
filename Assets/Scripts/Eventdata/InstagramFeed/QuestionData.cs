using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionData
{
    public InstagramQuestionData data = new();

    public QuestionData(InstagramQuestionData data)
    {
        this.data.image = data.image;
        this.data.Question = data.Question;
        this.data.Fact = data.Fact;
    }
}
