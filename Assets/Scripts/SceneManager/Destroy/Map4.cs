using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Map4 : SingletonDestroy<Map4>
{
    [SerializeField]
    Image BgBlack;

    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();
    }


    public void FadeOut()
    {
        ColorAlphaChange(BgBlack.color);
    }

    public void ColorAlphaChange(Color color)
    {
        var scene = SceneManager.GetActiveScene();
        BgBlack.raycastTarget = true;
        StartCoroutine(Change(color ,() => EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.Map4_Dialogue2}", () =>SceneManager.LoadScene(scene.buildIndex+1) ))));
    }


    public IEnumerator Change(Color color, Action action = null)
    {
        var a = color;
        while (color.a < 1)
        { 
            color.a += Time.deltaTime;
            BgBlack.color = color;
            yield return null;
        }
        action?.Invoke();
    }
}
