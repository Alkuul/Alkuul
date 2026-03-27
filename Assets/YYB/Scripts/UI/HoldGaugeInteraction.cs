using Alkuul.UI.Brewing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HoldGaugeInteraction : TechniqueInteractionBase
{
    [Header("UI Refs")]
    [SerializeField] private RectTransform gaugeTrackRect;
    [SerializeField] private RectTransform targetZoneRect;
    [SerializeField] private Slider gaugeSlider;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text cancelText;

    private float gauge01;
    private float zoneTime;
    private bool startedHolding;
    private bool wasHoldingLastFrame;

    protected override void OnBegin()
    {
        gauge01 = 0f;
        zoneTime = 0f;
        startedHolding = false;
        wasHoldingLastFrame = false;

        SetupTargetZoneVisual();

        if (instructionText != null)
        {
            if (spec.holdGaugeMode == HoldGaugeMode.MaintainInZone)
                instructionText.text = "마우스를 누른 채 게이지를 적정 구간에 유지하세요";
            else
                instructionText.text = "마우스를 눌러 게이지를 채우고 적정 구간에서 손을 떼세요";
        }

        if (cancelText != null)
            cancelText.text = "ESC : 취소";

        RefreshUI(false);
    }

    protected override void TickInteraction(float deltaTime)
    {
        bool holding = Input.GetMouseButton(0);

        if (holding)
        {
            startedHolding = true;
            gauge01 += spec.gaugeFillSpeed * deltaTime;
        }
        else
        {
            gauge01 -= spec.gaugeDrainSpeed * deltaTime;
        }

        gauge01 = Mathf.Clamp01(gauge01);

        bool inZone = gauge01 >= spec.targetZoneMin01 && gauge01 <= spec.targetZoneMax01;

        if (spec.holdGaugeMode == HoldGaugeMode.MaintainInZone)
        {
            if (inZone)
                zoneTime += deltaTime;

            RefreshUI(inZone);

            if (zoneTime >= spec.targetHoldTime)
            {
                float progress = GetCurrentProgress01();
                Complete(true, progress, progress, $"Gauge={gauge01:F2}");
                return;
            }
        }
        else
        {
            if (!holding && wasHoldingLastFrame && startedHolding)
            {
                float quality = EvaluateReleaseQuality(gauge01);
                bool success = inZone;
                Complete(success, quality, quality, success ? $"ReleasedInZone={gauge01:F2}" : $"ReleasedOutOfZone={gauge01:F2}");
                return;
            }

            RefreshUI(inZone);
        }

        wasHoldingLastFrame = holding;
    }

    protected override float GetCurrentProgress01()
    {
        if (spec.holdGaugeMode == HoldGaugeMode.MaintainInZone)
        {
            if (spec.targetHoldTime <= 0f) return 0f;
            return Mathf.Clamp01(zoneTime / spec.targetHoldTime);
        }

        return EvaluateReleaseQuality(gauge01);
    }

    private float EvaluateReleaseQuality(float value01)
    {
        float center = (spec.targetZoneMin01 + spec.targetZoneMax01) * 0.5f;
        float half = Mathf.Max(0.0001f, (spec.targetZoneMax01 - spec.targetZoneMin01) * 0.5f);
        float dist = Mathf.Abs(value01 - center);
        return 1f - Mathf.Clamp01(dist / half);
    }

    private void RefreshUI(bool inZone)
    {
        if (gaugeSlider != null)
            gaugeSlider.value = gauge01;

        if (progressBar != null)
            progressBar.value = GetCurrentProgress01();

        if (progressText != null)
        {
            if (spec.holdGaugeMode == HoldGaugeMode.MaintainInZone)
                progressText.text = $"{zoneTime:F1} / {spec.targetHoldTime:F1}s";
            else
                progressText.text = $"{gauge01 * 100f:F0}%";
        }

        if (statusText != null)
        {
            if (spec.holdGaugeMode == HoldGaugeMode.MaintainInZone)
                statusText.text = inZone ? "좋아요! 이 구간을 유지하세요" : "적정 구간에 맞추세요";
            else
                statusText.text = inZone ? "지금 손을 떼면 됩니다!" : "적정 구간까지 채우세요";
        }
    }

    private void SetupTargetZoneVisual()
    {
        if (gaugeTrackRect == null || targetZoneRect == null)
            return;

        float width = gaugeTrackRect.rect.width;
        float left = Mathf.Lerp(-width * 0.5f, width * 0.5f, spec.targetZoneMin01);
        float right = Mathf.Lerp(-width * 0.5f, width * 0.5f, spec.targetZoneMax01);

        targetZoneRect.anchorMin = new Vector2(0.5f, 0.5f);
        targetZoneRect.anchorMax = new Vector2(0.5f, 0.5f);
        targetZoneRect.pivot = new Vector2(0.5f, 0.5f);
        targetZoneRect.sizeDelta = new Vector2(right - left, targetZoneRect.sizeDelta.y);
        targetZoneRect.anchoredPosition = new Vector2((left + right) * 0.5f, targetZoneRect.anchoredPosition.y);
    }
}