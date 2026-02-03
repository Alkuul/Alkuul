using UnityEngine;
using UnityEngine.EventSystems;

public class JiggerFillDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private UIJiggerController jigger;

    public void OnDrop(PointerEventData eventData)
    {
        var go = eventData.pointerDrag;
        if (go == null) return;

        var data = go.GetComponent<UIIngredientData>();
        if (data == null || data.ingredient == null) return;

        if (jigger == null) jigger = GetComponentInParent<UIJiggerController>();
        if (jigger == null) return;

        jigger.SetIngredient(data.ingredient);
        Debug.Log($"[UI] Jigger filled: {data.ingredient.name}");
    }
}
