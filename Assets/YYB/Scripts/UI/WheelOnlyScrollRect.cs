using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelOnlyScrollRect : ScrollRect
{
    // 드래그로 스크롤되는 것만 막음. 휠 스크롤은 그대로 됨.
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
}
