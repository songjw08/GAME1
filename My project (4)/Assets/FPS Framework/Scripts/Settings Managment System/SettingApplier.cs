using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine;
using System;
using Akila.FPSFramework.UI;
using System.Linq;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/UI/Settings Menu/Setting Applier")]
    public class SettingApplier : MonoBehaviour
    {
        public string path = "Section/Option";
        public int selectedPathIndex;

        public SettingsManager settingsManager { get; set; }

        float sliderValue;
        bool toggleValue;
        int dropdownValue;
        int carouselSelectorValue;

        Toggle toggle;
        Slider slider;
        Dropdown dropdown;
        CarouselSelector carouselSelector;

        public void Save()
        {
            toggle = transform.SearchFor<Toggle>(true);
            slider = transform.SearchFor<Slider>(true);
            dropdown = transform.SearchFor<Dropdown>(true);
            carouselSelector = transform.SearchFor<CarouselSelector>(true);

            if (slider != null)
            {
                sliderValue = slider.value;
            }

            if (toggle != null)
            {
                toggleValue = toggle.isOn;
            }

            if (dropdown != null)
            {
                dropdownValue = dropdown.value;
            }

            if (carouselSelector != null)
            {
                carouselSelectorValue = carouselSelector.value;
            }

            SaveSystem.Save<float>(path, sliderValue);
            SaveSystem.Save<int>(path, dropdownValue);
            SaveSystem.Save<int>(path, carouselSelectorValue);
            SaveSystem.Save<bool>(path, toggleValue);
        }

        public void Load(bool apply = true)
        {
            Button button = transform.SearchFor<Button>(true);
            Toggle toggle = transform.SearchFor<Toggle>(true);
            Slider slider = transform.SearchFor<Slider>(true);
            Dropdown dropdown = transform.SearchFor<Dropdown>(true);
            CarouselSelector selector = transform.SearchFor<CarouselSelector>(true);

            if (button && !selector)
            {
                bool value = SaveSystem.Load<bool>(path);

                if (SaveSystem.HasKey(path))
                {
                    if(apply) Apply(toggleValue);
                    toggleValue = value;
                }
                else if (apply)
                    Apply(false);
            }

            if (toggle)
            {
                bool value = SaveSystem.Load<bool>(path);

                toggleValue = value;


                if (SaveSystem.HasKey(path))
                {
                    if (apply) Apply(toggleValue);
                    toggle.isOn = toggleValue;
                }
                else if(apply)
                    Apply(toggle.isOn);
            }

            if (slider)
            {
                float value = SaveSystem.Load<float>(path);

                if (SaveSystem.HasKey(path))
                {
                    if (apply) Apply(value);
                    slider.value = value;
                }
                else if (apply)
                    Apply(slider.value);
            }

            if (dropdown)
            {
                int value = SaveSystem.Load<int>(path);



                if (SaveSystem.HasKey(path))
                {
                    if (apply) Apply(dropdownValue);
                    dropdown.value = value;
                    dropdownValue = value;
                }
                else if (apply)
                    Apply(dropdown.value);
            }

            if (selector)
            {
                int value = SaveSystem.Load<int>(path);

                if (SaveSystem.HasKey(path))
                {
                    if (apply) Apply(value);
                    selector.value = value;
                    carouselSelectorValue = value;
                }
                else if (apply)
                    Apply(selector.value);
            }
        }

        public void SaveAll()
        {
            foreach (SettingApplier applier in FindObjectsOfType<SettingApplier>())
                applier.Save();
        }

        public void LoadAll(bool apply)
        {
            foreach (SettingApplier applier in FindObjectsOfType<SettingApplier>())
                applier.Load(apply);
        }

        private void OnDisable()
        {
            Save();
        }

        private void OnDestroy()
        {
            Save();
        }

        private void Start()
        {
            settingsManager = FindObjectOfType<SettingsManager>();

            Button button = transform.SearchFor<Button>();
            Toggle toggle = transform.SearchFor<Toggle>();
            Slider slider = transform.SearchFor<Slider>();
            Dropdown dropdown = transform.SearchFor<Dropdown>();
            CarouselSelector selector = transform.SearchFor<CarouselSelector>();

            if (button && !selector)
            {
                button.onClick.AddListener(() => { toggleValue = !toggleValue; Apply(toggleValue); });
            }

            if (settingsManager.autoApply)
            {
                if (toggle)
                {
                    toggle.onValueChanged.AddListener(Apply);
                }

                if (slider)
                {
                    slider.onValueChanged.AddListener(Apply);
                }

                if (dropdown)
                {
                    dropdown.onValueChanged.AddListener(Apply);
                }

                if (selector)
                {
                    selector.onValueChange?.AddListener(Apply);
                }
            }

            Load();
        }

        public void Apply(float value)
        {
            Apply(value, 0, false, 0);
        }

        public void Apply(int value)
        {
            Apply(0, value, false, 1);
        }

        public void Apply(bool value)
        {
            Apply(0, 0, value, 2);
        }

        private void Apply(float floatValue = 0, int intValue = 0, bool boolValue = false, float type = 0)
        {

            if (!settingsManager) return;

            string fileName = null;

            if(GetOption(ref fileName) == null)
            {
                Debug.LogError("Option not set.", gameObject);

                return;
            }

            string functionName = GetOption(ref fileName).functionName;

            SettingsPreset preset = settingsManager.settingsPresets.ToList().Find(p => p.name == fileName);

            MethodInfo method = preset.GetType().GetMethod(functionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method != null)
            {
                object[] parameters = null;

                if (type == 0) parameters = new object[] { (float)floatValue };
                if (type == 1) parameters = new object[] { (int)intValue };
                if (type == 2) parameters = new object[] { (bool)boolValue };

                try
                {
                    method.Invoke(preset, parameters);
                }
                catch (TargetInvocationException ex)
                {
                    // Unwrap the inner exception to get the actual exception thrown by the invoked method
                    Exception innerException = ex.InnerException;

                    if (innerException != null)
                    {
                        Debug.LogError($"An error occurred while invoking '{functionName}': {innerException.Message}\n{innerException.StackTrace}");
                    }
                    else
                    {
                        Debug.LogError($"An unknown error occurred while invoking '{functionName}': {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected error while invoking '{functionName}': {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogError($"Function '{functionName}' not found in preset.");
            }
        }

        public void ApplyAll()
        {
            foreach(SettingApplier applier in FindObjectsOfType<SettingApplier>(true))
            {
                if (applier.toggle)
                {
                    applier.Apply(applier.toggle.isOn);
                }

                if (applier.slider)
                {
                    applier.Apply(applier.slider.value);
                }

                if (applier.dropdown)
                {
                    applier.Apply(applier.dropdown.value);
                }

                if (applier.carouselSelector)
                {
                    applier.Apply(applier.carouselSelector.value);
                }
            }

            SaveAll();
        }

        public SettingOption GetOption(ref string _fileName)
        {
            if (!settingsManager) return null;

            string[] pathParts = path.Split('/');

            if (pathParts.Length < 2)
            {
                Debug.LogError("Invalid path format. Please use 'Section/Option' format.");
                return null;
            }


            string fileName = pathParts[0];
            string sectionName = pathParts[1];
            string optionName = pathParts[2];
            
            _fileName = fileName;

            SettingSection section = settingsManager.GetSection(fileName, sectionName);

            if (section == null)
            {
                Debug.LogError("Section '" + sectionName + "' not found.");
                return null;
            }

            SettingOption option = section.GetOption(optionName);

            if (option == null)
            {
                Debug.LogError("Option '" + optionName + "' not found in section '" + sectionName + "'.");
                return null;
            }

            return option;
        }
    }
}