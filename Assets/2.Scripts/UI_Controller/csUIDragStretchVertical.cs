using UnityEngine;
using UnityEngine.EventSystems;

public class csUIDragStretchVertical : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform target;  // stretch된 패널
    [SerializeField] private float minHeight = 200f;
    private float maxHeight;

    private Vector2 startMousePos;
    private float startOffsetY;

    private void Start()
    {
        maxHeight = Screen.height *0.7f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            target, eventData.position, eventData.pressEventCamera, out startMousePos);
        startOffsetY = target.offsetMax.y; // 상단 offset 기록
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            target, eventData.position, eventData.pressEventCamera, out currentMousePos);

        float deltaY = currentMousePos.y - startMousePos.y;

        // offsetMax.y는 “상단 여백”을 의미 (음수로 늘어남)
        float newOffsetMaxY = startOffsetY - deltaY;

        // 실제 높이를 계산해서 제한
        float currentHeight = target.rect.height - (newOffsetMaxY - startOffsetY);
        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

        // 새 높이에 맞게 offset 조정
        float desiredOffsetChange = (currentHeight - target.rect.height);
        target.offsetMax += new Vector2(0, desiredOffsetChange);
    }
}
