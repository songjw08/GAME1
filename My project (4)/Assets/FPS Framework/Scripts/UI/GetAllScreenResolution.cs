using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Akila.FPSFramework.UI;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/UI/Get All Screen Resolution")]
    public class GetAllScreenResolution : MonoBehaviour
    {
        private Dropdown dropdown;
        private CarouselSelector carouselSelector;

        private void Awake()
        {
            RegenrateList();
        }

        private void Start()
        {
            dropdown = GetComponent<Dropdown>();
            carouselSelector = GetComponent<CarouselSelector>();
        }

        [ContextMenu("Regenrate List")]
        private void RegenrateList()
        {
            dropdown = GetComponent<Dropdown>();
            carouselSelector = GetComponent<CarouselSelector>();

            List<string> carouselSelectorOptions = new List<string>();
            List<Dropdown.OptionData> dropdownOptions = new List<Dropdown.OptionData>();
            List<Resolution> resolutions = Screen.resolutions.ToList();

            dropdown?.ClearOptions();
            carouselSelector?.ClearOptions();

            foreach (Resolution resolution in FPSFrameworkCore.GetResolutions())
            {
                string resText = $"{resolution.width}x{resolution.height} {(int)resolution.refreshRateRatio.value}Hz";

                carouselSelectorOptions.Add(resText);
                dropdownOptions.Add(new Dropdown.OptionData() { text = resText });
            }

            dropdown?.AddOptions(dropdownOptions);
            carouselSelector?.AddOptions(carouselSelectorOptions.ToArray());

            int currentResIndex = 0;

            currentResIndex = resolutions.IndexOf(Screen.currentResolution);

            if (dropdown != null)
                dropdown.value = currentResIndex;

            if (carouselSelector != null) 
                carouselSelector.value = currentResIndex;
        }
    }
}