using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Akila.FPSFramework
{
    public interface IDamageable
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }

        public float health { get; set; }
        public float maxHealth { get; set; }
        public void Damage(float amount, GameObject damageSource);

        public  Vector3 damageDirection { get; set; }
        public bool deadConfirmed { get; set; }
        public GameObject damageSource { get; set; }
        public UnityEvent onDeath { get; }  
    }
}