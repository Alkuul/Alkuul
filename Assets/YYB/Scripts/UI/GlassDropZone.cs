using UnityEngine;
using UnityEngine.EventSystems;
using Alkuul.Domain;
using Alkuul.UI;

public class GlassDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private BrewingPanelBridge bridge;

    public void OnDrop(PointerEventData eventData)
    {
        var go = eventData.pointerDrag;
        if (go == null) return;

        var data = go.GetComponent<UIGarnishData>();
        if (data == null || data.garnish == null) return;

        if (bridge == null) bridge = FindObjectOfType<BrewingPanelBridge>();

        bool ok = bridge != null && bridge.SetGarnishes(data.garnish, true);
        Debug.Log($"[UI] Drop Garnish: {data.garnish.name} ok={ok}");
    }
}