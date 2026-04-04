using UnityEditor;
using UnityEngine;

namespace Core.TestSkillTree.Editor
{
    [CustomPropertyDrawer(typeof(NodeEffect))]
    public class NodeEffectDrawer : PropertyDrawer
    {
        private const float LineHeight = 20f;
        private const float Spacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var effectType = (NodeEffectType)property.FindPropertyRelative("effectType").enumValueIndex;
            int lines = effectType == NodeEffectType.FeatureUnlock ? 2 : 3;
            return lines * (LineHeight + Spacing) + Spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var effectTypeProp = property.FindPropertyRelative("effectType");
            var statTypeProp   = property.FindPropertyRelative("statType");
            var valuesProp     = property.FindPropertyRelative("valuesPerLevel");
            var featureProp    = property.FindPropertyRelative("feature");

            var effectType = (NodeEffectType)effectTypeProp.enumValueIndex;

            var row = new Rect(position.x, position.y + Spacing, position.width, LineHeight);

            EditorGUI.PropertyField(row, effectTypeProp);
            row.y += LineHeight + Spacing;

            if (effectType == NodeEffectType.FeatureUnlock)
            {
                EditorGUI.PropertyField(row, featureProp);
            }
            else
            {
                EditorGUI.PropertyField(row, statTypeProp);
                row.y += LineHeight + Spacing;
                EditorGUI.PropertyField(row, valuesProp, true);
            }

            EditorGUI.EndProperty();
        }
    }
}
