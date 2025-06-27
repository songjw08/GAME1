using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.Events;
using Akila.FPSFramework.Internal;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/First Person Controller")]
    [RequireComponent(typeof(CharacterManager))]
    [RequireComponent(typeof(CharacterController), typeof(CharacterInput))]
    public class FirstPersonController : MonoBehaviour, ICharacterController
    {
        [Header("Movement")]
        [Tooltip("The amount of time needed to walk or sprint in full speed.")]
        public float acceleration = 0.1f;
        [Tooltip("The amount of meters to move per second while walking.")]
        public float walkSpeed = 5;
        [Tooltip("The amount of meters to move per second while crouching.")]
        public float crouchSpeed = 3;
        [Tooltip("The amount of meters to move per second while sprinting.")]
        public float sprintSpeed = 10;
        [Tooltip("The amount of meters to move per second while tactical walking.")]
        public float tacticalSprintSpeed = 11;
        [Tooltip("The amount of force applied when jumping.")]
        public float jumpHeight = 6;
        [Tooltip("Player height while crouching.")]
        public float crouchHeight = 1.5f;
        [Tooltip("The amount of update calles in order to perform one step.")]
        public float stepInterval = 7;

        [Header("Slopes")]
        public bool slideDownSlopes = true;
        public float slopeSlideSpeed = 1;

        [Space]
        [Tooltip("Force multiplier from Physics/Gravity.")]
        public float gravity = 1;
        [Tooltip("Max speed the player can reach while falling")]
        public float maxFallSpeed = 350;
        [Tooltip("Force multiplier from Physics/Gravity when grounded")]
        public float stickToGroundForce = 0.5f;

        [Header("Camera")]
        [Tooltip("Camera or camera holder which will rotate when rotating view.")]
        public Transform _Camera;
        [Tooltip("Max angle of view rotation.")]
        public float maximumX = 90f;
        [Tooltip("Min angle of view rotation.")]
        public float minimumX = -90f;
        [Tooltip("Camera offset from the player.")]
        public Vector3 offset = new Vector3(0, -0.05f, 0);

        [Tooltip("Locks and reset cursor on start")]
        public bool lockCursor = true;
        public bool globalOrientation = false;

        [Header("Sensitivity")]
        [Header("Sensitivity")]
        [Tooltip("Sensitivity of camera movement while using mouse.")]
        public float sensitivityOnMouse = 1;
        [Tooltip("Sensitivity of camera movement while using Gamepad.")]
        public float sensitivityOnGamepad = 1;
        [Tooltip("Defines how camera sensitivity changes based on the field of view (FOV).")]
        public AnimationCurve fovToSensitivityCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
        [Tooltip("Toggle dynamic camera sensitivity adjustment based on the field of view (FOV).")]
        public bool isDynamicSensitivityEnabled = true;

        [Header("Audio")]
        [Tooltip("(optional) Footsteps list to play a random sound clip from while walking.")]
        public AudioProfile[] footstepsSFX;
        [Tooltip("(optional) Sound of jumping.")]
        public AudioProfile jumpSFX;
        [Tooltip("(optional) Sound of landing.")]
        public AudioProfile landSFX;

        public CollisionFlags CollisionFlags { get; set; }
        public CharacterController controller { get; set; }
        public CameraManager cameraManager { get; set; }
        public Actor Actor { get; set; }
        public CharacterManager characterManager { get; set; }

        public CharacterInput CharacterInput { get; private set; }
        public PlayerInput playerInput { get; set; }

        //input velocity
        private Vector3 desiredVelocityRef;
        private Vector3 desiredVelocity;
        private Vector3 slideVelocity;

        //out put velocity
        private Vector3 velocity;

        public Transform Orientation { get; set; }
        public float tacticalSprintAmount { get; set; }
        public bool canTacticalSprint { get; set; }
        float ICharacterController.sprintSpeed { get => sprintSpeed; }
        float ICharacterController.walkSpeed { get => walkSpeed; }
        float ICharacterController.tacticalSprintSpeed { get => tacticalSprintSpeed; }

        float ICharacterController.sensitivity
        {
            get
            {
                if(FPSFrameworkCore.GetActiveControlScheme() == ControlScheme.Gamepad)
                {
                    return sensitivityOnGamepad;
                }

                return sensitivityOnMouse * 0.1f;
            }
        }

        private Vector3 slopeDirection;

        private float yRotation;
        private float xRotation;

        private float speed;

        private float defaultHeight;
        private float defaultstepOffset;

        private float stepCycle;
        private float nextStep;

        public List<Audio> footStepsAudio = new List<Audio>();
        public Audio jumpAudio;
        public Audio landAudio;

        [Space]
        public UnityEvent<int> onStep = new UnityEvent<int>();
        public UnityEvent onJump = new UnityEvent();
        public UnityEvent onLand = new UnityEvent();

        public bool isCrouching { get; set; }

        public bool isActive { get; protected set; } = true;

        bool ICharacterController.isDynamicSensitivityEnabled => isDynamicSensitivityEnabled;

        AnimationCurve ICharacterController.fovToSensitivityCurve => fovToSensitivityCurve;

        public float currentGravityForce { get; protected set; }

        private Quaternion cameraRotation;
        private Quaternion playerRotation;


        protected virtual void Awake()
        {
            characterManager = GetComponent<CharacterManager>();
            playerInput = GetComponent<PlayerInput>();
            CharacterInput = GetComponent<CharacterInput>();
            Actor = GetComponent<Actor>();
            cameraManager = GetComponentInChildren<CameraManager>();



            if (GetComponentInChildren<IInventory>() != null) GetComponentInChildren<IInventory>().characterManager = characterManager;

            if (transform.Find("Orientation") != null)
            {
                Orientation = transform.Find("Orientation");
            }
            else
            {
                Orientation = new GameObject("Orientation").transform;
                Orientation.parent = transform;
                Orientation.localPosition = Vector3.zero;
                Orientation.localRotation = Quaternion.identity;
            }

            characterManager.orientation = Orientation;
            characterManager.Setup(Actor, controller, cameraManager, Orientation);
        }

        private void OnEnable()
        {
            footStepsAudio.Clear();

            foreach (AudioProfile profile in footstepsSFX)
            {
                if (profile != null)
                {
                    Audio newAudio = new Audio();
                    newAudio.Setup(this, profile);

                    footStepsAudio.Add(newAudio);
                }
            }

            if (jumpSFX)
            {
                jumpAudio = new Audio();
                jumpAudio.Setup(this, jumpSFX);
            }

            if (landSFX)
            {
                landAudio = new Audio();
                landAudio.Setup(this, landSFX);
            }
        }

        protected virtual void Start()
        {
            if (!_Camera) _Camera = GetComponentInChildren<Camera>().transform;

            //setup nesscary values
            controller = GetComponent<CharacterController>();

            ResetSpeed();

            //get defaults
            defaultHeight = controller.height;
            defaultstepOffset = controller.stepOffset;
            controller.skinWidth = controller.radius / 10;

            //hide and lock cursor if there is no pause menu in the scene
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            characterManager.onLand.AddListener(PlayLandSFX);
        }


        protected virtual void Update()
        {
            if (!isActive) return;

            //slide down slope if on maxed angle slope
            if (slideDownSlopes && OnMaxedAngleSlope())
                slideVelocity += new Vector3(slopeDirection.x, -slopeDirection.y, slopeDirection.z) * slopeSlideSpeed * Time.deltaTime;
            else
                //reset velocity if not on slope
                slideVelocity = Vector3.zero;

            Vector3 targetVelocity = (SlopeDirection() * CharacterInput.moveInput.y + Orientation.right * CharacterInput.moveInput.x).normalized * speed;

            //update desiredVelocity in order to normlize it and smooth the movement
            desiredVelocity = slideVelocity + Vector3.SmoothDamp(desiredVelocity, targetVelocity * CharacterInput.moveInput.magnitude, ref desiredVelocityRef, acceleration);


            if (!controller.isGrounded || OnSlope())
            {
                controller.stepOffset = 0;
            }
            else
            {
                controller.stepOffset = defaultstepOffset;
            }

            //copy desiredVelocity x, z with normlized values
            velocity.x = (desiredVelocity.x);
            velocity.z = (desiredVelocity.z);

            //update speed according to if player is holding sprint
            if (CharacterInput.sprintInput && !CharacterInput.tacticalSprintInput) speed = isCrouching ? crouchSpeed * speedMultiplier : sprintSpeed * speedMultiplier;
            else if (!CharacterInput.tacticalSprintInput) speed = speed = isCrouching ? crouchSpeed * speedMultiplier : walkSpeed * speedMultiplier;

            if (CharacterInput.tacticalSprintInput) speed = speed = isCrouching ? crouchSpeed * speedMultiplier : tacticalSprintSpeed * speedMultiplier;

            //Do crouching
            isCrouching = CharacterInput.crouchInput;
            ApplyCrouching();

            //update gravity and jumping
            if (controller.isGrounded)
            {
                //set small force when grounded in order to staplize the controller
                currentGravityForce = Physics.gravity.y * stickToGroundForce;


                //check jumping input
                if (CharacterInput.jumpInput)
                {
                    onJump?.Invoke();

                    //update velocity in order to jump
                    currentGravityForce += jumpHeight + (-Physics.gravity.y * gravity * stickToGroundForce);

                    //play jump sound
                    if (jumpSFX)
                        jumpAudio.PlayOneShot();
                }
                
                velocity.y = currentGravityForce;
            }
            else if (velocity.magnitude * 3.5f < maxFallSpeed)
            {
                //add gravity
                currentGravityForce += Physics.gravity.y * gravity * Time.deltaTime;
                velocity.y = currentGravityForce;
            }

            _Camera.position = transform.position + ((Vector3.up * (controller.height - 1) + offset));

            //move and update CollisionFlags in order to check if collition is coming from above ot center or bottom
            CollisionFlags = controller.Move(velocity * Time.deltaTime);

            //rotate camera
            UpdateCameraRotation();

            tacticalSprintAmount = CharacterInput.tacticalSprintInput ? 1 : 0;

            MoveWithMovingPlatforms();
        }

        public void ApplyCrouching()
        {
            //set controller height according to if player is crouching
            controller.height = isCrouching ?
            Mathf.Lerp(controller.height, crouchHeight, Time.deltaTime * 15) :
            Mathf.Lerp(controller.height, defaultHeight, Time.deltaTime * 15);
        }

        public virtual void PlayLandSFX()
        {
            onLand?.Invoke();

            if (landSFX)
                landAudio.PlayOneShot();
        }

        public virtual void FixedUpdate()
        {
            //update step sounds
            ProgressStepCycle();
        }

        protected virtual void ProgressStepCycle()
        {
            //stop if not grounded
            if (!controller.isGrounded || footstepsSFX.Length <= 0) return;

            //check if taking input and input
            if (controller.velocity.sqrMagnitude > 0 && (CharacterInput.moveInput.x != 0 || CharacterInput.moveInput.y != 0))
            {
                //update step cycle
                stepCycle += (controller.velocity.magnitude + (controller.velocity.magnitude * (!characterManager.IsVelocityZero() ? 1f : 1))) * Time.fixedDeltaTime;
            }

            //check step cycle not equal to next step in order to update right
            if (!(stepCycle > nextStep))
            {
                return;
            }

            //update
            nextStep = stepCycle + stepInterval;
           
            int currentFootStepIndex = Random.Range(0, footStepsAudio.GetLength());

            onStep?.Invoke(currentFootStepIndex);

            if (footstepsSFX != null)
            {
                Audio currentFootStepAudio = footStepsAudio[currentFootStepIndex];

                currentFootStepAudio.PlayOneShot();
            }
        }

        protected virtual void UpdateCameraRotation()
        {
            if (prevCamRotation != _Camera.rotation) OnCameraRotationUpdated();

            yRotation += CharacterInput.lookInput.x;
            xRotation -= CharacterInput.lookInput.y;

            xRotation = Mathf.Clamp(xRotation, minimumX, maximumX);

            cameraRotation = Quaternion.Slerp(cameraRotation, Quaternion.Euler(xRotation, yRotation, 0), Time.deltaTime * 100);
            playerRotation = Quaternion.Slerp(playerRotation, Quaternion.Euler(0, yRotation, 0), Time.deltaTime * 100);

            Orientation.SetRotation(playerRotation, !globalOrientation);
            _Camera.SetRotation(cameraRotation, !globalOrientation);

            prevCamRotation = _Camera.rotation;
        }

        private Quaternion prevCamRotation;

        protected virtual void OnCameraRotationUpdated() { }

        public virtual bool OnSlope()
        {
            //check if slope angle is more than 0
            if (SlopeAngle() > 0)
            {
                return true;
            }

            return false;
        }

        public virtual bool OnMaxedAngleSlope()
        {
            if (controller.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height))
            {
                slopeDirection = hit.normal;
                return Vector3.Angle(slopeDirection, Vector3.up) > controller.slopeLimit;
            }

            return false;
        }

        public virtual Vector3 SlopeDirection()
        {
            //setup a raycast from position to down at the bottom of the collider
            RaycastHit slopeHit;
            if (Physics.Raycast(Orientation.position, Vector3.down, out slopeHit, (controller.height / 2) + 0.1f))
            {
                //get the direction result according to slope normal
                return Vector3.ProjectOnPlane(Orientation.forward, slopeHit.normal);
            }

            //if not on slope then slope is forward ;)
            return Orientation.forward;
        }

        public virtual float SlopeAngle()
        {
            //setup a raycast from position to down at the bottom of the collider
            RaycastHit slopeHit;
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit))
            {
                //get the direction result according to slope normal
                return (Vector3.Angle(Vector3.down, slopeHit.normal) - 180) * -1;
            }

            //if not on slope then slope is forward ;)
            return 0;
        }
        private float speedMultiplier = 1;

        public virtual void SetSpeed(float speedMultiplier)
        {
            this.speedMultiplier = speedMultiplier;
        }

        public virtual void ResetSpeed()
        {
            speedMultiplier = 1;
        }

        public virtual bool MaxedCameraRotation()
        {
            return xRotation < -90 + 1 || xRotation > 90 - 1;
        }

        public void SetActive(bool value)
        {
            isActive = value;

            //Set active for the camera
            Camera[] cameras = GetComponentsInChildren<Camera>();

            foreach (Camera cam in cameras)
            {
                cam.enabled = value;
            }

            //Set active for the audio listener
            AudioListener[] audioListeners = GetComponentsInChildren<AudioListener>();

            foreach (AudioListener audioListener in audioListeners)
            {
                AudioEchoFilter echoFilter = audioListener.GetComponent<AudioEchoFilter>();
                AudioReverbFilter reverbFilter = audioListener.GetComponent<AudioReverbFilter>();
                AudioHighPassFilter highPassFilter = audioListener.GetComponent<AudioHighPassFilter>();
                AudioLowPassFilter lowPassFilter = audioListener.GetComponent<AudioLowPassFilter>();
                AudioDistortionFilter distortionFilter = audioListener.GetComponent<AudioDistortionFilter>();

                if(echoFilter) echoFilter.enabled = value;
                if(reverbFilter)reverbFilter.enabled = value;
                if(highPassFilter) highPassFilter.enabled = value;
                if(lowPassFilter) lowPassFilter.enabled = value;
                if(distortionFilter) distortionFilter.enabled = value;
                
                audioListener.enabled = value;
            }
        }

        protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
        {
            //if hit something while jumping from the above then go down again
            if (CollisionFlags == CollisionFlags.Above)
            {
                velocity.y = 0;
            }
        }

        private Vector3 feetPosition;
        private Vector3 totalVelocity;

        private void MoveWithMovingPlatforms()
        {
            // Calculate the position of the feet based on character height
            feetPosition = transform.position - (transform.up * (controller.height / 2));

            // Perform a sphere cast to detect surrounding objects
            RaycastHit[] hits = Physics.SphereCastAll(new Ray(feetPosition, Vector3.down), controller.radius, controller.radius / 2);


            // Loop through all hits to calculate total velocity from Speedometer components
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform != transform) // Ensure we're not processing the character itself
                {
                    Vector3 hitVelocity = GetTransformVelocity(hit.transform);

                    totalVelocity = hitVelocity;
                }
            }

            if(!controller.isGrounded)
            {
                //totalVelocity += Physics.gravity * gravity * Time.deltaTime;
            }

            // Move the character controller based on total velocity
            transform.position += totalVelocity;
        }

        private Vector3 GetTransformVelocity(Transform hitTransform)
        {
            Speedometer speedometer = hitTransform.GetComponent<Speedometer>();

            // If the Speedometer component exists, return its velocity
            if (speedometer != null)
            {
                return speedometer.velocity * Time.deltaTime; // Apply delta time for frame-rate independent movement
            }

            return Vector3.zero; // Return zero if no Speedometer is found
        }

        [ContextMenu("Setup/Network Components")]
        public void Convert()
        {
#if UNITY_EDITOR
            FPSFrameworkEditor.InvokeConvertMethod("ConvertFirstPersonController", this, new object[] { this });
#endif
        }
    }
}