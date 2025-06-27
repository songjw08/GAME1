using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [ExecuteAlways]
    [AddComponentMenu("Akila/FPS Framework/Utility/Copy Transform")]
    public class CopyTransform : MonoBehaviour
    {
        public UpdateMode updateMode;
        public Transform target;
        [Space]
        public bool position;
        public bool rotation;

        [Space]
        public float positionTime = 0.1f;
        public float rotationTime = 0.1f;

        private Vector3 currentPosTime;
        private float currentRotTime;

        private void Update()
        {
            if (updateMode == UpdateMode.Update) Copy();
        }

        private void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate) Copy();
        }

        private void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate) Copy();
        }

        private float interpolationProgress;

        private void Copy()
        {
            if (position)
                transform.position = Vector3.SmoothDamp(transform.position, target.position, ref currentPosTime, positionTime);

            if (rotation)
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * (1 / rotationTime));
        }
    }

    public enum UpdateMode
    {
        Update,
        FixedUpdate,
        LateUpdate
    }
}