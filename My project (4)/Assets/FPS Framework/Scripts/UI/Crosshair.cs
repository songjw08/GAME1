using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/UI/Crosshair"), RequireComponent(typeof(CanvasGroup))]

    public class Crosshair : MonoBehaviour
    {
        public float size = 1;
        public float smoothness = 10;

        public Color color = Color.white;
        public RectTransform crosshairHolder;

        [ReadOnly] public Firearm firearm;

        private float amount;

        private CanvasGroup canvasGroup;

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            if(firearm == null)
            {
                return;
            }

            foreach(Image image in crosshairHolder.GetComponentsInChildren<Image>())
            {
                image.color = color;
            }

            canvasGroup.alpha = Mathf.Lerp(1, 0, firearm.aimProgress * 2);
            
            amount = Mathf.Lerp(amount, firearm.currentSprayAmount, Time.deltaTime * smoothness);

            crosshairHolder.sizeDelta = Vector2.one * size * amount;
        }
    }
}