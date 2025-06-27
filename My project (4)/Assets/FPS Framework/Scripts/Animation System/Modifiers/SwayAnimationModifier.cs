using UnityEngine;
using UnityEngine.InputSystem;

namespace Akila.FPSFramework.Animation
{
    public class SwayAnimationModifier : ProceduralAnimationModifier
    {
        public InputAction inputAction;
        public float positionSmoothness = 5;
        public float rotationSmoothness = 10;
        public bool disableOnPaused = true;

        [Header("Input X")]
        public Vector3 positionInputX;
        public Vector3 rotationInputX;

        [Header("Input Y")]
        public Vector3 positionInputY;
        public Vector3 rotationInputY;

        [Header("Limits")]
        public Vector3 positionLimit = Vector3.one;
        public Vector3 rotationLimit = Vector3.one;

        private Vector3 resultPosition;
        private Vector3 resultRotation;

        protected void Start()
        {
            inputAction.Enable();
        }

        protected void Update()
        {
            resultPosition = Vector3.zero;
            resultRotation = Vector3.zero;

            float inputX = inputAction.ReadValue<Vector2>().x / Time.deltaTime;
            float inputY = inputAction.ReadValue<Vector2>().y / Time.deltaTime;

            inputX *= 0.01f;
            inputY *= 0.01f;

            Vector3 resultPositionInputX = new Vector3(positionInputX.x * inputX, positionInputX.y * inputX, positionInputX.z * inputX);
            Vector3 resultRotationInputX = new Vector3(rotationInputX.x * inputX, rotationInputX.y * inputX, rotationInputX.z * inputX);

            Vector3 resultPositionInputY = new Vector3(positionInputY.x * inputY, positionInputY.y * inputY, positionInputY.z * inputY);
            Vector3 resultRotationInputY = new Vector3(rotationInputY.x * inputY, rotationInputY.y * inputY, rotationInputY.z * inputY);

            resultPosition += resultPositionInputX + resultPositionInputY;
            resultRotation += resultRotationInputX + resultRotationInputY;

            if (FPSFrameworkCore.IsPaused)
            {
                resultPosition = Vector3.zero;
                resultRotation = Vector3.zero;
            }

            resultPosition.x = Mathf.Clamp(resultPosition.x, -positionLimit.x, positionLimit.x);
            resultPosition.y = Mathf.Clamp(resultPosition.y, -positionLimit.y, positionLimit.y);
            resultPosition.z = Mathf.Clamp(resultPosition.z, -positionLimit.z, positionLimit.z);

            resultRotation.x = Mathf.Clamp(resultRotation.x, -rotationLimit.x, rotationLimit.x);
            resultRotation.y = Mathf.Clamp(resultRotation.y, -rotationLimit.y, rotationLimit.y);
            resultRotation.z = Mathf.Clamp(resultRotation.z, -rotationLimit.z, rotationLimit.z);

            targetPosition = Vector3.Lerp(targetPosition, resultPosition, Time.deltaTime * positionSmoothness * globalSpeed);
            targetRotation = Vector3.Lerp(targetRotation, resultRotation, Time.deltaTime * rotationSmoothness * globalSpeed);
        }

        private void FixedUpdate()
        {


        }
    }
}