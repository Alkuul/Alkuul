using Alkuul.UI.Brewing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RollingInteraction : TechniqueInteractionBase
{
    [Header("UI Refs")]
    [SerializeField] private RectTransform pointA;
    [SerializeField] private RectTransform pointB;
    [SerializeField] private RectTransform rollingVisual;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text cancelText;

    private Canvas rootCanvas;
    private RectTransform visualParentRect;
    private bool nextTargetIsB;
    private int passCount;
    private Vector2 startVisualPos;

    protected override void OnInitialized()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (rollingVisual != null)
            visualParentRect = rollingVisual.parent as RectTransform;
    }

    protected override void OnBegin()
    {
        passCount = 0;
        nextTargetIsB = true;

        if (pointA != null && rollingVisual != null)
        {
            startVisualPos = pointA.anchoredPosition;
            rollingVisual.anchoredPosition = startVisualPos;
        }

        if (instructionText != null)
            instructionText.text = "마우스를 누른 채 A와 B를 왕복하며 롤링하세요";

        if (cancelText != null)
            cancelText.text = "ESC : 취소";

        RefreshUI();
    }

    protected override void TickInteraction(float deltaTime)
    {
        if (!Input.GetMouseButton(0))
            return;

        if (rollingVisual == null || visualParentRect == null || pointA == null || pointB == null)
            return;

        Camera cam = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = rootCanvas.worldCamera;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            visualParentRect,
            Input.mousePosition,
            cam,
            out localMousePos
        );

        rollingVisual.anchoredPosition = Vector2.Lerp(
            rollingVisual.anchoredPosition,
            localMousePos,
            deltaTime * spec.rollingFollowSpeed
        );

        RectTransform target = nextTargetIsB ? pointB : pointA;

        if (Vector2.Distance(rollingVisual.anchoredPosition, target.anchoredPosition) <= spec.pointDetectRadius)
        {
            passCount++;
            nextTargetIsB = !nextTargetIsB;
            RefreshUI();

            if (GetCurrentProgress01() >= 1f)
            {
                Complete(true, 1f, 1f, $"PassCount={passCount}");
            }
        }
    }

    protected override float GetCurrentProgress01()
    {
        if (spec.requiredRollPassCount <= 0)
            return 0f;

        return Mathf.Clamp01((float)passCount / spec.requiredRollPassCount);
    }

    protected override void OnCancelled()
    {
        if (rollingVisual != null)
            rollingVisual.anchoredPosition = startVisualPos;
    }

    private void RefreshUI()
    {
        float progress = GetCurrentProgress01();

        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"{passCount} / {spec.requiredRollPassCount}";

        if (statusText != null)
            statusText.text = nextTargetIsB ? "다음 목표: B" : "다음 목표: A";
    }
}