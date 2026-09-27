using System.Collections;
using _TevLib.Extension.DoT;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using Members.KJY._01.Scripts.Dice.Battle;
using Members.KJY._01.Scripts.Dice.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    public class DiceCatalogPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private DiceDataSO[] faces;
        [SerializeField] private BattleRewardItem itemPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TweenSequencer openMotion, closeMotion;
        [SerializeField] private SoundClipSO clickSound;
        private bool _built;
        public bool IsOpen => panelRoot.activeSelf;

        private void Awake() => panelRoot.SetActive(false);

        public void Open()
        {
            panelRoot.SetActive(true);
            panelGroup.interactable = true;
            if (!_built)
            {
                foreach (var face in faces)
                {
                    var item = Instantiate(itemPrefab, content);
                    item.Bind(face, 1f);
                    item.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ServiceLocator.Get<IAudioService>().Play(clickSound);
                        description.text = face.MainName + "\n\n" + face.GetDescription(1f);
                    });
                    item.Reveal();
                }
                _built = true;
            }
            description.text = "면마다 역할에 맞는 스킬이 달라집니다.\n\n표식 → 공격으로 연계하고, 격려 → 다음 스킬 강화로 순서를 설계해 보세요.\n\n면을 선택하면 역할별 효과를 확인할 수 있습니다.";
            openMotion.Sequence();
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
