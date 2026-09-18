using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Core.Items.Editor
{
    [CustomPropertyDrawer(typeof(EquipmentStatIdAttribute))]
    public sealed class EquipmentStatIdDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var ids = EquipmentStats.Ids.ToArray();
            var index = System.Array.IndexOf(ids, property.stringValue);
            var labels = new[] { string.IsNullOrEmpty(property.stringValue) ? "Select stat…" : "Unknown: " + property.stringValue }
                .Concat(ids).ToArray();
            EditorGUI.BeginProperty(position, label, property);
            var selected = EditorGUI.Popup(position, label.text, index + 1, labels);
            if (selected > 0) property.stringValue = ids[selected - 1];
            EditorGUI.EndProperty();
        }
    }
}
