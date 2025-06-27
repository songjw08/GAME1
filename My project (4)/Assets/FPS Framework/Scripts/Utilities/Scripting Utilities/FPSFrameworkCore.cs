using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Akila.FPSFramework
{
    public static class FPSFrameworkCore
    {
        public static int GetRefreshRate()
        {
            double result = 0;
            Resolution[] resolutions = Screen.resolutions;

            foreach (Resolution res in resolutions)
            {
                if (res.refreshRateRatio.value > result) result = res.refreshRateRatio.value;
            }

            return (int)result;
        }

        public static Resolution[] GetResolutions()
        {
            List<Resolution> resolutions = new List<Resolution>();

            foreach (Resolution res in Screen.resolutions)
            {
                if (res.width >= 800 && res.height >= 600 && res.refreshRateRatio.value >= GetRefreshRate())
                {
                    resolutions.Add(res);
                }
            }

            resolutions.Reverse();

            return resolutions.ToArray();
        }

        public static Vector3 MultiplyVectors(Vector3 a, Vector3 b)
        {
            a.x *= b.x;
            a.y *= b.y;
            a.z *= b.z;

            return a;
        }

        public static Vector3 GetVector3Direction(Vector3Direction direction)
        {
            Vector3 vector = Vector3.zero;

            switch (direction)
            {
                case Vector3Direction.forward:
                    vector = Vector3.forward;
                    break;
                case Vector3Direction.back:
                    vector = Vector3.back;
                    break;
                case Vector3Direction.right:
                    vector = Vector3.right;
                    break;
                case Vector3Direction.left:
                    vector = Vector3.left;
                    break;
                case Vector3Direction.up:
                    vector = Vector3.up;
                    break;
                case Vector3Direction.down:
                    vector = Vector3.down;
                    break;
            }

            return vector;
        }

        public static Quaternion GetFromToRotation(RaycastHit raycastHit, Vector3Direction direction)
        {
            Quaternion result = new Quaternion();

            switch (direction)
            {
                case Vector3Direction.forward:
                    result = Quaternion.FromToRotation(Vector3.forward, raycastHit.normal);
                    break;

                case Vector3Direction.back:
                    result = Quaternion.FromToRotation(Vector3.back, raycastHit.normal);
                    break;

                case Vector3Direction.right:
                    result = Quaternion.FromToRotation(Vector3.right, raycastHit.normal);
                    break;

                case Vector3Direction.left:
                    result = Quaternion.FromToRotation(Vector3.left, raycastHit.normal);
                    break;

                case Vector3Direction.up:
                    result = Quaternion.FromToRotation(Vector3.up, raycastHit.normal);
                    break;

                case Vector3Direction.down:
                    result = Quaternion.FromToRotation(Vector3.down, raycastHit.normal);
                    break;
            }

            return result;
        }

        private static ControlScheme currentControlScheme;

        public static ControlScheme GetActiveControlScheme()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            Gamepad gamepad = Gamepad.current;
            Touchscreen touchscreen = Touchscreen.current;

            if(keyboard != null)
            {
                if (IsKeyboardInputReceived(keyboard))
                    currentControlScheme = ControlScheme.Keyboard;
            }

            if(mouse != null)
            {
                if (IsMouseInputReceived(mouse))
                    currentControlScheme = ControlScheme.Mouse;
            }

            if(gamepad != null)
            {
                if (IsGamepadInputReceived(gamepad))
                    currentControlScheme = ControlScheme.Gamepad;
            }

            if(touchscreen != null)
            {
                if(IsTouchInputReceived(touchscreen))
                    currentControlScheme = ControlScheme.TouchScreen;
            }

            return currentControlScheme;
        }

        public static bool IsMouseInputReceived(Mouse mouse)
        {
            // Check mouse buttons
            if (mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.middleButton.isPressed)
                return true;

            // Check extra buttons (if the mouse has them)
            if (mouse.forwardButton?.isPressed == true || mouse.backButton?.isPressed == true)
                return true;

            // Check mouse movement
            if (mouse.delta.ReadValue() != Vector2.zero)
                return true;

            // Check scroll wheel
            if (mouse.scroll.ReadValue() != Vector2.zero)
                return true;

            return false; // No mouse input detected
        }

        public static bool IsKeyboardInputReceived(Keyboard keyboard)
        {
            if (keyboard.anyKey.IsPressed()) 
                return true;

            return false;
        }

        public static bool IsGamepadInputReceived(Gamepad gamepad)
        {
            // Check buttons
            if (gamepad.buttonSouth.isPressed || gamepad.buttonNorth.isPressed ||
                gamepad.buttonEast.isPressed || gamepad.buttonWest.isPressed)
                return true;

            // Check triggers
            if (gamepad.leftTrigger.isPressed || gamepad.rightTrigger.isPressed)
                return true;

            // Check bumpers
            if (gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed)
                return true;

            // Check thumbsticks
            if (gamepad.leftStick.ReadValue() != Vector2.zero ||
                gamepad.rightStick.ReadValue() != Vector2.zero)
                return true;

            // Check thumbstick button presses (L3, R3)
            if (gamepad.leftStickButton.isPressed || gamepad.rightStickButton.isPressed)
                return true;

            // Check D-pad
            if (gamepad.dpad.ReadValue() != Vector2.zero)
                return true;

            // Check start/select buttons
            if (gamepad.startButton.isPressed || gamepad.selectButton.isPressed)
                return true;

            return false; // No gamepad input detected
        }

        public static bool IsTouchInputReceived(Touchscreen touchscreen)
        {
            // Check if there are any active touches
            if (touchscreen.touches.Count > 0)
            {
                foreach (var touch in touchscreen.touches)
                {
                    if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.None)
                        return true; // Active touch detected
                }
            }

            return false; // No touch input detected
        }

        public static bool IsActive { get; set; } = true;

        public static bool IsPaused { get; set; } = false;
        
        public static float FieldOfView { get; set; } = 60;
        public static float WeaponFieldOfView { get; set; } = 60;

        public static float SensitivityMultiplier { get; set; } = 1;
        public static float XSensitivityMultiplier { get; set; } = 1;
        public static float YSensitivityMultiplier { get; set; } = 1;
    }
}