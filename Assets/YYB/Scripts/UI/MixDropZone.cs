using UnityEngine;
using UnityEngine.EventSystems;
using Alkuul.Domain;

public class MixDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private BrewingPanelBridge bridge;
    [SerializeField] private float defaultMl = 30f;

    [Header("Optional")]
    [SerializeField] private bool requireJigger = true;   // 지거만 받게(추천)
    [SerializeField] private bool clearJiggerAfterPour = true;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("[MixDropZone] eventData is null");
            return;
        }

        var go = eventData.pointerDrag;
        if (go == null)
        {
            Debug.LogWarning("[MixDropZone] pointerDrag is null");
            return;
        }

        // (선택) 지거만 허용
        if (requireJigger)
        {
            var jigger = go.GetComponent<UIJiggerController>();
            if (jigger == null)
            {
                Debug.Log($"[MixDropZone] Ignored drop (not jigger): {go.name}");
                return;
            }
        }

        // 드롭된 오브젝트에서 데이터 찾기 (본체/자식 모두)
        var data = go.GetComponent<UIIngredientData>();
        if (data == null) data = go.GetComponentInChildren<UIIngredientData>();

        if (data == null)
        {
            Debug.LogWarning($"[MixDropZone] UIIngredientData missing on {go.name}");
            return;
        }

        if (data.ingredient == null)
        {
            Debug.LogWarning($"[MixDropZone] Dropped but ingredient is null: {go.name}");
            return;
        }

        if (bridge == null)
            bridge = FindObjectOfType<BrewingPanelBridge>(true);

        if (bridge == null)
        {
            Debug.LogError("[MixDropZone] BrewingPanelBridge not found in scene (BrewingScene에 Bridge가 있어야 함)");
            return;
        }

        float ml = (data.ml > 0f) ? data.ml : defaultMl;

        bridge.OnPortionAdded(data.ingredient, ml);
        Debug.Log($"[UI] Drop Ingredient: {data.ingredient.name} {ml}ml");

        // 붓고 나면 지거 비우기(선택)
        if (clearJiggerAfterPour)
        {
            var jigger = go.GetComponent<UIJiggerController>();
            jigger?.Clear();
        }
    }
}
