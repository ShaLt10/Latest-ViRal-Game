using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback allows you to destroy a target gameobject, either via Destroy, DestroyImmediate, or SetActive:False
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to change the brain state")]
    [FeedbackPath("AI/BrainChangeState")]
    public class MMF_BrainChangeState : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        /// sets the inspector color for this feedback
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
#endif
        public AIBrain TargetBrain;
        public string StartStateName;
        public string TargetStateName;

        /// <summary>
        /// On Play we change the state of our Behaviour if needed
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }
            Proceed();
        }

        protected override void CustomInitialization(MMF_Player owner)
        {
            base.CustomInitialization(owner);
            if (TargetBrain == null) return;    
            TargetBrain.TransitionToState(StartStateName);
        }

        /// <summary>
        /// Changes the status of the Behaviour
        /// </summary>
        /// <param name="state"></param>
        protected virtual void Proceed()
        {
            TargetBrain.TransitionToState(TargetStateName);
        }
    }
}
