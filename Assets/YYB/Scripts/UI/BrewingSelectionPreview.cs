using System.Collections.Generic;
using Alkuul.Domain;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BrewingSelectionPreview : MonoBehaviour
{
    [Header("Bridge")]
    [SerializeField] private BrewingPanelBridge bridge;

    [Header("Glass Preview")]
    [SerializeField] private Image glassImage;
    [SerializeField] private bool hideGlassWhenEmpty = false;

    [Header("Garnish Preview")]
    [SerializeField] private List<Image> garnishSlots = new();
    [SerializeField] private bool hideEmptyGarnishSlots = true;

    [Header("Garnish Full Preview")]
    [SerializeField] private Image garnishImage;
    [SerializeField] private bool hideGarnishWhenEmpty = false;
    [SerializeField] private bool useLastSelectedGarnish = true;
    
    private Sprite defaultGlassSprite;
    private Sprite defaultGarnishSprite;

    private void Awake()
    {
        if (bridge == null) bridge = FindObjectOfType<BrewingPanelBridge>(true);
        if (glassImage == null) glassImage = GetComponent<Image>();

        if (glassImage != null)
        {
            defaultGlassSprite = glassImage.sprite;
        }

        if (garnishImage != null)
        {
            defaultGarnishSprite = garnishImage.sprite;
        }
    }

    private void OnEnable()
    {
        if (bridge != null)
        {
            bridge.GlassChanged += HandleGlassChanged;
            bridge.GarnishesChanged += HandleGarnishesChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (bridge != null)
        {
            bridge.GlassChanged -= HandleGlassChanged;
            bridge.GarnishesChanged -= HandleGarnishesChanged;
        }
    }

    private void Refresh()
    {
        HandleGlassChanged(bridge != null ? bridge.SelectedGlass : null);
        HandleGarnishesChanged(bridge != null ? bridge.SelectedGarnishes : null);
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

    private void HandleGarnishesChanged(IReadOnlyList<GarnishSO> garnishes)
    {
        if (garnishImage != null)
        {
            GarnishSO selectedGarnish = null;
            if (garnishes != null && garnishes.Count > 0)
            {
                selectedGarnish = useLastSelectedGarnish
                    ? garnishes[garnishes.Count - 1]
                    : garnishes[0];
            }

            Sprite nextSprite = selectedGarnish != null ? selectedGarnish.icon : defaultGarnishSprite;
            garnishImage.sprite = nextSprite;

            if (hideGarnishWhenEmpty)
            {
                garnishImage.enabled = selectedGarnish != null && nextSprite != null;
            }
            else
            {
                garnishImage.enabled = nextSprite != null;
            }
        }

        if (garnishSlots == null || garnishSlots.Count == 0) return;

        for (int i = 0; i < garnishSlots.Count; i++)
        {
            Image slot = garnishSlots[i];
            if (slot == null) continue;

            Sprite nextSprite = null;
            if (garnishes != null && i < garnishes.Count && garnishes[i] != null)
            {
                nextSprite = garnishes[i].icon;
            }

            slot.sprite = nextSprite;

            if (hideEmptyGarnishSlots)
            {
                slot.enabled = nextSprite != null;
            }
            else
            {
                slot.enabled = true;
            }
        }
    }
}