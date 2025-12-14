using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenRotateControl : Singleton<ScreenRotateControl>
{

    protected override void OnDestroy()
    {
        base.OnDestroy(); // Important for cleanup!
    }
    void Awake()
    {
        //LockOrientationToManual();
    }

    public void LockOrientationToManual()
    {
        SetLandscape();
        // Disable all auto-rotate options
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
    }

    public void SetPortrait()
    {
        Screen.orientation = ScreenOrientation.Portrait;
    }

    public void SetLandscape()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}
