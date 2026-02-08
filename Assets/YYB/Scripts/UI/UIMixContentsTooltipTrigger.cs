using System.Text;
using Alkuul.Domain;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMixContentsTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UIHoverTooltip tooltip;
    [SerializeField] private BrewingPanelBridge bridge;

    [Header("Emotion Display")]
    [SerializeField] private int emotionTopK = 3;
    [SerializeField] private float emotionMinPct = 3f;

    private void Awake()
    {
        if (tooltip == null) tooltip = FindObjectOfType<UIHoverTooltip>(true);
        if (bridge == null) bridge = FindObjectOfType<BrewingPanelBridge>(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null || bridge == null) return;

        Drink drink = bridge.PreviewDrink();
        string name = "믹싱글라스";
        string abv = BuildHeader(drink, bridge.UsesIce);
        string contents = BuildContents(drink);

        tooltip.Show(name, abv, contents);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Hide();
    }

    private string BuildHeader(Drink drink, bool usesIce)
    {
        string total = $"총량: {drink.totalMl:0.#} ml" + (usesIce ? " (얼음 포함)" : "");
        string abv = $"도수: {drink.finalABV:0.#}%";
        return $"{total}\n{abv}";
    }

    private string BuildContents(Drink drink)
    {
        var sb = new StringBuilder();

        sb.AppendLine("감정(가중치):");
        sb.AppendLine(EmotionFormat.ToPercentLines(drink.emotions, emotionTopK, emotionMinPct));

        return sb.ToString().TrimEnd();
    }
}