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
    private float zoomSpeed = 0.004f; // 모바일 핀치 줌 속도
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

        csMapManager.Instance.SetMapOpened(true);

        // 이벤트를 이용하여 스크롤이 발생했을 시 스크롤의 값을 변수에 저장한다.
        // 스크롤 값이 120과 -120만 받아오기 때문에 0.02f를 곱하여 낮출 수 있다.
        // 카메라의 FOV값을 불러옴
    }
    private void OnDisable()
    {
        zoomSlider.onValueChanged.RemoveAllListeners();

        csMapManager.Instance.SetMapOpened(false);
    }
    private void OnZoomChanged(float value)
    {
        float oldScale = mapRawImage.rectTransform.localScale.x;
        float newScale = Mathf.Lerp(minScale, maxScale, value);

        if (Mathf.Approximately(oldScale, newScale))
            return;

        RectTransform rt = mapRawImage.rectTransform;
        RectTransform parentRt = rt.parent as RectTransform;

        //  화면 중앙(뷰포트 중심)의 월드 좌표 → 로컬 좌표 변환
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, screenCenter, null, out localPoint);

        //  줌 기준점 (현재 화면 중앙이 아니라 현재 지도상에서 보이는 "같은 위치")
        Vector3 zoomCenterWorld = parentRt.TransformPoint(localPoint);

        //  지도 위치와 기준점 사이의 벡터 계산
        Vector3 dir = rt.position - zoomCenterWorld;

        //  스케일 비율 계산
        float scaleRatio = newScale / oldScale;

        //  새 위치 = 기준점 + (기존 벡터 * 비율)
        rt.position = zoomCenterWorld + (dir * scaleRatio);

        //  새 스케일 적용
        rt.localScale = new Vector3(newScale, newScale, 1f);

        csMapManager.Instance.ClampMapPosition();
    }


    // 모바일용 줌 계산기
    private void ZoomOnMobile()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 t0Prev = t0.position - t0.deltaPosition;
        Vector2 t1Prev = t1.position - t1.deltaPosition;

        float prevDistance = Vector2.Distance(t0Prev, t1Prev);
        float currentDistance = Vector2.Distance(t0.position, t1.position);

        float delta = (currentDistance - prevDistance) * zoomSpeed;

        // 미세 흔들림 보정
        if (Mathf.Abs(delta) < 0.01f)
            return;

        float targetScale = Mathf.Clamp(currentScale + delta, minScale, maxScale);

        // 부드럽게 보간 (선택 사항)
        currentScale = Mathf.Lerp(currentScale, targetScale, 0.5f);
        mapRawImage.rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
        Debug.Log("Zoomed to: " + currentScale);
    }
}
