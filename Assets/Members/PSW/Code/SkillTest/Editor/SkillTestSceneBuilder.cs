using System;
using System.Linq;
using _LumenLib.PoolingSystem.Runtime;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using Members.KJY._01.Scripts.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Members.PSW.Code.SkillTest.Editor
{
    public static class SkillTestSceneBuilder
    {
        public const string ScenePath = "Assets/Members/PSW/Scene/SkillTestScene.unity";
        private static TMP_FontAsset _font;

        [MenuItem("Tools/PSW/Rebuild Skill Test Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before rebuilding the skill test scene.");
            if (SceneManager.GetSceneByPath(ScenePath).isLoaded)
                throw new InvalidOperationException("Close SkillTestScene before rebuilding it.");
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Members/KJY/10.Fonts/MonaS10.asset");
                var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
                camera.tag = "MainCamera";
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.orthographic = true;
                camera.orthographicSize = 4.5f;
                camera.rect = new Rect(0f, 0f, 0.65f, 1f);
                camera.backgroundColor = new Color(0.08f, 0.1f, 0.16f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.allowHDR = true;
                var cameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                cameraData.renderPostProcessing = true;
                cameraData.volumeLayerMask = 1;
                var volume = new GameObject("Global Volume").AddComponent<Volume>();
                volume.isGlobal = true;
                volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                    "Assets/Members/PSW/Scene/SkillTestScene/Global Volume Profile.asset");
                var light = new GameObject("Battle Light 2D").AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.pointLightOuterRadius = 100f;
                CopyBackground();

                var pool = new GameObject("Battle Particle Pool").AddComponent<ObjectPool>();
                Set(pool, "poolList", AssetDatabase.LoadAssetAtPath<PoolingListSO>("Assets/Members/KJY/00.GameModule/Pool/Pooling List.asset"));
                var types = Find<PlayerDataSO>();
                var enemyData = Find<EnemyDataSO>().First(data => data.EnemyAc != null && data.EnemyImage != null);
                var layout = new GameObject("Test Combatants").AddComponent<TweenLayoutGroup>();
                var actors = new[]
                {
                    Actor("시전자", types[0], new Vector3(-2.4f, 0f), layout, false),
                    Actor("아군 1", types[1], new Vector3(-2.8f, 1.8f), layout, false),
                    Actor("아군 2", types[2], new Vector3(-2.8f, -1.8f), layout, false),
                    Actor("적 1", enemyData, new Vector3(2.3f, 1.8f), layout, true),
                    Actor("적 2", enemyData, new Vector3(2.6f, 0f), layout, true),
                    Actor("적 3", enemyData, new Vector3(2.3f, -1.8f), layout, true)
                };
                var controller = new GameObject("Skill Test Controller").AddComponent<SkillTestController>();
                var skillRoot = new GameObject("Active Skill").transform;
                Set(controller, "caster", actors[0]); Set(controller, "skillParent", skillRoot);
                SetArray(controller, "actors", actors); SetArray(controller, "playerTypes", types);
                SetArray(controller, "enemyTypes", Find<EnemyDataSO>()
                    .Where(data => data.EnemyAc != null && data.EnemyImage != null && data.ImageColor != null)
                    .GroupBy(data => data.AttackType).OrderBy(group => group.Key).Select(group => group.First()).ToArray());
                var skills = Find<SkillDataSO>().OrderBy(skill => skill.SkillName).ThenBy(AssetDatabase.GetAssetPath).ToArray();
                SetArray(controller, "skills", skills);

                var canvas = new GameObject("Skill Test UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1600f, 900f);
                scaler.matchWidthOrHeight = 0.5f;
                var jobs = Rect("Unit Jobs", canvas.transform, Vector2.zero, new Vector2(.65f, .16f));
                jobs.gameObject.AddComponent<Image>().color = new Color(.065f, .075f, .11f, .92f);
                var autoButton = Button("Auto Job", jobs, "시전자 자동 직업: 꺼짐");
                Place((RectTransform)autoButton.transform, 18, 5, 310, 30);
                UnityEventTools.AddPersistentListener(autoButton.onClick, controller.ToggleAutoJob);
                Set(controller, "autoJobButton", autoButton);
                var jobHint = Label("Job Hint", jobs, "직업 버튼을 누르면 변경 · 수동 선택한 직업 유지", 17);
                Place(jobHint.rectTransform, 350, 5, 660, 30);
                var jobButtons = actors.Select((actor, index) =>
                {
                    var button = Button(actor.name + " Job", jobs, actor.name + " 직업 변경 ▶");
                    Place((RectTransform)button.transform, 18 + index % 3 * 340, 43 + index / 3 * 44, 320, 36);
                    UnityEventTools.AddIntPersistentListener(button.onClick, controller.CycleJob, index);
                    return button;
                }).ToArray();
                SetArray(controller, "jobButtons", jobButtons);
                var panel = Rect("Skill Browser", canvas.transform, new Vector2(.65f, 0f), Vector2.one);
                panel.gameObject.AddComponent<Image>().color = new Color(.065f, .075f, .11f, .98f);
                var title = Label("Title", panel, "SKILL TEST / 전체 스킬", 25);
                Place(title.rectTransform, 18, 12, 520, 40);
                var instructions = Label("Instructions", panel, "클릭하여 시전 · 시전 중 중복 입력 방지\n스크롤 / 검색으로 선택 · 초기화로 시전 취소", 16);
                Place(instructions.rectTransform, 18, 57, 520, 45);
                var searchRect = Rect("Search", panel);
                Place(searchRect, 18, 110, 520, 40);
                searchRect.gameObject.AddComponent<Image>().color = new Color(.16f, .18f, .25f);
                var input = searchRect.gameObject.AddComponent<TMP_InputField>();
                var inputText = Label("Text", searchRect, "", 18);
                Stretch(inputText.rectTransform, 10);
                var placeholder = Label("Placeholder", searchRect, "스킬 이름 검색...", 18);
                Stretch(placeholder.rectTransform, 10); placeholder.color = Color.gray;
                input.textViewport = searchRect; input.textComponent = inputText; input.placeholder = placeholder;
                Set(controller, "search", input);

                var viewport = Rect("Skill List", panel, new Vector2(0, .34f), new Vector2(1, 1));
                viewport.offsetMin = new Vector2(18, 0); viewport.offsetMax = new Vector2(-18, -162);
                viewport.gameObject.AddComponent<Image>().color = new Color(.1f, .115f, .16f);
                viewport.gameObject.AddComponent<RectMask2D>();
                var content = Rect("Skills", viewport, new Vector2(0, 1), Vector2.one);
                content.pivot = new Vector2(.5f, 1);
                var vertical = content.gameObject.AddComponent<VerticalLayoutGroup>();
                vertical.spacing = 5; vertical.childControlHeight = true; vertical.childForceExpandHeight = false;
                vertical.childControlWidth = true; vertical.childForceExpandWidth = true;
                content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var scroll = viewport.gameObject.AddComponent<ScrollRect>();
                scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.scrollSensitivity = 35;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                var buttons = skills.Select(skill =>
                {
                    var button = Button(skill.name, content, $"{skill.SkillName}   [{skill.Target}{(skill.IsArea ? " / 전체" : "")}]\n<size=12>{skill.name}</size>");
                    button.gameObject.AddComponent<LayoutElement>().preferredHeight = 55;
                    var label = button.GetComponentInChildren<TMP_Text>();
                    label.rectTransform.offsetMin = new Vector2(64, 0);
                    var iconRect = Rect("Skill Icon", button.transform);
                    Place(iconRect, 8, 5, 45, 45);
                    var icon = iconRect.gameObject.AddComponent<Image>();
                    icon.sprite = skill.Icon; icon.preserveAspect = true; icon.raycastTarget = false;
                    icon.enabled = skill.Icon != null;
                    if (skill.SkillLogicExecutor == null) button.interactable = false;
                    return button;
                }).ToArray();
                SetArray(controller, "skillButtons", buttons);
                var bottom = Rect("Controls", panel, Vector2.zero, new Vector2(1, .33f));
                var reset = Button("Reset", bottom, "체력 / 상태 / 위치 초기화");
                Place((RectTransform)reset.transform, 18, 0, 300, 38);
                UnityEventTools.AddPersistentListener(reset.onClick, controller.ResetBattle);
                var sliderRect = Rect("Skill Level", bottom); Place(sliderRect, 335, 8, 195, 22);
                sliderRect.gameObject.AddComponent<Image>().color = new Color(.2f, .24f, .32f);
                var slider = sliderRect.gameObject.AddComponent<Slider>();
                slider.minValue = 1; slider.maxValue = 20; slider.wholeNumbers = true; slider.value = 1;
                var handle = Rect("Handle", sliderRect); handle.sizeDelta = new Vector2(16, 0);
                var handleImage = handle.gameObject.AddComponent<Image>(); handleImage.color = new Color(.35f, .8f, 1f);
                slider.handleRect = handle; slider.targetGraphic = handleImage; Set(controller, "level", slider);
                var status = Label("Status", bottom, "스킬을 선택하세요.", 16); Place(status.rectTransform, 18, 50, 520, 110);
                Set(controller, "status", status);
                var health = Label("Health", bottom, "", 15); Place(health.rectTransform, 18, 165, 520, 125);
                Set(controller, "health", health);
                // 설정 프리팹이 프로젝트 공통 EventSystem을 소유한다.
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Members/PSW/Prefabs/Resources/SettingsWindow.prefab"));
                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log($"Skill test scene saved: {ScenePath} ({skills.Length} skills)");
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void CopyBackground()
        {
            // RealScene의 StageBackground가 사용하는 실제 전투 배경.
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Members/KJY/00.GameModule/Submission/ForestBackground.prefab");
            var copy = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            copy.name = "Battle Background";
            copy.transform.position = new Vector3(0, 0, 5);
        }

        private static SkillTestActor Actor(string name, AgentDataSO data, Vector3 position, TweenLayoutGroup layout, bool enemy)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Members/KJY/00.GameModule/PlayPrefabs/{(enemy ? "Enemy" : "Player")}.prefab");
            var body = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            body.name = name + " Body"; body.transform.position = position; body.SetActive(true);
            var agent = body.GetComponent<AbstractAgent>();
            var actor = new GameObject(name).AddComponent<SkillTestActor>();
            actor.transform.SetParent(layout.transform); actor.transform.position = position;
            var anchor = new GameObject(name + " Default Position").transform; anchor.position = position;
            Set(actor, "data", data); Set(actor, "<MyAgent>k__BackingField", agent);
            Set(actor, "<DefaultPosition>k__BackingField", anchor); Set(actor, "<LineConnectTrm>k__BackingField", anchor);
            Set(actor, "<DiceLayoutGroup>k__BackingField", layout); Set(actor, "<DiceLayoutTarget>k__BackingField", actor.transform);
            var renderer = agent.AgentRenderer.GetComponent<SpriteRenderer>();
            renderer.sprite = data is PlayerDataSO player ? player.PlayerImage : ((EnemyDataSO)data).EnemyImage;
            var animator = body.GetComponentInChildren<Animator>(true);
            animator.runtimeAnimatorController = data is PlayerDataSO p ? p.AnimCon : ((EnemyDataSO)data).EnemyAc;
            return actor;
        }

        private static T[] Find<T>() where T : Object => AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { "Assets/Members" })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(path => path).Select(AssetDatabase.LoadAssetAtPath<T>).Where(value => value != null).ToArray();

        private static void Set(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(Object target, string field, Object[] values)
        {
            var serialized = new SerializedObject(target); var array = serialized.FindProperty(field); array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform Rect(string name, Transform parent) => Rect(name, parent, Vector2.zero, Vector2.one);
        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
        }
        private static void Stretch(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, 0); rect.offsetMax = new Vector2(-padding, 0);
        }
        private static TMP_Text Label(string name, Transform parent, string text, int size)
        {
            var label = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = _font; label.text = text; label.fontSize = size; label.color = Color.white;
            label.raycastTarget = false; label.alignment = TextAlignmentOptions.MidlineLeft;
            return label;
        }
        private static Button Button(string name, Transform parent, string text)
        {
            var rect = Rect(name, parent); var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(.16f, .22f, .32f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var label = Label("Label", rect, text, 18); Stretch(label.rectTransform, 10);
            return button;
        }
    }
}
