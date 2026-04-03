using Alkuul.Domain.Brewing;
using Alkuul.UI.Brewing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StirInteraction : TechniqueInteractionBase
{
    [Header("UI Refs")]
    [SerializeField] private RectTransform centerPoint;
    [SerializeField] private RectTransform stirVisual;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text cancelText;

    private Canvas rootCanvas;
    private bool hasPrevDir;
    private Vector2 prevDir;
    private float accumulatedAngle;

    private Vector2 originalVisualPos;
    private Vector2 targetVisualPos;

    protected override void OnInitialized()
    {
        rootCanvas = GetComponentInParent<Canvas>();
    }

    protected override void OnBegin()
    {
        hasPrevDir = false;
        accumulatedAngle = 0f;

        if (stirVisual != null)
        {
            originalVisualPos = stirVisual.anchoredPosition;
            targetVisualPos = originalVisualPos;
        }

        if (instructionText != null)
            instructionText.text = "마우스를 누른 채 원을 그리며 저어주세요";

        if (cancelText != null)
            cancelText.text = "ESC : 취소";

        RefreshUI();
    }

    protected override void TickInteraction(float deltaTime)
    {
        if (!Input.GetMouseButton(0))
        {
            hasPrevDir = false;
            ReturnVisualToOrigin(deltaTime);
            return;
        }

        if (centerPoint == null || stirVisual == null || spec == null)
            return;

        Camera cam = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = rootCanvas.worldCamera;

        Vector2 centerScreenPos = RectTransformUtility.WorldToScreenPoint(cam, centerPoint.position);
        Vector2 mouse = Input.mousePosition;
        Vector2 dir = mouse - centerScreenPos;

        if (dir.magnitude < spec.stirRadiusMin)
            return;

        dir.Normalize();

        // ----- 시각 표현: StirVisual을 중심점 주변 원 둘레로 이동 -----
        Vector2 centerAnchored = centerPoint.anchoredPosition;
        targetVisualPos = centerAnchored + dir * spec.visualOrbitRadius;

        stirVisual.anchoredPosition = Vector2.Lerp(
            stirVisual.anchoredPosition,
            targetVisualPos,
            deltaTime * spec.visualFollowSpeed
        );

        // ----- 판정: 방향 변화 누적 -----
        if (!hasPrevDir)
        {
            prevDir = dir;
            hasPrevDir = true;
            return;
        }

        float angle = Vector2.SignedAngle(prevDir, dir);
        float absAngle = Mathf.Abs(angle);

        if (absAngle > 0.01f && absAngle <= spec.maxAcceptedAngleStep)
        {
            accumulatedAngle += absAngle;
            RefreshUI();

            if (GetCurrentProgress01() >= 1f)
            {
                Complete(true, 1f, 1f, $"AccumulatedAngle={accumulatedAngle:F1}");
                return;
            }
        }

        prevDir = dir;
    }

    protected override float GetCurrentProgress01()
    {
        if (spec == null || spec.targetRotationAmount <= 0f)
            return 0f;

        return Mathf.Clamp01(accumulatedAngle / spec.targetRotationAmount);
    }

    protected override void OnCompleted(TechniqueInteractionResult result)
    {
        RefreshUI();
    }

    protected override void OnCancelled()
    {
        ResetVisualImmediate();
    }

    private void RefreshUI()
    {
        float progress = GetCurrentProgress01();

        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null && spec != null)
            progressText.text = $"{accumulatedAngle:F0} / {spec.targetRotationAmount:F0}";
    }

    private void ReturnVisualToOrigin(float deltaTime)
    {
        if (stirVisual == null)
            return;

        stirVisual.anchoredPosition = Vector2.Lerp(
            stirVisual.anchoredPosition,
            originalVisualPos,
            deltaTime * 10f
        );
    }

    private void ResetVisualImmediate()
    {
        if (stirVisual == null)
            return;

        stirVisual.anchoredPosition = originalVisualPos;
    }
}