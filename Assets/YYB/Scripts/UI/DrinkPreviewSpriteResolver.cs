using System;
using System.Collections.Generic;
using Alkuul.Domain;
using UnityEngine;

public static class DrinkPreviewSpriteResolver
{
    private const string GlassFrameFolder = "DrinkPreview/GlassFrame";
    private const string LiquidOverlayFolder = "DrinkPreview/LiquidOverlay";
    private const string GarnishOverlayFolder = "DrinkPreview/GarnishOverlay";

    private static readonly HashSet<string> SupportedGarnishKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "cheese",
        "cherry",
        "garlic_chip",
        "grapefruit",
        "ice",
        "kama",
        "lime_peel",
        "mint",
        "orange",
        "pineapple",
        "lavender",
        "slice_lemon",
        "slice_orange"
    };

    public static Sprite ResolveGlassFrame(GlassSO glass)
    {
        string glassKey = GetGlassKey(glass);
        if (string.IsNullOrEmpty(glassKey))
            return null;

        return Resources.Load<Sprite>($"{GlassFrameFolder}/{glassKey}_frame");
    }

    public static Sprite ResolveLiquidOverlay(GlassSO glass, Drink drink, int portionCount)
    {
        if (glass == null || portionCount <= 0)
            return null;

        string glassKey = GetGlassKey(glass);
        if (string.IsNullOrEmpty(glassKey))
            return null;

        string emotionKey = GetDominantPrimaryEmotionKey(drink);
        if (string.IsNullOrEmpty(emotionKey))
            return null;

        return Resources.Load<Sprite>($"{LiquidOverlayFolder}/{glassKey}_liquid_{emotionKey}");
    }

    public static Sprite ResolveGarnishOverlay(GlassSO glass, IReadOnlyList<GarnishSO> garnishes)
    {
        if (glass == null || garnishes == null || garnishes.Count == 0)
            return null;

        string glassKey = GetGlassKey(glass);
        if (string.IsNullOrEmpty(glassKey))
            return null;

        // 마지막에 선택한 것부터 뒤에서 앞으로 탐색
        for (int i = garnishes.Count - 1; i >= 0; i--)
        {
            var garnish = garnishes[i];
            if (garnish == null) continue;

            string garnishKey = GetGarnishKey(garnish);
            if (string.IsNullOrEmpty(garnishKey)) continue;
            if (!SupportedGarnishKeys.Contains(garnishKey)) continue;

            var sprite = Resources.Load<Sprite>($"{GarnishOverlayFolder}/{glassKey}_garnish_{garnishKey}");
            if (sprite != null)
                return sprite;
        }

        return null;
    }

    public static string GetGlassKey(GlassSO glass)
    {
        if (glass == null) return null;

        string raw = !string.IsNullOrWhiteSpace(glass.id) ? glass.id : glass.name;
        return NormalizeGlassKey(raw);
    }

    public static string GetGarnishKey(GarnishSO garnish)
    {
        if (garnish == null) return null;

        string raw = !string.IsNullOrWhiteSpace(garnish.id) ? garnish.id : garnish.name;
        return NormalizeGarnishKey(raw);
    }

    public static string GetDominantPrimaryEmotionKey(Drink drink)
    {
        var e = drink.emotions;

        float joy = e.joy;
        float sadness = e.sadness;
        float anger = e.anger;
        float fear = e.fear;
        float disgust = e.disgust;
        float surprise = e.surprise;

        float max = joy;
        string key = "joy";

        if (sadness > max) { max = sadness; key = "sadness"; }
        if (anger > max) { max = anger; key = "anger"; }
        if (fear > max) { max = fear; key = "fear"; }
        if (disgust > max) { max = disgust; key = "disgust"; }
        if (surprise > max) { max = surprise; key = "surprise"; }

        if (max <= 0.0001f)
            return null;

        return key;
    }

    private static string NormalizeGlassKey(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        string s = raw.Trim()
            .Replace(" ", "")
            .Replace("_", "")
            .Replace("-", "")
            .ToLowerInvariant();

        return s switch
        {
            "metalcup" => "MetalCup",
            "shotglass" => "ShotGlass",
            "oldfashioned" => "OldFashioned",
            "cocktail" => "Cocktail",
            "highball" => "Highball",
            "hurricane" => "Hurricane",
            _ => raw.Trim()
        };
    }

    private static string NormalizeGarnishKey(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        string s = raw.Trim()
            .Replace(" ", "_")
            .Replace("-", "_")
            .ToLowerInvariant();

        return s switch
        {
            "garlicchip" => "garlic_chip",
            "garlic_chip" => "garlic_chip",
            "limepeel" => "lime_peel",
            "lime_peel" => "lime_peel",
            "slicelemon" => "slice_lemon",
            "slice_lemon" => "slice_lemon",
            "sliceorange" => "slice_orange",
            "slice_orange" => "slice_orange",
            "ravender" => "lavender",
            "lavender" => "lavender",
            _ => s
        };
    }
}