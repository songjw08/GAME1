using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Akila.FPSFramework.Animation
{
    [AddComponentMenu("Akila/FPS Framework/Animation/Modifiers/Wave Modifier"), RequireComponent(typeof(ProceduralAnimation))]
    public class WaveAnimationModifier : ProceduralAnimationModifier
    {
        public float speed = 1;
        public float amount = 1;
        public WaveProfile position = new WaveProfile();
        public WaveProfile rotation = new WaveProfile();
        public bool syncWithAnimation;
        public float syncSpeed = 5;

        private float inSyncAmount = 1;

        private void Update()
        {
            targetPosition = position.result;
            targetRotation = rotation.result;

            if (syncWithAnimation)
            {
                if (targetAnimation.isPlaying)
                    inSyncAmount = Mathf.Lerp(inSyncAmount, 1, Time.deltaTime * syncSpeed);
                else
                    inSyncAmount = Mathf.Lerp(inSyncAmount, 0, Time.deltaTime * syncSpeed);
            }
            else
            {
                inSyncAmount = 1;
            }

            position.Update(speed * globalSpeed, amount * inSyncAmount);
            rotation.Update(speed * globalSpeed, amount * inSyncAmount);
        }

        [Serializable]
        public class WaveProfile
        {
            public Vector3 amount;
            public Vector3 speed = new Vector3(1, 1, 1);

            [HideInInspector]
            public Vector3 result;
            private Vector3 time;

            public void Update(float globalSpeed, float globalAmount)
            {
                time.x += Time.deltaTime * speed.x * globalSpeed;
                time.y += Time.deltaTime * speed.y * globalSpeed;
                time.z += Time.deltaTime * speed.z * globalSpeed;

                result.x = amount.x * speed.x * Mathf.Sin(time.x) * globalAmount;
                result.y = amount.y * speed.y * Mathf.Sin(time.y) * globalAmount;
                result.z = amount.z * speed.z * Mathf.Sin(time.z) * globalAmount;
            }
        }
    }
}