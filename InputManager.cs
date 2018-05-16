using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CROSS_PLATFORM_INPUT
using UnityStandardAssets.CrossPlatformInput;
#endif
namespace AxlPlay
{
    public class InputManager : MonoBehaviour
    {
        public string PauseBt = "Pause";

        public string ChangeItemAxis = "ChangeItem";
        public string PickUpItemAxis = "Pickup";
        public string GrabKey = "Grab";
        public string ShowPlayerListKey = "ShowPlayerList";
        public string RunAxis = "Run";
        public string HoldBreathSniper = "HoldBreath";
        public string TurnAroundX = "Mouse X";
        public string TurnAroundY = "Mouse Y";
        public string MoveHorizontal = "Horizontal";
        public string MoveVertical = "Vertical";
        public string Jump = "Jump";
        public string Crouch = "Crouch";
        public string Reload = "Reload";
        public static InputManager inputManager;

        private void Awake()
        {
            inputManager = this;
        }

        public bool GetButtonDown(string button)
        {


            if (Application.isMobilePlatform)
            {
#if CROSS_PLATFORM_INPUT

                if (CrossPlatformInputManager.GetButtonDown(button))
                    return true;
#endif
            }

            else
            {
                if (Input.GetButtonDown(button))


                    return true;

            }
            return false;
        }
        public bool GetButton(string button)
        {

            if (Application.isMobilePlatform)
            {
#if CROSS_PLATFORM_INPUT

                if (CrossPlatformInputManager.GetButton(button))
                    return true;
#endif

            }
            else
            {
                if (Input.GetButton(button))
                    return true;
            }
            return false;
        }
        public bool GetButtonUp(string button)
        {

            if (Application.isMobilePlatform)
            {
#if CROSS_PLATFORM_INPUT

                if (CrossPlatformInputManager.GetButtonUp(button))
                    return true;
#endif

            }
            else
            {
                if (Input.GetButtonUp(button))
                    return true;
            }
            return false;
        }
        public float GetAxis(string axis)
        {

            if (Application.isMobilePlatform)
            {
#if CROSS_PLATFORM_INPUT

                return CrossPlatformInputManager.GetAxis(axis);
#endif

            }
            else
            {
                return Input.GetAxis(axis);

            }
            return 0f;
        }
        public float GetAxisRaw(string axis)
        {
            if (Application.isMobilePlatform)
            {
#if CROSS_PLATFORM_INPUT

                return CrossPlatformInputManager.GetAxisRaw(axis);
#endif

            }
            else
            {
                return Input.GetAxisRaw(axis);

            }
            return 0f;
        }
    }
}