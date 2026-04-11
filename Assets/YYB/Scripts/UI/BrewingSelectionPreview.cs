using System.Collections.Generic;
using Alkuul.Domain;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BrewingSelectionPreview : MonoBehaviour
{
    [Header("Bridge")]
    [SerializeField] private BrewingPanelBridge bridge;

    [Header("3-Layer Preview")]
    [SerializeField] private Image glassFrameImage;
    [SerializeField] private Image liquidOverlayImage;
    [SerializeField] private Image garnishOverlayImage;

    [Header("Fallback / Visibility")]
    [SerializeField] private bool hideFrameWhenNoGlass = true;
    [SerializeField] private bool hideLiquidWhenEmpty = true;
    [SerializeField] private bool hideGarnishWhenEmpty = true;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private string _lastGlassKey;
    private string _lastEmotionKey;
    private string _lastGarnishKey;
    private int _lastPortionCount = -1;
    private bool _lastForceHidden;

    private bool forceHidden;

    private void Awake()
    {
        if (bridge == null)
            bridge = FindObjectOfType<BrewingPanelBridge>(true);
    }

    private void OnEnable()
    {
        if (bridge != null)
        {
            bridge.GlassChanged += HandleGlassChanged;
            bridge.GarnishesChanged += HandleGarnishesChanged;
        }

        ForceRefresh();
    }

    private void OnDisable()
    {
        if (bridge != null)
        {
            bridge.GlassChanged -= HandleGlassChanged;
            bridge.GarnishesChanged -= HandleGarnishesChanged;
        }
    }

    private void LateUpdate()
    {
        RefreshIfChanged();
    }

    public void HideVisualOnly()
    {
        forceHidden = true;
        ApplyHiddenVisualState();
    }

    public void RestoreFromBridge()
    {
        forceHidden = false;
        ForceRefresh();
    }

    private void HandleGlassChanged(GlassSO _)
    {
        if (forceHidden)
            forceHidden = false;

        ForceRefresh();
    }

    private void HandleGarnishesChanged(IReadOnlyList<GarnishSO> _)
    {
        if (forceHidden)
            forceHidden = false;

        ForceRefresh();
    }

    private void RefreshIfChanged()
    {
        if (bridge == null) return;

        string glassKey = bridge.SelectedGlass != null
            ? DrinkPreviewSpriteResolver.GetGlassKey(bridge.SelectedGlass)
            : null;

        string emotionKey = null;
        if (bridge.SelectedGlass != null && bridge.CurrentPortionCount > 0)
            emotionKey = DrinkPreviewSpriteResolver.GetDominantPrimaryEmotionKey(bridge.PreviewDrink());

        string garnishKey = GetCurrentDisplayGarnishKey(bridge.SelectedGarnishes);

        bool changed =
            _lastGlassKey != glassKey ||
            _lastEmotionKey != emotionKey ||
            _lastGarnishKey != garnishKey ||
            _lastPortionCount != bridge.CurrentPortionCount ||
            _lastForceHidden != forceHidden;

        if (!changed) return;

        ForceRefresh();
    }

    private void ForceRefresh()
    {
        if (bridge == null) return;

        _lastForceHidden = forceHidden;
        _lastPortionCount = bridge.CurrentPortionCount;
        _lastGlassKey = bridge.SelectedGlass != null
            ? DrinkPreviewSpriteResolver.GetGlassKey(bridge.SelectedGlass)
            : null;
        _lastEmotionKey = bridge.SelectedGlass != null && bridge.CurrentPortionCount > 0
            ? DrinkPreviewSpriteResolver.GetDominantPrimaryEmotionKey(bridge.PreviewDrink())
            : null;
        _lastGarnishKey = GetCurrentDisplayGarnishKey(bridge.SelectedGarnishes);

        if (forceHidden)
        {
            ApplyHiddenVisualState();
            return;
        }

        RefreshFrame();
        RefreshLiquid();
        RefreshGarnish();

        if (verboseLog)
        {
            Debug.Log($"[Preview] frame={_lastGlassKey}, liquid={_lastEmotionKey}, garnish={_lastGarnishKey}, portions={_lastPortionCount}");
        }
    }

    private void RefreshFrame()
    {
        if (glassFrameImage == null) return;

        Sprite sprite = DrinkPreviewSpriteResolver.ResolveGlassFrame(bridge.SelectedGlass);
        glassFrameImage.sprite = sprite;

        if (hideFrameWhenNoGlass)
            glassFrameImage.enabled = sprite != null;
        else
            glassFrameImage.enabled = true;
    }

    private void RefreshLiquid()
    {
        if (liquidOverlayImage == null) return;

        Sprite sprite = DrinkPreviewSpriteResolver.ResolveLiquidOverlay(
            bridge.SelectedGlass,
            bridge.PreviewDrink(),
            bridge.CurrentPortionCount
        );

        liquidOverlayImage.sprite = sprite;

        if (hideLiquidWhenEmpty)
            liquidOverlayImage.enabled = sprite != null;
        else
            liquidOverlayImage.enabled = true;
    }

    private void RefreshGarnish()
    {
        if (garnishOverlayImage == null) return;

        Sprite sprite = DrinkPreviewSpriteResolver.ResolveGarnishOverlay(
            bridge.SelectedGlass,
            bridge.SelectedGarnishes
        );

        garnishOverlayImage.sprite = sprite;

        if (hideGarnishWhenEmpty)
            garnishOverlayImage.enabled = sprite != null;
        else
            garnishOverlayImage.enabled = true;
    }

    private void ApplyHiddenVisualState()
    {
        if (glassFrameImage != null) glassFrameImage.enabled = false;
        if (liquidOverlayImage != null) liquidOverlayImage.enabled = false;
        if (garnishOverlayImage != null) garnishOverlayImage.enabled = false;
    }

    private string GetCurrentDisplayGarnishKey(IReadOnlyList<GarnishSO> garnishes)
    {
        if (garnishes == null || garnishes.Count == 0)
            return null;

        for (int i = garnishes.Count - 1; i >= 0; i--)
        {
            if (garnishes[i] == null) continue;

            string key = DrinkPreviewSpriteResolver.GetGarnishKey(garnishes[i]);
            if (string.IsNullOrEmpty(key)) continue;

            // 실제 오버레이 파일이 있는 가니쉬만 표시 대상으로 인정
            if (DrinkPreviewSpriteResolver.ResolveGarnishOverlay(bridge.SelectedGlass, new List<GarnishSO> { garnishes[i] }) != null)
                return key;
        }

        return null;
    }
}