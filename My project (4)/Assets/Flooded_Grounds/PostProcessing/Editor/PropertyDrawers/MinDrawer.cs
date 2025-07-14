using UnityEngine;
using UnityEditor;

namespace UnityEditor.CustomDrawers
{
    [CustomPropertyDrawer(typeof(MinAttribute))]
    sealed class MinDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MinAttribute minAttribute = (MinAttribute)attribute;
            float min = minAttribute.min;

            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int value = EditorGUI.IntField(position, label, property.intValue);
                property.intValue = Mathf.Max(value, (int)min);
            }
            else if (property.propertyType == SerializedPropertyType.Float)
            {
                float value = EditorGUI.FloatField(position, label, property.floatValue);
                property.floatValue = Mathf.Max(value, min);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [Min] with float or int.");
            }

            EditorGUI.EndProperty();
        }
    }
}