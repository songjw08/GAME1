using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Health System/Damageable Group")]
    public class DamageableGroup : MonoBehaviour, IDamageableGroup
    {
        public HumanBodyBones bone;
        public float damageMultipler = 1;

        private IDamageable damageable;

        public string uniqueID => throw new System.NotImplementedException();

        private void Start()
        {
            damageable = GetComponentInParent<IDamageable>();
        }

        public IDamageable GetDamageable()
        {
            return damageable;
        }

        public HumanBodyBones GetBone()
        {
            return bone;
        }

        public float GetDamageMultipler()
        {
            return damageMultipler;
        }
    }
}