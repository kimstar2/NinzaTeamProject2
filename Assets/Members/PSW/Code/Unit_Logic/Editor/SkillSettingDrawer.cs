using Members.PSW.Code.Unit_Logic.Runtime.Structs;
using UnityEditor;
using UnityEngine;

namespace Members.PSW.Code.Unit_Logic.Editor
{
    [CustomPropertyDrawer(typeof(SkillSetting))]
    public class SkillSettingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            int previousIndent = EditorGUI.indentLevel;

            try
            {
                position.height = EditorGUIUtility.singleLineHeight;
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
                if (!property.isExpanded)
                    return;

                EditorGUI.indentLevel++;
                SerializedProperty skillType = property.FindPropertyRelative(nameof(SkillSetting.skillType));
                DrawNextProperty(ref position, skillType);
                SerializedProperty useSelf = property.FindPropertyRelative(nameof(SkillSetting.useSelf));
                DrawNextProperty(ref position, useSelf);

                SerializedProperty enemyTargetAll = property.FindPropertyRelative(nameof(SkillSetting.enemyTargetAll));
                DrawNextProperty(ref position, enemyTargetAll);
                SerializedProperty teamTargetAll = property.FindPropertyRelative(nameof(SkillSetting.teamTargetAll));
                DrawNextProperty(ref position, teamTargetAll);

                SerializedProperty value = GetValueProperty(property);
                if (value != null)
                    DrawNextProperty(ref position, value);
            }
            finally
            {
                EditorGUI.indentLevel = previousIndent;
                EditorGUI.EndProperty();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Foldout, skill type, three targeting toggles, and the selected skill value.
            int lines = property.isExpanded ? (GetValueProperty(property) != null ? 6 : 5) : 1;
            if (property.isExpanded)
            {
                SerializedProperty useSelf = property.FindPropertyRelative(nameof(SkillSetting.useSelf));
                if (!useSelf.hasMultipleDifferentValues && useSelf.boolValue)
                    lines++;
            }

            return lines * EditorGUIUtility.singleLineHeight
                + (lines - 1) * EditorGUIUtility.standardVerticalSpacing;
        }

        private static void DrawNextProperty(ref Rect position, SerializedProperty property)
        {
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(position, property);
        }

        private static SerializedProperty GetValueProperty(SerializedProperty property)
        {
            SerializedProperty skillType = property.FindPropertyRelative(nameof(SkillSetting.skillType));
            if (skillType.hasMultipleDifferentValues)
                return null;

            switch ((SkillType)skillType.intValue)
            {
                case SkillType.Damage:
                    return property.FindPropertyRelative(nameof(SkillSetting.damage));
                case SkillType.Heal:
                    return property.FindPropertyRelative(nameof(SkillSetting.heal));
                case SkillType.Buff:
                    return property.FindPropertyRelative(nameof(SkillSetting.buff));
                case SkillType.Shield:
                    return property.FindPropertyRelative(nameof(SkillSetting.shield));
                default:
                    return null;
            }
        }
    }
}
