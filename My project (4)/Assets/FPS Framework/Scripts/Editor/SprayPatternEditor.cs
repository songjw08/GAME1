using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Akila.FPSFramework
{
    [CustomEditor(typeof(SprayPattern))]
    public class SprayPatternEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SprayPattern pattern = (SprayPattern)target;

            Undo.RecordObject(pattern, $"Modified {pattern}");
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Spray Settings", EditorStyles.boldLabel);
            pattern.totalAmount = EditorGUILayout.FloatField("Total Amount", pattern.totalAmount);

            pattern.passiveMultiplier = EditorGUILayout.Slider("Passive Multiplier", pattern.passiveMultiplier, 0, 1);
            pattern.rampUpTime = EditorGUILayout.Slider("Ramp Up Time", pattern.rampUpTime, 0, 1);
            pattern.recoveryTime = EditorGUILayout.Slider("Recovery Time", pattern.recoveryTime, 0, 1);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recoil Settings", EditorStyles.boldLabel);
            pattern.isRandomized = EditorGUILayout.Toggle("Is Randomized", pattern.isRandomized);

            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(pattern.isRandomized);
            pattern.verticalRecoil = EditorGUILayout.CurveField("Vertical Recoil", pattern.verticalRecoil);
            pattern.horizontalRecoil = EditorGUILayout.CurveField("Horizontal Recoil", pattern.horizontalRecoil);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(pattern);
            }
        }
    }
}