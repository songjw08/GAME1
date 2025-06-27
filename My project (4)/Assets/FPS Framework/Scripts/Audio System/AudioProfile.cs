using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine;
using System;
using UnityEngine.Serialization;


namespace Akila.FPSFramework
{
    [CreateAssetMenu(fileName = "New Audio Profile", menuName = "Akila/FPS Framework/Audio Profile")]
    public class AudioProfile : ScriptableObject
    {
        [FormerlySerializedAs("clip")]
        public AudioClip audioClip;
        public AudioMixerGroup output;
        public bool mute;
        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;
        public bool playOnAwake = false;
        public bool loop;

        [Space]
        [Range(0, 256)] public int priority = 128;
        [Range(0, 1)] public float volume = 1;
        [Range(-3, 3)] public float pitch = 1;
        [Range(-1, 1)] public float stereoPan = 0;
        [Range(0, 1)] public float spatialBlend = 0;
        [Range(0, 1.1f)] public float reverbZoneMix = 1;

        [Header("3D Sound Settings")]
        [Range(0, 5)] public float dopplerLevel = 1;
        [Range(0, 360)] public float spread = 0;
        public float minDistance = 1;
        public float maxDistance = 500;

        [Space]
        public bool dynamicPitch = false;
        public float pitchOffset = 0;

        [Header("6D Sound Settings")]
        public float forwardFactor = 0;
        public float backwardFactor = 0;
        public float rightFactor = 0;
        public float leftFactor = 0;
        public float upFactor = 0;
        public float downFactor = 0;

        public AnimationCurve _6DSoundCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });

        [Space]
        public List<CustomAudioLayer> audioLayers = new List<CustomAudioLayer>();

        public void print(string message)
        {
            Debug.Log(message);
        }

        [Serializable]
        public class CustomAudioLayer
        {
            public float time = 0;
            public AudioClip audioClip;
        }
    }
}