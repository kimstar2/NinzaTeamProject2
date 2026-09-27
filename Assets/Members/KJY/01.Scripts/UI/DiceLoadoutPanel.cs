using System.Collections;
using System.Collections.Generic;
using _TevLib.Extension.DoT;
using DevLib.ServiceLocator;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Dice.Battle;
using Members.KJY._01.Scripts.Dice.Data;
using Members.KJY._01.Scripts.Service;
using Members.PSW.Code.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    public class DiceLoadoutPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private TMP_Text playerName, faceDescription, equippedDescription, notice;
        [SerializeField] private BattleRewardItem[] equippedFaces;
        [SerializeField] private GameObject[] playerHighlights;
        [SerializeField] private Button equipButton;
        [SerializeField] private GameObject emptyBagHint;
        [SerializeField] private BattleRewardItem rewardPrefab;
        [SerializeField] private Transform rewardParent;
        [SerializeField] private TweenSequencer openMotion, closeMotion;
        private readonly List<BattleRewardItem> _items = new();
        private BattleDataStorage _storage;
        private BattleInventory _inventory;
        private PlayerDataSO _player;
        private DiceFaceType _slot;
        private RewardDiceFragmentSO _selected;
        private BattleRewardItem _selectedItem;

        private static string FaceName(DiceFaceType face) => face switch
        {
            DiceFaceType.Front => "앞면", DiceFaceType.Back => "뒷면",
            DiceFaceType.Left => "왼쪽 면", DiceFaceType.Right => "오른쪽 면",
            DiceFaceType.Top => "윗면", _ => "아랫면"
        };

        private void Awake()
        {
            panelRoot.SetActive(false);
            for (int i = 0; i < equippedFaces.Length; i++)
            {
                int slot = i;
                equippedFaces[i].GetComponent<Button>().onClick.AddListener(() => SelectSlot(slot));
            }
        }

        public void Open()
        {
            _storage = ServiceLocator.Get<IBattleDataStorage>().Instance;
            ServiceLocator.TryGet<Inventory>(out var inventory);
            _inventory = inventory as BattleInventory;
            if (_inventory == null) return;
            panelRoot.SetActive(true);
            panelGroup.interactable = true;
            SelectPlayer(0);
            RefreshRewards();
            openMotion.Sequence();
        }

        public void SelectPlayer(int index)
        {
            _player = _storage.GetRunTimePlayerData((PlayerType)index);
            _selected = null;
            _selectedItem?.SetSelected(false);
            _selectedItem = null;
            playerName.text = index switch { 0 => "탱커", 1 => "전사", 2 => "힐러", _ => "마법사" };
            for (int i = 0; i < playerHighlights.Length; i++) playerHighlights[i].SetActive(i == index);
            RefreshFaces();
            SelectSlot(0);
        }

        private void RefreshFaces()
        {
            for (int i = 0; i < equippedFaces.Length; i++)
            {
                var face = (DiceFaceType)i;
                equippedFaces[i].Bind(_player.DiceList.GetDiceData(face), _player.DiceList.GetLevel(face));
                equippedFaces[i].Reveal();
            }
        }

        public void SelectSlot(int index)
        {
            _slot = (DiceFaceType)index;
            for (int i = 0; i < equippedFaces.Length; i++)
                equippedFaces[i].SetSelected(i == index);
            RefreshDescription();
        }

        private void RefreshRewards()
        {
            foreach (var item in _items) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
            _items.Clear();
            foreach (var reward in _inventory.DiceFragments)
            {
                if (reward == null || reward.DiceData == null) continue;
                var item = Instantiate(rewardPrefab, rewardParent);
                item.Bind(reward);
                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _selectedItem?.SetSelected(false);
                    _selectedItem = item;
                    item.SetSelected(true);
                    _selected = reward;
                    RefreshDescription();
                });
                item.Reveal();
                _items.Add(item);
            }
            emptyBagHint.SetActive(_items.Count == 0);
            RefreshDescription();
        }

        private string Describe(DiceDataSO face, float level)
        {
            if (face == null) return "빈 면입니다.";
            var skill = face.GetSkillDataStruct(_player.AttackType).SkillData;
            string body = skill == null ? "이 캐릭터가 사용할 수 없는 면입니다." :
                !skill.IsSuitable(_player.AttackType) ? $"{skill.SkillName}\n<color=#E57373>직업이 맞지 않아 장착할 수 없습니다. (적합: {skill.SuitableDescription})</color>" :
                $"{skill.SkillName}\n{skill.GetDescription(level)}";
            return $"<color=#B5A5F4>{face.MainName}</color>  <size=80%>Lv.{level:0.#}</size>\n" + body;
        }

        private void RefreshDescription()
        {
            equippedDescription.text = Describe(_player.DiceList.GetDiceData(_slot), _player.DiceList.GetLevel(_slot));
            faceDescription.text = _selected != null ? Describe(_selected.DiceData, _selected.Level) :
                "가방에서 면을 골라\n바뀔 스킬을 확인하세요.";
            equipButton.interactable = _selected != null && _selected.DiceData != null &&
                _selected.DiceData.CanUse(_player.AttackType);
            notice.text = _selected != null ? $"{playerName.text} · {FaceName(_slot)} 교체" :
                _items.Count == 0 ? "전투에서 얻은 면이 가방에 모입니다." : $"{FaceName(_slot)} 선택 · 오른쪽에서 교체할 면을 고르세요.";
        }

        public void Equip()
        {
            if (_selected == null || !_inventory.EquipFace(_player, _slot, _selected)) return;
            _selected = null;
            _selectedItem = null;
            RefreshFaces();
            RefreshRewards();
            SelectSlot((int)_slot);
            notice.text = "장착했습니다. 이전 면은 가방으로 돌아갑니다.";
        }

        public void Close() { if (panelGroup.interactable) StartCoroutine(ClosePanel()); }
        private IEnumerator ClosePanel()
        {
            panelGroup.interactable = false;
            openMotion.Stop();
            closeMotion.Sequence();
            yield return new WaitUntil(() => !closeMotion.HasTween);
            panelRoot.SetActive(false);
        }
    }
}
