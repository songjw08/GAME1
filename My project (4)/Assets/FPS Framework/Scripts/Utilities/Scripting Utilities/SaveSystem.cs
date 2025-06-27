using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Akila.FPSFramework
{
    /// <summary>
    /// A flexible save system for JSON-based file management.
    /// </summary>
    public static class SaveSystem
    {
        // Default save path for all JSON files
        public static readonly string SavePath = $"{Application.persistentDataPath}/";

        // Internal state for managing key-value pairs
        private static List<Key> keys = new List<Key>();
        private static bool isLoaded = false;

        #region General Save/Load Methods

        /// <summary>
        /// Saves an object as a JSON file.
        /// </summary>
        public static void SaveObject(object obj, string fileName = "file")
        {
            string json = JsonUtility.ToJson(obj, true);
            string path = $"{SavePath}{fileName}.json";

            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads an object from a JSON file.
        /// </summary>
        public static T LoadObject<T>(string fileName = "file")
        {
            string path = $"{SavePath}{fileName}.json";

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }

            Debug.LogWarning($"File not found: {path}");
            return default;
        }

        /// <summary>
        /// Loads all JSON files in the save path into an array of objects.
        /// </summary>
        public static T[] LoadAllObjects<T>()
        {
            List<T> objects = new List<T>();

            foreach (string file in Directory.GetFiles(SavePath, "*.json"))
            {
                string json = File.ReadAllText(file);
                objects.Add(JsonUtility.FromJson<T>(json));
            }

            return objects.ToArray();
        }

        /// <summary>
        /// Deletes a specific save file.
        /// </summary>
        public static void DeleteFile(string fileName = "file")
        {
            string path = $"{SavePath}{fileName}.json";

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                Debug.LogWarning($"File not found: {path}");
            }
        }

        /// <summary>
        /// Deletes all JSON files in the save path.
        /// </summary>
#if UNITY_EDITOR
        [MenuItem(MenuPaths.ClearSaves)]
#endif
        public static void DeleteAllFiles()
        {
            foreach (string file in Directory.GetFiles(SavePath, "*.json"))
            {
                File.Delete(file);
            }

            Debug.Log("Deleted all save files.");
        }

        #endregion

        #region Key-Value Save/Load Methods

        /// <summary>
        /// Saves a key-value pair of any type.
        /// </summary>
        public static void Save<T>(string key, T value)
        {
            EnsureLoaded();

            Key existingKey = keys.Find(k => k.name == key);

            if (existingKey == null)
            {
                existingKey = new Key(key);
                keys.Add(existingKey);
            }

            if (value is int)
                existingKey.intValue = (int)(object)value;
            else if (value is float)
                existingKey.floatValue = (float)(object)value;
            else if (value is bool)
                existingKey.boolValue = (bool)(object)value;
            else if (value is string)
                existingKey.stringValue = (string)(object)value;

            SaveKeyList();
        }

        /// <summary>
        /// Loads a value of any type using a key, with a default fallback.
        /// </summary>
        public static T Load<T>(string key, T defaultValue = default)
        {
            EnsureLoaded();

            Key existingKey = keys.Find(k => k.name == key);

            if (existingKey != null)
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)existingKey.intValue;
                if (typeof(T) == typeof(float))
                    return (T)(object)existingKey.floatValue;
                if (typeof(T) == typeof(bool))
                    return (T)(object)existingKey.boolValue;
                if (typeof(T) == typeof(string))
                    return (T)(object)existingKey.stringValue;
            }

            return defaultValue;
        }

        public static bool HasKey(string key)
        {
            LoadKeyList();

            bool value = keys.Find(k => k.name == key) != null;

            return value;
        }

        /// <summary>
        /// Saves the key list to a JSON file.
        /// </summary>
        private static void SaveKeyList()
        {
            string json = JsonConvert.SerializeObject(keys, Formatting.Indented);
            File.WriteAllText($"{SavePath}keys.json", json);
        }

        /// <summary>
        /// Loads the key list from a JSON file.
        /// </summary>
        private static void LoadKeyList()
        {
            string path = $"{SavePath}keys.json";

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                keys = JsonConvert.DeserializeObject<List<Key>>(json) ?? new List<Key>();
            }
            else
            {
                keys = new List<Key>();
                Debug.LogWarning("Key list file not found.");
            }
        }

        /// <summary>
        /// Ensures the key list is loaded only once.
        /// </summary>
        private static void EnsureLoaded()
        {
            if (!isLoaded)
            {
                LoadKeyList();
                isLoaded = true;
            }
        }

        #endregion

        #region Key Class

        [System.Serializable]
        public class Key
        {
            public string name;
            public float floatValue;
            public int intValue;
            public bool boolValue;
            public string stringValue;

            public Key(string name)
            {
                this.name = name;
            }
        }

        #endregion
    }
}
