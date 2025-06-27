using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    public class AimAssistTarget : MonoBehaviour
    {
        public Damageable healthSystem { get; private set; }

        private void Awake()
        {
            healthSystem = transform.SearchFor<Damageable>();
        }
    }
}