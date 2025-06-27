using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Akila.FPSFramework
{
    /// <summary>
    /// Handles audio playback, control, and custom events in the FPS Framework.
    /// </summary>
    public class Audio
    {
        /// <summary>
        /// The main active audio listener in the scene. 
        /// </summary>
        public AudioListener audioListener;

        /// <summary>
        /// The audio profile containing settings for the audio.
        /// </summary>
        public AudioProfile audioProfile;

        /// <summary>
        /// The AudioSource component used for playing audio.
        /// </summary>
        public AudioSource audioSource;

        /// <summary>
        /// The GameObject to which the audio is attached.
        /// </summary>
        public MonoBehaviour gameObject;

        /// <summary>
        /// List of custom audio events triggered at specific times.
        /// </summary>
        public List<CustomAudioEvent> events = new List<CustomAudioEvent>();

        public bool isEventsEnabled;

        /// <summary>
        /// A random value between 0 and the pitch offset in audio profile. That is added to the <see cref="AudioSource.pitch"/> to vary the pitch.
        /// </summary>
        public float randomizedPitchOffset;

        public float sixDimensionsPitchOffset;

        /// <summary>
        /// Sets up the Audio class with a target GameObject and a template audio profile.
        /// </summary>
        /// <param name="obj">The target GameObject for the audio.</param>
        /// <param name="profile">The audio profile template to use.</param>
        public void Setup(MonoBehaviour obj, AudioProfile profile)
        {
            if(obj == null)
            {
                Debug.LogError("Target object is null. Setup aborted.");
                return;
            }

            if (profile == null)
            {
                Debug.LogError("AudioProfile is null. Setup aborted.", obj);
                return;
            }

            // If an AudioSource already exists, destroy the old one to prevent duplicates
            if (audioSource != null)
            {
                GameObject.Destroy(audioSource);
            }

            // Add a new AudioSource component if it doesn't already exist
            if (audioSource == null)
            {
                audioSource = obj.gameObject.AddComponent<AudioSource>();

                // Log an error and exit if adding the AudioSource component fails
                if (audioSource == null)
                {
                    Debug.LogError("Failed to add AudioSource component.", obj);
                    return;
                }
            }


            gameObject = obj;
            audioProfile = profile;

            // Update AudioSource settings from the provided template
            ApplySettings(profile);

            // Set up custom audio events based on the layers in the template
            foreach (var layer in profile.audioLayers)
            {
                if (layer.audioClip == null)
                {
                    Debug.LogWarning("CustomAudioLayer has no AudioClip assigned.", obj);
                    continue;
                }

                if (layer.time < 0)
                    Debug.LogError("[Audio] Audio Profile's sound layer time can't be less than zero. Resetting to 0.", obj);

                float time = Mathf.Clamp(layer.time, 0.001f, float.MaxValue);

                events.Add(new CustomAudioEvent(time, () =>
                {
                    if (audioSource)
                        audioSource.PlayOneShot(layer.audioClip);
                    else
                        Debug.LogWarning("AudioSource is null. Cannot play audio clip.", obj);
                }));
            }

            gameObject.StartCoroutine(Update());
        }

        private IEnumerator Update()
        {
            while (Application.isPlaying)
            {
                yield return null;

                if (audioSource == null)
                {
                    audioSource = gameObject.GetComponent<AudioSource>();

                    if (audioSource == null)
                        Debug.LogError("AudioSource is null. Cannot update pitch.", gameObject);
                }
                else
                {
                    float finalPitch = 0;

                    if (audioProfile.dynamicPitch)
                    {
                        finalPitch += Time.timeScale * (audioProfile.pitch + randomizedPitchOffset);
                    }
                    else
                    {
                        finalPitch += audioProfile.pitch;
                    }

                    finalPitch += sixDimensionsPitchOffset;

                    audioSource.pitch = finalPitch;
                }

                AudioListener[] listeners = GameObject.FindObjectsOfType<AudioListener>();

                foreach (AudioListener listener in listeners)
                {
                    if (listener.TryGetComponent<Camera>(out Camera camera))
                    {
                        if (camera != null && camera.enabled && listener.enabled)
                            audioListener = listener;
                    }
                }


                if (audioListener)
                {
                    float distance = Vector3.Distance(gameObject.transform.position, audioListener.transform.position);
                    float blendVal = distance / audioProfile.maxDistance;

                    Vector3 direction = (audioListener.transform.position - gameObject.transform.position).normalized;

                    // Calculate dot products with the 6 directions
                    float forwardDot = Mathf.Max(0, Vector3.Dot(direction, Vector3.forward));
                    float backwardDot = Mathf.Max(0, Vector3.Dot(direction, -Vector3.forward));
                    float rightDot = Mathf.Max(0, Vector3.Dot(direction, Vector3.right));
                    float leftDot = Mathf.Max(0, Vector3.Dot(direction, -Vector3.right));
                    float upDot = Mathf.Max(0, Vector3.Dot(direction, Vector3.up));
                    float downDot = Mathf.Max(0, Vector3.Dot(direction, -Vector3.up));

                    // Sum of all dot products
                    float totalDot = forwardDot + backwardDot + rightDot + leftDot + upDot + downDot;
                    float dirValue = 0;

                    // Avoid division by zero
                    if (totalDot > 0)
                    {
                        // Calculate the blended DirValue based on the weighted average of the multipliers
                        dirValue +=
                            (forwardDot * audioProfile.forwardFactor +
                             backwardDot * audioProfile.backwardFactor +
                             rightDot * audioProfile.rightFactor +
                             leftDot * audioProfile.leftFactor +
                             upDot * audioProfile.upFactor +
                             downDot * audioProfile.downFactor) / totalDot;
                    }

                    sixDimensionsPitchOffset = Mathf.Lerp(0, dirValue, audioProfile._6DSoundCurve.Evaluate(blendVal));
                }
            }
        }

        /// <summary>
        /// Plays the audio with the current audio profile settings.
        /// </summary>
        public void Play()
        {
            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot play audio.");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot play audio.");
                return;
            }

            EnableEvents();

            ApplySettings(audioProfile);
            CalculateRandomPitch();
            InvokeCustomEvents();
            audioSource.Play();
        }

        /// <summary>
        /// Plays the audio as a one-shot sound effect.
        /// </summary>
        public void PlayOneShot()
        {
            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot play one-shot audio.");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot play one-shot audio.");
                return;
            }

            EnableEvents();

            ApplySettings(audioProfile);
            CalculateRandomPitch();
            InvokeCustomEvents();

            if (audioProfile.audioClip)
                audioSource.PlayOneShot(audioProfile.audioClip);
        }

        /// <summary>
        /// Plays a specific audio clip as a one-shot sound effect.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        public void PlayOneShot(AudioClip clip)
        {
            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot play one-shot audio.");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot play one-shot audio.");
                return;
            }

            if (clip == null)
            {
                Debug.LogWarning("AudioClip is null. Cannot play one-shot audio.");
                return;
            }

            EnableEvents();

            ApplySettings(audioProfile);
            CalculateRandomPitch();
            InvokeCustomEvents();
            audioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Pauses the currently playing audio.
        /// </summary>
        public void Pause()
        {
            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot pause audio.");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot pause audio.");
                return;
            }

            audioSource.Pause();
        }

        /// <summary>
        /// Resumes the paused audio.
        /// </summary>
        public void Unpause()
        {
            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot unpause audio.");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot unpause audio.");
                return;
            }

            audioSource.UnPause();
        }

        /// <summary>
        /// Stops the currently playing audio.
        /// </summary>
        public void Stop()
        {
            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot stop audio.");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot stop audio.");
                return;
            }

            audioSource.Stop();
        }

        public void EnableEvents()
        {
            isEventsEnabled = true;
        }

        public void DisableEvents()
        {
            isEventsEnabled = false;
        }

        /// <summary>
        /// Updates the AudioSource settings using the provided audio profile.
        /// </summary>
        /// <param name="audioProfile">The audio profile with updated settings.</param>
        public void ApplySettings(AudioProfile audioProfile)
        {
            this.audioProfile = audioProfile;

            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot update AudioSource settings.");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot update AudioSource settings.");
                return;
            }

            audioSource.clip = audioProfile.audioClip;
            audioSource.outputAudioMixerGroup = audioProfile.output;
            audioSource.mute = audioProfile.mute;
            audioSource.bypassEffects = audioProfile.bypassEffects;
            audioSource.bypassListenerEffects = audioProfile.bypassListenerEffects;
            audioSource.bypassReverbZones = audioProfile.bypassReverbZones;
            audioSource.playOnAwake = audioProfile.playOnAwake;
            audioSource.loop = audioProfile.loop;
            audioSource.priority = audioProfile.priority;

            audioSource.volume = audioProfile.volume * FPSFrameworkSettings.masterAudioVolume;
            audioSource.pitch = audioProfile.pitch;
            audioSource.panStereo = audioProfile.stereoPan;
            audioSource.spatialBlend = audioProfile.spatialBlend;
            audioSource.reverbZoneMix = audioProfile.reverbZoneMix;
            audioSource.dopplerLevel = audioProfile.dopplerLevel;
            audioSource.spread = audioProfile.spread;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = audioProfile.minDistance;
            audioSource.maxDistance = audioProfile.maxDistance;
        }

        /// <summary>
        /// Calculates and applies a randomized pitch offset based on the audio profile.
        /// </summary>
        /// <remarks>
        /// If <see cref="audioProfile"/> or <see cref="audioSource"/> is null, an error is logged, and the pitch is not updated.
        /// When dynamic pitch is enabled, a random value between 0 and the specified pitch offset is used.
        /// </remarks>
        public void CalculateRandomPitch()
        {
            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot randomize pitch.");
                return;
            }

            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot randomize pitch.");
                return;
            }

            if (audioProfile.dynamicPitch)
            {
                randomizedPitchOffset = Random.Range(0, audioProfile.pitchOffset);
            }
        }


        /// <summary>
        /// Invokes custom audio events based on the current pitch and event timing.
        /// </summary>
        private async void InvokeCustomEvents()
        {
            if (Application.isPlaying == false) return;

            // Check if the audio profile is null and log an error if it is, then return.
            if (audioProfile == null)
            {
                Debug.LogError("AudioProfile is null. Cannot invoke custom events.");
                return;
            }

            // If there are no custom audio events, skip further processing.
            if (events.Count == 0)
            {
                // Don't proceed if there are no events.
                return;
            }

            // Find the highest time among all custom audio events.
            float highestTime = events.Max(e => e.time);

            // Initialize time variables.
            float time = -Time.deltaTime;
            float currentTime = 0;
            float previousTime = 0;

            // Continue looping until the current time exceeds the highest event time.
            while (time < highestTime + Time.deltaTime)
            {
                // Increment time using Time.deltaTime.
                time += Time.deltaTime;

                // Update the current time.
                currentTime = time;

                if (Application.isPlaying == false) return;


                //Update the current audio source pitch while the events are begin invoked.
                if (audioSource) audioSource.pitch = Time.timeScale * audioProfile.pitch;

                // Loop through each custom audio event.
                foreach (CustomAudioEvent customEvent in events)
                {
                    // Check if the current time has passed the event's time, but the previous time hasn't.
                    if (currentTime > customEvent.time && previousTime < customEvent.time)
                    {
                        // Invoke the event action if it exists.
                        if(isEventsEnabled) customEvent?.Invoke();
                    }
                }

                // Update the previous time to the current time.
                previousTime = currentTime;

                // Yield to allow other operations to run, making this method asynchronous.
                await Task.Yield();
            }
        }
    }

    /// <summary>
    /// Represents a custom audio event that can be invoked at a specific time.
    /// </summary>
    public class CustomAudioEvent
    {
        public float time;
        public UnityAction action;

        public CustomAudioEvent(float time, UnityAction action)
        {
            this.time = time;
            this.action = action;
        }

        public void Invoke()
        {
            if (action == null)
            {
                Debug.LogWarning("CustomAudioEvent action is null. Skipping invocation.");
                return;
            }

            action.Invoke();
        }
    }
}
