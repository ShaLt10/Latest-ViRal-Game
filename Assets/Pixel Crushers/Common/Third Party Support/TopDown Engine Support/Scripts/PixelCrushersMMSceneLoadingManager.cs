using System.Collections;
using UnityEngine;
using MoreMountains.Tools;

namespace PixelCrushers.TopDownEngineSupport
{

    /// <summary>
    /// This subclass of MMSceneLoadingManager tells the Pixel Crushers save system
    /// to save the outgoing scene's state before changing scenes, and then applies
    /// saved state to the newly-loaded scene.
    /// </summary>
    public class PixelCrushersMMSceneLoadingManager : MMSceneLoadingManager
    {

        protected override IEnumerator LoadAsynchronously()
        {
            PixelCrushers.SaveSystem.RecordSavedGameData();
            PixelCrushers.SaveSystem.BeforeSceneChange();
            yield return base.LoadAsynchronously();
            yield return new WaitForEndOfFrame(); // Let components in newly-loaded scene initialize themselves first.
            PixelCrushers.SaveSystem.ApplySavedGameData();
        }
    }
}