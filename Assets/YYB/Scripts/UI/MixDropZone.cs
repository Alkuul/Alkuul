using UnityEngine;
using UnityEngine.EventSystems;
using Alkuul.Domain;

public class MixDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private BrewingPanelBridge bridge;
    [SerializeField] private float defaultMl = 30f;

    [Header("Optional")]
    [SerializeField] private bool requireJigger = true;
    [SerializeField] private bool clearJiggerAfterPour = true;

    public void OnDrop(PointerEventData eventData)
    {
        var go = eventData?.pointerDrag;
        if (go == null) return;

        if (bridge == null)
            bridge = FindObjectOfType<BrewingPanelBridge>(true);

        if (bridge == null)
        {
            Debug.LogError("[MixDropZone] BrewingPanelBridge not found");
            return;
        }

        var jigger = go.GetComponentInParent<UIJiggerController>();

        if (requireJigger && jigger == null)
        {
            Debug.Log($"[MixDropZone] Ignored drop (need jigger): {go.name}");
            return;
        }

        UIIngredientData data =
            (jigger != null ? jigger.GetComponent<UIIngredientData>() : null) ??
            go.GetComponent<UIIngredientData>() ??
            go.GetComponentInParent<UIIngredientData>() ??
            go.GetComponentInChildren<UIIngredientData>();

        if (data == null || data.ingredient == null)
        {
            Debug.LogWarning($"[MixDropZone] No ingredient to pour. dropped={go.name}");
            return;
        }

        float ml = (data.ml > 0f) ? data.ml : defaultMl;

        bridge.OnPortionAdded(data.ingredient, ml);
        Debug.Log($"[MixDropZone] Pour {(jigger != null ? "Jigger" : "Direct")} : {data.ingredient.name} {ml}ml");

        if (clearJiggerAfterPour && jigger != null)
            jigger.Clear();
    }
}
