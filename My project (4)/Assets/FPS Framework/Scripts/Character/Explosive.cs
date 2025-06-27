using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using Akila.FPSFramework.Internal;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Weapons/Explosive")]
    [RequireComponent(typeof(Rigidbody))]
    public class Explosive : MonoBehaviour, IDamageable, IOnHit
    {
        [Header("Base")]
        [HideInInspector] public ExplosionType type = ExplosionType.RayTracking;
        [HideInInspector] public LayerMask layerMask = -1;
        [HideInInspector] public float deathRadius = 10;
        [HideInInspector] public float damageRadius = 20;
        [HideInInspector] public float damage = 150;
        [HideInInspector] public float force = 7;
        [HideInInspector] public float delay = 5;
        [HideInInspector] public float friction = 1;

        [Header("Extras")]
        [HideInInspector] public bool ignoreGlobalScale = false;
        [HideInInspector] public bool sticky = false;
        [HideInInspector] public bool damageable = false;
         public float health = 25;
        [HideInInspector] public bool exlopeAfterDelay;
        [HideInInspector] public bool destroyOnExplode = true;
        [HideInInspector] public float clearDelay = 60;

        [Header("VFX")]
        [HideInInspector] public GameObject explosion;
        [HideInInspector] public GameObject craterDecal;
        public GameObject explosionEffect;
        public float explosionEffectForce = 1;
        [HideInInspector] public Vector3 explosionEffactOffcet;
        [HideInInspector] public Vector3 explosionEffactRotationOffset;

        [Space]
        [HideInInspector] public float explosionSize = 1;
        [HideInInspector] public float craterSize = 1;
        [HideInInspector] public float cameraShake = 1;

        [Header("Audio")]
        [HideInInspector] public bool audioLowPassFilter;
        [HideInInspector] public float lowPassCutoffFrequency = 1500;
        [HideInInspector] public float lowPassTime = 2f;
        [HideInInspector] public float lowPassSmoothness = 0.1f;

        [Header("Debug")]
        [HideInInspector] public bool debug;
        [HideInInspector] public bool ranges;
        [HideInInspector] public bool rays;

        private Rigidbody rb;

        public bool exploded { get; set; }
        public bool deadConfirmed { get; set; }
        public Vector3 damageDirection { get; set; }
        public float maxHealth { get; set; }
        public float scale { get { return ignoreGlobalScale ? 1 : transform.lossyScale.magnitude; } }

        float IDamageable.health { get => health; set => health = value; }
        public GameObject damageSource { get; set; }

        public bool isActive { get; set; } = true;

        public UnityEvent onDeath { get; }

        public Action onExplode;
        public Action<Transform, Vector3, Vector3, bool> onExplosionApplied;

        public GameObject sourcePlayer;

        private void Start()
        {
            maxHealth = health;
            if (exlopeAfterDelay) Explode(delay);
            
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (health <= 0) Explode();
        }

        public void Explode(float delay)
        {
            Invoke(nameof(DoExplode), delay);
        }

        //just to use invoke
        private void DoExplode() => Explode();

        public void AddEffects()
        {
            if (explosion)
            {
                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;

                pos = transform.position + explosionEffactOffcet;
                rot = transform.rotation * Quaternion.Euler(explosionEffactRotationOffset);

                GameObject newExplosion = Instantiate(explosion, pos, rot);
                newExplosion.transform.localScale *= explosionSize;
                Destroy(newExplosion, clearDelay);
            }

            if (craterDecal)
            {
                RaycastHit ray;
                if (Physics.Raycast(transform.position, Vector3.down, out ray, deathRadius * scale))
                {
                    GameObject newDecal = Instantiate(craterDecal, ray.point + Vector3.up * 0.01f, Quaternion.FromToRotation(Vector3.up, ray.normal));
                    newDecal.transform.localScale *= craterSize;
                    Destroy(newDecal, clearDelay);
                }
            }

            if (explosionEffect)
            {
                GameObject effect = Instantiate(explosionEffect, transform.position, transform.rotation);
                effect.SetActive(true);

                Destroy(effect, clearDelay);
            }
        }

        /// <summary>
        /// Tries to explode this explosive
        /// </summary>
        /// <param name="addEffects">If true the explosion will result in a crater on the ground and other effects.</param>
        public void Explode()
        {
            if (exploded) return;
            
            onExplode?.Invoke();
            
            exploded = true;

            Collider[] nearColliders = Physics.OverlapSphere(transform.position, deathRadius * scale, layerMask);
            Collider[] farColliders = Physics.OverlapSphere(transform.position, damageRadius * scale, layerMask);

            if (isActive)
                AddEffects();

            foreach (Collider collider in nearColliders)
            {
                var dir = -(transform.position - collider.transform.position);

                if (type == ExplosionType.RayTracking)
                {
                    if (Physics.Raycast(transform.position, dir, out RaycastHit hit))
                        ApplyExplosion(hit.transform, transform.position, dir, true);
                }

                if (type == ExplosionType.Standard)
                {
                    ApplyExplosion(collider.transform, transform.position, dir, true);
                }
            }

            foreach (Collider collider in farColliders)
            {
                var dir = -(transform.position - collider.transform.position);

                if (type == ExplosionType.RayTracking)
                {
                    if (Physics.Raycast(transform.position, dir, out RaycastHit hit))
                        ApplyExplosion(hit.transform, transform.position, dir);
                }

                if (type == ExplosionType.Standard)
                {
                    ApplyExplosion(collider.transform, transform.position, dir);
                    InvokeCallbacks(sourcePlayer, new RaycastHit(), transform.position, dir, collider.transform);
                }

                if (collider.transform.TryGetComponent(out CharacterManager controller) && isActive)
                {
                    float distanceFromPlayer = Vector3.Distance(transform.position, controller.transform.position);

                    float percentage = distanceFromPlayer / damageRadius * scale;

                    float inversedPercentage = Mathf.InverseLerp(1, 0, percentage);

                    if (distanceFromPlayer <= deathRadius * scale) percentage = 1;

                    controller.cameraManager.ShakeCameras(cameraShake * inversedPercentage);

                    if (GamepadManager.Instance) GamepadManager.Instance.BeginVibration(1, 1, 0.8f);
                    
                    if (audioLowPassFilter)
                    {
                        controller.audioFiltersManager.SetLowPass(lowPassCutoffFrequency * distanceFromPlayer / damageRadius * scale, 1000 * Time.deltaTime);
                        controller.audioFiltersManager.ResetLowPass(lowPassSmoothness * Time.deltaTime, lowPassTime);
                    }
                }
            }

            if (destroyOnExplode && isActive) Destroy(gameObject);
        }

        public void InvokeCallbacks(GameObject sourcePlayer, RaycastHit hit,Vector3 origin, Vector3 direction, Transform transform)
        {
            HitInfo hitInfo = new HitInfo(sourcePlayer, hit, origin, direction);

            //Get on any hit interface
            IOnAnyHit onAnyHit = transform.GetComponent<IOnAnyHit>();
            IOnAnyHitInChildren onAnyHitInChildren = transform.GetComponent<IOnAnyHitInChildren>();
            IOnAnyHitInParent onAnyHitInParent = transform.GetComponent<IOnAnyHitInParent>();

            //calls OnHit() for any object with IHitable interface
            IOnExplosionHit onHit = transform.GetComponent<IOnExplosionHit>();
            IOnExplosionHitInChildren onHitInChildren = transform.GetComponentInChildren<IOnExplosionHitInChildren>();
            IOnExplosionHitInParent onHitInParent = transform.GetComponentInParent<IOnExplosionHitInParent>();

            //calls OnHit() for any object with IHitable interface
            IOnRayTrackingExplosionHit rayTrackingOnHit = transform.GetComponent<IOnRayTrackingExplosionHit>();
            IOnRayTrackingExplosionHitInChildren rayTrackingOnHitInChildren = transform.GetComponentInChildren<IOnRayTrackingExplosionHitInChildren>();
            IOnRayTrackingExplosionHitInParent rayTrackingOnHitInParent = transform.GetComponentInParent<IOnRayTrackingExplosionHitInParent>();

            //Call on any hits
            if (onAnyHit != null) onAnyHit.OnAnyHit(hitInfo);
            if (onAnyHitInChildren != null && onAnyHit == null) onAnyHitInChildren.OnAnyHitInChildren(hitInfo);
            if (onAnyHitInParent != null && onAnyHit == null) onAnyHitInParent.OnAnyHitInParent(hitInfo);

            //Call on hit
            if (onHit != null) onHit.OnExplosionHit(hitInfo);
            if (onHitInChildren != null && onHit == null) onHitInChildren.OnExplosionHitInChildren(hitInfo);
            if (onHitInParent != null && onHit == null) onHitInParent.OnExplosionHitInParent(hitInfo);

            //Call on ray tracking hit
            if (rayTrackingOnHit != null) rayTrackingOnHit.OnRayTrackingExplosionHit(hitInfo);
            if (rayTrackingOnHitInChildren != null && rayTrackingOnHit == null) rayTrackingOnHitInChildren.OnRayTrackingExplosionHitInChildren(hitInfo);
            if (rayTrackingOnHitInParent != null && rayTrackingOnHit == null) rayTrackingOnHitInParent.OnRayTrackingExplosionHitInParent(hitInfo);
        }

        public void ApplyExplosion(Transform obj, Vector3 origin, Vector3 dir, bool kill = false)
        {
            onExplosionApplied?.Invoke(obj, origin, dir, kill);

            if (!isActive)
                return;

            if (obj != transform)
            {
                float finalDamage = damage;

                if (obj.SearchFor<IDamageable>() != null)
                {
                    IDamageable damageable = obj.SearchFor<IDamageable>();

                    float distanceFromDamageable = Vector3.Distance(origin, obj.position);
                    float damageMultiplier = Mathf.Lerp(1, 0, distanceFromDamageable / damageRadius);

                    finalDamage *= damageMultiplier;

                    if (kill) finalDamage = damageable.maxHealth;
                    
                    damageable.damageDirection = (damageable.transform.position - transform.position).normalized * force;

                    damageable.Damage(finalDamage, damageSource.gameObject);
                    damageable.damageDirection = dir * force;

                }
            }

            if(obj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddExplosionForce(force, transform.position, damageRadius, 1, ForceMode.VelocityChange);
            }
        }

        private void ApplyFriction()
        {
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;

            float coefficientOfFriction = friction / 100;

            rb.AddForce(-velocity * coefficientOfFriction, ForceMode.Impulse);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (sticky)
            {
                rb.isKinematic = true;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (friction > 0) ApplyFriction();
        }

        private void OnDrawGizmos()
        {
            if (!debug) return;

            if (ranges)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, deathRadius * scale);

                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, damageRadius * scale);
            }

            if (rays && type == ExplosionType.RayTracking)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius * scale, layerMask);
                foreach (Collider collider in colliders)
                {
                    RaycastHit hit;
                    var dir = -(transform.position - collider.transform.position);
                    if (Physics.Raycast(transform.position, dir, out hit))
                    {
                        if (hit.transform == transform) return;
                        bool hasDamageble = hit.transform.TryGetComponent(out IDamageable damageable) && collider.transform.GetComponentInChildren<IDamageableGroup>() == null;
                        bool hadDamageableGroup = hit.transform.TryGetComponent(out IDamageableGroup damageableGroup);
                        bool hasRigidbody = hit.transform.TryGetComponent(out Rigidbody rb);

                        if (hasDamageble || hadDamageableGroup || hasRigidbody)
                        {
                            Gizmos.color = hit.distance >= deathRadius ? Color.white : Color.red;
                            Gizmos.DrawLine(transform.position, hit.point);
                            Gizmos.DrawSphere(hit.point, 0.1f * transform.lossyScale.magnitude);
                        }
                    }
                }

                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, new Vector3(0, -deathRadius * scale, 0) * transform.lossyScale.magnitude);

            }

            if (rays && type == ExplosionType.Standard)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius * scale, layerMask);
                foreach (Collider collider in colliders)
                {
                    bool hasDamageble = collider.transform.TryGetComponent(out IDamageable damageable) && collider.transform.GetComponentInChildren<IDamageableGroup>() == null;
                    bool hadDamageableGroup = collider.transform.TryGetComponent(out IDamageableGroup damageableGroup);
                    bool hasRigidbody = collider.transform.TryGetComponent(out Rigidbody rb);

                    if (hasDamageble || hadDamageableGroup || hasRigidbody)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(transform.position, collider.transform.position);
                        Gizmos.DrawSphere(collider.transform.position, 0.1f * transform.lossyScale.magnitude);
                    }
                }

                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, new Vector3(0, -damageRadius * scale, 0) * transform.lossyScale.magnitude);

            }
        }
        
        public void Damage(float amount, GameObject damageSource)
        {
            if(!damageable) return;

            health -= amount;
            this.damageSource = damageSource;
        }

        public void OnHit(HitInfo hitInfo)
        {
            sourcePlayer = hitInfo.sourcePlayer;
        }

        [ContextMenu("Setup/Network Components")]
        private void SetupNetworkComponents()
        {
#if UNITY_EDITOR
            FPSFrameworkEditor.InvokeConvertMethod("ConvertExplosive", this, new object[] { this });
#endif
        }
    }

    public enum ExplosionType
    {
        Standard,
        RayTracking
    }
}