using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Akila.FPSFramework
{
    [CustomEditor(typeof(Actor))]
    public class ActorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}