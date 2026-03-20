using Alkuul.Domain.Brewing;
using Alkuul.UI.Brewing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShakeInteraction : TechniqueInteractionBase
{
    [Header("UI Refs")]
    [SerializeField] private RectTransform shakeTargetImage;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text cancelText;

    private Vector2 originalTargetPos;
    private Vector3 lastMousePosition;
    private bool initializedMouse;
    private int lastDirection;
    private int shakeCount;

    protected override void OnBegin()
    {
        shakeCount = 0;
        lastDirection = 0;
        initializedMouse = false;

        if (shakeTargetImage != null)
            originalTargetPos = shakeTargetImage.anchoredPosition;

        if (instructionText != null)
            instructionText.text = "마우스를 누른 채 위아래로 흔들어 쉐이킹을 완료하세요";

        if (cancelText != null)
            cancelText.text = "ESC : 취소";

        RefreshUI();
    }

    protected override void TickInteraction(float deltaTime)
    {
        if (!Input.GetMouseButton(0))
        {
            ReturnTargetToOrigin(deltaTime);
            return;
        }

        Vector3 current = Input.mousePosition;

        if (!initializedMouse)
        {
            lastMousePosition = current;
            initializedMouse = true;
            return;
        }

        float deltaY = current.y - lastMousePosition.y;

        if (spec != null && Mathf.Abs(deltaY) >= spec.minShakeDistance)
        {
            int dir = deltaY > 0f ? 1 : -1;

            if (lastDirection != 0 && dir != lastDirection)
            {
                shakeCount++;
            }

            lastDirection = dir;
            lastMousePosition = current;

            if (shakeTargetImage != null)
            {
                float amount = spec.visualShakeAmount;
                shakeTargetImage.anchoredPosition = originalTargetPos + new Vector2(0f, dir * amount);
            }

            RefreshUI();

            if (GetCurrentProgress01() >= 1f)
            {
                Complete(true, 1f, 1f, $"ShakeCount={shakeCount}");
            }
        }
        else
        {
            ReturnTargetToOrigin(deltaTime);
        }
    }

    protected override float GetCurrentProgress01()
    {
        if (spec == null || spec.targetShakeCount <= 0)
            return 0f;

        return Mathf.Clamp01((float)shakeCount / spec.targetShakeCount);
    }

    protected override void OnCompleted(TechniqueInteractionResult result)
    {
        if (shakeTargetImage != null)
            shakeTargetImage.anchoredPosition = originalTargetPos;

        RefreshUI();
    }

    protected override void OnCancelled()
    {
        if (shakeTargetImage != null)
            shakeTargetImage.anchoredPosition = originalTargetPos;
    }

    private void RefreshUI()
    {
        float progress = GetCurrentProgress01();

        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null && spec != null)
            progressText.text = $"{shakeCount} / {spec.targetShakeCount}";
    }

    private void ReturnTargetToOrigin(float deltaTime)
    {
        if (shakeTargetImage == null)
            return;

        shakeTargetImage.anchoredPosition = Vector2.Lerp(
            shakeTargetImage.anchoredPosition,
            originalTargetPos,
            deltaTime * 10f
        );
    }
}