using UnityEditor;
using UnityEngine;

namespace _TevLib.Extension.DoT.Editor
{
    [CustomEditor(typeof(TweenStepSO))]
    public sealed class TweenStepSOEditor : UnityEditor.Editor
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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            SerializedProperty actionType = Find(ActionTypeName);
            EditorGUILayout.PropertyField(actionType, ActionTypeLabel);

            SequenceActionType action = (SequenceActionType)actionType.enumValueIndex;
            if (IsSingleTween(action))
            {
                DrawTweenTimingFields();
            }
            else
            {
                SerializedProperty insertType = Find(InsertTypeName);
                EditorGUILayout.PropertyField(insertType, InsertTypeLabel);

                SequenceInsertType insert = (SequenceInsertType)insertType.enumValueIndex;
                if (UsesCallback(insert))
                {
                    EditorGUILayout.PropertyField(Find(CallbackName), CallbackLabel);
                }
                else if (UsesInterval(insert))
                {
                    EditorGUILayout.PropertyField(Find(DurationName), IntervalLabel);
                }
                else
                {
                    DrawTweenFields(action);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTweenFields(SequenceActionType action)
        {
            DrawTweenTimingFields();

            if (IsRotationAction(action))
                EditorGUILayout.PropertyField(Find(UsingFastBeyondName), FastBeyondLabel);

            if (UsesTransformValue(action))
            {
                DrawTransformValueFields(action);
                return;
            }

            EditorGUILayout.PropertyField(
                GetActionValueProperty(action),
                GetActionValueLabel(action));
        }

        private void DrawTransformValueFields(SequenceActionType action)
        {
            SerializedProperty randomizeValue = Find(IsRandomizeValueName);
            EditorGUILayout.PropertyField(randomizeValue, RandomizeValueLabel);

            GUIContent valueLabel = GetActionValueLabel(action);
            if (!randomizeValue.boolValue)
            {
                EditorGUILayout.PropertyField(Find(MinTransformValueName), valueLabel);
                return;
            }

            EditorGUILayout.PropertyField(
                Find(MinTransformValueName),
                new GUIContent($"Min {valueLabel.text}"));
            EditorGUILayout.PropertyField(
                Find(MaxTransformValueName),
                new GUIContent($"Max {valueLabel.text}"));
        }

        private void DrawTweenTimingFields()
        {
            EditorGUILayout.PropertyField(Find(EaseTypeName), EaseTypeLabel);
            EditorGUILayout.PropertyField(Find(DurationName), DurationLabel);
        }

        private SerializedProperty GetActionValueProperty(SequenceActionType action)
        {
            switch (action)
            {
                case SequenceActionType.DoCanvasAlpha:
                case SequenceActionType.DoFade:
                case SequenceActionType.DoFillAmount:
                    return Find(FadeValueName);
                case SequenceActionType.DoColor:
                    return Find(ColorValueName);
                default:
                    return Find(FadeValueName);
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

        private SerializedProperty Find(string propertyName)
            => serializedObject.FindProperty(propertyName);

        private static bool IsSingleTween(SequenceActionType action)
            => action == SequenceActionType.DoTween;

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
