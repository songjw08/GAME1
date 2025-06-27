using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Health System/Damageable")]
    public class Damageable : MonoBehaviour, IDamageable
    {
        public HealthType type = HealthType.Other;
        public float health = 100;
        public float destroyDelay;
        [Range(0, 1)] public float damageCameraShake = 0.3f;

        [Space]
        public bool destoryOnDeath;
        public bool destroyRoot;
        public bool ragdolls;
        public GameObject deathEffect;

        [Space]
        public UnityEvent OnDeath;

        public Actor Actor { get; set; }
        public Ragdoll ragdoll { get; set; }
        public GameObject damageSource { get; set; }
        public Vector3 damageDirection { get; set; }
        public float maxHealth { get; set; }
        public IDamageableGroup[] groups { get; set; }
        private bool died;
        public bool deadConfirmed { get; set; }

        private void Awake()
        {
            maxHealth = health;
        }

        private void Start()
        {
            Actor = GetComponent<Actor>();
            ragdoll = GetComponent<Ragdoll>();

            OnDeath.AddListener(Die);

            if (type == HealthType.Player)
            {
                if (Actor && Actor.characterManager != null) DeathCamera.Instance?.Disable();

                groups = GetComponentsInChildren<IDamageableGroup>();

                if (Actor && Actor.characterManager != null)
                {
                    if (UIManager.Instance && UIManager.Instance.HealthDisplay && Actor)
                    {
                        UIManager.Instance.HealthDisplay?.UpdateCard(health, Actor.actorName, false);
                        UIManager.Instance.HealthDisplay.actorNameText.text = Actor.actorName;
                    }
                }
            }

            if(type == HealthType.Other)
            {
                if (ragdoll || Actor) Debug.LogWarning($"{this} has humanoid components and it's type is Other please change type to Humanoid to avoid errors.");
            }
        }

        public bool allowDamageScreen { get; set; } = true;
        public bool allowRespawn { get; set; } = true;
        float IDamageable.health { get => health; set => health = value; }

        private float previousHealth;

        private void Update()
        {
            if (health != previousHealth)
            {
                if (health > previousHealth)
                {
                }
                else
                {
                    UpdateSystem();
                }

                previousHealth = health;
            }
        }

        private void UpdateSystem()
        {
            if (!died && health <= 0)
            {
                OnDeath?.Invoke();
            }

            if (type == HealthType.Player && Actor.characterManager != null)
            {
                Actor.characterManager.cameraManager.ShakeCameras(damageCameraShake);
            }

            UpdateUI(1);
        }

        private void UpdateUI(float alpha)
        {
            if (!allowDamageScreen) return;

            if (type != HealthType.Player) return;
            
            if(Actor == null)
            {
                Debug.LogError("Couldn't find Actor on Damageable", gameObject);
                return;
            }

            if(Actor.characterManager == null)
            {
                Debug.LogError("Couldn't find CharacterManager on Damagable.", gameObject);
                return;
            }

            UIManager uIManager = UIManager.Instance;

            if (uIManager == null)
            {
                Debug.LogError("UIManager is not set. Make sure to have at a UIManager in your scene.", gameObject);
                return;
            }

            if (damageSource != null)
                UIManager.Instance?.DamageIndicator?.Show(damageSource.transform.position, alpha);

            UIManager.Instance?.HealthDisplay?.UpdateCard(health, Actor.actorName, true);
        }

        private void Die()
        {
            if (!isActive) return;

            if(type == HealthType.Player)
            {
                if (Actor.respawnable) Actor.deaths++;
            }

            if (destoryOnDeath && !destroyRoot) Destroy(gameObject, destroyDelay);
            if (destoryOnDeath && destroyRoot) Destroy(gameObject.transform.parent.gameObject, destroyDelay);
            if (!died) Respwan();

            if (ragdoll) ragdoll.Enable(damageDirection);

            if (deathEffect)
            {
                GameObject effect = Instantiate(deathEffect, transform.position, transform.rotation);
                effect.SetActive(true);
            }

            if (damageSource && type == HealthType.Player) DeathCamera.Instance?.Enable(gameObject, damageSource);
            
            died = true;
        }

        public UnityEvent onRespawn;

        private void Respwan()
        {
            if (type == HealthType.Other || !Actor) return;

            onRespawn?.Invoke();

            if (Actor.respawnable)
            {
                Actor.Respwan(SpawnManager.Instance.respawnDelay);
            }
        }

        public void Damage(float amount, GameObject damageSource)
        {
            health -= amount;
            this.damageSource = damageSource;
        }

        public bool isActive { get; set; } = true;

        public UnityEvent onDeath => OnDeath;
    }

    public enum HealthType
    {
        Player = 0,
        NPC = 1,
        Other = 2
    }
}