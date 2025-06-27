using Akila.FPSFramework.Animation;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Camera Manager")]
    public class CameraManager : MonoBehaviour
    {
        [Header("FOV Kick")]
        public float FOVKick = 5f;
        public float overlayFOVKick = 5f;
        public float FOVKickSmoothness = 10f;
        public Camera mainCamera;
        public Camera overlayCamera;

        [Header("Lean")]
        public float rotationAngle = 4f;
        public float offset = 0.35f;
        public float smoothness = 10f;

        [Header("Camera Shake")]
        public CameraShaker mainCameraShaker;
        public float mainCameraShakeMagnitude = 1.6f;
        public float cameraShakeRoughness = 7f;
        public float cameraShakeFadeInTime = 0.2f;
        public float cameraShakeFadeOutTime = 2f;

        [Header("Camera Recoil")]
        public float recoilDampTime = 10f;
        public Vector3 recoilAmount = new Vector3(-3f, 4f, 4f);

        [Header("Head Bob")]
        public float headbobAmount = 20f;
        public float headbobRotationAmount = 30f;

        private float headbobTimer;

        public CharacterManager CharacterManager { get; set; }
        [HideInInspector] public AudioFiltersManager audioFiltersManager;
        private float movementPercentage;
        [HideInInspector] public float fieldOfView;
        [HideInInspector] public float overlayFieldOfView;

        private CharacterInput characterInput;
        private SettingsManager settingsManager;
        private Vector3 currentRecoil;

        private float defaultFieldOfView;
        private float defaultOverlayFieldOfView;
        private float currentLeanAngle;
        private Vector3 leanRightPosition;
        private Vector3 leanLeftPosition;

        public bool UseFOVKick { get; set; } = true;
        public bool UseLean { get; set; } = true;
        public bool UseCameraShake { get; set; } = true;
        public bool UseCameraRecoil { get; set; } = true;
        public bool UseHeadbob { get; set; } = true;

        private Vector3 ResultPosition => ResultLeanPosition + HeadbobPosition;
        private Vector3 ResultRotation => ResultLeanRotation + ResultRecoilRotation + HeadbobRotation;

        public Vector3 ResultLeanPosition { get; set; }
        public Vector3 ResultLeanRotation { get; set; }
        public Vector3 ResultRecoilRotation { get; set; }
        public Vector3 HeadbobPosition { get; set; }
        public Vector3 HeadbobRotation { get; set; }

        public bool isLeaningRight { get; set; }
        public bool isLeaningLeft { get; set; }

        private ProceduralAnimator proceduralAnimator { get; set; }

        public bool isActive { get; set; } = true;

        private void Start()
        {
            InitializeCameras();
            InitializeLean();
            InitializeFieldOfView();

            proceduralAnimator = GetComponent<ProceduralAnimator>();
        }

        private void FixedUpdate()
        {
            if (UseCameraRecoil)
                ApplyRecoilDamping();

            Inventory inventory = transform.SearchFor<Inventory>();

            if (inventory != null)
            {
                if (inventory.items.Count <= 0)
                {
                    targetFOV = FPSFrameworkCore.FieldOfView;
                    targetOverlayFOV = FPSFrameworkCore.WeaponFieldOfView;
                }
            }
        }

        private void Update()
        {
            UpdateFieldOfView();
            UpdateMovementPercentage();
            HandleLean();
            if (UseHeadbob) UpdateHeadbob();

            transform.localPosition = ResultPosition;
            transform.localRotation = Quaternion.Euler(ResultRotation);
        }

        private void InitializeCameras()
        {
            mainCamera ??= Camera.main;
            CharacterManager = GetComponentInParent<CharacterManager>();
            audioFiltersManager = FindObjectOfType<AudioListener>().GetComponent<AudioFiltersManager>();
            characterInput = GetComponentInParent<CharacterInput>();
            settingsManager = FindObjectOfType<SettingsManager>();
        }

        private void InitializeLean()
        {
            if (UseLean)
            {
                leanRightPosition = new Vector3(offset, 0, 0);
                leanLeftPosition = new Vector3(-offset, 0, 0);
            }
        }

        private void InitializeFieldOfView()
        {
            if (settingsManager == null)
            {
                if (mainCamera)
                {
                    fieldOfView = mainCamera.fieldOfView;
                    defaultFieldOfView = mainCamera.fieldOfView;
                }

                if (overlayCamera)
                {
                    overlayFieldOfView = overlayCamera.fieldOfView;
                    defaultOverlayFieldOfView = overlayCamera.fieldOfView;
                }
            }
            else
            {
                if (mainCamera) mainCamera.fieldOfView = FPSFrameworkCore.FieldOfView;
                if (overlayCamera) overlayCamera.fieldOfView = FPSFrameworkCore.WeaponFieldOfView;
                fieldOfView = FPSFrameworkCore.FieldOfView;
                overlayFieldOfView = FPSFrameworkCore.WeaponFieldOfView;
            }

            targetFOV = fieldOfView;
            targetOverlayFOV = overlayFieldOfView;
        }

        private void ApplyRecoilDamping()
        {
            currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, 35f * Time.deltaTime);
            ResultRecoilRotation = Vector3.Slerp(ResultRecoilRotation, currentRecoil, recoilDampTime * Time.fixedDeltaTime);
        }

        private void UpdateFieldOfView()
        {
            fieldOfView = Mathf.Lerp(fieldOfView, targetFOV, Time.deltaTime * 10);
            overlayFieldOfView = Mathf.Lerp(overlayFieldOfView, targetOverlayFOV, Time.deltaTime * 10);

            if (settingsManager != null)
            {
                defaultFieldOfView = FPSFrameworkCore.FieldOfView;
                defaultOverlayFieldOfView = FPSFrameworkCore.WeaponFieldOfView;
            }

            if (mainCamera)
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, fieldOfView + FOVKickResult(), Time.deltaTime * FOVKickSmoothness);

            if (overlayCamera)
                overlayCamera.fieldOfView = Mathf.Lerp(overlayCamera.fieldOfView, overlayFieldOfView + OverlayFOVKickResult(), Time.deltaTime * FOVKickSmoothness);
        }

        private void UpdateMovementPercentage()
        {
            if (CharacterManager == null) return;

            movementPercentage = CharacterManager.velocity.magnitude / CharacterManager.character.sprintSpeed;
            movementPercentage = Mathf.Clamp(movementPercentage, 0, 1.3f);
        }

        private void HandleLean()
        {
            if (!UseLean) return;

            if (!isActive) return;

                isLeaningRight = characterInput.leanRightInput;
                isLeaningLeft = characterInput.leanLeftInput;
            

            if (isLeaningRight)
            {
                ResultLeanPosition = Vector3.Lerp(ResultLeanPosition, leanRightPosition, Time.deltaTime * smoothness);
                currentLeanAngle = Mathf.Lerp(currentLeanAngle, -rotationAngle, Time.deltaTime * smoothness);
            }
            else if (isLeaningLeft)
            {
                ResultLeanPosition = Vector3.Lerp(ResultLeanPosition, leanLeftPosition, Time.deltaTime * smoothness);
                currentLeanAngle = Mathf.Lerp(currentLeanAngle, rotationAngle, Time.deltaTime * smoothness);
            }
            else
            {
                ResultLeanPosition = Vector3.Lerp(ResultLeanPosition, Vector3.zero, Time.deltaTime * smoothness);
                currentLeanAngle = Mathf.Lerp(currentLeanAngle, 0, Time.deltaTime * smoothness);
            }

            ResultLeanRotation = new Vector3(0, 0, currentLeanAngle);
        }

        private float FOVKickResult()
        {
            if (!UseFOVKick) return 0;
            return movementPercentage > 0.8f ? FOVKick * movementPercentage * movementPercentage : 0;
        }

        private float OverlayFOVKickResult()
        {
            if (!UseFOVKick) return 0;
            return movementPercentage > 0.8f ? overlayFOVKick * movementPercentage : 0;
        }

        private void UpdateHeadbob()
        {
            headbobTimer += Time.deltaTime * CharacterManager.velocity.magnitude;

            float posX = (headbobAmount / 200f) * Mathf.Sin(headbobTimer);
            float posY = (headbobAmount / 200f) * Mathf.Sin(headbobTimer * 2f);
            float rotZ = (headbobRotationAmount / 200f) * Mathf.Sin(headbobTimer);
            float multiplier = CharacterManager.velocity.magnitude / CharacterManager.character.tacticalSprintSpeed;

            Vector3 posResult = new Vector3(posX, posY) * multiplier;
            Vector3 rotResult = new Vector3(0, 0, rotZ) * multiplier;

            if (!CharacterManager.IsVelocityZero() && CharacterManager.isGrounded)
            {
                HeadbobPosition = Vector3.Lerp(HeadbobPosition, posResult, Time.deltaTime * 5);
                HeadbobRotation = Vector3.Lerp(HeadbobRotation, rotResult, Time.deltaTime * 20);
            }
            else
            {
                HeadbobPosition = Vector3.Lerp(HeadbobPosition, Vector3.zero, Time.deltaTime * 5);
                HeadbobRotation = Vector3.Lerp(HeadbobRotation, Vector3.zero, Time.deltaTime * 5);
            }
        }

        public void ApplyRecoil(float vertical, float horizontal, float shakeMultiplier, bool isAiming = false)
        {
            if (!UseCameraRecoil) return;

            float multiplier = isAiming ? 1f : 0.7f;
            CharacterManager.AddLookValue(vertical * multiplier, horizontal * multiplier);
            currentRecoil += new Vector3(recoilAmount.x, Random.Range(-recoilAmount.y, recoilAmount.y), Random.Range(-recoilAmount.z, recoilAmount.z)) * multiplier * shakeMultiplier;
        }

        public void ShakeCameras(float multiplier)
        {
            if (UseCameraShake && mainCameraShaker != null)
                mainCameraShaker.Shake(mainCameraShakeMagnitude * multiplier, cameraShakeRoughness, cameraShakeFadeInTime, cameraShakeFadeOutTime);
        }

        public void ShakeCameras(float multiplier, float fadeOutTime)
        {
            if (UseCameraShake && mainCameraShaker != null)
                mainCameraShaker.Shake(mainCameraShakeMagnitude * multiplier, cameraShakeRoughness, cameraShakeFadeInTime, fadeOutTime);
        }

        public void ShakeCameras(float multiplier, float roughness, float fadeOutTime)
        {
            if (UseCameraShake && mainCameraShaker != null)
                mainCameraShaker.Shake(mainCameraShakeMagnitude * multiplier, roughness, cameraShakeFadeInTime, fadeOutTime);
        }

        float targetFOV;
        float targetOverlayFOV;

        public void SetFieldOfView(float main, float overlay, float t = 1)
        {
            targetFOV = Mathf.Lerp(defaultFieldOfView, main, t);
            targetOverlayFOV = Mathf.Lerp(defaultOverlayFieldOfView, overlay, t);
        }

        public void ResetFieldOfView()
        {
            targetFOV = defaultFieldOfView;
            targetOverlayFOV = defaultOverlayFieldOfView;
        }
    }
}
