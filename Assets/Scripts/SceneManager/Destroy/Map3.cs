using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map3 : SingletonDestroy<Map3>
{
    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();
    }
}
