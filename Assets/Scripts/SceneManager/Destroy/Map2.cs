using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map2 : SingletonDestroy<Map2>
{
    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();
    }
}