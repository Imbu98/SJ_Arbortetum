using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class csZoomSliderController : MonoBehaviour
{
    [SerializeField] private RawImage mapRawImage;
    [SerializeField] private Slider zoomSlider;

    public float minScale = 1f;
    public float maxScale = 2f;
    private float zoomSpeed = 0.01f; // 모바일 핀치 줌 속도
    private float currentScale = 1f;

    private Vector3 baseScale;

    private void Update()
    {

#if UNITY_ANDROID || UNITY_IOS
        // 모바일 핀치 줌 처리
        if (Input.touchCount == 2)
        {
            ZoomOnMobile();
        }
#endif
    }
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

        // 이벤트를 이용하여 스크롤이 발생했을 시 스크롤의 값을 변수에 저장한다.
        // 스크롤 값이 120과 -120만 받아오기 때문에 0.02f를 곱하여 낮출 수 있다.
        // 카메라의 FOV값을 불러옴
    }
    private void OnDisable()
    {
    }
    private void OnZoomChanged(float value)
    {
        float targetScale = Mathf.Lerp(minScale, maxScale, value);

        //// 확대/축소 전의 중심 좌표 (현재 화면상 기준)
        //Vector2 pivotOffset = mapRawImage.rectTransform.rect.size * 0.5f * (mapRawImage.rectTransform.localScale.x - targetScale);

        //// anchoredPosition 보정: 가운데 기준으로 맞추기
        //mapRawImage.rectTransform.anchoredPosition += pivotOffset;

        // 실제 스케일 적용
        mapRawImage.rectTransform.localScale = new Vector3(targetScale, targetScale, 1f);
    }

    // 모바일용 줌 계산기
    private void ZoomOnMobile()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        // 이전 프레임의 두 손가락 위치
        Vector2 t0Prev = t0.position - t0.deltaPosition;
        Vector2 t1Prev = t1.position - t1.deltaPosition;

        // 이전/현재 거리 계산
        float prevDistance = Vector2.Distance(t0Prev, t1Prev);
        float currentDistance = Vector2.Distance(t0.position, t1.position);

        // 변화량 → 스케일 변경
        float delta = (currentDistance - prevDistance) * zoomSpeed;
        float targetScale = Mathf.Clamp(currentScale + delta, minScale, maxScale);

        // 실제 스케일 적용
        mapRawImage.rectTransform.localScale = new Vector3(targetScale, targetScale, 1f);
        currentScale = targetScale;

        Debug.Log("Zoomed to: " + currentScale);
    }
}
