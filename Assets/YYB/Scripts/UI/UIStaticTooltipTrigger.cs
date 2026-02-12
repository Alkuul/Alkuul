using UnityEngine;
using UnityEngine.EventSystems;

public class UIStaticTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UIHoverTooltip tooltip;

    [Header("Tooltip Text")]
    [SerializeField] private string title = "Info";
    [SerializeField] private string summary = "";
    [TextArea(2, 8)]
    [SerializeField] private string details = "";

    private void Awake()
    {
        if (tooltip == null)
            tooltip = FindObjectOfType<UIHoverTooltip>(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null) return;

        tooltip.Show(title, summary, details);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Hide();
    }
}