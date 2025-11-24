using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class csTitleAnimation : MonoBehaviour
{
    [SerializeField] private Image swirl;

    Tween floatTween;
    Tween fadeTween;

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
        // 시작 상태 초기화
        swirl.color = new Color(1f, 1f, 1f, 1f);

        Vector3 startPos = swirl.rectTransform.anchoredPosition;

        // 위아래 부드러운 둥둥 모션
        floatTween = swirl.rectTransform
            .DOAnchorPosY(startPos.y + 25f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // 숨쉬듯이 알파 변화
        fadeTween = swirl
            .DOFade(0.7f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopEffect()
    {
        floatTween?.Kill();
        fadeTween?.Kill();

        // 원래 상태로 복귀
        if (swirl != null)
        {
            swirl.color = new Color(1f, 1f, 1f, 0f);
            swirl.rectTransform.localRotation = Quaternion.identity;
            swirl.rectTransform.localScale = Vector3.one;
        }
    }
}
