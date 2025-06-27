using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;


namespace Akila.FPSFramework.Internal
{
#if UNITY_EDITOR
    public static class FPSFrameworkEditor
    {
        public static Canvas FindOrCreateCanvas()
        {
            Canvas canvas = null;

            // 1. Try to find a Canvas in the active selection
            if (Selection.activeGameObject != null)
            {
                canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
            }

            // 2. If no Canvas is found in selection, search the scene
            if (!canvas)
            {
                canvas = GameObject.FindObjectOfType<Canvas>();
            }

            // 3. If no Canvas exists in the scene, create one
            if (!canvas)
            {
                canvas = CreateCanvas();
            }

            // Ensure an EventSystem exists
            EnsureEventSystemExists();

            // Set the Canvas render mode to ScreenSpaceOverlay (default)
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Set the found or created Canvas as the active selection in the editor
            Selection.activeGameObject = canvas.gameObject;

            return canvas;
        }

        public static Canvas CreateCanvas()
        {
            // Create the Canvas GameObject
            GameObject canvasObject = new GameObject("Canvas");

            // Add the required components
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        public static void EnsureEventSystemExists()
        {
            if (!GameObject.FindObjectOfType<EventSystem>())
            {
                // Create a new EventSystem GameObject if none exists
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
        }


        [MenuItem(MenuPaths.Help, false, 0)]
        public static void OpenHelp()
        {
            Application.OpenURL("https://akila.gitbook.io/fps-framework/");
        }

        public static void EnterRenameMode()
        {
            EditorApplication.delayCall += () =>
            {
                EditorGUIUtility.editingTextField = true;
                EditorApplication.delayCall += () =>
                {
                    var editorWindow = EditorWindow.focusedWindow;
                    if (editorWindow != null)
                    {
                        editorWindow.SendEvent(new Event
                        {
                            keyCode = KeyCode.F2,
                            type = EventType.KeyDown
                        });
                    }
                };
            };
        }

        //Used to invoke component upgrading in FPS Framework Pro
        public static void InvokeConvertMethod(string methodName, object obj, object[] parameters)
        {
            bool notFound = true;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == "FPSFrameworkMultiplayerEditor")
                    {
                        type.GetMethod(methodName).Invoke(obj, parameters);
                        notFound = false;
                    }
                }
            }

            if (notFound)
            {
                Debug.LogError("Please install 'FPS Framework: Multiplayer Edition' before trying to network your components.");
            }
        }

        #region Unity Editor
#if UNITY_EDITOR
        // Function to check if the custom define symbol exists
        public static bool CheckIfDefineSymbolExists(string defineSymbol)
        {
            // Get the current Scripting Define Symbols for the Standalone platform
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            // Return true if the symbol exists, otherwise false
            return currentDefines.Contains(defineSymbol);
        }

        // Function to add a custom define symbol
        public static void AddCustomDefineSymbol(string defineSymbol)
        {
            // Get the current Scripting Define Symbols for the Standalone platform
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            // Check if the symbol is already defined
            if (!currentDefines.Contains(defineSymbol))
            {
                // Append the define symbol to the existing symbols
                currentDefines += ";" + defineSymbol;
                // Update the Scripting Define Symbols for Standalone platform
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, currentDefines);
                UnityEngine.Debug.Log($"{defineSymbol} added.");
            }
            else
            {
                UnityEngine.Debug.Log($"{defineSymbol} is already defined.");
            }
        }

        // Function to remove a custom define symbol
        public static void RemoveCustomDefineSymbol(string defineSymbol)
        {
            // Get the current Scripting Define Symbols for the Standalone platform
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            // Check if the symbol exists
            if (currentDefines.Contains(defineSymbol))
            {
                // Remove the symbol from the list
                currentDefines = currentDefines.Replace(";" + defineSymbol, "").Replace(defineSymbol + ";", "");
                // Update the Scripting Define Symbols for Standalone platform
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, currentDefines);
                UnityEngine.Debug.Log($"{defineSymbol} removed.");
            }
            else
            {
                UnityEngine.Debug.Log($"{defineSymbol} is not defined.");
            }
        }
#endif
#endregion
    }
#endif
}