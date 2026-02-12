using System.Linq;
using Alkuul.Domain;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITechniqueTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UIHoverTooltip tooltip;
    [SerializeField] private TechniqueSO technique;

    [Header("Fallback Text")]
    [SerializeField] private string defaultSummary = "조주 도구";
    [SerializeField] private string defaultDetails = "여기에 도구 설명을 입력하세요.";

    private void Awake()
    {
        if (tooltip == null) tooltip = FindObjectOfType<UIHoverTooltip>(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null || technique == null) return;

        string title = string.IsNullOrWhiteSpace(technique.displayName) ? technique.name : technique.displayName;
        string summary = string.IsNullOrWhiteSpace(technique.tooltipSummary) ? defaultSummary : technique.tooltipSummary;

        string details = string.IsNullOrWhiteSpace(technique.tooltipDetails)
            ? BuildTagFallback(technique.tags)
            : technique.tooltipDetails;

        if (string.IsNullOrWhiteSpace(details))
            details = defaultDetails;

        tooltip.Show(title, summary, details);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Hide();
    }

    private static string BuildTagFallback(string[] tags)
    {
        if (tags == null || tags.Length == 0) return string.Empty;

        var validTags = tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
        if (validTags.Length == 0) return string.Empty;

        return "태그: " + string.Join(", ", validTags);
    }
}