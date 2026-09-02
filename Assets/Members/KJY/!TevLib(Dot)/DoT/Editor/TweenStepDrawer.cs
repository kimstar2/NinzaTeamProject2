using Members.KJY._TevLib_Dot_.DoT;
using UnityEditor;
using UnityEngine;

namespace _TevLib.DoT__Extension_.Editor
{
    [CustomPropertyDrawer(typeof(TweenStep))]
    public sealed class TweenStepDrawer : PropertyDrawer
    {
        private const string ActionTypeName = "<ActionType>k__BackingField";
        private const string InsertTypeName = "<InsertType>k__BackingField";
        private const string EaseTypeName = "<EaseType>k__BackingField";
        private const string DurationName = "<Duration>k__BackingField";
        private const string TransformValueName = "<TransformValue>k__BackingField";
        private const string FadeValueName = "<FadeValue>k__BackingField";
        private const string IntervalName = "<Interval>k__BackingField";
        private const string CallbackName = "<Callback>k__BackingField";

        private static readonly GUIContent ActionTypeLabel = new GUIContent("Action Type");
        private static readonly GUIContent InsertTypeLabel = new GUIContent("Insert Type");
        private static readonly GUIContent EaseTypeLabel = new GUIContent("Ease Type");
        private static readonly GUIContent DurationLabel = new GUIContent("Duration");
        private static readonly GUIContent PositionLabel = new GUIContent("Position");
        private static readonly GUIContent AnchoredPositionLabel = new GUIContent("Anchored Position");
        private static readonly GUIContent LocalScaleLabel = new GUIContent("Local Scale");
        private static readonly GUIContent LocalRotationLabel = new GUIContent("Local Rotation");
        private static readonly GUIContent FadeValueLabel = new GUIContent("Fade Value");
        private static readonly GUIContent IntervalLabel = new GUIContent("Interval");
        private static readonly GUIContent CallbackLabel = new GUIContent("Callback");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                property.isExpanded,
                label,
                true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            SerializedProperty actionType = Find(property, ActionTypeName);
            SerializedProperty insertType = Find(property, InsertTypeName);
            SerializedProperty easeType = Find(property, EaseTypeName);
            SerializedProperty duration = Find(property, DurationName);
            SerializedProperty transformValue = Find(property, TransformValueName);
            SerializedProperty fadeValue = Find(property, FadeValueName);
            SerializedProperty interval = Find(property, IntervalName);
            SerializedProperty callback = Find(property, CallbackName);

            float y = foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = previousIndent + 1;

            DrawProperty(ref y, position, actionType, ActionTypeLabel);
            DrawProperty(ref y, position, insertType, InsertTypeLabel);

            SequenceActionType action = (SequenceActionType)actionType.enumValueIndex;
            if (action != SequenceActionType.DoNone)
                DrawProperty(ref y, position, easeType, EaseTypeLabel);

            DrawProperty(ref y, position, duration, DurationLabel);
            DrawActionValue(ref y, position, action, transformValue, fadeValue);

            SequenceInsertType insert = (SequenceInsertType)insertType.enumValueIndex;
            DrawInsertValue(ref y, position, insert, interval, callback);

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
                return height;

            SerializedProperty actionType = Find(property, ActionTypeName);
            SerializedProperty insertType = Find(property, InsertTypeName);

            height = AddHeight(height, Find(property, ActionTypeName));
            height = AddHeight(height, Find(property, InsertTypeName));

            SequenceActionType action = (SequenceActionType)actionType.enumValueIndex;
            if (action != SequenceActionType.DoNone)
                height = AddHeight(height, Find(property, EaseTypeName));

            height = AddHeight(height, Find(property, DurationName));

            if (UsesTransformValue(action))
                height = AddHeight(height, Find(property, TransformValueName));
            else if (action == SequenceActionType.DoCanvasAlpha)
                height = AddHeight(height, Find(property, FadeValueName));

            SequenceInsertType insert = (SequenceInsertType)insertType.enumValueIndex;
            if (UsesInterval(insert))
                height = AddHeight(height, Find(property, IntervalName));
            else if (UsesCallback(insert))
                height = AddHeight(height, Find(property, CallbackName));

            return height;
        }

        private static void DrawActionValue(
            ref float y,
            Rect position,
            SequenceActionType action,
            SerializedProperty transformValue,
            SerializedProperty fadeValue)
        {
            switch (action)
            {
                case SequenceActionType.DoAnchoredPosition:
                    DrawProperty(ref y, position, transformValue, AnchoredPositionLabel);
                    break;
                case SequenceActionType.DoCanvasAlpha:
                    DrawProperty(ref y, position, fadeValue, FadeValueLabel);
                    break;
                case SequenceActionType.DoLocalScale:
                    DrawProperty(ref y, position, transformValue, LocalScaleLabel);
                    break;
                case SequenceActionType.DoMove:
                    DrawProperty(ref y, position, transformValue, PositionLabel);
                    break;
                case SequenceActionType.DoLocalRotation:
                    DrawProperty(ref y, position, transformValue, LocalRotationLabel);
                    break;
            }
        }

        private static void DrawInsertValue(
            ref float y,
            Rect position,
            SequenceInsertType insert,
            SerializedProperty interval,
            SerializedProperty callback)
        {
            if (UsesInterval(insert))
                DrawProperty(ref y, position, interval, IntervalLabel);
            else if (UsesCallback(insert))
                DrawProperty(ref y, position, callback, CallbackLabel);
        }

        private static void DrawProperty(
            ref float y,
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true);
            Rect propertyRect = new Rect(position.x, y, position.width, height);
            EditorGUI.PropertyField(propertyRect, property, label, true);
            y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        private static float AddHeight(float currentHeight, SerializedProperty property)
            => currentHeight
               + EditorGUIUtility.standardVerticalSpacing
               + EditorGUI.GetPropertyHeight(property, true);

        private static SerializedProperty Find(SerializedProperty property, string relativeName)
            => property.FindPropertyRelative(relativeName);

        private static bool UsesTransformValue(SequenceActionType action)
            => action == SequenceActionType.DoAnchoredPosition
               || action == SequenceActionType.DoLocalScale
               || action == SequenceActionType.DoMove
               || action == SequenceActionType.DoLocalRotation;

        private static bool UsesInterval(SequenceInsertType insert)
            => insert == SequenceInsertType.PrependInterval
               || insert == SequenceInsertType.AppendInterval;

        private static bool UsesCallback(SequenceInsertType insert)
            => insert == SequenceInsertType.PrependCallback
               || insert == SequenceInsertType.AppendCallback
               || insert == SequenceInsertType.JoinCallback;
    }
}
