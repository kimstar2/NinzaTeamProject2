using System;
using System.Collections.Generic;
using System.Linq;
using DevLib.ServiceLocator;
using Members.CJY.Scripts;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using Members.KJY._01.Scripts.Dice.Battle;
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
        private NodeConnect _enterRequest;
        private NodeDeveloperToolsView _view;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            // Supports entering Play mode with domain/scene reload disabled.
            if (FindFirstObjectByType<NodeDeveloperTools>() != null) return;
            var root = new GameObject("[Developer Tools] Node Tools");
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
            if (_open && (_map == null || !_map.isActiveAndEnabled ||
                (keyboard?.escapeKey.wasPressedThisFrame == true && !_view.IsSearchFocused)))
                Close();
            if (_open) _view.Resize();

            // Execute scene changes after the UI event that requested them.
            if (_enterRequest != null)
            {
                var node = _enterRequest;
                _enterRequest = null;
                Close();
                if (_nodeEvent == null || !_nodeEvent.DeveloperEnter(node))
                {
                    Open();
                    _view?.SetStatus("진입 실패: 전투 확인창을 닫거나 노드/전투 데이터를 확인하세요.");
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
            if (_view == null)
            {
                var template = Resources.Load<GameObject>("DeveloperToolsPanel");
                if (template == null)
                {
                    Debug.LogError("DeveloperToolsPanel 프리팹이 없습니다.", this);
                    return;
                }
                _view = new NodeDeveloperToolsView(template, transform, Close, tab =>
                {
                    _tab = tab;
                    PopulateRows();
                }, GrantCoins);
            }
            _open = true;
            // Prevent clicks and keyboard navigation from reaching the map behind this modal.
            foreach (var input in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                if (!input.enabled) continue;
                _suspendedInput.Add(input);
                input.enabled = false;
            }
            _view.Show();
            PopulateRows();
        }

        private void Close()
        {
            _open = false;
            _view?.Hide();
            foreach (var input in _suspendedInput)
                if (input != null) input.enabled = true;
            _suspendedInput.Clear();
        }

        private void PopulateRows()
        {
            _view.ClearRows();
            if (_tab == 0) AddNodes();
            else AddFaces();
            _view.SetStatus(_tab == 0
                ? "이동은 맵 위치만 변경합니다. 진입은 해당 전투·이벤트를 시작합니다."
                : "Lv.1 주사위 조각을 배낭에 지급합니다. 지급 후 배낭에서 장착하세요.");
            _view.ApplyFilter();
        }

        private void AddNodes()
        {
            foreach (var node in _nodes)
            {
                string kind = node.info.type switch
                {
                    NodeType.Start => "시작", NodeType.Battle => "전투", NodeType.Elite => "정예",
                    NodeType.Boss => "보스", NodeType.Event => "이벤트", NodeType.Rest => "휴식",
                    NodeType.Shop => "상점", _ => node.info.typeName
                };
                string position = $"열 {node.column} / 칸 {node.lane}";
                _view.AddRow(node.info.icon, kind, position, new Color32(153, 130, 245, 255),
                    $"{kind} {node.info.typeName} {node.info.type} {position}", "진입",
                    () => _enterRequest = node,
                    () => _view.SetStatus(_map.DeveloperMoveTo(node)
                        ? $"이동 완료: {kind} · {position}" : "이동 실패: 전투 확인창을 닫고 다시 시도하세요."));
            }
        }

        private void AddFaces()
        {
            foreach (var face in _faces)
            {
                string name = string.IsNullOrWhiteSpace(face.MainName) ? face.name : face.MainName;
                string skills = string.Join(" / ", face.SkillDataStructs.Where(entry => entry.SkillData != null)
                    .Select(entry => entry.SkillData.SkillName).Distinct());
                string roles = string.Join(" · ", face.GetUsableTypes().Select(SkillDataSO.RoleName));
                string grade = face.DiceGrade != null ? face.DiceGrade.DisplayName : "일반";
                Color gradeColor = face.DiceGrade != null ? face.DiceGrade.GradeColor : Color.white;
                var icon = face.Icon != null ? face.Icon : face.GetIcon(face.GetUsableTypes()[0]);
                _view.AddRow(icon, name, $"{grade} · Lv.1 · {roles}\n{skills}", gradeColor,
                    $"{name} {skills} {roles} {grade}", "지급하기", () => Grant(face));
            }
        }

        private void GrantCoins()
        {
            if (!ServiceLocator.TryGet<Inventory>(out var inventory) ||
                inventory is not BattleInventory battleInventory || battleInventory == null)
            {
                _view.SetStatus("코인 인벤토리가 없습니다. 모험을 시작한 뒤 이용하세요.");
                return;
            }
            int added = battleInventory.AddGold(100);
            foreach (var hud in FindObjectsByType<Members.KJY._01.Scripts.UI.MapHud>(FindObjectsSortMode.None))
                hud.DeveloperRefresh(_map);
            _view.SetStatus(added == 100
                ? $"코인 100개 지급 완료 · 보유 코인 {battleInventory.Gold:N0}개"
                : $"보유 한도에 도달했습니다. {added}개 지급 · 보유 코인 {battleInventory.Gold:N0}개");
        }

        private void Grant(DiceDataSO face)
        {
            if (!ServiceLocator.TryGet<Inventory>(out var inventory) || inventory == null)
            {
                _view.SetStatus("인벤토리가 없습니다. 모험을 시작한 뒤 이용하세요.");
                return;
            }
            if (inventory.DiceFragments.Count >= inventory.MaxSlots)
            {
                _view.SetStatus($"배낭이 가득 찼습니다. ({inventory.MaxSlots}칸)");
                return;
            }
            var fragment = ScriptableObject.CreateInstance<RewardDiceFragmentSO>();
            fragment.hideFlags = HideFlags.DontSave;
            fragment.Initialize(face, 1f);
            if (inventory.AddFragment(fragment))
            {
                _rewards.Add((inventory, fragment));
                _view.SetStatus($"{face.MainName} 지급 완료 ({inventory.DiceFragments.Count}/{inventory.MaxSlots})");
            }
            else
            {
                Destroy(fragment);
                _view.SetStatus("주사위 면을 추가하지 못했습니다.");
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
            _view?.Dispose();
        }
    }
}
