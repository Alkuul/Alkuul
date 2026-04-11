using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISinkDropZone : MonoBehaviour, IDropHandler
{
    [Header("Refs")]
    [SerializeField] private BrewingPanelBridge bridge;
    [SerializeField] private Toggle iceToggle;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = true;

    private void Awake()
    {
        if (bridge == null)
            bridge = FindObjectOfType<BrewingPanelBridge>(true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var marker =
            dragged.GetComponent<UITrashDraggableMarker>() ??
            dragged.GetComponentInParent<UITrashDraggableMarker>();

        if (marker == null) return;

        if (marker.Kind != UITrashDraggableMarker.TrashKind.MixingGlass)
            return;

        bridge?.DiscardMixToSink();

        if (iceToggle != null)
            iceToggle.SetIsOnWithoutNotify(false);

        if (verboseLog)
            Debug.Log("[Sink] Mixing glass discarded -> full brewing state reset.");
    }
}