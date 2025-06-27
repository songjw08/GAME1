using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework
{
    ///Class name is a shortcut for 'Akila Main Scriptable Object'
    public class AMSO : ScriptableObject
    {
        public bool shortenMenus = false;
        public float masterAudioVolume = 1;
        public float masterAnimationSpeed = 1;
        public int maxAnimationFramerate = 120;
    }
}