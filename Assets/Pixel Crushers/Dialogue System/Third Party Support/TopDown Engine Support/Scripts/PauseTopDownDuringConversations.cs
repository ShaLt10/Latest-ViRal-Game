using MoreMountains.TopDownEngine;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelCrushers.DialogueSystem.TopDownEngineSupport
{

    /// <summary>
    /// Pauses TopDown and/or disables player input during conversations.
    /// If you add it to the Dialogue Manager, it will affect all conversations.
    /// If you add it to a player, it will only affect conversations that the
    /// player is involved in.
    /// 
    /// You can also add a copy of this component to a quest log window. 
    /// Configure OnOpen() to call Pause and OnClose() to call Unpause.
    /// Untick the quest log window's Pause While Open and Unlock Cursor While
    /// Open checkboxes since this script will handle it.
    /// 
    /// You can add an MMCursorVisible component to the dialogue UI's 
    /// Dialogue Panel or quest log window's Main Panel to show the cursor
    /// while open.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/TopDown Engine/Pause TopDown During Conversations")]
    public class PauseTopDownDuringConversations : MonoBehaviour
    {
        [Tooltip("Tell Topdown Engine to pause during conversations.")]
        public bool pauseDuringConversations = true;

        [Tooltip("Disable TopDown player input during conversations.")]
        public bool disableInputDuringConversations = true;

        public string[] floatAnimatorParametersToStop = new string[] { "Speed" };
        public string[] boolAnimatorParametersToStop = new string[] { "Walking", "Running", "Jumping" };

        protected int pauseDepth = 0;
        protected bool prevSendNavEvents = false;

        protected virtual void OnConversationStart(Transform actor)
        {
            Pause();
        }

        private void OnConversationEnd(Transform actor)
        {
            Unpause();
        }

        public virtual void Pause()
        {
            // In case we get multiple requests to pause before unpause, only unpause after last call to Unpause:
            if (pauseDepth == 0)
            {
                prevSendNavEvents = EventSystem.current.sendNavigationEvents;
                if (pauseDuringConversations)
                {
                    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
                    if (GUIManager.Instance != null && GUIManager.Instance.PauseScreen != null)
                    {
                        GUIManager.Instance.PauseScreen.SetActive(false);
                    }
                }
                if (disableInputDuringConversations)
                {
                    SetTopDownInput(false);
                    SetAllPlayersComponents(false);
                }
                EventSystem.current.sendNavigationEvents = true;
            }
            pauseDepth++;
        }

        public virtual void Unpause()
        {
            pauseDepth--;
            if (pauseDepth == 0)
            {
                EventSystem.current.sendNavigationEvents = prevSendNavEvents;
                if (pauseDuringConversations)
                {
                    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
                    if (GUIManager.Instance != null && GUIManager.Instance.PauseScreen != null)
                    {
                        GUIManager.Instance.PauseScreen.SetActive(false);
                    }
                }
                if (disableInputDuringConversations)
                {
                    SetTopDownInput(true);
                    SetAllPlayersComponents(true);
                }
            }
        }

        protected virtual void SetTopDownInput(bool value)
        {
            SetAllInputManagers(value);
        }

        protected virtual void SetAllInputManagers(bool value)
        {
            foreach (var inputManager in FindObjectsOfType<InputManager>())
            {
                inputManager.InputDetectionActive = value;
            }
        }

        protected virtual void SetAllPlayersComponents(bool value)
        {
            if (value)
            {
                MoreMountains.TopDownEngine.LevelManager.Instance.UnFreezeCharacters();
            }
            else
            {
                foreach (Character player in MoreMountains.TopDownEngine.LevelManager.Instance.Players)
                {
                    player.LinkedInputManager.RunButton.TriggerButtonUp();
                    var characterRun = player.GetComponent<CharacterRun>();
                    if (characterRun != null) characterRun.RunStop();
                    player.GetComponent<CharacterMovement>().ResetSpeed();
                    player.MovementState.ChangeState(CharacterStates.MovementStates.Idle);

                }

                MoreMountains.TopDownEngine.LevelManager.Instance.FreezeCharacters();
                StartCoroutine(StopAnimators());
            }

            foreach (Character player in MoreMountains.TopDownEngine.LevelManager.Instance.Players)
            {
                player.ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);
                player.MovementState.ChangeState(CharacterStates.MovementStates.Idle);

                var characterPause = player.FindAbility<CharacterPause>();
                if (characterPause != null)
                {
                    if (value == true)
                    {
                        characterPause.UnPauseCharacter();
                    }
                    else
                    {
                        characterPause.PauseCharacter();
                    }
                }

                var characterMovement = player.FindAbility<CharacterMovement>();
                if (characterMovement != null)
                {
                    characterMovement.PermitAbility(value);
                    characterMovement.MovementForbidden = !value;
                    if (value == false) characterMovement.StopStartFeedbacks();
                }

                foreach (CharacterHandleWeapon characterHandleWeapon in player.GetComponents<CharacterHandleWeapon>())
                {
                    characterHandleWeapon.ShootStop();
                    characterHandleWeapon.PermitAbility(value);
                    if (characterHandleWeapon.CurrentWeapon != null)
                    {
                        if (characterHandleWeapon.WeaponAimComponent != null)
                        {
                            characterHandleWeapon.WeaponAimComponent.enabled = value;
                        }
                    }
                }

                PermitAbility<CharacterJump2D>(player, value);
                PermitAbility<CharacterJump3D>(player, value);
            }
        }

        protected virtual void PermitAbility<T>(Character player, bool value) where T : CharacterAbility
        {
            var ability = player.FindAbility<T>();
            if (ability != null)
            {
                ability.PermitAbility(value);
                if (value == false) ability.StopStartFeedbacks();
            }
        }

        protected IEnumerator StopAnimators()
        {
            yield return null;
            foreach (Character player in MoreMountains.TopDownEngine.LevelManager.Instance.Players)
            {
                var animator = player.GetComponent<Character>()._animator;
                foreach (var floatParameter in floatAnimatorParametersToStop)
                {
                    animator.SetFloat(floatParameter, 0);
                }
                foreach (var boolParameter in boolAnimatorParametersToStop)
                {
                    animator.SetBool(boolParameter, false);
                }
                var movement = player.FindAbility<CharacterMovement>();
                if (movement != null && movement.WalkParticles != null)
                {
                    foreach (var ps in movement.WalkParticles)
                    {
                        ps.Stop();
                    }
                }
            }
        }
    }
}
