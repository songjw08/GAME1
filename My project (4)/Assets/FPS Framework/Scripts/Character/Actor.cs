using UnityEngine;
using System;
using UnityEngine.Events;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Player/Actor")]
    [DisallowMultipleComponent()]
    public class Actor : MonoBehaviour
    {
        /// <summary>
        /// The name of the actor, this will be set automaticly if on actor repsawn.
        /// </summary>
        [Header("Base")]
        public string actorName;

        public GameObject playerPrefab;
        public int teamId = 0;
        public bool respawnable = true;

        [Header("Statistics")]
        public int kills;
        public int deaths;

        public bool respawnActive { get; set; } = true;

        public UnityEvent onRespawn { get; set; }

        /// <summary>
        /// The character manager for this actor. It's used to get the data of your FPS Controller as it could be custom.
        /// </summary>
        public CharacterManager characterManager { get; set; }

        /// <summary>
        /// The IDamageable interface of this actor. It's used to damage the actor.
        /// </summary>
        public IDamageable damageable { get; set; }

        /// <summary>
        /// The inventory of this actor. It's used to manage the actor's items, ammo and ect...
        /// </summary>
        public IInventory inventory { get; private set; }

        protected virtual void Awake()
        {
            inventory = transform.SearchFor<IInventory>();
            characterManager = GetComponent<CharacterManager>();
            damageable = GetComponent<IDamageable>();

        }

        protected virtual void Start()
        {
            if (UIManager.Instance && UIManager.Instance.HealthDisplay)
            {
                UIManager.Instance.HealthDisplay?.UpdateCard(damageable.health, actorName, false);
                UIManager.Instance.HealthDisplay.slider.maxValue = damageable.health;
                UIManager.Instance.HealthDisplay.backgroundSlider.maxValue = damageable.health;
                UIManager.Instance.HealthDisplay.actorNameText.text = actorName;
            }

            if (damageable != null)
                damageable.onDeath.AddListener(ConfirmDeath);
        }

        protected virtual void Update()
        {
            if (UIManager.Instance && characterManager != null)
                UIManager.Instance.HealthDisplay?.UpdateCard(damageable.health, actorName, true);
        }

        public void ConfirmDeath()
        {
            if(damageable == null)
            {
                Debug.LogError("Damagable (IDamagable) is not set.", gameObject);

                return;
            }

            if(damageable.damageSource == null)
            {
                Debug.LogError("DamageSource in Damageable is not set.", gameObject);

                return;
            }

            //Return if already death is confirmed
            if (damageable.deadConfirmed) return;

            Actor killer = damageable.damageSource.GetComponent<Actor>();
            UIManager uIManager = UIManager.Instance;

            killer.kills++;
            this.deaths++;

            if (uIManager != null)
            {
                KillFeed killFeed = uIManager.KillFeed;
                Hitmarker hitmarker = uIManager.Hitmarker;

                if (killFeed != null)
                {
                    killFeed.Show(killer, actorName, false);
                }

                if(hitmarker != null)
                {
                    hitmarker.Show(true);
                }

            }

            //Confirm death
            damageable.deadConfirmed = true;
        }

        public void Respwan(float respawnDelay)
        {
            onRespawn?.Invoke();
            if (respawnActive == false) return;

            SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
            
            spawnManager.SpawnActor("bot", respawnDelay);
        }
    }
}