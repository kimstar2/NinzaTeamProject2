using System;
using System.Collections;
using System.Linq;
using Members.KJY._01.Scripts.Agent;
using Members.KJY._01.Scripts.Agent.Player;
using Members.KJY._01.Scripts.Agent.Enemy;
using Members.KJY._01.Scripts.Agent.SkillSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.PSW.Code.SkillTest
{
    public sealed class SkillTestController : MonoBehaviour
    {
        [SerializeField] private SkillDataSO[] skills;
        [SerializeField] private PlayerDataSO[] playerTypes;
        [SerializeField] private EnemyDataSO[] enemyTypes;
        [SerializeField] private Button[] jobButtons;
        [SerializeField] private Button autoJobButton;
        [SerializeField] private bool autoJob;
        [SerializeField] private SkillTestActor caster;
        [SerializeField] private SkillTestActor[] actors;
        [SerializeField] private Button[] skillButtons;
        [SerializeField] private TMP_Text status;
        [SerializeField] private TMP_Text health;
        [SerializeField] private TMP_InputField search;
        [SerializeField] private Slider level;
        [SerializeField] private Transform skillParent;
        private SkillLogicExecutor _active;
        private Coroutine _watchdog;
        private bool _busy;

        private void Start()
        {
            for (int i = 0; i < skillButtons.Length; i++)
            {
                int index = i;
                skillButtons[i].onClick.AddListener(() => PlaySkill(index));
                skillButtons[i].interactable = skills[i] != null && skills[i].SkillLogicExecutor != null;
            }
            search.onValueChanged.AddListener(Filter);
            RefreshJobLabels();
            status.text = $"스킬 {skills.Length}개 · 버튼을 누르면 즉시 시전\n자동 대상: 적 / 아군 / 자신 · 모든 캐릭터 최소 HP 1";
            CombatStatusVisual.PopupFont = status.font;
        }

        private void Update()
        {
            health.text = $"스킬 배율: {level.value:0}\n" + string.Join("\n", actors.Select(actor =>
                $"{actor.name}: {actor.MyAgent.HealthModule.CurrentHealth:0}/{actor.MyAgent.HealthModule.DefaultMaxHealth:0}"));
        }

        public void PlaySkill(int index)
        {
            if (_busy || index < 0 || index >= skills.Length) return;
            SkillDataSO skill = skills[index];
            if (skill == null || skill.SkillLogicExecutor == null) return;
            if (autoJob)
            {
                var suitable = playerTypes.FirstOrDefault(player => skill.IsSuitable(player.AttackType));
                caster.SetData(suitable != null ? suitable : playerTypes[0]);
                RefreshJobLabels();
            }
            foreach (var actor in actors) actor.SkillLevel = level.value;
            var target = skill.Target == SkillDataSO.TargetType.Self ? caster :
                actors.FirstOrDefault(actor => actor != caster && skill.CanTarget(caster, actor));
            if (target == null)
            {
                status.text = $"{skill.SkillName}: 유효한 대상이 없습니다.";
                return;
            }
            _busy = true;
            SetJobInteractable(false);
            foreach (var button in skillButtons) button.interactable = false;
            status.text = $"시전: {skill.SkillName} → {target.name}\n{skill.GetDescription(level.value)}";
            _active = Instantiate(skill.SkillLogicExecutor, skillParent);
            _active.OnSkillFinished += Finish;
            _watchdog = StartCoroutine(WatchCast());
            try { _active.SkillExecute(caster, target, AgentType.Player, skill); }
            catch (Exception error)
            {
                Debug.LogException(error, this);
                status.text = $"{skill.SkillName}: 실행 오류. Console을 확인하세요.";
                CancelCast();
            }
        }

        private IEnumerator WatchCast()
        {
            yield return new WaitForSecondsRealtime(20f);
            _watchdog = null;
            status.text = "시전 제한 시간(20초)을 넘었습니다. Console과 스킬 연결을 확인하세요.";
            CancelCast();
        }

        private void Finish()
        {
            if (_active != null) _active.OnSkillFinished -= Finish;
            _active = null;
            if (_watchdog != null) StopCoroutine(_watchdog);
            _watchdog = null;
            _busy = false;
            SetJobInteractable(true);
            for (int i = 0; i < skillButtons.Length; i++)
                skillButtons[i].interactable = skills[i] != null && skills[i].SkillLogicExecutor != null;
        }

        private void CancelCast()
        {
            if (_active != null)
            {
                _active.PlayIdleAnim();
                Destroy(_active.gameObject);
            }
            Finish();
        }

        public void ResetBattle()
        {
            CancelCast();
            // Destroy의 스킬 취소/트윈 정리가 완료된 뒤 위치를 복구한다.
            StartCoroutine(ResetAfterCast());
        }

        private IEnumerator ResetAfterCast()
        {
            _busy = true;
            SetJobInteractable(false);
            yield return null;
            foreach (var actor in actors) actor.ResetActor();
            _busy = false;
            SetJobInteractable(true);
            status.text = "체력·상태이상·위치를 초기화했습니다. 스킬을 선택하세요.";
        }

        private void Filter(string query)
        {
            for (int i = 0; i < skillButtons.Length; i++)
                skillButtons[i].gameObject.SetActive(string.IsNullOrWhiteSpace(query) ||
                    (skills[i].SkillName + " " + skills[i].name).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public void CycleJob(int actorIndex)
        {
            if (_busy || actorIndex < 0 || actorIndex >= actors.Length) return;
            var actor = actors[actorIndex];
            AgentDataSO[] choices = actor.AgentData is PlayerDataSO ? playerTypes : (AgentDataSO[])enemyTypes;
            if (choices.Length == 0) return;
            int current = Array.IndexOf(choices, actor.AgentData);
            actor.Effects.Clear();
            actor.SetData(choices[(current + 1) % choices.Length]);
            if (actor == caster) autoJob = false;
            RefreshJobLabels();
        }

        public void ToggleAutoJob()
        {
            if (_busy) return;
            autoJob = !autoJob;
            RefreshJobLabels();
        }

        private void RefreshJobLabels()
        {
            for (int i = 0; i < jobButtons.Length; i++)
                jobButtons[i].GetComponentInChildren<TMP_Text>().text =
                    $"{actors[i].name}: {SkillDataSO.RoleName(actors[i].AgentData.AttackType)}  ▶";
            autoJobButton.GetComponentInChildren<TMP_Text>().text = $"시전자 자동 직업: {(autoJob ? "켜짐" : "꺼짐")}";
        }

        private void SetJobInteractable(bool value)
        {
            foreach (var button in jobButtons) button.interactable = value;
            autoJobButton.interactable = value;
        }

        private void OnDestroy()
        {
            if (_active != null) _active.OnSkillFinished -= Finish;
            if (search != null) search.onValueChanged.RemoveListener(Filter);
        }
    }
}
