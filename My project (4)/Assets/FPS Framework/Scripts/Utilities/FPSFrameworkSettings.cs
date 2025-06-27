using UnityEditor;
using UnityEngine;

namespace Akila.FPSFramework
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class FPSFrameworkSettings
    {
        public static bool shortenMenus
        {
            get
            {
                if(AMSO != null)
                    return AMSO.shortenMenus;

                return false;
            }
            set
            {
                if (AMSO == null) LoadActiveSettings();

                AMSO.shortenMenus = value;
            }
        }

        public static float masterAudioVolume
        {
            get
            {
                if(AMSO != null)
                    return AMSO.masterAudioVolume;

                return 1;
            }
            set
            {
                if (AMSO == null) LoadActiveSettings();

                AMSO.masterAudioVolume = value;
            }
        }

        public static float masterAnimationSpeed
        {
            get
            {
                if(AMSO != null)
                    return AMSO.masterAnimationSpeed;

                return 1;
            }
            set
            {
                if (AMSO == null) LoadActiveSettings();

                AMSO.masterAnimationSpeed = value;
            }
        }

        public static int maxAnimationFramerate
        {
            get
            {
                if(AMSO != null)
                    return AMSO.maxAnimationFramerate;

                return 120;
            }
            set
            {
                if (AMSO == null) LoadActiveSettings();

                AMSO.maxAnimationFramerate = value;
            }
        }

        public static AMSO AMSO
        {
            get
            {
                if(_preset == null)
                    LoadActiveSettings();

                return _preset;
            }
        }

        private static AMSO _preset;

        private static void LoadActiveSettings()
        {
            _preset = Resources.Load<AMSO>("FPS Framework Settings Preset");
        }

#if UNITY_EDITOR
        static FPSFrameworkSettings()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && !IsValidConfiguration())
            {
                Debug.LogError("Play Mode stopped due to a null settings preset in FPS Framework settings. Please assign one and try again.");

                EditorUtility.DisplayDialog(
                    "Play Mode Blocked",
                    "Play Mode was stopped because the settings preset in FPS Framework settings is null. Please reimport 'Data' folder and try again.",
                    "OK"
                );

                EditorApplication.isPlaying = false;
            }
        }

        private static bool IsValidConfiguration()
        {
            return AMSO != null;
        }

        [CustomEditor(typeof(AMSO))]
        protected class FPSFSPresetEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                EditorGUILayout.HelpBox("Modifying 'FPSFSPreset' directly from the preset is unavailable. Open the FPS Framework Settings window to make changes.", MessageType.Info);

                if (GUILayout.Button("Open Settings"))
                {
                    EditorApplication.ExecuteMenuItem(MenuPaths.Settings);
                }
            }
        }
#endif
    }
}