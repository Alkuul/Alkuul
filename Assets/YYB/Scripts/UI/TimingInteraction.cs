using Alkuul.UI.Brewing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimingInteraction : TechniqueInteractionBase
{
    [Header("UI Refs")]
    [SerializeField] private RectTransform timingTrackRect;
    [SerializeField] private RectTransform hitZoneRect;
    [SerializeField] private RectTransform markerRect;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text cancelText;

    private float marker01;
    private int direction = 1;
    private int hitCount;
    private int missCount;

    protected override void OnBegin()
    {
        marker01 = 0f;
        direction = 1;
        hitCount = 0;
        missCount = 0;

        SetupHitZoneVisual();
        UpdateMarkerVisual();

        if (instructionText != null)
            instructionText.text = "마커가 구간에 들어왔을 때 클릭하세요";

        if (cancelText != null)
            cancelText.text = "ESC : 취소";

        RefreshUI(false);
    }

    protected override void TickInteraction(float deltaTime)
    {
        if (timingTrackRect == null || markerRect == null)
            return;

        marker01 += direction * spec.markerSpeed01 * deltaTime;

        if (marker01 >= 1f)
        {
            marker01 = 1f;
            direction = -1;
        }
        else if (marker01 <= 0f)
        {
            marker01 = 0f;
            direction = 1;
        }

        UpdateMarkerVisual();

        if (Input.GetMouseButtonDown(0))
        {
            bool inZone = marker01 >= spec.hitZoneMin01 && marker01 <= spec.hitZoneMax01;

            if (inZone)
            {
                hitCount++;

                if (hitCount >= spec.targetHitCount)
                {
                    float quality = 1f - Mathf.Clamp01((float)missCount / Mathf.Max(1, spec.targetHitCount));
                    Complete(true, quality, 1f, $"Hits={hitCount}, Misses={missCount}");
                    return;
                }
            }
            else
            {
                missCount++;
            }

            RefreshUI(inZone);
        }
    }

    protected override float GetCurrentProgress01()
    {
        if (spec.targetHitCount <= 0)
            return 0f;

        return Mathf.Clamp01((float)hitCount / spec.targetHitCount);
    }

    private void SetupHitZoneVisual()
    {
        if (timingTrackRect == null || hitZoneRect == null)
            return;

        float width = timingTrackRect.rect.width;
        float left = Mathf.Lerp(-width * 0.5f, width * 0.5f, spec.hitZoneMin01);
        float right = Mathf.Lerp(-width * 0.5f, width * 0.5f, spec.hitZoneMax01);

        hitZoneRect.anchorMin = new Vector2(0.5f, 0.5f);
        hitZoneRect.anchorMax = new Vector2(0.5f, 0.5f);
        hitZoneRect.pivot = new Vector2(0.5f, 0.5f);
        hitZoneRect.sizeDelta = new Vector2(right - left, hitZoneRect.sizeDelta.y);
        hitZoneRect.anchoredPosition = new Vector2((left + right) * 0.5f, hitZoneRect.anchoredPosition.y);
    }

    private void UpdateMarkerVisual()
    {
        if (timingTrackRect == null || markerRect == null)
            return;

        float width = timingTrackRect.rect.width;
        float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, marker01);
        markerRect.anchoredPosition = new Vector2(x, markerRect.anchoredPosition.y);
    }

    private void RefreshUI(bool lastHitSuccess)
    {
        if (progressBar != null)
            progressBar.value = GetCurrentProgress01();

        if (progressText != null)
            progressText.text = $"{hitCount} / {spec.targetHitCount}";

        if (statusText != null)
        {
            if (hitCount == 0 && missCount == 0)
                statusText.text = "구간에 맞춰 클릭하세요";
            else
                statusText.text = lastHitSuccess ? "성공!" : "빗나감";
        }
    }
}