using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Linq;
namespace Akila.FPSFramework.Animation
{
    [AddComponentMenu("Akila/FPS Framework/Animation/Procedural Animation")]
    public class ProceduralAnimation : MonoBehaviour
    {
        [Header("BASE"), Space]
        public string Name = "New Procedural Animation";
        public float length = 0.15f;
        [Range(0, 1)] public float weight = 1;
        public bool loop = false;
        public bool autoStop = false;
        public bool perModifierConnections = true;
        public bool playOnAwake = false;
        [Space]
        public TriggerType triggerType;
        public InputAction triggerInputAction;

        [Space(6), Separator, Space(6)]
        public ProceduralAnimationEvents events = new ProceduralAnimationEvents();
        public List<CustomProceduralAnimationEvent> customEvents;
        public List<ProceduralAnimationConnection> connections = new List<ProceduralAnimationConnection>();

        public MoveAnimationModifier[] moveAnimationModifiers { get; protected set; }
        public SpringAnimationModifier[] springAnimationModifiers { get; protected set; }
        public KickAnimationModifier[] kickAnimationModifiers { get; protected set; }
        public SwayAnimationModifier[] swayAnimationModifiers { get; protected set; }
        public WaveAnimationModifier[] waveAnimationModifiers { get; protected set; }
        public OffsetAnimationModifier[] offsetAnimationModifiers { get; protected set; }

        public bool isActive { get; set; } = true;

        /// <summary>
        /// final position result for this clip
        /// </summary>
        public Vector3 targetPosition
        {
            get
            {
                return GetTargetModifiersPosition() * weight;
            }
        }

        /// <summary>
        /// final rotation result for this clip
        /// </summary>
        public Vector3 targetRotation
        {
            get
            {
                return GetTargetModifiersRotation() * weight;
            }
        }

        /// <summary>
        /// current animation progress by value from 0 to 1
        /// </summary>
        public float progress { get; set; }
        public bool isPlaying { get; set; }
        public bool isPaused { get; set; }

        private bool isTrigged;

        //acutal velocity
        private float currentVelocity;

        /// <summary>
        /// current animation movement speed
        /// </summary>
        public float velocity { get => currentVelocity; }

        /// <summary>
        /// List of all modifieres applied to this animation
        /// </summary>
        public List<ProceduralAnimationModifier> modifiers { get; set; } = new List<ProceduralAnimationModifier>();
        public bool alwaysStayIdle { get; set; }

        private void Awake()
        {
            RefreshModifiers();
            triggerInputAction.Enable();

            foreach (ProceduralAnimationModifier modifier in modifiers)
            {
                modifier.targetAnimation = this;
            }

            moveAnimationModifiers = GetComponents<MoveAnimationModifier>();
            springAnimationModifiers = GetComponents<SpringAnimationModifier>();
            kickAnimationModifiers = GetComponents<KickAnimationModifier>();
            swayAnimationModifiers = GetComponents<SwayAnimationModifier>();
            waveAnimationModifiers = GetComponents<WaveAnimationModifier>();
            offsetAnimationModifiers = GetComponents<OffsetAnimationModifier>();
        }

        private void OnEnable()
        {
            if(playOnAwake == true)
            {
                Play(0);
            }
        }

        private void Start()
        {
            GetComponentInParent<ProceduralAnimator>().RefreshClips();
        }

        bool isTriggred;
        float lastTriggerTime;

        private void Update()
        {
            if (isActive == false) return;

            //Handles the custom events and progress for this animation.
            HandleEvents();

            if(triggerType == TriggerType.Hold)
            {
                if (triggerInputAction.IsPressed()) Play();
                else Stop();
            }

            if(triggerType == TriggerType.Tab)
            {
                if (triggerInputAction.triggered) isTrigged = !isTrigged;

                if (isTrigged) Play();
                else Stop();
            }

            if(triggerType == TriggerType.DoubleTab)
            {
                triggerInputAction.HasDoupleClicked(ref isTrigged, ref lastTriggerTime, 0.3f);

                if (isTrigged) Play();
                else Stop();
            }

            if (!isPaused)
                UpdateProgress();

            if(loop && progress >= 0.999f)
            {
                progress = 0;
            }

            if(autoStop && progress >= 0.999f || HasToAvoid())
            {
                Stop();
            }
        }

        public void Play(float fixedTime = -1)
        {
            foreach(ProceduralAnimationConnection connection in connections)
            {
                if(connection.target == null)
                {
                    Debug.LogError($"[Procedural Animation] Connection's target reference is null or missing on {gameObject.name}. This instance will be ignored.", gameObject);
                }
            }

            if (!HasToAvoid())
            {
                isPaused = false;
                isPlaying = true;
            }

            if (fixedTime >= 0)
                progress = fixedTime;

            events.OnPlay?.Invoke();
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Stop()
        {
            isPlaying = false;
        }

        private void UpdateProgress()
        {
            float masterSpeed = 1;

            masterSpeed = FPSFrameworkSettings.masterAnimationSpeed;

            if (isPlaying)
                progress = Mathf.SmoothDamp(progress, 1, ref currentVelocity, length / masterSpeed);

            if(!isPlaying || HasToAvoid())
                progress = Mathf.SmoothDamp(progress, 0, ref currentVelocity, length / masterSpeed);
        }

        private bool prevPlaying;

        private void HandleEvents()
        {
            if(isPlaying && !prevPlaying)
            {
                events.OnPlayed?.Invoke();
            }

            if(!isPlaying && prevPlaying)
            {
                events.OnStoped?.Invoke();
            }

            foreach (CustomProceduralAnimationEvent animationEvent in customEvents) animationEvent.UpdateEvent(this);

            prevPlaying = isPlaying;
        }

        /// <summary>
        /// returns all the clip modifiers for this clip in a List of ProceduralAnimationClip and refreshes the animtor clips 
        /// </summary>
        public List<ProceduralAnimationModifier> RefreshModifiers()
        {
            modifiers = GetComponentsInChildren<ProceduralAnimationModifier>().ToList();

            return modifiers;
        }

        public bool HasToAvoid()
        {
            bool result = false;

            if (alwaysStayIdle || FPSFrameworkCore.IsActive == false) return true;

            foreach (ProceduralAnimationConnection connection in connections)
            {
                if (connection.target != null)
                {
                    if (connection.type == ProceduralAnimationConnectionType.AvoidInTrigger)
                    {
                        if (connection.target && connection.target.isPlaying) result = true;
                    }

                    if (connection.type == ProceduralAnimationConnectionType.AvoidInIdle)
                    {
                        if (!connection.target.isPlaying) result = true;
                    }
                }
            }

            return result;
        }

        public float GetAvoidanceFactor(ProceduralAnimation animation)
        {
            if(animation == null) return 0f;

            return Mathf.Lerp(1, 0, animation.progress);
        }

        /// <summary>
        /// final position result for this modifier
        /// </summary>
        public Vector3 GetTargetModifiersPosition()
        {
            Vector3 result = Vector3.zero;

            float avoidanceFactor = 1;

            foreach (ProceduralAnimationConnection connection in connections)
            {
                if (connection.target != null)
                {
                    if (connection.type == ProceduralAnimationConnectionType.AvoidInTrigger)
                    {
                        avoidanceFactor *= GetAvoidanceFactor(connection.target);
                    }

                    if (connection.type == ProceduralAnimationConnectionType.AvoidInIdle)
                    {
                        avoidanceFactor *= Mathf.Lerp(1, 0, GetAvoidanceFactor(connection.target));
                    }
                }
            }

            foreach (ProceduralAnimationModifier modifier in modifiers) result += modifier.targetPosition;

            if (perModifierConnections)
                result *= avoidanceFactor;

            return result;
        }

        /// <summary>
        /// final rotation result for this modifier
        /// </summary>
        public Vector3 GetTargetModifiersRotation()
        {
            Vector3 result = Vector3.zero;

            float avoidanceFactor = 1;

            foreach (ProceduralAnimationConnection connection in connections)
            {
                if (connection.target != null)
                {
                    if (connection.type == ProceduralAnimationConnectionType.AvoidInTrigger)
                    {
                        avoidanceFactor *= GetAvoidanceFactor(connection.target);
                    }

                    if (connection.type == ProceduralAnimationConnectionType.AvoidInIdle)
                    {
                        avoidanceFactor *= Mathf.Lerp(1, 0, GetAvoidanceFactor(connection.target));
                    }
                }
            }

            foreach (ProceduralAnimationModifier modifier in modifiers) result += modifier.targetRotation;

            if (perModifierConnections)
                result *= avoidanceFactor;

            return result;
        }

        public enum TriggerType
        {
            None = 0,
            Tab = 1,
            Hold = 2,
            DoubleTab = 3
        }
    }
}