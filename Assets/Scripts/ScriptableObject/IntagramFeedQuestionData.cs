using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[CreateAssetMenu(fileName = "FaktaOpini", menuName = "Game/InstagramFeed/DialogueSequence")]
public class IntagramFeedQuestionData : ScriptableObject
{
    public List<InstagramQuestionData> datas;

    public InstagramQuestionData GetQuestion(int i)
    { 
        return datas[i];
    }

    public void Randomize()
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = datas.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            var value = datas[k];
            datas[k] = datas[n];
            datas[n] = value;
        }
    }
}

[Serializable]
public class InstagramQuestionData
{
    public Sprite image;
    [TextArea]
    public string Question;
    public bool Fact;

    public InstagramQuestionData(InstagramQuestionData data)
    {
        this.image = data.image;
        this.Question = data.Question;
        this.Fact = data.Fact;
    }

    public InstagramQuestionData()
    {
        this.image = null;
        this.Question = "";
        this.Fact = false;
    }
}