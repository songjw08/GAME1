using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace Akila.FPSFramework.Examples
{

    [CreateAssetMenu(fileName = "New Settings Preset URP", menuName = "Akila/FPS Framework/Settings System/Settings Preset URP")]
    public class SettingsPresetURP : SettingsPreset
    {
        public UniversalRenderPipelineAsset GetURPAsset()
        {
            if (QualitySettings.GetRenderPipelineAssetAt(0) != null)
                return (UniversalRenderPipelineAsset)QualitySettings.GetRenderPipelineAssetAt(0);

            return (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
        }

        public void SetMSAA(int value)
        {
            int resultValue = 0;

            // Set QualitySettings.antiAliasing based on resultValue
            QualitySettings.antiAliasing = resultValue switch
            {
                0 => 0,    // No anti-aliasing
                1 => 2,    // 2x MSAA
                2 => 4,    // 4x MSAA
                3 => 8,    // 8x MSAA
                _ => 0,    // Default to no anti-aliasing for other values
            };

            // Set msaaSampleCount in the URP Asset
            GetURPAsset().msaaSampleCount = resultValue switch
            {
                0 => 1,    // No MSAA
                1 => 2,    // 2x MSAA
                2 => 4,    // 4x MSAA
                3 => 8,    // 8x MSAA
                _ => 1,    // Default to no MSAA for other values
            };

            GetURPAsset().msaaSampleCount = resultValue;
        }

        public void SetShadowResolution(int value)
        {
            // Map the value (0-3) to actual resolution sizes
            int resolution = value switch
            {
                0 => 256,    // Low resolution
                1 => 512,    // Medium resolution
                2 => 1024,   // High resolution
                3 => 2048,   // Very high resolution
                _ => 1024,   // Default to high resolution if out of range
            };

            var urpAsset = GetURPAsset();
            var type = urpAsset.GetType();

            // Set the shadowmap resolutions
            type.GetField("mainLightShadowmapResolution", BindingFlags.Public | BindingFlags.Instance)
                ?.SetValue(urpAsset, resolution);
            type.GetField("additionalLightsShadowmapResolution", BindingFlags.Public | BindingFlags.Instance)
                ?.SetValue(urpAsset, resolution);
        }

        public void SetShadowDistance(int value)
        {
            float distance = 0;

            if (value == 0) distance = 200;
            if (value == 1) distance = 150;
            if (value == 2) distance = 100;
            if (value == 3) distance = 50;
            if (value == 4) distance = 30;

            GetURPAsset().shadowDistance = distance;
        }

        public void SetShadowCascade(int value)
        {
            int count = 0;
            if (value == 0) count = 4;
            if (value == 1) count = 2;
            if (value == 2) count = 1;

            GetURPAsset().shadowCascadeCount = count;
        }

        public void SetSoftShadow(int value)
        {
            // Get the URP asset dynamically
            var urpAsset = GetURPAsset();

            // Get the Type of the URP asset
            Type urpAssetType = urpAsset.GetType();

            // Get the PropertyInfo for 'supportsSoftShadows'
            PropertyInfo softShadowsProperty = urpAssetType.GetProperty("supportsSoftShadows");

            if (softShadowsProperty != null && softShadowsProperty.CanWrite)
            {
                // Set the value using reflection
                softShadowsProperty.SetValue(urpAsset, value == 0);
            }
            else
            {
                Debug.LogError("Property 'supportsSoftShadows' not found or cannot be written.");
            }
        }

        public void SetPostProcssing(int value)
        {
            float finalAmount = 1;

            Volume volume = FindAnyObjectByType<Volume>();

            //if (volume == null) return;

            if (value == 0) finalAmount = 1;
            if (value == 1) finalAmount = 0.8f;
            if (value == 2) finalAmount = 0.6f;
            if (value == 3) finalAmount = 0.5f;
            if (value == 4) finalAmount = 0.2f;
            if (value == 5) finalAmount = 0;

            volume.weight = finalAmount;
        }
    }
}