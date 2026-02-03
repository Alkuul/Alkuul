using Alkuul.Domain;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIIngredientTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UIHoverTooltip tooltip;
    [SerializeField] private UIIngredientData data;

    [Header("Emotion Display")]
    [SerializeField] private int emotionTopK = 7;
    [SerializeField] private float emotionMinPct = 1f;

    private void Awake()
    {
        if (tooltip == null) tooltip = FindObjectOfType<UIHoverTooltip>(true);
        if (data == null) data = GetComponent<UIIngredientData>();
        if (data == null) data = GetComponentInChildren<UIIngredientData>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null || data == null || data.ingredient == null) return;

        IngredientSO ing = data.ingredient;

        string name = string.IsNullOrWhiteSpace(ing.displayName) ? ing.name : ing.displayName;
        string abv = $"µµ¼ö: {ing.abv:0.#}%";
        string emotions = EmotionFormat.ToPercentLines(ing.emotions, emotionTopK, emotionMinPct);

        tooltip.Show(name, abv, emotions);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Hide();
    }
}
