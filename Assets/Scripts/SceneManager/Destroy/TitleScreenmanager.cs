using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenmanager : SingletonDestroy<TitleScreenmanager>
{
    private void Awake()
    {
        ScreenRotateControl.Instance.LockOrientationToManual();
        ScreenRotateControl.Instance.SetLandscape();

    }
}
