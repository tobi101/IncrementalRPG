using System;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    [CustomPropertyDrawer(typeof(BigDouble))]
    public sealed class BigDoubleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mantissaProperty = property.FindPropertyRelative("mantissa");
            var exponentProperty = property.FindPropertyRelative("exponent");
            if (mantissaProperty == null || exponentProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "Invalid BigDouble serialization");
                return;
            }

            var current = BigDouble.Zero;
            try
            {
                current = new BigDouble(mantissaProperty.doubleValue, exponentProperty.longValue);
            }
            catch (ArgumentOutOfRangeException) { }
            catch (OverflowException) { }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var input = EditorGUI.DelayedTextField(position, label, current.ToScientificString());
            if (EditorGUI.EndChangeCheck() && BigDouble.TryParse(input, out var parsed))
            {
                mantissaProperty.doubleValue = parsed.Mantissa;
                exponentProperty.longValue = parsed.Exponent;
            }
            EditorGUI.EndProperty();
        }
    }
}
