using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace Akila.FPSFramework
{
    public interface ICharacterController
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }

        void SetSpeed(float speedMultiplier);
        void ResetSpeed();
        float sensitivity { get; }
        bool isDynamicSensitivityEnabled { get; }
        AnimationCurve fovToSensitivityCurve { get; }
        float sprintSpeed { get; }
        float walkSpeed { get; }
        float tacticalSprintSpeed { get; }
        float tacticalSprintAmount { get; }
        bool MaxedCameraRotation();
    }
}