using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class csZoomSliderController : MonoBehaviour
{
    [SerializeField] private RawImage mapRawImage;

    public float minScale = 1f;
    public float maxScale = 2f;
    private float zoomSpeed = 0.004f; // 모바일 핀치 줌 속도
    private float currentScale = 1f;

    private float smoothDelta = 0f;
    private const int smoothFrames = 5;

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
               csMapManager.Instance.SetMapOpened(true);
             
    }
    private void OnDisable()
    {
      

        csMapManager.Instance.SetMapOpened(false);
    }
  


    // 모바일용 줌 계산기
    private void ZoomOnMobile()
    {
        if (Input.touchCount < 2)
            return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        // 이전/현재 거리
        Vector2 t0Prev = t0.position - t0.deltaPosition;
        Vector2 t1Prev = t1.position - t1.deltaPosition;

        float prevDistance = Vector2.Distance(t0Prev, t1Prev);
        float currentDistance = Vector2.Distance(t0.position, t1.position);
        float delta = (currentDistance - prevDistance);

        // DeadZone
        if (Mathf.Abs(delta) < 0.3f)
            return;

        // Smoothing
        smoothDelta = Mathf.Lerp(smoothDelta, delta, 1f / smoothFrames);

        float oldScale = currentScale;
        float newScale = Mathf.Clamp(oldScale + smoothDelta * 0.005f, minScale, maxScale);

        if (Mathf.Approximately(newScale, oldScale))
            return;

        currentScale = newScale;

        RectTransform rt = mapRawImage.rectTransform;
        RectTransform parentRt = rt.parent as RectTransform;

        // 두 손가락 가운데(Screen 중심)
        Vector2 pinchCenter = (t0.position + t1.position) * 0.5f;

        //  부모 RectTransform 기준 LocalPoint 변환
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt,
            pinchCenter,
            null,
            out localPoint
        );

        //  부모 기준 월드 좌표
        Vector3 zoomCenterWorld = parentRt.TransformPoint(localPoint);

        // 현재 지도 위치와 기준점 사이 거리 벡터
        Vector3 dir = rt.position - zoomCenterWorld;

        //  Scale 비율
        float scaleRatio = newScale / oldScale;

        //  새 위치 계산
        Vector3 newPos = zoomCenterWorld + (dir * scaleRatio);

        //  Scale 적용
        rt.localScale = new Vector3(newScale, newScale, 1f);

        // Position 적용
        rt.position = newPos;

        // 맵 경계 보정
        csMapManager.Instance.ClampMapPosition();

        Debug.Log("Zoomed with Pinch Center: " + currentScale);
    }
}
