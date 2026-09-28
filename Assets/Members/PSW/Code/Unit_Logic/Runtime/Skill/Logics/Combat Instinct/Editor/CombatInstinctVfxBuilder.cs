using System;
using System.Reflection;
using Members.PSW.Code.Unit_Logic.Runtime.Skill.Logics.Combat_Instinct;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Members.PSW.Editor
{
    public static class CombatInstinctVfxBuilder
    {
        private const string Folder = "Assets/Members/PSW/VFX/Material/Combat Instinct/";
        private const string PrefabPath = "Assets/Members/PSW/GameModule/Skills/Skill Logic Prefab/CombatInstinct Executor.prefab";

        [MenuItem("Tools/PSW/Rebuild Combat Instinct VFX")]
        public static void Build()
        {
            var slash = Material("Instinct Blade", 0, .07f, .35f);
            var ring = Material("Instinct Ring", 1, .035f, 0);
            var spark = Material("Instinct Sparks", 2, .13f, 0);
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var logic = root.GetComponentInChildren<CombatInstinct>();
                var serialized = new SerializedObject(logic);
                // 교체 범위는 이 스킬 프리팹의 기존 파티클뿐이다.
                foreach (var particle in root.GetComponentsInChildren<ParticleSystem>(true))
                    if (particle != null) Object.DestroyImmediate(particle.gameObject);
                var lineParent = (Transform)serialized.FindProperty("lineParent").objectReferenceValue;
                var charge = Particle("Gathering Energy", lineParent, ring, .35f, 1, 1.3f, 1.3f, 0);
                SizeCurve(charge, new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, .08f)));
                var blade = Particle("Giant Blade", logic.transform, slash, .7f, 1, 6.8f, 6.8f, 64);
                SizeCurve(blade, new AnimationCurve(new Keyframe(0, .1f), new Keyframe(.22f, 1.12f), new Keyframe(.45f, 1), new Keyframe(1, .94f)));
                var echo = Particle("Crossing Afterimage", blade.transform, slash, .5f, 1, 5.7f, 5.7f, -48);
                var echoMain = echo.main; echoMain.startDelay = .08f;
                var flash = Particle("Impact Ring", logic.transform, ring, .55f, 1, 5, 5, 0);
                SizeCurve(flash, new AnimationCurve(new Keyframe(0, .08f), new Keyframe(.3f, .7f), new Keyframe(1, 1.4f)));
                var shards = Particle("Radial Shards", flash.transform, spark, .5f, 32, .7f, .12f, 0);
                var main = shards.main; main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 9f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(.2f, .55f);
                main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
                var shape = shards.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = .15f;
                var trails = Particle("Dash Wisps", charge.transform, spark, .3f, 10, .7f, .08f, 0);
                var trailShape = trails.shape; trailShape.enabled = true; trailShape.shapeType = ParticleSystemShapeType.Circle; trailShape.radius = .6f;
                Set(serialized, "effect", blade); Set(serialized, "shinyEffect", flash);
                serialized.FindProperty("timeSet.attackerMoveDuration").floatValue = .16f;
                serialized.FindProperty("timeSet.attackWaitDuration").floatValue = .22f;
                serialized.FindProperty("timeSet.waitFadeLineTime").floatValue = .06f;
                serialized.FindProperty("timeSet.lineFadeDuration").floatValue = .1f;
                serialized.FindProperty("timeSet.effectDuration").floatValue = .65f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var events = new EffectEvent
                {
                    onLineStart = new UnityEvent(), onLineEnd = new UnityEvent(),
                    onEffectStart = new UnityEvent(), onEffectImpact = new UnityEvent(), onEffectEnd = new UnityEvent()
                };
                UnityEventTools.AddPersistentListener(events.onLineStart, charge.Play);
                UnityEventTools.AddPersistentListener(events.onLineEnd, charge.Stop);
                UnityEventTools.AddPersistentListener(events.onEffectStart, blade.Play);
                UnityEventTools.AddPersistentListener(events.onEffectImpact, flash.Play);
                typeof(CombatInstinct).GetField("effectEvent", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(logic, events);
                foreach (var line in root.GetComponentsInChildren<LineRenderer>(true))
                {
                    line.sharedMaterial = spark;
                    line.widthMultiplier = line.name == "Line" ? .14f : .05f;
                    line.sortingLayerName = "VFX"; line.sortingOrder = 20;
                }
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static Material Material(string name, float style, float width, float curve)
        {
            string path = Folder + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("PSW/VFX/Instinct Energy"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetFloat("_Style", style); material.SetFloat("_Width", width); material.SetFloat("_Curve", curve);
            material.SetColor("_CoreColor", new Color(2f, 2.3f, 2.5f));
            material.SetColor("_GlowColor", new Color(.12f, .65f, 1.4f));
            EditorUtility.SetDirty(material);
            return material;
        }

        private static ParticleSystem Particle(string name, Transform parent, Material material, float lifetime, short count, float width, float height, float angle)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var particle = go.AddComponent<ParticleSystem>();
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particle.main;
            main.loop = false; main.playOnAwake = false; main.duration = lifetime; main.startLifetime = lifetime;
            main.startSpeed = 0; main.maxParticles = 64; main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.startSize3D = true; main.startSizeX = width; main.startSizeY = height; main.startSizeZ = 1;
            main.startRotation = angle * Mathf.Deg2Rad; main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var shape = particle.shape; shape.enabled = false;
            var emission = particle.emission; emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0, count) });
            var color = particle.colorOverLifetime; color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(.35f, .75f, 1), 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, .06f), new GradientAlphaKey(1, .35f), new GradientAlphaKey(0, 1) });
            color.color = gradient;
            var renderer = go.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = material;
            renderer.sortingLayerName = "VFX"; renderer.sortingOrder = 30; renderer.maxParticleSize = 2;
            return particle;
        }

        private static void SizeCurve(ParticleSystem particle, AnimationCurve curve)
        {
            var size = particle.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, curve);
        }
        private static void Set(SerializedObject target, string name, Object value) => target.FindProperty(name).objectReferenceValue = value;
    }
}
