using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// ScrollRect에 OnEndDrag 이벤트를 추가한 확장 클래스
/// </summary>
public class csCustomScrollRect : ScrollRect
{
    [System.Serializable]
    public class EndDragEvent : UnityEvent<PointerEventData> { }

    // Inspector에서 연결 가능한 EndDrag 이벤트
    public EndDragEvent onEndDragEvent = new EndDragEvent();

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        onEndDragEvent?.Invoke(eventData);
    }
}
