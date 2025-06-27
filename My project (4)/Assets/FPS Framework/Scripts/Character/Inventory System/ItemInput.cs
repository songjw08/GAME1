using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Akila.FPSFramework
{
    /// <summary>
    /// Handles player input for item interactions such as aiming, reloading, and switching fire modes.
    /// </summary>
    [AddComponentMenu("Akila/FPS Framework/Player/Item Input")]
    public class ItemInput : MonoBehaviour
    {
        /// <summary>
        /// Indicates whether aiming is toggled.
        /// </summary>
        public bool ToggleAim { get; protected set; }

        /// <summary>
        /// The input controls for the player.
        /// </summary>
        public Controls Controls { get; private set; }

        /// <summary>
        /// The inventory item holder for managing equipped items.
        /// </summary>
        public IInventory inventory { get; private set; }

        /// <summary>
        /// Reference to the character input script for player actions.
        /// </summary>
        public CharacterInput CharacterInput { get; private set; }

        // Input flags
        public bool ReloadInput { get; private set; }
        public bool FireModeSwitchInput { get; private set; }
        public bool SightModeSwitchInput { get; private set; }
        public bool AimInput { get; private set; }
        public bool DropInput { get; private set; }
        public bool TriggeredFire { get; private set; }
        public bool HeldFire { get; private set; }

        /// <summary>
        /// Action for throwable item usage.
        /// </summary>
        public InputAction ThrowAction { get; private set; }

        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        private void Start()
        {
            try
            {
                inventory = GetComponentInParent<IInventory>();

                if (inventory == null)
                {
                    Debug.LogError("ItemInput: IInventory component not found in parent. Ensure it exists in the hierarchy.");
                    return;
                }

                CharacterInput = inventory.characterManager.characterInput;

                AddInputListeners();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ItemInput: Initialization failed. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the component is enabled.
        /// Resets the aim input to prevent unintended behavior.
        /// </summary>
        private void OnEnable()
        {
            AimInput = false;

            InitializeControls();
        }

        private void OnDestroy()
        {
            Controls.Disable();
        }

        private void OnDisable()
        {
            Controls.Disable();
        }

        /// <summary>
        /// Handles input updates every frame.
        /// </summary>
        private void Update()
        {
            if (!FPSFrameworkCore.IsActive)
                return;

            // Update input states
            ReloadInput = Controls.Firearm.Reload.triggered;
            FireModeSwitchInput = Controls.Firearm.FireModeSwich.triggered;
            SightModeSwitchInput = Controls.Firearm.SightModeSwitch.triggered;
            DropInput = Controls.Firearm.Drop.triggered;

            ToggleAim = CharacterInput.toggleAim;

            TriggeredFire = Controls.Firearm.Fire.triggered;
            HeldFire = Controls.Firearm.Fire.IsPressed();
        }

        /// <summary>
        /// Initializes the controls and enables the necessary input actions.
        /// </summary>
        private void InitializeControls()
        {
            Controls = new Controls();
            Controls.Firearm.Enable();
            Controls.Throwable.Enable();
            ThrowAction = Controls.Throwable.Throw;
        }

        /// <summary>
        /// Adds listeners for input events such as aiming.
        /// </summary>
        private void AddInputListeners()
        {
            // Aim input handling
            Controls.Firearm.Aim.performed += context =>
            {
                if (FPSFrameworkCore.IsActive)
                {
                    AimInput = ToggleAim ? !AimInput : true;
                }
                else
                {
                    AimInput = false;
                }
            };

            Controls.Firearm.Aim.canceled += context =>
            {
                if (FPSFrameworkCore.IsActive && !ToggleAim)
                {
                    AimInput = false;
                }
            };
        }
    }
}
