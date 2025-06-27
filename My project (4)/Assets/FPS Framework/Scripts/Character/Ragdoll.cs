using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Ragdoll")]
    public class Ragdoll : MonoBehaviour
    {
        public Rigidbody[] rigidbodies { get; set; }
        public Animator animator;

        private ICharacterController Controller;

        private void Start()
        {
            Controller = GetComponent<ICharacterController>();
            rigidbodies = GetComponentsInChildren<Rigidbody>();
            if (!animator) animator = transform.SearchFor<Animator>();

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }

            Disable();
        }

        public void Enable(Vector3 damageForce)
        {
            if (animator) animator.enabled = false;
            this.damageForce = damageForce;

            foreach (Rigidbody rigidbody in rigidbodies)
            {
                if (rigidbody != GetComponent<Rigidbody>())
                {
                    rigidbody.isKinematic = false;
                }
            }

            ApplyForces();
        }

        private Vector3 damageForce;

        private void ApplyForces()
        {
            foreach (Rigidbody rigidbody in rigidbodies)
            {
                if (rigidbody != GetComponent<Rigidbody>())
                {
                    rigidbody.isKinematic = false;
                    rigidbody.velocity += damageForce / rigidbodies.Length;
                }
            }
        }

        public void Disable()
        {
            if (animator) animator.enabled = true;

            foreach (Rigidbody rigidbody in rigidbodies)
            {
                if (rigidbody != GetComponent<Rigidbody>())
                {
                    rigidbody.isKinematic = true;
                }
            }
        }
    }
}