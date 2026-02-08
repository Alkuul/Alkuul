using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelOnlyScrollRect : ScrollRect
{
    // 드래그로 스크롤되는 것만 막음. 휠 스크롤은 그대로 됨.
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }

    private void Update()
    {
        if (!IsActive())
        {
            return;
        }

        if (EventSystem.current == null)
        {
            return;
        }

        Vector2 scrollDelta = Input.mouseScrollDelta;
        if (Mathf.Approximately(scrollDelta.y, 0f) && Mathf.Approximately(scrollDelta.x, 0f))
        {
            return;
        }

        RectTransform targetRect = viewport != null ? viewport : (RectTransform)transform;
        if (!RectTransformUtility.RectangleContainsScreenPoint(targetRect, Input.mousePosition, GetEventCamera()))
        {
            return;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            scrollDelta = scrollDelta
        };

        base.OnScroll(eventData);
    }

    private Camera GetEventCamera()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            return Camera.main;
        }

        return canvas.worldCamera;
    }
}
