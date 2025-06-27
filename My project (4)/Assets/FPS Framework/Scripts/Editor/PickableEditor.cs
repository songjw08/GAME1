using UnityEditor;
using UnityEngine;
using Akila.FPSFramework;

namespace Akila.FPSFramework
{
    [CustomEditor(typeof(Pickable))]
    public class PickableEditor : Editor
    {
        private SerializedProperty nameProp;
        private SerializedProperty interactionNameProp;
        private SerializedProperty typeProp;
        private SerializedProperty interactSoundProp;
        private SerializedProperty itemProp;
        private SerializedProperty collectableIdentifierProp;
        private SerializedProperty collectableCountProp;

        private void OnEnable()
        {
            nameProp = serializedObject.FindProperty("Name");
            interactionNameProp = serializedObject.FindProperty("interactionName");
            typeProp = serializedObject.FindProperty("type");
            interactSoundProp = serializedObject.FindProperty("interactSound");
            itemProp = serializedObject.FindProperty("item");
            collectableIdentifierProp = serializedObject.FindProperty("collectableIdentifier");
            collectableCountProp = serializedObject.FindProperty("collectableCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Pickable Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(nameProp, new GUIContent("Name"));
            EditorGUILayout.PropertyField(interactionNameProp, new GUIContent("Hint Text"));
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Type"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(interactSoundProp, new GUIContent("Interaction Sound"));

            PickableType type = (PickableType)typeProp.enumValueIndex;

            EditorGUILayout.Space();

            if (type == PickableType.Item)
            {
                EditorGUILayout.LabelField("Item Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(itemProp, new GUIContent("Item To Pickup"));
            }
            else if (type == PickableType.Collectable)
            {
                EditorGUILayout.LabelField("Collectable Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(collectableIdentifierProp, new GUIContent("Collectable Identifier"));
                EditorGUILayout.PropertyField(collectableCountProp, new GUIContent("Amount To Collect"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}