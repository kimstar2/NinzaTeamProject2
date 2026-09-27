using DG.Tweening;
using Members.LYW.Scripts.Event;
using Members.KJY._01.Scripts.UI;
using NaughtyAttributes;
using UnityEngine;

public class CompleteUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup completeUIGroup;
    [SerializeField] private TextExplainer textExplainer;
    [SerializeField, Scene] private string returnScene;
    private Sequence _sequence;
    
    public void Complete(string text)
    {
        if (_sequence != null) return;
        completeUIGroup.blocksRaycasts = true;
        Sequence seq = _sequence = DOTween.Sequence();
        seq.Append(completeUIGroup.DOFade(1f, 0.75f).SetEase(Ease.OutQuad));
        seq.AppendInterval(1.5f);
        seq.AppendCallback(() =>
        {
            textExplainer.StartTexting(text);
        });
        seq.AppendInterval(Mathf.Max(5f, textExplainer.GetPrintDuration(text) + 1f));
        seq.Append(completeUIGroup.transform.GetComponent<RectTransform>().DOAnchorPosY(1080f, 0.75f).SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            if (!string.IsNullOrEmpty(returnScene)) SceneTransition.Load(returnScene);
        });

        seq.Play();
    }

    private void OnDestroy() => _sequence?.Kill();
}
