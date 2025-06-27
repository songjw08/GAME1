using Akila.FPSFramework.Animation;
using Akila.FPSFramework.Internal;
using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Weapone/Firearm")]
    [RequireComponent(typeof(FirearmAttachmentsManager))]
    public class Firearm : InventoryItem
    {
        [Tooltip("The firearm preset that defines all values for this firearm. This preset is a ScriptableObject.")]
        public FirearmPreset preset;

        [Tooltip("The Transform from which the shots are fired.")]
        public Transform muzzle;

        [Tooltip("The Transform from which shell casings are ejected.")]
        public Transform casingEjectionPort;

        [Tooltip("Particle system effects that play when chambering a bullet.")]
        public ParticleSystem chamberingEffects;

        [Tooltip("Events related to this firearm."), Space]
        public FirearmEvents events;

        public InventoryCollectable ammoProfile { get; set; }

        private Crosshair crosshair;
        /// <summary>
        /// Pattern for bullet spread when hip firing.
        /// </summary>
        private SprayPattern hipFireSprayPattern;

        /// <summary>
        /// Pattern for bullet spread when aiming down sights.
        /// </summary>
        private SprayPattern aimDownSightsSprayPattern;

        /// <summary>
        /// The sound that plays when firing the firearm.
        /// </summary>
        public Audio fireAudio;

        /// <summary>
        /// The currently extrenally assigned audio.
        /// </summary>
        public Audio currentFireAudio;

        /// <summary>
        /// The sound that plays after the shot (the tail sound).
        /// </summary>
        public Audio fireTailAudio;

        /// <summary>
        /// The sound that plays when reloading the firearm.
        /// </summary>
        public Audio reloadAudio;

        /// <summary>
        /// The sound that plays when reloading an empty magazine.
        /// </summary>
        public Audio reloadEmptyAudio;

        /// <summary>
        /// The multiplier for tactical sprint speed.
        /// </summary>
        private float tacticalSprintMultiplier;

        /// <summary>
        /// The HUD elements related to this firearm.
        /// </summary>
        public FirearmHUD firearmHUD { get; set; }

        /// <summary>
        /// Manages attachments for this firearm.
        /// </summary>
        public FirearmAttachmentsManager firearmAttachmentsManager { get; protected set; }

        /// <summary>
        /// Array of particle systems related to this firearm.
        /// </summary>
        public ParticleSystem[] firearmParticleEffects { get; set; }

        /// <summary>
        /// The current fire mode setting of the firearm.
        /// </summary>
        public InventoryItem.FireMode currentFireMode { get; set; }

        /// <summary>
        /// The number of shots fired in the current session.
        /// </summary>
        public int shotsFired { get; protected set; }

        public float currentSprayAmount { get; protected set; }

        // The current velocity of the spray pattern, affecting how much the spread deviates over time.
        private float currentSprayVelocity;

        // The current multiplier for the spray pattern, affecting the overall intensity of the spread.
        private float currentSprayMultiplier;

        /// <summary>
        /// The current amount of ammo remaining in the magazine.
        /// </summary>
        public int remainingAmmoCount { get; set; }

        /// <summary>
        /// The current count of ammo for the specific ammo type.
        /// </summary>
        public int remainingAmmoTypeCount { get; set; }

        /// <summary>
        /// The maximum capacity of the magazine.
        /// </summary>
        public int magazineCapacity { get; set; }

        /// <summary>
        /// Whether the firearm is currently reloading.
        /// </summary>
        public bool isReloading { get; set; }

        /// <summary>
        /// Whether the firearm is currently firing.
        /// </summary>
        public bool isFiring { get; set; }

        /// <summary>
        /// Whether the firearm is out of ammo.
        /// </summary>
        public bool isOutOfAmmo { get; set; }

        /// <summary>
        /// Whether the player is attempting to fire the firearm.
        /// </summary>
        public bool attemptingToFire { get; protected set; }

        /// <summary>
        /// Whether the firearm is ready to fire.
        /// </summary>
        public bool readyToFire { get; protected set; }

        /// <summary>
        /// A flag that determines whether the firing action is prevented. 
        /// This is useful for disabling the ability to fire, such as when the player is interacting with an inventory UI or performing other non-combat actions.
        /// </summary>
        public bool firePrevented { get; set; }

        /// <summary>
        /// Whether the firearm is ready to be reloaded.
        /// </summary>
        public bool readyToReload { get; protected set; }

        /// <summary>
        /// Controls the visibility of the HUD.
        /// </summary>
        public bool isHudActive { get; set; } = true;

        /// <summary>
        /// Controls the audio system's activation state.
        /// </summary>
        public bool isAudioActive { get; set; } = true;

        /// <summary>
        /// Controls the particle system effects's activation state.
        /// </summary>
        public bool isParticleEffectsActive { get; set; } = true;

        /// <summary>
        /// <summary>
        /// Indicates whether throwing casings is enabled. Defaults to true.
        /// </summary>
        public bool isThrowingCasingActive { get; set; } = true;

        /// <summary>
        /// Controls whether player input is enabled.
        /// </summary>
        public bool isInputActive { get; set; } = true;

        /// <summary>
        /// Represents the current progress of the aiming animation, ranging from 0 (not aiming) to 1 (fully aimed).
        /// </summary>
        public float aimProgress
        {
            get
            {
                if(aimingAnimation != null)
                    return aimingAnimation.progress;

                return 0;
            }
        }

        /// <summary>
        /// Tracks the delay between shots fired.
        /// </summary>
        private float fireTimer;

        // Action to be invoked when firing is started, passing the position, rotation, and direction of the projectile.
        public Action<Vector3, Quaternion, Vector3> onFire;

        // Action to be invoked when firing is completed, passing the position, rotation, and direction of the projectile.
        public Action<Vector3, Quaternion, Vector3> onFireDone;

        // Action to be invoked when the casing has been thrown.
        public Action onCasingThrown;

        private bool isPreviouslyReloading;


        protected override void Start()
        {
            // Call the base class Start method to ensure any inherited initialization is performed.
            base.Start();

            // Check if a valid preset is provided
            if (preset == null)
            {
                Debug.LogWarning("Preset is null. No audio setup will be performed.");
                return;
            }

            // Setup fire audio if the preset provides it
            if (preset.fireSound != null)
            {
                fireAudio = new Audio();
                fireAudio.Setup(this, preset.fireSound);

                // Set the current fire audio profile based on the preset.
                currentFireAudio = fireAudio;
            }

            // Setup fire tail audio if the preset provides it
            if (preset.fireTailSound != null)
            {
                fireTailAudio = new Audio();
                fireTailAudio.Setup(this, preset.fireTailSound);
            }

            // Setup reload audio if the preset provides it
            if (preset.reloadSound != null)
            {
                reloadAudio = new Audio();
                reloadAudio.Setup(this, preset.reloadSound);
            }

            // Setup reload empty audio if the preset provides it
            if (preset.reloadEmptySound != null)
            {
                reloadEmptyAudio = new Audio();
                reloadEmptyAudio.Setup(this, preset.reloadEmptySound);
            }

            // Get the required components from the GameObject.
            firearmAttachmentsManager = GetComponent<FirearmAttachmentsManager>();
            itemInput = GetComponent<ItemInput>();

            // Validate the firearm preset.
            if (preset == null)
            {
                Debug.LogError("Firearm preset is not assigned.", gameObject);
                return;
            }

            // Set the replacement object based on the preset.
            replacement = preset.replacement;

            // Get all particle systems attached to the firearm.
            firearmParticleEffects = GetComponentsInChildren<ParticleSystem>();

            // Instantiate the HUD for the firearm and set its reference to this firearm.
            if (preset.firearmHud == null)
            {
                Debug.LogError("FirearmHUD is not set in the preset. Firearm's HUD won't be initialized.", gameObject);
            }
            else
            {
                firearmHUD = Instantiate(preset.firearmHud, transform);
                firearmHUD.firearm = this;
            }

            if(preset.crosshair == null)
            {
                Debug.LogError("Crosshair is not set in the preset. Fire's crosshair won't be initialized.", gameObject);
            }
            else
            {
                crosshair = Instantiate(preset.crosshair, firearmHUD.transform);
                crosshair.firearm = this;
            }

            // Initialize spray patterns, use a default if none is provided in the preset.
            if (preset.sprayPattern == null)
            {
                Debug.LogError($"Spray pattern is not assigned in the preset. Using a default instance. For better control and accuracy, consider assigning a custom spray pattern.", preset);
                hipFireSprayPattern = ScriptableObject.CreateInstance<SprayPattern>();
            }
            else
            {
                hipFireSprayPattern = preset.sprayPattern;
            }

            if (preset.aimSprayPattern == null)
            {
                Debug.LogError($"Aim spray pattern is not assigned in the preset. Using a default instance. For better control and accuracy, consider assigning a custom spray pattern.", preset);
                hipFireSprayPattern = ScriptableObject.CreateInstance<SprayPattern>();
            }
            else
            {
                aimDownSightsSprayPattern = preset.aimSprayPattern;
            }

            //Initialize ammo profile from inventory
            foreach(InventoryCollectable collectable in inventory.collectables)
            {
                if (collectable.GetIdentifier() == preset.ammoType)
                    ammoProfile = collectable;
            }

            // Initialize ammo profile if not set
            if (ammoProfile == null)
            {
                Debug.LogError("Ammo profile is not set. Using a default instance.", preset);

                ammoProfile = new InventoryCollectable();

                ammoProfile.identifier = ScriptableObject.CreateInstance<AmmoProfileData>();

                ammoProfile.identifier.displayName = "Unknown Ammo Profile";

                ammoProfile.SetCount(500);
            }


            // Set the initial ammo and magazine capacity based on the preset.
            remainingAmmoCount = preset.reserve;
            magazineCapacity = preset.magazineCapacity;

            // Validate muzzle and casing ejection port transforms.
            if (!muzzle)
            {
                Debug.LogError("Muzzle transform is not assigned. Defaulting to the firearm's transform.", gameObject);
                muzzle = transform;
            }

            if (!casingEjectionPort)
            {
                Debug.LogError("Casing ejection port transform is not assigned. Defaulting to the firearm's transform.", gameObject);
                casingEjectionPort = transform;
            }

            // Initialize reloading state.
            isReloading = false;

            // If the firearm has an inventory and character manager, reset the character's speed.
            if (characterManager != null)
            {
                ICharacterController character = characterManager.character;

                if (character != null)
                {
                    character.ResetSpeed();
                }
            }
        }

        /// <summary>
        /// Updates the firearm's state, including handling reloading, ammo, and animation states.
        /// </summary>
        protected override void Update()
        {
            // Call the base class Update method to ensure any inherited actions is performed.
            base.Update();

            ProceduralAnimation reloadingAnimation = proceduralAnimator.GetAnimation("Reloading");

            if (reloadingAnimation)
                reloadingAnimation.isPlaying = isReloading;

            // Check if preset is assigned
            if (preset == null)
            {
                Debug.LogError($"The preset is not set. All firearm functionality will be disabled. A preset is essential for proper operation of the firearm.", gameObject);

                return;
            }

            // Update input and movement
            UpdateInput();
            AdjustPlayerSpeed();

            // Stop reloading if magazine is full and reload method is scripted
            if (preset.reloadMethod == ReloadType.Scripted && remainingAmmoCount >= magazineCapacity)
            {
                isReloading = false;
            }

            remainingAmmoTypeCount = ammoProfile.count;

            // Handle reloading state based on ammo count
            if (ammoProfile.count <= 0)
            {
                isReloading = false;
            }

            // Clamp remaining ammo count within magazine capacity
            remainingAmmoCount = Mathf.Clamp(remainingAmmoCount, 0, magazineCapacity);

            // Reset shots fired if it exceeds the preset shot count
            if (shotsFired >= preset.shotCount)
            {
                shotsFired = 0;
            }

            // Fire if in firing state
            if (isFiring)
            {
                Fire();
            }

            if (proceduralAnimator)
            {
                if (aimingAnimation)
                {
                    characterManager.isAiming = aimingAnimation.isPlaying;
                    characterManager.attemptingToAim = aimingAnimation.velocity != 0;
                }
            }

            foreach (Animator animator in animators)
            {
                animator.SetBool("Is Reloading", isReloading);
                animator.SetInteger("Ammo", remainingAmmoCount);
                animator.SetFloat("ADS Amount", aimProgress);
                animator.SetFloat("Sprint Amount", tacticalSprintMultiplier);
            }

            if (isFiring)
            {
                hipFireSprayPattern.RampupMagnitude(ref currentSprayMultiplier, ref currentSprayVelocity);
                aimDownSightsSprayPattern.RampupMagnitude(ref currentSprayMultiplier, ref currentSprayVelocity);
            }

            if (isFiring == false && characterManager.velocity.magnitude <= 0)
            {
                hipFireSprayPattern.ResetMagnitude(ref currentSprayMultiplier, ref currentSprayVelocity);
                aimDownSightsSprayPattern.ResetMagnitude(ref currentSprayVelocity, ref currentSprayVelocity);
            }

            if (characterManager.IsAlmostStopped() == false && !isAiming)
            {
                float multiplier = Mathf.Lerp(0.5f, 1, characterManager.velocity.magnitude / character.sprintSpeed);

                currentSprayAmount = hipFireSprayPattern.totalAmount * multiplier;
            }
            else
            {
                currentSprayAmount = Mathf.Lerp(hipFireSprayPattern.totalAmount * currentSprayMultiplier, aimDownSightsSprayPattern.totalAmount * currentSprayMultiplier, aimProgress);
            }
        }
        

        private void LateUpdate()
        {
            if (isReloading && !isPreviouslyReloading)
            {
                events.onReloadStart?.Invoke();
            }

            if (!isReloading && isPreviouslyReloading)
            {
                events.OnReloadComplete?.Invoke();
            }

            isPreviouslyReloading = isReloading;
        }


        /// <summary>
        /// Updates the player's movement speed based on the current action (firing, aiming, or idle).
        /// </summary>
        protected virtual void AdjustPlayerSpeed()
        {
            // Check if the preset is assigned
            if (preset == null)
            {
                Debug.LogError("The preset is not assigned. Player speed adjustments will not be applied. Please ensure a valid preset is configured.", gameObject);
                return;
            }

            // Check if the character manager is assigned
            if (characterManager == null)
            {
                Debug.LogError("The character manager is not assigned. Player speed adjustments will not be applied. Please assign a valid character manager.", gameObject);
                return;
            }

            // Check if the character is assigned in the character manager
            if (characterManager.character == null)
            {
                Debug.LogError("The character is not assigned in your character manager. Player speed adjustments will not be applied. Please assign a valid character.", characterManager.gameObject);
                return;
            }

            ICharacterController character = characterManager.character;

            // Adjust player speed based on the current state
            if (isFiring)
            {
                character.SetSpeed(preset.fireWalkPlayerSpeed);
            }
            else if (isAiming)
            {
                character.SetSpeed(preset.aimWalkPlayerSpeed);
            }
            else
            {
                character.SetSpeed(preset.basePlayerSpeed);
            }
        }

        protected virtual void UpdateInput()
        {
            if (FPSFrameworkCore.IsActive == false)
                return;

            // Reset spray pattern magnitude if not firing
            if (!isFiring)
            {
                hipFireSprayPattern?.ResetMagnitude(ref currentSprayMultiplier, ref currentSprayVelocity);
                aimDownSightsSprayPattern?.ResetMagnitude(ref currentSprayMultiplier, ref currentSprayVelocity);
            }

            // Cancel reloading if firing and allowed
            if (preset.canCancelReloading && isFiring)
            {
                CancelReload();
            }

            // Exit if item input is not active
            if (!itemInput || !isInputActive)
            {
                return;
            }

            // Handle item input actions
            if (itemInput.DropInput)
            {
                Drop();
            }

            if (itemInput.ReloadInput)
            {
                Reload();
            }

            bool isRotationDefault = true;

            // Check if procedural animator's rotation is default
            if (proceduralAnimator)
            {
                isRotationDefault = proceduralAnimator.IsDefaultingInRotation(preset.maxAimDeviation, true, true, false);
            }

            if (preset.isFireActive)
            {
                // Handle fire mode switch
                if (preset.fireMode == FireMode.Selective)
                {
                    if (itemInput.FireModeSwitchInput)
                    {
                        currentFireMode = (currentFireMode == FireMode.Auto) ? FireMode.SemiAuto: FireMode.Auto;

                        events.OnFireModeChange?.Invoke();
                        
                        Debug.Log($"Selective Mode Switched To: {currentFireMode}");
                    }
                }
                else
                {
                    currentFireMode = preset.fireMode;
                }

                // Update firing status based on fire mode
                if (readyToFire)
                {
                    isFiring = (currentFireMode == FireMode.Auto)
                        ? itemInput.Controls.Firearm.Fire.IsPressed()
                        : itemInput.Controls.Firearm.Fire.triggered;
                }
                else
                {
                    isFiring = false;
                }

                // Attempt to fire if conditions are met
                bool isFireInputActive = (currentFireMode == FireMode.Auto)
                    ? itemInput.Controls.Firearm.Fire.IsPressed()
                    : itemInput.Controls.Firearm.Fire.triggered;

                if (itemInput.Controls.Firearm.Fire.triggered && remainingAmmoCount == 0 && preset.canAutomaticallyReload)
                {
                    Reload();
                }

                attemptingToFire = isFireInputActive && remainingAmmoCount > 0;

                // Determine readiness to reload and fire
                readyToReload = !(remainingAmmoCount >= magazineCapacity || remainingAmmoTypeCount <= 0 || isReloading);
                readyToFire = (!isReloading || preset.canCancelReloading) && isRotationDefault && remainingAmmoCount > 0
                    && !IsPlayingRestrictedAnimation() && !FPSFrameworkCore.IsPaused;
            }
        }

        /// <summary>
        /// Initiates the firing sequence based on the current firing state and preset settings.
        /// </summary>
        public void Fire()
        {
            // Exit if not ready to fire
            if (!readyToFire || firePrevented)
            {
                return;
            }

            Vector3 firePosition = Vector3.zero;
            Quaternion fireRotation = Quaternion.identity;
            Vector3 fireDirection = Vector3.zero;

            Camera mainCamera = Camera.main;

            // Determine the firing position and direction based on preset settings
            switch (preset.shootingDirection)
            {
                case ShootingDirection.MuzzleForward:

                    firePosition = muzzle.position;
                    fireRotation = muzzle.rotation;
                    fireDirection = muzzle.forward;

                    break;

                case ShootingDirection.CameraForward:

                    firePosition = mainCamera.transform.position;
                    fireRotation = mainCamera.transform.rotation;
                    fireDirection = mainCamera.transform.forward;

                    break;

                case ShootingDirection.FromMuzzleToCameraForward:

                    RaycastHit hitInfo;
                    if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hitInfo, preset.hittableLayers))
                    {
                        if (hitInfo.distance > 5f)
                        {
                            firePosition = muzzle.position;
                            fireRotation = muzzle.rotation;
                            fireDirection = (hitInfo.point - muzzle.position).normalized;
                        }
                        else
                        {
                            firePosition = mainCamera.transform.position;
                            fireRotation = mainCamera.transform.rotation;
                            fireDirection = mainCamera.transform.forward;
                        }
                    }
                    else
                    {
                        firePosition = muzzle.position;
                        fireRotation = muzzle.rotation;
                        fireDirection = mainCamera.transform.forward;
                    }

                    break;

                case ShootingDirection.FromCameraToMuzzleForward:

                    firePosition = mainCamera.transform.position;
                    fireRotation = mainCamera.transform.rotation;

                    RaycastHit hitInfo2;
                    if (Physics.Raycast(muzzle.position, muzzle.forward, out hitInfo2) && hitInfo2.distance > 5f)
                    {
                        fireDirection = (hitInfo2.point - mainCamera.transform.position).normalized;
                    }
                    else
                    {
                        fireDirection = muzzle.forward;
                    }

                    break;
            }


            // Execute the firing logic with the calculated position, rotation, and direction
            Fire(firePosition, fireRotation, fireDirection);
        }

        /// <summary>
        /// Executes the firing action with specified position, rotation, and direction.
        /// </summary>
        /// <param name="position">The position from which to fire.</param>
        /// <param name="rotation">The rotation of the firearm during firing.</param>
        /// <param name="direction">The direction in which the projectile or hit scan will be fired.</param>
        public void Fire(Vector3 position, Quaternion rotation, Vector3 _direction)
        {
            // Exit if not ready to fire or fire timer has not elapsed
            if (!readyToFire || Time.time <= fireTimer)
            {
                return;
            }

            Vector3 finalDirection = Vector3.zero;

            onFire?.Invoke(position, rotation, finalDirection);

            // Update fire timer
            fireTimer = Time.time + 60f / preset.fireRate;

            // Check if currently playing a restricted animation
            if (!IsPlayingRestrictedAnimation())
            {
                shotsFired = 0;
                finalDirection = GetSprayPattern(_direction);

                FireDone(position, rotation, finalDirection);

                // Apply fire logic if not set to always apply fire
                if (!preset.alwaysApplyFire)
                {
                    ApplyFireOnce();
                }
            }

            // Handle gamepad vibration if enabled
            GamepadManager gamepadManager = GamepadManager.Instance;

            if (gamepadManager)
            {
                gamepadManager.BeginVibration(preset.gamepadVibrationAmountRight, preset.gamepadVibrationAmountLeft, preset.gamepadVibrationDuration);
            }
        }

        private Vector3 position;
        private Quaternion rotation;
        private Vector3 direction;

        protected virtual void InvokeFireDone()
        {
            FireDone(position, rotation, direction);
        }

        /// <summary>
        /// Handles the final steps of the firing process, including visual effects and projectile/hit scan logic.
        /// </summary>
        /// <param name="position">The origin position of the projectile or hit scan.</param>
        /// <param name="rotation">The firearm's rotation at the moment of firing.</param>
        /// <param name="direction">The direction in which the projectile or hit scan is fired.</param>
        public virtual void FireDone(Vector3 position, Quaternion rotation, Vector3 direction)
        {
            this.position = position;
            this.rotation = rotation;
            this.direction = direction;

            onFireDone?.Invoke(position, rotation, direction);

            // Trigger this event before executing local firing logic to facilitate networking.
            // This allows you to add a listener to the onFireDone event and make it a server command.
            // Since elements like ammo count, camera recoil, and shell casing spawn don't require full networking, 
            // they are handled locally.

            // Apply firing logic regardless of active status if always apply fire is enabled.
            if (preset.alwaysApplyFire)
            {
                ApplyFireOnce();
            }
            
            // Cancel any pending FireDone invocations
            CancelInvoke();

            // Increment shots fired and handle multiple shots if needed
            shotsFired++;
            if (shotsFired < preset.shotCount && remainingAmmoCount > 0)
            {
                if (preset.shotDelay <= 0f)
                {
                    FireDone(position, rotation, direction);
                }
                else if (shotsFired >= 1)
                {
                    Invoke(nameof(InvokeFireDone), preset.shotDelay);
                }
            }

            // Exit the method if the firearm is not active.
            if (!isActive)
            {
                return;
            }

            // Play particle effects except for chambering effects
            foreach (ParticleSystem particleSystem in firearmParticleEffects)
            {
                if (particleSystem != chamberingEffects)
                {
                    particleSystem.Play();
                }
            }

            // Handle projectile shooting
            if (preset.shootingMechanism == InventoryItem.ShootingMechanism.Projectiles)
            {
                if (preset.projectile)
                {
                    SpawnProjectile(position, rotation, direction, preset.muzzleVelocity, preset.range);
                }
                else
                {
                    Debug.LogError($"{Name}'s projectile field is null. The firearm will not fire. Assign it and try again.");
                }
            }

            // Handle hitscan shooting
            if (preset.shootingMechanism == InventoryItem.ShootingMechanism.Hitscan)
            {
                Ray ray = new Ray(muzzle.position, direction);

                if (Physics.Raycast(ray, out RaycastHit hit, preset.range, preset.hittableLayers))
                {
                    float damage = preset.alwaysApplyFire ? preset.damage : preset.damage / preset.shotCount;
                    Firearm.UpdateHits(this, preset.defaultDecal, ray, hit, damage, preset.decalDirection);
                }
            }
        }

        /// <summary>
        /// Applies the firing effects, including recoil, ammo consumption, animations, and camera shake.
        /// </summary>
        public void ApplyFireOnce()
        {
            // Apply recoil if enabled
            if (preset.isRecoilActive)
            {
                ApplyRecoil();
            }

            // Decrease remaining ammo count and update spray amount
            remainingAmmoCount--;

            // Play firing animation across all animators
            foreach (Animator animator in animators)
            {
                animator?.CrossFade("Fire", preset.fireTransition, 0, 0f);
            }

            foreach(ParticleSystem effect in firearmParticleEffects)
            {
                if (isParticleEffectsActive && effect != chamberingEffects) effect.Play();
            }

            // Shake camera if applicable
            CharacterManager characterManager = base.characterManager;
            CameraManager cameraManager = characterManager?.cameraManager;

            cameraManager?.ShakeCameras(preset.cameraShakeAmount, preset.cameraShakeRoughness, preset.cameraShakeDuration);

            // Handle firing audio
            if (isAudioActive && preset.isAudioActive)
            {
                currentFireAudio?.PlayOneShot();

                fireTailAudio?.Stop();
                fireTailAudio?.PlayOneShot();
            }

            // Throw casing after firing
            ThrowCasing();
        }

        /// <summary>
        /// Spawns a projectile at the specified position and rotation, with a given direction, speed, and range.
        /// </summary>
        /// <param name="position">The world position where the projectile should spawn.</param>
        /// <param name="rotation">The rotation of the projectile when it spawns.</param>
        /// <param name="direction">The direction in which the projectile will travel.</param>
        /// <param name="speed">The speed of the projectile.</param>
        /// <param name="range">The range the projectile can travel before it is destroyed or becomes ineffective.</param>
        /// <returns>Returns the spawned <see cref="Projectile"/> instance.</returns>
        /// <exception cref="System.NullReferenceException">Thrown when the projectile prefab or preset is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the speed or range is less than or equal to zero.</exception>
        public Projectile SpawnProjectile(Vector3 position, Quaternion rotation, Vector3 direction, float speed, float range)
        {
            // Check if the preset or projectile prefab is null
            if (preset == null)
            {
                Debug.LogError("Firearm preset is not set.", gameObject);

                return null;
            }

            // Check if the preset or projectile prefab is null
            if (preset.projectile == null)
            {
                Debug.LogError("Projectile prefab is not set.", preset);

                return null;
            }

            // Instantiate the projectile prefab at the given position and rotation
            Projectile newProjectile = Instantiate(preset.projectile, position, rotation);

            // Initialize the velocity of the projectile to zero
            Vector3 initialVelocity = Vector3.zero;

            // If a character manager exists, add its velocity to the initial velocity of the projectile
            if (characterManager)
            {
                initialVelocity += characterManager.velocity;
            }

            // Set up the new projectile with the given parameters: owner, direction, initial velocity, speed, and range
            newProjectile.Setup(this, preset.shotCount <= 1 ? direction : GetSprayPattern(direction), initialVelocity, speed, range);

            // Return the newly spawned projectile
            return newProjectile;
        }

        /// <summary>
        /// Updates the state of objects hit by a projectile, including applying damage, handling decals, and applying forces.
        /// </summary>
        /// <param name="firearm">The firearm that fired the projectile.</param>
        /// <param name="projectile">The projectile that hit the target.</param>
        /// <param name="defaultDecal">The default decal to apply on the hit surface.</param>
        /// <param name="ray">The ray that represents the projectile's path.</param>
        /// <param name="hit">Information about the hit result.</param>
        /// <param name="damage">The amount of damage to apply.</param>
        /// <param name="decalDirection">The direction for orienting the decal.</param>
        public static void UpdateHits(Firearm firearm, GameObject defaultDecal, Ray ray, RaycastHit hit, float damage, Vector3Direction decalDirection)
        {
            // Check if the hit object should be ignored based on IgnoreHitDetection component
            if (hit.transform.TryGetComponent(out IgnoreHitDetection ignoreHitDetection))
            {
                return;
            }

            //Exit if firearm is not set.
            if (firearm == null)
            {
                Debug.LogError($"Firearm is not set {new System.Diagnostics.StackTrace()}.");

                return;
            }

            //Exit if firearm preset is not set
            if(firearm.preset == null)
            {
                Debug.Log($"Firearm preset is not set {new System.Diagnostics.StackTrace()}.", firearm);

                return;
            }

            if(firearm.character == null)
            {
                Debug.LogError($"Character (ICharacterController) in the firearm is not set.", firearm);

                return;
            }

            Actor actor = firearm.actor;
            
            FirearmPreset preset = firearm.preset;

            FirearmAttachmentsManager firearmAttachmentsManager = firearm.firearmAttachmentsManager;

            ICharacterController character = firearm.character;

            // Invoke hit callbacks for the firearm
            InvokeHitCallbacks(character.gameObject, ray, hit);

            // Exit if the hit target is the same as the character
            if (hit.transform == character.transform)
            {
                return;
            }

            float damageMultiplier = 1;

            // Handle damageable groups
            if (hit.transform.TryGetComponent(out IDamageableGroup damageableGroup))
            {
                damageMultiplier = damageableGroup.GetDamageMultipler() * firearmAttachmentsManager.damage;
            }

            IDamageable damageable = hit.transform.SearchFor<IDamageable>();

            // Handle damageable objects
            if (damageable != null && damageable.health > 0)
            {
                float totalDamage = damage * damageMultiplier;
                
                damageable.Damage(totalDamage, actor.gameObject);

                bool shouldHighlight = damageable.health <= damageable.maxHealth * 0.3f;

                if (firearm.character != null)
                {
                    UIManager uiManager = UIManager.Instance;

                    if (uiManager != null)
                    {
                        Hitmarker hitmarker = uiManager.Hitmarker;
                        hitmarker?.Show(shouldHighlight);
                    }
                }
            }

            // Handle custom decals
            if (hit.transform.TryGetComponent(out CustomDecal customDecal))
            {
                defaultDecal = customDecal.decalVFX;
            }

            // Exit if the hit target is the same as the character manager
            if (firearm?.characterManager?.transform == hit.transform)
            {
                return;
            }

            // Apply default or custom decal
            if (defaultDecal != null)
            {
                Vector3 hitPoint = hit.point;
                Quaternion decalRotation = FPSFrameworkCore.GetFromToRotation(hit, decalDirection);
                GameObject decalInstance = Instantiate(defaultDecal, hitPoint, decalRotation);

                decalInstance.transform.localScale *= firearm.preset.decalSize;
                decalInstance.transform.SetParent(hit.transform);

                float decalLifetime = customDecal?.lifeTime ?? 60f;
                Destroy(decalInstance, decalLifetime);
            }

            // Apply force to the rigidbody if present
            if (hit.rigidbody != null)
            {
                float impactForce = firearm.preset.shotDelay <= 0f
                    ? (firearm.preset.impactForce / firearm.preset.shotCount)
                    : firearm.preset.impactForce;

                hit.rigidbody.AddForceAtPosition(-hit.normal * impactForce, hit.point, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Invokes hit-related callbacks on the hit object, as well as its children and parents, if applicable.
        /// </summary>
        /// <param name="sourcePlayer">The player responsible for the hit.</param>
        /// <param name="ray">The ray that caused the hit.</param>
        /// <param name="hit">Information about the hit.</param>
        public static void InvokeHitCallbacks(GameObject sourcePlayer, Ray ray, RaycastHit hit)
        {
            if (hit.transform.GetComponent<IgnoreHitDetection>()) return;

            // Create a HitInfo object to store details about the hit
            HitInfo hitInfo = new HitInfo(sourcePlayer, hit, ray.origin, ray.direction);

            // Retrieve the GameObject that was hit
            GameObject obj = hit.transform.gameObject;

            // Try to get the IOnAnyHit interface implementation on the hit object, its children, and its parent
            IOnAnyHit onAnyHit = obj.transform.GetComponent<IOnAnyHit>();
            IOnAnyHitInChildren onAnyHitInChildren = obj.transform.GetComponent<IOnAnyHitInChildren>();
            IOnAnyHitInParent onAnyHitInParent = obj.transform.GetComponent<IOnAnyHitInParent>();

            // Try to get the IOnHit interface implementation on the hit object, its children, and its parent
            IOnHit onHit = obj.transform.GetComponent<IOnHit>();
            IOnHitInChildren onHitInChildren = obj.transform.GetComponentInChildren<IOnHitInChildren>();
            IOnHitInParent onHitInParent = obj.transform.GetComponentInParent<IOnHitInParent>();

            // Invoke the OnHit method on the IOnHit interface, if implemented
            onHit?.OnHit(hitInfo);

            // Invoke the OnHitInChildren method on the IOnHitInChildren interface, if implemented
            onHitInChildren?.OnHitInChildren(hitInfo);

            // Invoke the OnHitInParent method on the IOnHitInParent interface, if implemented
            onHitInParent?.OnHitInParent(hitInfo);

            // Invoke the OnAnyHit method on the IOnAnyHit interface, if implemented
            onAnyHit?.OnAnyHit(hitInfo);

            // Invoke the OnAnyHitInChildren method on the IOnAnyHitInChildren interface, if implemented
            onAnyHitInChildren?.OnAnyHitInChildren(hitInfo);

            // Invoke the OnAnyHitInParent method on the IOnAnyHitInParent interface, if implemented
            onAnyHitInParent?.OnAnyHitInParent(hitInfo);
        }

        /// <summary>
        /// Instantiates and ejects a casing from the firearm, applying velocity and optional chambering effects.
        /// </summary>
        public virtual void ThrowCasing()
        {
            // Ensure the onCasingThrown is invoked before exiting the method.
            // This allows external scripts to invoke the casing throw function, 
            // which can be used for networked casing spawning (e.g., for network code integration).
            onCasingThrown?.Invoke();

            //Exit if throw casing is false
            if (!isThrowingCasingActive) return;

            // Exit if casing prefab or ejection port is not assigned
            if (!preset.casing)
            {
                Debug.LogError("Casing prefab is not assigned in the preset.", preset);
                return;
            }

            if (!casingEjectionPort)
            {
                Debug.LogError("Casing ejection port is not assigned.", gameObject);
                return;
            }

            // Instantiate the casing at the ejection port's position and rotation
            GameObject newCasing = Instantiate(preset.casing, casingEjectionPort.position, casingEjectionPort.rotation);
            Rigidbody casingRigidbody = newCasing.GetComponent<Rigidbody>();

            if (casingRigidbody == null)
            {
                Debug.LogError("Failed to get Rigidbody component from instantiated casing.", gameObject);
                return;
            }

            // Get the velocity of the ejection port's Speedometer, if available
            Speedometer speedometer = casingEjectionPort.GetComponent<Speedometer>();
            Vector3 speedometerVelocity = speedometer ? speedometer.velocity : Vector3.zero;

            // Calculate the casing's velocity based on preset direction, speed, and a random factor
            Vector3 casingDirection = transform.GetDirection(preset.casingDirection);

            float randomFactor = UnityEngine.Random.Range(0.6f, 1f);

            Vector3 casingVelocity = casingDirection * preset.casingVelocity * randomFactor + speedometerVelocity;

            // Apply the calculated velocity to the casing's Rigidbody
            casingRigidbody.velocity = casingVelocity;

            // Destroy the casing object after 5 seconds
            Destroy(casingRigidbody.gameObject, 5f);

            // Play chambering effects if assigned
            if (chamberingEffects)
            {
                chamberingEffects.Play();
            }
        }

        /// <summary>
        /// Applies recoil effects to the weapon, including playing recoil animations and adjusting camera recoil based on the current settings.
        /// </summary>
        private void ApplyRecoil()
        {
            // Play recoil animations if the procedural animator is assigned
            if (proceduralAnimator)
            {
                recoilAnimation?.Play(0); // Play recoil animation from the beginning
                recoilAimAnimation?.Play(0); // Play recoil aim animation from the beginning
            }

            // Apply camera recoil adjustments if the character manager is assigned
            if (characterManager)
            {
                // Calculate the recoil values based on the preset and firearm attachments, considering aiming state
                float verticalRecoil = preset.verticalRecoil * firearmAttachmentsManager.recoil;
                float horizontalRecoil = preset.horizontalRecoil * firearmAttachmentsManager.recoil;
                float cameraRecoil = preset.cameraRecoil * firearmAttachmentsManager.recoil;

                // Apply the calculated recoil to the camera manager
                characterManager.cameraManager.ApplyRecoil(verticalRecoil, horizontalRecoil, cameraRecoil, itemInput.AimInput);
            }
        }

        /// <summary>
        /// Initiates the reloading process if the weapon is ready and ammo is available.
        /// </summary>
        public void Reload()
        {
            // Exit early if the weapon is not ready to reload
            if (!readyToReload)
            {
                return;
            }

            // Check if there is ammo available to reload
            if (ammoProfile.count > 0)
            {
                // Reset the MagThrown flag if WeaponEvents component exists
                var weaponEvents = GetComponentInChildren<WeaponEvents>();
                if (weaponEvents != null)
                {
                    weaponEvents.MagThrown = false;
                }

                // Set reloading state and trigger reload event
                isReloading = true;
                events.OnReload?.Invoke();

                // Start the reload process
                StartReload();
            }
            else
            {
                // No ammo available, so set reloading state to false
                isReloading = false;
            }
        }

        /// <summary>
        /// Starts the reload animation or timer based on the reload method specified in the preset.
        /// </summary>
        private void StartReload()
        {
            // Use scripted reload method if specified
            if (preset.reloadMethod == ReloadType.Scripted)
            {
                foreach (var animator in animators)
                {
                    animator.CrossFade(preset.reloadStateName, preset.reloadTransitionTime, 0, 0f);
                }

                isReloading = true;
                return;
            }

            // Use default reload timing based on remaining ammo
            float reloadTime = remainingAmmoCount <= 0 ? preset.emptyReloadTime : preset.reloadTime;

            Invoke(nameof(ApplyReload), reloadTime);

            //Play normal reloading sounds
            if (isAudioActive && remainingAmmoCount > 0)
            {
                currentFireAudio?.DisableEvents();
                reloadAudio?.PlayOneShot();
            }

            //Play empty mag reloading sounds
            if (isAudioActive && remainingAmmoCount <= 0)
            {
                currentFireAudio?.DisableEvents();
                reloadEmptyAudio?.PlayOneShot();
            }
        }

        /// <summary>
        /// Applies the reload by adjusting ammo counts based on the reload method and available ammo.
        /// </summary>
        public void ApplyReload()
        {
            // Exit if there is no ammo available
            if (ammoProfile.count <= 0)
            {
                return;
            }

            // Apply default reload method
            if (preset.reloadMethod == ReloadType.Default)
            {
                int ammoNeeded = magazineCapacity - remainingAmmoCount;
                int ammoToReload = Mathf.Min(ammoProfile.count, ammoNeeded);

                if (ammoProfile.identifier.displayName != "No Ammo Data")
                {
                    ammoProfile.count -= ammoToReload;
                }

                remainingAmmoCount += ammoToReload;
            }

            // Mark reloading as complete
            isReloading = false;
        }

        /// <summary>
        /// Applies a specified amount of reload and updates ammo counts. Adjusts ammoProfile and remainingAmmoCount accordingly.
        /// </summary>
        /// <param name="amount">The amount of ammo to add to the magazine.</param>
        public void ApplyReloadOnce(int amount = 1)
        {
            // Exit if there is no ammo available
            if (ammoProfile.count <= 0)
            {
                isReloading = false;
                return;
            }

            // Apply reload incrementally
            ammoProfile.count -= amount;
            remainingAmmoCount += amount;

            ammoProfile.count = Mathf.Clamp(ammoProfile.count, 0, int.MaxValue);

            //Play normal reloading sounds
            if (isAudioActive && remainingAmmoCount > 0)
            {
                currentFireAudio?.DisableEvents();
                reloadAudio?.PlayOneShot();
            }
        }

        /// <summary>
        /// Cancels the current reload process, resetting any ongoing reload animation and state.
        /// </summary>
        public void CancelReload()
        {
            // Exit if not currently reloading
            if (!isReloading)
            {
                return;
            }

            // Cancel any ongoing reload process
            CancelInvoke(nameof(ApplyReload));

            isReloading = false;

            // Reset the reload animation state
            foreach (var animator in animators)
            {
                animator.SetBool("Is Reloading", false);
            }

            events.OnReloadCancel?.Invoke();
        }

        /// <summary>
        /// Calculates the spread pattern for the given direction based on the current aiming state.
        /// </summary>
        /// <param name="direction">The direction to calculate the spread for.</param>
        /// <returns>A <see cref="Vector3"/> representing the calculated spread pattern.</returns>
        public Vector3 GetSprayPattern(Vector3 direction)
        {
            if (isAiming)
            {
                return aimDownSightsSprayPattern.CalculatePattern(this, direction, currentSprayMultiplier, currentSprayAmount);
            }

            return hipFireSprayPattern.CalculatePattern(this, direction, currentSprayMultiplier, currentSprayAmount);
        }

        /// <summary>
        /// Checks if any of the animators are currently playing a restricted animation.
        /// </summary>
        /// <returns>
        /// <c>true</c> if any animator is playing a restricted animation; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPlayingRestrictedAnimation()
        {
            // Initialize result as false
            bool isPlayingRestricted = false;

            // Iterate through all animators
            foreach (Animator animator in base.animators)
            {
                // Check if the animator is playing any restricted animation
                foreach (string restrictedAnimation in this.preset.restrictedAnimations)
                {
                    // If the animator is playing a restricted animation, update the result and break out of the loop
                    if (animator.IsPlaying(restrictedAnimation))
                    {
                        isPlayingRestricted = true;
                        break; // Exit the inner loop to avoid redundant checks
                    }
                }

                // If a restricted animation is found, exit the outer loop
                if (isPlayingRestricted)
                {
                    break;
                }
            }

            // Return whether a restricted animation is being played
            return isPlayingRestricted;
        }



        private void OnEnable()
        {
            // Cancel any ongoing reload actions
            CancelReload();
        }


        private void OnDisable()
        {
            // Cancel any ongoing reload actions
            CancelReload();

            // Check if a valid preset is provided
            if (character == null)
            {
                return;
            }

            if(characterManager)
            {
                characterManager.attemptingToAim = false;
                characterManager.isAiming = false;
            }

            character.ResetSpeed();
        }

        private void OnDestroy()
        {
            // Check if the character reference is null
            if (character == null)
            {
                return;
            }

            // Reset the character's speed
            character.ResetSpeed();
        }

        [ContextMenu("Setup/Network Components")]
        private void SetupNetworkComponents()
        {
#if UNITY_EDITOR
            FPSFrameworkEditor.InvokeConvertMethod("ConvertFirearm", this, new object[] { this });
#endif
        }
    }
}
