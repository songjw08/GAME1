using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Character Input")]
    public class CharacterInput : MonoBehaviour
    {
        public bool toggleAim = false;
        public bool toggleCrouch = true;
        public bool toggleLean = false;
        public bool allowTacticalSprining = true;

        public Action onLeanRight;
        public Action onLeanLeft;

        /// <summary>
        /// Main input actions class.
        /// </summary>
        public Controls controls;

        /// <summary>
        /// The target FPS Controller.
        /// </summary>
        public CharacterManager characterManager { get; protected set; }

        /// <summary>
        /// The current main camera (Cashed)
        /// </summary>
        public Camera mainCamera { get; protected set; }

        /// <summary>
        /// The result value (Vector2) of the move (Forward, Backward, Right & Left).
        /// </summary>
        public Vector2 moveInput { get; protected set; }

        /// <summary>
        /// The result value (Vector2) of the camera look (Up, Down, Right & Left).
        /// </summary>
        public Vector2 rawLookInput { get; protected set; }

        /// <summary>
        /// The result value (Vector 2) of the camera look (Up, Down, right, and Left) multiplied with senstivity and other factors.
        /// </summary>
        public Vector2 lookInput { get; protected set; }

        /// <summary>
        /// Is performing sprint input?
        /// </summary>
        public bool sprintInput { get; set; }

        /// <summary>
        /// Is performing tac sprint input?
        /// </summary>
        public bool tacticalSprintInput { get; set; }

        /// <summary>
        /// Using this raw input because the check douple clickes method needs a field not a property.
        /// </summary>
        [HideInInspector] public bool rawTacticalSprintInput;

        /// <summary>
        /// Is performing jump input?
        /// </summary>
        public bool jumpInput { get; set; }

        /// <summary>
        /// Is performing crouch input?
        /// </summary>
        public bool crouchInput { get; set; }

        /// <summary>
        /// Is performing lean right input?
        /// </summary>
        public bool leanRightInput { get; set; }

        /// <summary>
        /// Is performing lean Left input?
        /// </summary>
        public bool leanLeftInput { get; set; }

        /// <summary>
        /// The value added from the function AddLookAmount(). Setting this to anything will rotate the camera using the value.
        /// </summary>
        public Vector2 addedLookValue { get; set; }

        /// <summary>
        /// Using this value to always get sprint input regardless of the player state.
        /// This is used to then filter the input and choose when to use it.
        /// </summary>
        [HideInInspector] public bool rawSprintInput;

        private float lastSprintClickTime;

        private void Start()
        {
            characterManager = GetComponent<CharacterManager>();
        }

        protected void Update()
        {
            if (FPSFrameworkCore.IsActive == false)
                return;

            //Read values in the update. You can't change input values from the external class as it will reset itself.
            moveInput = controls.Player.Move.ReadValue<Vector2>();

            Vector2 rawLookInput_Unmultiplied = controls.Player.Look.ReadValue<Vector2>();
            rawLookInput = 100 * new Vector2(rawLookInput_Unmultiplied.x * FPSFrameworkCore.XSensitivityMultiplier, rawLookInput_Unmultiplied.y * FPSFrameworkCore.YSensitivityMultiplier) * FPSFrameworkCore.SensitivityMultiplier;

            //Choose when to turn off sprinting input and when to use it.
            if (moveInput.y < 0)
            {
                tacticalSprintInput = false;
                sprintInput = false;

            }
            else
            {
                tacticalSprintInput = moveInput.y > 0 && allowTacticalSprining ? rawTacticalSprintInput : false;
                sprintInput = moveInput.y > 0 ? rawSprintInput : false;
            }

            if (tacticalSprintInput) sprintInput = false;

            //Update tac sprint input.
            controls.Player.TacticalSprint.HasDoupleClicked(ref rawTacticalSprintInput, ref lastSprintClickTime);

            //Jump input
            jumpInput = controls.Player.Jump.triggered;

            Vector2 lookInput = new Vector2();

            //Find the main camera if it's null only.
            mainCamera = Camera.main;

            float sensitivity = 1;

            float finalSensitivity = 1;

            if (mainCamera)
            {
                if (characterManager.character != null)
                {
                    sensitivity = characterManager.character.sensitivity;

                    if (characterManager.character.isDynamicSensitivityEnabled)
                    {
                        finalSensitivity = sensitivity * 
                            (characterManager.character.fovToSensitivityCurve.Evaluate(mainCamera.fieldOfView / 60));
                    }
                    else
                    {
                        finalSensitivity = sensitivity;
                    }
                }
            }
            else
            {
                if (characterManager.character != null) sensitivity = characterManager.character.sensitivity;

                finalSensitivity = sensitivity;
            }
            
            if (FPSFrameworkCore.IsPaused) finalSensitivity = 0;
            
            lookInput = addedLookValue + (rawLookInput * finalSensitivity);

            this.lookInput = (new Vector2(lookInput.x, lookInput.y) / 200) + addedLookValue;

            addedLookValue = Vector2.zero;

            if ((sprintInput || jumpInput) && crouchInput)
                crouchInput = false;
        }

        private void LateUpdate()
        {
            if (leanRightInput && leanLeftInput || sprintInput || tacticalSprintInput)
            {
                leanRightInput = false;
                leanLeftInput = false;
            }
        }

        protected void AddInputListner()
        {
            //Sprint
            controls.Player.Sprint.performed += context =>
            {
                rawSprintInput = true;
            };

            controls.Player.Sprint.canceled += context =>
            {
                rawSprintInput = false;
            };

            //Crouch
            controls.Player.Crouch.performed += context =>
            {
                if (!toggleCrouch)
                    crouchInput = true;
            };

            controls.Player.Crouch.canceled += context =>
            {
                if (!toggleCrouch)
                    crouchInput = false;
                else
                    crouchInput = !crouchInput;
            };

            //Lean Right
            controls.Player.LeanRight.performed += context =>
            {
                if (!toggleLean)
                {
                    leanRightInput = true;
                }
            };

            controls.Player.LeanRight.canceled += context =>
            {
                if (!toggleLean)
                {
                    leanRightInput = false;
                }
                else
                {
                    leanLeftInput = false;
                    leanRightInput = !leanRightInput;
                }
            };

            //Lean Left
            controls.Player.LeanLeft.performed += context =>
            {
                if (!toggleLean)
                    leanLeftInput = true;
            };

            controls.Player.LeanLeft.canceled += context =>
            {
                if (!toggleLean)
                    leanLeftInput = false;
                else
                {
                    leanRightInput = false;
                    leanLeftInput = !leanLeftInput;
                }
            };
        }

        /// <summary>
        /// Adds amount of rotation from the given Vector2 value.
        /// </summary>
        /// <param name="value"></param>
        public void AddLookValue(Vector2 value)
        {
            addedLookValue += value;
        }


        private void OnEnable()
        {
            //Initinaling input actins for this class.
            controls = new Controls();

            controls.Player.Enable();

            //Using event logic to allow external disabling of the input.
            //Example: You could set the sprint value from the external class without it resting itself.
            AddInputListner();
        }

        private void OnDestroy()
        {
            controls.Player.Disable();

            controls.Dispose();
        }

        private void OnDisable()
        {
            controls.Player.Disable();

            controls.Disable();
        }
    }
}