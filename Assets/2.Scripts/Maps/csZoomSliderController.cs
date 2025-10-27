using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class csZoomSliderController : MonoBehaviour
{
    [SerializeField] private RawImage mapRawImage;
    [SerializeField] private Slider zoomSlider;

    public float minScale = 1f;
    public float maxScale = 2f;


    private Vector3 baseScale;
    private void OnEnable()
    {
        // 슬라이더 초기값 보정 (0~1 범위)
        zoomSlider.minValue = 0f;
        zoomSlider.maxValue = 1f;
        zoomSlider.value = 0f; // 기본 1배 크기
        if (zoomSlider != null)
        {
            // 슬라이더 값 변화 시 이벤트 등록
            zoomSlider.onValueChanged.AddListener(OnZoomChanged);
        }
    }

    private void OnDisable()
    {

    }
    private void OnZoomChanged(float value)
    {
        float targetScale = Mathf.Lerp(minScale, maxScale, value);

        // 확대/축소 전의 중심 좌표 (현재 화면상 기준)
        Vector2 pivotOffset = mapRawImage.rectTransform.rect.size * 0.5f * (mapRawImage.rectTransform.localScale.x - targetScale);

        // anchoredPosition 보정: 가운데 기준으로 맞추기
        mapRawImage.rectTransform.anchoredPosition += pivotOffset;

        // 실제 스케일 적용
        mapRawImage.rectTransform.localScale = new Vector3(targetScale, targetScale, 1f);
    }

}
