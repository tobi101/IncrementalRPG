using UnityEditor;
using UnityEngine;

namespace Core.TestSkillTree.Editor
{
    [CustomPropertyDrawer(typeof(NodeEffect))]
    public class NodeEffectDrawer : PropertyDrawer
    {
        private const float LineHeight = 18f;
        private const float Spacing    = 2f;
        private const float RowStep    = LineHeight + Spacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var effectType = (NodeEffectType)property.FindPropertyRelative("effectType").enumValueIndex;

            float height = RowStep; // effectType

            if (effectType == NodeEffectType.FeatureUnlock)
            {
                height += RowStep; // feature
            }
            else
            {
                height += RowStep; // statType
                height += RowStep; // valuesPerLevel foldout

                var valuesProp = property.FindPropertyRelative("valuesPerLevel");
                if (valuesProp.isExpanded)
                {
                    int maxLevel = property.serializedObject.FindProperty("maxLevel").intValue;
                    height += RowStep * maxLevel;
                }
            }

            return height + Spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var effectTypeProp = property.FindPropertyRelative("effectType");
            var statTypeProp   = property.FindPropertyRelative("statType");
            var valuesProp     = property.FindPropertyRelative("valuesPerLevel");
            var featureProp    = property.FindPropertyRelative("feature");

            var effectType = (NodeEffectType)effectTypeProp.enumValueIndex;
            int maxLevel   = property.serializedObject.FindProperty("maxLevel").intValue;

            var row = new Rect(position.x, position.y + Spacing, position.width, LineHeight);

            EditorGUI.PropertyField(row, effectTypeProp);
            row.y += RowStep;

            if (effectType == NodeEffectType.FeatureUnlock)
            {
                EditorGUI.PropertyField(row, featureProp);
            }
            else
            {
                EditorGUI.PropertyField(row, statTypeProp);
                row.y += RowStep;

                // Sync array size to maxLevel
                if (valuesProp.arraySize != maxLevel)
                    valuesProp.arraySize = maxLevel;

                // Foldout header (no Size field)
                valuesProp.isExpanded = EditorGUI.Foldout(row, valuesProp.isExpanded, "Values Per Level", true);
                row.y += RowStep;

                if (valuesProp.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < maxLevel; i++)
                    {
                        EditorGUI.PropertyField(row, valuesProp.GetArrayElementAtIndex(i), new GUIContent($"Level {i + 1}"));
                        row.y += RowStep;
                    }
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
