#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.ServiceLocator;
using Members.CJY.Scripts;
using Members.KJY._01.Scripts.Dice.Data;
using Members.PSW.Code.InventorySystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DeveloperTools
{
    // Temporary debug UI owns its input and rewards; gameplay never references this component.
    public sealed class NodeDeveloperTools : MonoBehaviour
    {
        private readonly List<EventSystem> _suspendedInput = new();
        private readonly List<(Inventory inventory, RewardDiceFragmentSO fragment)> _rewards = new();
        private readonly List<NodeConnect> _nodes = new();
        private DiceDataSO[] _faces = Array.Empty<DiceDataSO>();
        private NodeMaker _map;
        private NodeEvent _nodeEvent;
        private bool _open, _chordHeld;
        private int _tab;
        private string _search = "", _message = "";
        private Vector2 _scroll;
        private NodeConnect _enterRequest;
        private Font _font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            // Supports entering Play mode with domain/scene reload disabled.
            if (FindFirstObjectByType<NodeDeveloperTools>() != null) return;
            var root = new GameObject("[Development Only] Node Tools");
            root.AddComponent<NodeDeveloperTools>();
            DontDestroyOnLoad(root);
        }

        private void OnEnable() => SceneManager.activeSceneChanged += OnSceneChanged;
        private void OnSceneChanged(Scene previous, Scene next) => Close();
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            Close();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            bool chord = keyboard != null && (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
                && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
                && keyboard.spaceKey.isPressed && keyboard.tKey.isPressed;
            if (chord && !_chordHeld)
            {
                if (_open) Close();
                else Open();
            }
            _chordHeld = chord;
            if (_open && (_map == null || !_map.isActiveAndEnabled || keyboard?.escapeKey.wasPressedThisFrame == true))
                Close();

            // Execute scene changes outside an IMGUI layout/repaint event.
            if (_enterRequest != null)
            {
                var node = _enterRequest;
                _enterRequest = null;
                Close();
                if (_nodeEvent == null || !_nodeEvent.DeveloperEnter(node))
                {
                    Open();
                    _message = "진입 실패: 전투 확인창을 닫거나 노드/전투 데이터를 확인하세요.";
                }
            }
            for (int i = _rewards.Count - 1; i >= 0; i--)
            {
                var reward = _rewards[i];
                if (reward.inventory != null && reward.inventory.DiceFragments.Contains(reward.fragment)) continue;
                if (reward.fragment != null) Destroy(reward.fragment);
                _rewards.RemoveAt(i);
            }
        }

        private void Open()
        {
            if (Members.KJY._01.Scripts.UI.SceneTransition.IsBusy) return;
            // This removable diagnostic tool discovers only the active map on demand.
            _map = FindObjectsByType<NodeMaker>(FindObjectsSortMode.None)
                .FirstOrDefault(map => map.isActiveAndEnabled && map.gameObject.scene == SceneManager.GetActiveScene());
            if (_map == null) return;
            _nodes.Clear();
            _nodes.AddRange(_map.DeveloperNodes);
            if (_nodes.Count == 0) return;
            _nodeEvent = _map.GetComponent<NodeEvent>();
            var catalog = Resources.Load<DiceCatalogSO>(DiceCatalogSO.ResourcePath);
            _faces = catalog == null || catalog.Faces == null ? Array.Empty<DiceDataSO>()
                : catalog.Faces.Where(face => face != null && face.GetUsableTypes().Count > 0)
                    .Distinct().OrderBy(face => face.MainName).ToArray();
            _message = "이동은 맵 위치만 변경합니다. 진입은 해당 전투/이벤트를 시작합니다.";
            _open = true;
            // Prevent clicks and keyboard navigation from reaching the map behind this modal.
            foreach (var input in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                if (!input.enabled) continue;
                _suspendedInput.Add(input);
                input.enabled = false;
            }
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 16);
        }

        private void Close()
        {
            _open = false;
            foreach (var input in _suspendedInput)
                if (input != null) input.enabled = true;
            _suspendedInput.Clear();
        }

        private void OnGUI()
        {
            if (!_open || _map == null) return;
            var oldFont = GUI.skin.font;
            if (_font != null) GUI.skin.font = _font;
            float width = Mathf.Min(820, Screen.width - 20);
            float height = Mathf.Min(700, Screen.height - 20);
            GUILayout.BeginArea(new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height), GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("개발자 툴 · Ctrl + Shift + Space + T", GUILayout.Height(28));
            bool close = GUILayout.Button("닫기 (Esc)", GUILayout.Width(100));
            GUILayout.EndHorizontal();
            int tab = GUILayout.Toolbar(_tab, new[] { "노드 이동 / 진입", "스킬 가져오기" });
            if (tab != _tab) { _tab = tab; _scroll = Vector2.zero; _search = ""; }
            GUILayout.Label("검색 (노드 종류 / 좌표 / 스킬 이름)");
            _search = GUILayout.TextField(_search);
            GUILayout.Label(_message, GUI.skin.box, GUILayout.Height(55));
            _scroll = GUILayout.BeginScrollView(_scroll);
            if (_tab == 0) DrawNodes();
            else DrawSkills();
            GUILayout.EndScrollView();
            GUILayout.Label("변경 사항은 실제 진행에 적용됩니다. 스킬은 Lv.1 주사위 조각으로 배낭에 추가됩니다.");
            GUILayout.EndArea();
            GUI.skin.font = oldFont;
            if (close) Close();
        }

        private bool Matches(string value) => string.IsNullOrWhiteSpace(_search)
            || (value ?? "").IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;

        private void DrawNodes()
        {
            foreach (var node in _nodes)
            {
                string label = $"열 {node.column} / 칸 {node.lane} · {node.info.typeName} ({node.info.type})";
                if (!Matches(label)) continue;
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(label);
                if (GUILayout.Button("이동", GUILayout.Width(65)))
                    _message = _map.DeveloperMoveTo(node) ? $"이동 완료: {label}" : "이동 실패: 전투 확인창을 닫고 다시 시도하세요.";
                if (GUILayout.Button("진입", GUILayout.Width(65))) _enterRequest = node;
                GUILayout.EndHorizontal();
            }
        }

        private void DrawSkills()
        {
            if (_faces.Length == 0) GUILayout.Label("사용 가능한 스킬이 없습니다. Resources/DiceCatalog를 확인하세요.");
            foreach (var face in _faces)
            {
                string skills = string.Join(" / ", face.SkillDataStructs.Where(entry => entry.SkillData != null)
                    .Select(entry => entry.SkillData.SkillName).Distinct());
                string label = $"{face.MainName} · {skills}";
                if (!Matches(label)) continue;
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(label);
                if (GUILayout.Button("가져오기", GUILayout.Width(90))) Grant(face);
                GUILayout.EndHorizontal();
            }
        }

        private void Grant(DiceDataSO face)
        {
            if (!ServiceLocator.TryGet<Inventory>(out var inventory) || inventory == null)
            {
                _message = "인벤토리가 없습니다. 모험을 시작한 뒤 이용하세요.";
                return;
            }
            if (inventory.DiceFragments.Count >= inventory.MaxSlots)
            {
                _message = $"배낭이 가득 찼습니다. ({inventory.MaxSlots}칸)";
                return;
            }
            var fragment = ScriptableObject.CreateInstance<RewardDiceFragmentSO>();
            fragment.hideFlags = HideFlags.DontSave;
            fragment.Initialize(face, 1f);
            if (inventory.AddFragment(fragment))
            {
                _rewards.Add((inventory, fragment));
                _message = $"{face.MainName} 획득 완료 ({inventory.DiceFragments.Count}/{inventory.MaxSlots})";
            }
            else
            {
                Destroy(fragment);
                _message = "스킬을 추가하지 못했습니다.";
            }
        }

        private void OnDestroy()
        {
            Close();
            foreach (var reward in _rewards)
            {
                if (reward.inventory != null) reward.inventory.RemoveFragment(reward.fragment);
                if (reward.fragment != null) Destroy(reward.fragment);
            }
            if (_font != null) Destroy(_font);
        }
    }
}
#endif
