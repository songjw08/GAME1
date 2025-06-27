using UnityEngine;

namespace Akila.FPSFramework
{
    [CreateAssetMenu(fileName = "New Spray Pattern", menuName = "Akila/FPS Framework/Weapons/Spray Pattern")]
    public class SprayPattern : ScriptableObject
    {
        // Maximum spread amount
        public float totalAmount = 5f;

        // Multiplier applied when not shooting
        [Range(0, 1)]
        public float passiveMultiplier = 0.1f;

        // Time taken to ramp up the spread
        public float rampUpTime = 0.1f;

        // Time taken to reset the spread back to passive
        public float recoveryTime = 0.05f;

        // Toggle for randomizing the pattern
        public bool isRandomized = true;

        [Space]

        // Vertical and horizontal recoil curves
        public AnimationCurve verticalRecoil = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(1, 0));

        public AnimationCurve horizontalRecoil = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(1, 0));

        public void RampupMagnitude(ref float value, ref float velocity)
        {
            if (value >= 0.99f)
            {
                velocity = 0;
                value = 0;
            }

            value = Mathf.SmoothDamp(value, 1, ref velocity, rampUpTime);
        }

        public Vector3 CalculatePattern(Firearm firearm, Vector3 direction, float curvePosition, float amount = -1)
        {
            float finalAmount = amount >= 0 ? amount : totalAmount;

            

            Vector3 recoilOffset = Vector3.zero;

            if (isRandomized)
            {
                recoilOffset += Random.insideUnitSphere;
            }
            else
            {
                recoilOffset.x += horizontalRecoil.Evaluate(curvePosition);
                recoilOffset.y += verticalRecoil.Evaluate(curvePosition);

                recoilOffset.z += recoilOffset.x * recoilOffset.y;
            }

            // Calculate final direction with applied spread
            return Vector3.Slerp(direction, recoilOffset, amount * firearm.firearmAttachmentsManager.spread / 180f);
        }

        /// <summary>
        /// Resets the current spread magnitude towards the passive multiplier.
        /// </summary>
        public void ResetMagnitude(ref float currentValue, ref float currentVelocity)
        {
            currentValue = Mathf.SmoothDamp(currentValue, passiveMultiplier, ref currentVelocity, recoveryTime);
        }
    }

}