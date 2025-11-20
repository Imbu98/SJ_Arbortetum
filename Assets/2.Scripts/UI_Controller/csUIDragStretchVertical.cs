using UnityEngine;
using UnityEngine.EventSystems;

public class csUIDragStretchVertical : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform target;  // stretch된 패널
    [SerializeField] private float minHeight = 200f;
    [SerializeField] private float maxHeight;

    private float startHeight;
    private Vector2 startPointerPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startHeight = target.rect.height;
        startPointerPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 1) Y 변화는 "Screen 기준"으로 직접 계산 → 최적 & 부드러움
        float deltaY = eventData.position.y - startPointerPos.y;

        // 2) 높이 = 시작 높이 + 손가락 이동량
        float newHeight = Mathf.Clamp(startHeight + deltaY, minHeight, maxHeight);

        // 3) offsetMax 기반으로 높이 변경 (pivot = bottom)
        float deltaHeight = newHeight - target.rect.height;
        target.offsetMax += new Vector2(0, deltaHeight);
    }
}
