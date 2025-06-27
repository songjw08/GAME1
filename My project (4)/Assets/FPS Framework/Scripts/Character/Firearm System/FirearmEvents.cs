using System;
using UnityEngine.Events;

namespace Akila.FPSFramework
{
    [Serializable]
    public class FirearmEvents
    {
        public UnityEvent OnFire;
        public UnityEvent onReloadStart;
        public UnityEvent OnReload;
        public UnityEvent OnReloadComplete;
        public UnityEvent OnReloadCancel;
        public UnityEvent OnFireModeChange;
    }
}