using UnityEngine;
using MoreMountains.Tools;

namespace PixelCrushers.TopDownEngineSupport
{

    /// <summary>
    /// Adds option to integrate Pixel Crushers Save System saving with Topdown Engine SaveLoadManager.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Common/Save System/TopDown Engine/Save System TopDown Event Listener")]
    public class SaveSystemTopDownEventListener : MonoBehaviour, MMEventListener<MMGameEvent>
    {

        [Tooltip("Include Pixel Crushers Save System data in More Mountains SaveLoadManager requests.")]
        public bool handleMMSaveLoadEvents = false;

        /// <summary>
        /// On enable, we start listening for MMGameEvents.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (handleMMSaveLoadEvents) this.MMEventStartListening<MMGameEvent>();
        }

        /// <summary>
        /// On disable, we stop listening for MMGameEvents.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (handleMMSaveLoadEvents) this.MMEventStopListening<MMGameEvent>();
        }


        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == "Save")
            {
                Save();
            }
            if (gameEvent.EventName == "Load")
            {
                Load();
            }
        }

        protected const string _saveFolderName = "PixelCrushers/";
        protected const string _saveFileExtension = ".data";

        public void Save()
        {
            var data = PixelCrushers.SaveSystem.RecordSavedGameData();
            MMSaveLoadManager.Save(data, gameObject.name + _saveFileExtension, _saveFolderName);

        }

        public void Load()
        {
            var data = (PixelCrushers.SavedGameData)MMSaveLoadManager.Load(typeof(PixelCrushers.SavedGameData), gameObject.name + _saveFileExtension, _saveFolderName);
            if (data != null) PixelCrushers.SaveSystem.ApplySavedGameData(data);
        }

    }
}
