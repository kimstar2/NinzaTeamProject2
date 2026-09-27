using System;
using _TevLib.Extension.DoT;
using DevLib.CoreLib.Runtime;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using Members.KJY._01.Scripts.Agent.Player.Dice;
using Members.KJY._01.Scripts.Events;
using Members.KJY._01.Scripts.Events.Dice;
using Members.KJY._01.Scripts.Events.Dice.Agent.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    public class BattleTutorial : MonoBehaviour
    {
        private const string SeenKey = "OverRoll.BattleTutorial.v2";
        private enum AdvanceOn { Inspect, Lock, Reroll, Connection, StartBattle }

        [Serializable]
        private struct Step
        {
            public string title;
            [TextArea(3, 6)] public string description;
            public AdvanceOn advanceOn;
        }

        [SerializeField] private EventChannelSO eventChannel;
        [SerializeField] private PlayerDiceRollManager rollManager;
        [SerializeField] private PointerEventTrigger[] dicePointers = Array.Empty<PointerEventTrigger>();
        [SerializeField] private GameObject panel;
        [SerializeField] private Button helpButton;
        [SerializeField] private TMP_Text heading, description, progress;
        [SerializeField] private TweenSequencer openMotion, closeMotion, pageMotion;
        [SerializeField] private Step[] steps;
        [SerializeField] private SoundClipSO clickSound;
        private int _step;
        private bool _offered, _open, _inBattle, _waitingForRoll;

        private void OnEnable()
        {
            rollManager.onAllDiceRollEnd.AddListener(Offer);
            foreach (var pointer in dicePointers) pointer.onEnter.AddListener(HandleInspect);
            eventChannel.AddListener<OnPlayerRoll>(HandleRoll);
            eventChannel.AddListener<OnDiceLock>(HandleLock);
            eventChannel.AddListener<OnBattleChainChanged>(HandleConnection);
            eventChannel.AddListener<OnStartBattle>(HandleStart);
            eventChannel.AddListener<OnEndBattle>(HandleEnd);
        }

        private void OnDisable()
        {
            rollManager.onAllDiceRollEnd.RemoveListener(Offer);
            foreach (var pointer in dicePointers) pointer.onEnter.RemoveListener(HandleInspect);
            eventChannel.RemoveListener<OnPlayerRoll>(HandleRoll);
            eventChannel.RemoveListener<OnDiceLock>(HandleLock);
            eventChannel.RemoveListener<OnBattleChainChanged>(HandleConnection);
            eventChannel.RemoveListener<OnStartBattle>(HandleStart);
            eventChannel.RemoveListener<OnEndBattle>(HandleEnd);
            openMotion.Stop();
            closeMotion.Stop();
            pageMotion.Stop();
        }

        private void Offer()
        {
            helpButton.interactable = !_inBattle;
            if (_waitingForRoll)
            {
                _waitingForRoll = false;
                Advance(AdvanceOn.Reroll);
            }
            if (_offered) return;
            _offered = true;
            if (!PlayerPrefs.HasKey(SeenKey))
            {
                Show();
                if (_open) ServiceLocator.Get<IAudioService>().Play(clickSound);
            }
        }

        public void Show()
        {
            if (_inBattle || !rollManager.AllDiceRollEnd || steps.Length == 0) return;
            _step = 0;
            _waitingForRoll = false;
            _open = true;
            panel.SetActive(true);
            RefreshPage();
            openMotion.Sequence();
        }

        private void Advance(AdvanceOn action)
        {
            if (!_open || steps[_step].advanceOn != action) return;
            if (++_step >= steps.Length) { Finish(); return; }
            RefreshPage();
        }

        public void Finish()
        {
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
            Hide();
        }

        private void Hide()
        {
            if (!_open) return;
            _open = false;
            _waitingForRoll = false;
            closeMotion.Sequence();
        }

        private void RefreshPage()
        {
            heading.text = steps[_step].title;
            description.text = steps[_step].description;
            progress.text = $"직접 해보기  {_step + 1} / {steps.Length}";
            pageMotion.Sequence();
        }

        private void HandleInspect()
        {
            if (rollManager.AllDiceRollEnd) Advance(AdvanceOn.Inspect);
        }

        private void HandleRoll(OnPlayerRoll evt)
        {
            helpButton.interactable = false;
            _waitingForRoll = _open && steps[_step].advanceOn == AdvanceOn.Reroll;
        }

        private void HandleLock(OnDiceLock evt)
        {
            if (evt.IsLock) Advance(AdvanceOn.Lock);
        }

        private void HandleConnection(OnBattleChainChanged evt)
        {
            if (evt.isAdd) Advance(AdvanceOn.Connection);
        }

        private void HandleStart(OnStartBattle evt)
        {
            _inBattle = true;
            helpButton.interactable = false;
            Advance(AdvanceOn.StartBattle);
            Hide();
        }

        private void HandleEnd(OnEndBattle evt)
        {
            _inBattle = false;
            helpButton.interactable = rollManager.AllDiceRollEnd;
        }
    }
}
