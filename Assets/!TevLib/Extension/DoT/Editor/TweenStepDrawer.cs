using UnityEditor;
using UnityEngine;

namespace _TevLib.Extension.DoT.Editor
{
    [CustomPropertyDrawer(typeof(TweenStep))]
    public sealed class TweenStepDrawer : PropertyDrawer
    {
        private const string ActionTypeName = "<ActionType>k__BackingField";
        private const string InsertTypeName = "<InsertType>k__BackingField";
        private const string EaseTypeName = "<EaseType>k__BackingField";
        private const string DurationName = "<Duration>k__BackingField";
        private const string IsRandomizeValueName = "<IsRandomizeValue>k__BackingField";
        private const string UsingFastBeyondName = "<UsingFastBeyond>k__BackingField";
        private const string MinTransformValueName = "<MinTransformValue>k__BackingField";
        private const string MaxTransformValueName = "<MaxTransformValue>k__BackingField";
        private const string FadeValueName = "<FadeValue>k__BackingField";
        private const string ColorValueName = "<ColorValue>k__BackingField";
        private const string CallbackName = "<Callback>k__BackingField";

        private static readonly GUIContent InsertTypeLabel = new GUIContent("Insert Type");
        private static readonly GUIContent ActionTypeLabel = new GUIContent("Action Type");
        private static readonly GUIContent EaseTypeLabel = new GUIContent("Ease Type");
        private static readonly GUIContent DurationLabel = new GUIContent("Duration");
        private static readonly GUIContent IntervalLabel = new GUIContent("Interval");
        private static readonly GUIContent CallbackLabel = new GUIContent("Callback");
        private static readonly GUIContent PositionLabel = new GUIContent("Position");
        private static readonly GUIContent AnchoredPositionLabel = new GUIContent("Anchored Position");
        private static readonly GUIContent LocalScaleLabel = new GUIContent("Local Scale");
        private static readonly GUIContent LocalRotationLabel = new GUIContent("Local Rotation");
        private static readonly GUIContent LocalPositionLabel = new GUIContent("Local Position");
        private static readonly GUIContent RotationLabel = new GUIContent("Rotation");
        private static readonly GUIContent SizeDeltaLabel = new GUIContent("Size Delta");
        private static readonly GUIContent FadeValueLabel = new GUIContent("Fade Value");
        private static readonly GUIContent AlphaLabel = new GUIContent("Alpha");
        private static readonly GUIContent FillAmountLabel = new GUIContent("Fill Amount");
        private static readonly GUIContent ColorLabel = new GUIContent("Color");
        private static readonly GUIContent RandomizeValueLabel = new GUIContent("Randomize Value");
        private static readonly GUIContent FastBeyondLabel = new GUIContent("Fast Beyond 360");

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

            float y = foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = previousIndent + 1;

            SerializedProperty actionType = Find(property, ActionTypeName);
            DrawProperty(ref y, position, actionType, ActionTypeLabel);

            SequenceActionType action = (SequenceActionType)actionType.enumValueIndex;
            if (IsSingleTween(action))
            {
                DrawTweenTimingFields(ref y, position, property);
            }
            else
            {
                SerializedProperty insertType = Find(property, InsertTypeName);
                DrawProperty(ref y, position, insertType, InsertTypeLabel);

                SequenceInsertType insert = (SequenceInsertType)insertType.enumValueIndex;
                if (UsesCallback(insert))
                {
                    DrawProperty(ref y, position, Find(property, CallbackName), CallbackLabel);
                }
                else if (UsesInterval(insert))
                {
                    DrawProperty(ref y, position, Find(property, DurationName), IntervalLabel);
                }
                else
                {
                    DrawTweenFields(ref y, position, property, action);
                }
            }

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
                return height;

            SerializedProperty actionType = Find(property, ActionTypeName);
            height = AddHeight(height, actionType);

            SequenceActionType action = (SequenceActionType)actionType.enumValueIndex;
            if (IsSingleTween(action))
            {
                height = AddHeight(height, Find(property, EaseTypeName));
                return AddHeight(height, Find(property, DurationName));
            }

            SerializedProperty insertType = Find(property, InsertTypeName);
            height = AddHeight(height, insertType);
            SequenceInsertType insert = (SequenceInsertType)insertType.enumValueIndex;
            if (UsesCallback(insert))
                return AddHeight(height, Find(property, CallbackName));

            if (UsesInterval(insert))
                return AddHeight(height, Find(property, DurationName));

            height = AddHeight(height, Find(property, EaseTypeName));
            height = AddHeight(height, Find(property, DurationName));

            if (IsRotationAction(action))
                height = AddHeight(height, Find(property, UsingFastBeyondName));

            if (UsesTransformValue(action))
            {
                SerializedProperty randomizeValue = Find(property, IsRandomizeValueName);
                height = AddHeight(height, randomizeValue);
                height = AddHeight(height, Find(property, MinTransformValueName));

                if (randomizeValue.boolValue)
                    height = AddHeight(height, Find(property, MaxTransformValueName));

                return height;
            }

            return AddHeight(height, GetActionValueProperty(property, action));
        }

        private static void DrawTweenFields(
            ref float y,
            Rect position,
            SerializedProperty property,
            SequenceActionType action)
        {
            DrawTweenTimingFields(ref y, position, property);

            if (IsRotationAction(action))
                DrawProperty(
                    ref y,
                    position,
                    Find(property, UsingFastBeyondName),
                    FastBeyondLabel);

            if (UsesTransformValue(action))
            {
                DrawTransformValueFields(ref y, position, property, action);
                return;
            }

            DrawProperty(
                ref y,
                position,
                GetActionValueProperty(property, action),
                GetActionValueLabel(action));
        }

        private static void DrawTransformValueFields(
            ref float y,
            Rect position,
            SerializedProperty property,
            SequenceActionType action)
        {
            SerializedProperty randomizeValue = Find(property, IsRandomizeValueName);
            DrawProperty(ref y, position, randomizeValue, RandomizeValueLabel);

            GUIContent valueLabel = GetActionValueLabel(action);
            if (!randomizeValue.boolValue)
            {
                DrawProperty(ref y, position, Find(property, MinTransformValueName), valueLabel);
                return;
            }

            DrawProperty(
                ref y,
                position,
                Find(property, MinTransformValueName),
                new GUIContent($"Min {valueLabel.text}"));
            DrawProperty(
                ref y,
                position,
                Find(property, MaxTransformValueName),
                new GUIContent($"Max {valueLabel.text}"));
        }

        private static void DrawTweenTimingFields(
            ref float y,
            Rect position,
            SerializedProperty property)
        {
            DrawProperty(ref y, position, Find(property, EaseTypeName), EaseTypeLabel);
            DrawProperty(ref y, position, Find(property, DurationName), DurationLabel);
        }

        private static bool IsSingleTween(SequenceActionType action)
            => action == SequenceActionType.DoTween;

        private static SerializedProperty GetActionValueProperty(
            SerializedProperty property,
            SequenceActionType action)
        {
            switch (action)
            {
                case SequenceActionType.DoCanvasAlpha:
                case SequenceActionType.DoFade:
                case SequenceActionType.DoFillAmount:
                    return Find(property, FadeValueName);
                case SequenceActionType.DoColor:
                    return Find(property, ColorValueName);
                default:
                    return Find(property, FadeValueName);
            }
        }

        private static GUIContent GetActionValueLabel(SequenceActionType action)
        {
            switch (action)
            {
                case SequenceActionType.DoAnchoredPosition:
                    return AnchoredPositionLabel;
                case SequenceActionType.DoCanvasAlpha:
                    return FadeValueLabel;
                case SequenceActionType.DoLocalScale:
                    return LocalScaleLabel;
                case SequenceActionType.DoLocalRotation:
                    return LocalRotationLabel;
                case SequenceActionType.DoColor:
                    return ColorLabel;
                case SequenceActionType.DoFade:
                    return AlphaLabel;
                case SequenceActionType.DoLocalMove:
                    return LocalPositionLabel;
                case SequenceActionType.DoRotate:
                    return RotationLabel;
                case SequenceActionType.DoSizeDelta:
                    return SizeDeltaLabel;
                case SequenceActionType.DoFillAmount:
                    return FillAmountLabel;
                default:
                    return PositionLabel;
            }
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
            => action != SequenceActionType.DoTween
               && action != SequenceActionType.DoCanvasAlpha
               && action != SequenceActionType.DoColor
               && action != SequenceActionType.DoFade
               && action != SequenceActionType.DoFillAmount;

        private static bool IsRotationAction(SequenceActionType action)
            => action == SequenceActionType.DoRotate
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
