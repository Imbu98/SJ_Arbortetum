using UnityEngine;
using UnityEngine.EventSystems;

public class csVirtualJoystic : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;

    float leverRange;
    public Vector2 InputVector { get; private set; }

    public bool isInput = false;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, eventData.pressEventCamera, out pos))
        {
            // 중심을 기준으로 정규화 (-1 ~ 1)
            pos.x = (pos.x / (background.sizeDelta.x / 2));
            pos.y = (pos.y / (background.sizeDelta.y / 2));

            InputVector = new Vector2(pos.x, pos.y);
            if (InputVector.magnitude > 1.0f)
                InputVector = InputVector.normalized;

            // 핸들 위치 적용 (조이스틱 배경 내부로 제한)
            handle.anchoredPosition = new Vector2(
                InputVector.x * (background.sizeDelta.x / 2),
                InputVector.y * (background.sizeDelta.y / 2));
        }
        isInput = true;

    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnPointerUp(PointerEventData eventData)
    {
        this.isInput = false;

        InputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
