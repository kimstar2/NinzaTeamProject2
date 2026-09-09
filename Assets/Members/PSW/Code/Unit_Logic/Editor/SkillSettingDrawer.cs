using Members.PSW.Code.Unit_Logic.Runtime.Skill;
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
            int lines = property.isExpanded ? (GetValueProperty(property) != null ? 4 : 3) : 1;
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
                case SkillType.Debuff:
                    return property.FindPropertyRelative(nameof(SkillSetting.debuff));
                case SkillType.Shield:
                    return property.FindPropertyRelative(nameof(SkillSetting.shield));
                default:
                    return null;
            }
        }
    }
}
