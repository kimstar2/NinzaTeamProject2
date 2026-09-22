using DG.Tweening;
using Members.LYW.Scripts.Event;
using UnityEngine;

public class CompleteUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup completeUIGroup;
    [SerializeField] private TextExplainer textExplainer;
    
    public void Complete(string text)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(completeUIGroup.DOFade(1f, 0.75f).SetEase(Ease.OutQuad));
        seq.AppendInterval(1.5f);
        seq.AppendCallback(() =>
        {
            textExplainer.StartTexting(text);
        });
        seq.AppendInterval(5f);
        seq.Append(completeUIGroup.transform.GetComponent<RectTransform>().DOAnchorPosY(1080f, 0.75f).SetEase(Ease.OutQuad));

        seq.Play();
    }
}
