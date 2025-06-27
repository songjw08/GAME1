using UnityEngine;
using System.Collections.Generic;
using Akila.FPSFramework.Internal;
using Akila.FPSFramework;
using UnityEditor;

namespace Akila.FPSFramework
{
    public class FPSFrameworkSettingsWindow : EditorWindow
    {
        public static AMSO preset;

        private int selectedTab = 0;
        private Vector2 sidebarScroll;
        private Vector2 contentScroll;

        private List<string> tabs = new List<string>
    {
        "Animation",
        "Audio",
        "Editor"
    };

        private void OnEnable()
        {
            preset = FPSFrameworkSettings.AMSO;
        }

        [MenuItem(MenuPaths.Settings, false, -100)]
        public static void OpenWindow()
        {
            FPSFrameworkSettingsWindow window = GetWindow<FPSFrameworkSettingsWindow>("FPS Framework Settings");
            window.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);
            // Sidebar
            DrawSidebar();
            EditorGUI.EndDisabledGroup();

            // Separator (Thin Vertical Line)
            DrawSeparator();

            // Content
            DrawContent();

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (preset != null) EditorUtility.SetDirty(preset);
            }
        }

        private void DrawSidebar()
        {
            if (preset == null) selectedTab = 0;

            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);

            for (int i = 0; i < tabs.Count; i++)
            {
                bool isSelected = (i == selectedTab);
                Rect rect = GUILayoutUtility.GetRect(200, 30, GUILayout.ExpandWidth(true));

                GUIStyle buttonStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(15, 10, 5, 5),
                    normal = { textColor = Color.white },
                    hover = { textColor = new Color(0.82f, 0.82f, 0.82f) }, // Light gray for hover
                    fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal
                };

                // Draw background highlight for selected tab
                if (isSelected)
                {
                    EditorGUI.DrawRect(rect, new Color(0.2f, 0.4f, 0.8f, 0.5f));
                }

                EditorGUI.BeginDisabledGroup(preset == null && i != 0);

                // Button interaction (prevents resizing issue)
                if (GUI.Button(rect, tabs[i], buttonStyle))
                {
                    selectedTab = i;
                }

                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }



        private void DrawSeparator()
        {
            Rect separatorRect = GUILayoutUtility.GetRect(1, Screen.height, GUILayout.Width(2));
            EditorGUI.DrawRect(separatorRect, new Color(0.2f, 0.2f, 0.2f, 1f)); // Dark gray line
        }

        private void DrawContent()
        {
            EditorGUILayout.BeginVertical();
            contentScroll = EditorGUILayout.BeginScrollView(contentScroll, GUILayout.ExpandHeight(true));

            switch (selectedTab)
            {
                case 0: DrawAnimationSettings(); break;
                case 1: DrawAudioSettings(); break;
                case 2: DrawEditorSettings(); break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawAnimationSettings()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15 // Increased font size
            };

            EditorGUILayout.LabelField("Animation", titleStyle);
            EditorGUILayout.Space();

            preset.masterAnimationSpeed = EditorGUILayout.Slider("Master Speed", preset.masterAnimationSpeed, 0f, 1f);
            preset.maxAnimationFramerate = EditorGUILayout.IntField("Max Framerate", preset.maxAnimationFramerate);
        }

        private void DrawAudioSettings()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15 // Increased font size
            };

            EditorGUILayout.LabelField("Audio", titleStyle);
            EditorGUILayout.Space();

            preset.masterAudioVolume = EditorGUILayout.Slider("Master Volume", preset.masterAudioVolume, 0f, 1f);
        }

        private void DrawEditorSettings()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15 // Increased font size
            };

            EditorGUILayout.LabelField("Editor", titleStyle);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling || Application.isPlaying);

            preset.shortenMenus = EditorGUILayout.Toggle("Shorten Menus", preset.shortenMenus);

            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isCompiling || Application.isPlaying)
            {
                EditorGUILayout.HelpBox("'Shorten Menus' is disabled during script compilation and in play mode because it requires recompilation.", MessageType.Info);
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (preset.shortenMenus)
                    FPSFrameworkEditor.AddCustomDefineSymbol("FPS_FRAMEWORK_SHORTEN_MENUS");
                else
                    FPSFrameworkEditor.RemoveCustomDefineSymbol("FPS_FRAMEWORK_SHORTEN_MENUS");
            }
        }
    }
}