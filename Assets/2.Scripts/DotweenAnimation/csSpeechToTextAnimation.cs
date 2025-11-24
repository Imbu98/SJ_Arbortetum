using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class csSpeechToTextAnimation : MonoBehaviour
{
    [SerializeField] private Image swirl;

    Tween rotateTween;
    Sequence pulseSeq;

    void OnEnable()
    {
        PlayEffect();
    }

    void OnDisable()
    {
        StopEffect();
    }

    private void PlayEffect()
    {
        swirl.color = new Color(1f, 1f, 1f, 1f);
        swirl.transform.localScale = Vector3.one * 1.0f;

        // È¸Àü
        rotateTween = swirl.transform
            .DORotate(new Vector3(0, 0, 360), 3f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);

        // È®»ê Pulse È¿°ú
        pulseSeq = DOTween.Sequence();
        pulseSeq.Append(swirl.transform.DOScale(1.3f, 1.2f).SetEase(Ease.OutQuad));
        pulseSeq.Join(swirl.DOFade(0.0f, 1.2f));
        pulseSeq.Append(swirl.DOFade(1.0f, 0.2f));
        pulseSeq.Append(swirl.transform.DOScale(1.0f, 0.2f));
        pulseSeq.SetLoops(-1);
    }

    private void StopEffect()
    {
        rotateTween?.Kill();
        pulseSeq?.Kill();

        // ÃÊ±âÈ­ (²¨Á³À» ¶§ µü ¸ØÃß°í ¼û±è)
        if (swirl != null)
        {
            swirl.color = new Color(1f, 1f, 1f, 0f);
            swirl.transform.localScale = Vector3.one;
            swirl.transform.localRotation = Quaternion.identity;
        }
    }
}


