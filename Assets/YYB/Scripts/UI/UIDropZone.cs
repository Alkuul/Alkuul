using UnityEngine;
using UnityEngine.EventSystems;

public class UIDropZone : MonoBehaviour, IDropHandler
{
    public enum ZoneType { Door, Bed }
    [SerializeField] private ZoneType type;

    public void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag?.GetComponent<UIDraggablePortrait>();
        if (drag == null) return;

        if (type == ZoneType.Door) drag.NotifyDroppedToDoor();
        else drag.NotifyDroppedToBed();
    }
}
