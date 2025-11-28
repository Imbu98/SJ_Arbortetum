using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class csZoomSliderController : MonoBehaviour
{
    [SerializeField] private RectTransform mapParentReact;

    public float minScale = 1f;
    public float maxScale = 2f;
    private float zoomSpeed = 0.004f; // 모바일 핀치 줌 속도
    private float currentScale = 1f;

    private float smoothDelta = 0f;
    private const int smoothFrames = 5;

    private bool isZooming = false;
    private bool isRotating = false;

    private const float zoomThreshold = 3.0f;    // 줌 감지 민감도
    private const float rotateThreshold = 2f;  // 회전 감지 민감도


    private void Update()
    {

#if UNITY_ANDROID || UNITY_IOS
        // 모바일 핀치 줌 처리
        CheckPinchGestureUsingDirection();
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

    private void CheckPinchGestureUsingDirection()
    {
        if (Input.touchCount < 2)
        {
            isZooming = false;
            isRotating = false;
            return;
        }

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        // 손가락 이동 방향 벡터
        Vector2 dir0 = t0.deltaPosition.normalized;
        Vector2 dir1 = t1.deltaPosition.normalized;

        // 두 벡터의 각도 차이
        float angleBetweenFingers = Vector2.Angle(dir0, dir1);

        // 줌/회전 기본 값
        float distDelta = Vector2.Distance(t0.position, t1.position) -
                          Vector2.Distance(t0.position - t0.deltaPosition,
                                           t1.position - t1.deltaPosition);

        float angleDelta = Mathf.DeltaAngle(
            Mathf.Atan2(t1.position.y - t0.position.y, t1.position.x - t0.position.x) * Mathf.Rad2Deg,
            Mathf.Atan2((t1.position - t1.deltaPosition).y - (t0.position - t0.deltaPosition).y,
                        (t1.position - t1.deltaPosition).x - (t0.position - t0.deltaPosition).x) * Mathf.Rad2Deg
        );

        // --------------------------------------------------
        // 제스처 판단 (핵심)
        // --------------------------------------------------
        if (!isZooming && !isRotating)
        {
            if (angleBetweenFingers < 25f)
            {
                // 손가락이 거의 같은 방향 → 줌 의도
                if (Mathf.Abs(distDelta) > zoomThreshold)
                {
                    isZooming = true;
                }
            }
            else if (angleBetweenFingers > 30f)
            {
                // 손가락이 다른 방향/비틀림 → 회전 의도
                if (Mathf.Abs(angleDelta) > rotateThreshold)
                {
                    isRotating = true;
                }
            }
            else
            {
                // 안정화 영역 → 판정 보류
                return;
            }
        }

        // --------------------------------------------------
        // 제스처 유지
        // --------------------------------------------------
        if (isZooming)
        {
            ZoomOnMobile(distDelta);
        }
        if (isRotating)
        {
            RotateAroundPinchCenter(angleDelta);
        }
    }




    // 모바일용 줌 계산기
    private void ZoomOnMobile(float deltaDist)
    {
        if (Input.touchCount < 2)
            return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);


        // DeadZone
        if (Mathf.Abs(deltaDist) < 0.3f)
            return;

        // Smoothing
        smoothDelta = Mathf.Lerp(smoothDelta, deltaDist, 1f / smoothFrames);

        float oldScale = currentScale;
        float newScale = Mathf.Clamp(oldScale + smoothDelta * 0.005f, minScale, maxScale);

        if (Mathf.Approximately(newScale, oldScale))
            return;

        currentScale = newScale;

        RectTransform rt = mapParentReact;
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

        csMapManager.Instance.SetObjectScale(currentScale);

        Debug.Log("Zoomed with Pinch Center: " + currentScale);
    }

    private void RotateAroundPinchCenter(float deltaAngle)
    {
        // 길찾기 중이면 회전 불가
        if (csMapManager.Instance.E_searchStatus == Data.SearchStatus.SearchPath)
        {
            return;
        }

        RectTransform rt = mapParentReact;
        RectTransform parentRt = rt.parent as RectTransform;

        // ---- 1) 두 손가락 가운데(Screen Center) 얻기 ----
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);
        Vector2 pinchCenter = (t0.position + t1.position) * 0.5f;

        // ---- 2) pinchCenter를 Parent 기준 Local 좌표로 변환 ----
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt,
            pinchCenter,
            null,
            out localPoint
        );
        // ---- 3) Local → World 변환 ----
        Vector3 pivotWorld = parentRt.TransformPoint(localPoint);

        // ---- 4) 회전 전 벡터 ----
        Vector3 beforeVec = rt.position - pivotWorld;

        // ---- 5) 회전 적용 ----
        rt.Rotate(0, 0, deltaAngle);

        // ---- 6) 회전 후 벡터 ----
        Vector3 afterVec = Quaternion.Euler(0, 0, deltaAngle) * beforeVec;

        // ---- 7) 위치 보정 (Pivot 고정) ----
        rt.position = pivotWorld + afterVec;

        // ---- 8) ★ 회전값 정규화 (0~360 유지) ----
        float z = rt.eulerAngles.z % 360f;
        if (z < 0) z += 360f;
        rt.rotation = Quaternion.Euler(0, 0, z);

        // ---- 9) 마커 역회전 적용 ----
        csMapManager.Instance.RotateObject();
    }
}
