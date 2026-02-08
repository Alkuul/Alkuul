using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Alkuul.Domain;
using Alkuul.UI;

public class GlassDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private BrewingPanelBridge bridge;
    [SerializeField] private Image glassImage;
    [SerializeField] private bool hideGlassWhenEmpty = false;

    private Sprite defaultGlassSprite;

    private void Awake()
    {
        if (bridge == null) bridge = FindObjectOfType<BrewingPanelBridge>(true);
        if (glassImage == null) glassImage = GetComponent<Image>();

        if (glassImage != null)
        {
            defaultGlassSprite = glassImage.sprite;
        }
    }

    private void OnEnable()
    {
        if (bridge != null)
        {
            bridge.GlassChanged += HandleGlassChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (bridge != null)
        {
            bridge.GlassChanged -= HandleGlassChanged;
        }
    }

    private void Refresh()
    {
        HandleGlassChanged(bridge != null ? bridge.SelectedGlass : null);
    }

    private void HandleGlassChanged(GlassSO glass)
    {
        if (glassImage == null) return;

        Sprite nextSprite = glass != null && glass.icon != null ? glass.icon : defaultGlassSprite;
        glassImage.sprite = nextSprite;

        if (hideGlassWhenEmpty)
        {
            glassImage.enabled = nextSprite != null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var go = eventData.pointerDrag;
        if (go == null) return;

        var data = go.GetComponent<UIGarnishData>();
        if (data == null || data.garnish == null) return;

        if (bridge == null) bridge = FindObjectOfType<BrewingPanelBridge>();

        bool ok = bridge != null && bridge.SetGarnishes(data.garnish, true);
        if (glassImage != null)
        {
            Sprite nextSprite = data.garnish.icon != null ? data.garnish.icon : defaultGlassSprite;
            glassImage.sprite = nextSprite;

            if (hideGlassWhenEmpty)
            {
                glassImage.enabled = nextSprite != null;
            }
        }
        Debug.Log($"[UI] Drop Garnish: {data.garnish.name} ok={ok}");
    }
}