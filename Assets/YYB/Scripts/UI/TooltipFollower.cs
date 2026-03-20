using UnityEngine;
using UnityEngine.UI;

public class TooltipFollower : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private Canvas rootCanvas;

    [Header("Position")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(12f, -12f);
    [SerializeField] private float screenPadding = 8f;

    private RectTransform canvasRect;

    private void Awake()
    {
        if (tooltipRect == null)
            tooltipRect = transform as RectTransform;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
            canvasRect = rootCanvas.transform as RectTransform;
    }

    private void LateUpdate()
    {
        UpdateTooltipPosition();
    }

    public void UpdateTooltipPosition()
    {
        if (tooltipRect == null || rootCanvas == null || canvasRect == null)
            return;

        // 텍스트/레이아웃 변경 직후 크기 반영
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

        Camera cam = null;
        if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = rootCanvas.worldCamera;

        // 마우스 스크린 좌표 -> Canvas 로컬 좌표
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            cam,
            out localMousePos
        );

        Vector2 tooltipSize = tooltipRect.rect.size;
        Rect canvasBounds = canvasRect.rect;

        // 기본: 마우스 오른쪽 아래 근처
        Vector2 pivot = new Vector2(0f, 1f);
        Vector2 offset = cursorOffset;

        // 오른쪽으로 넘치면 왼쪽에 띄움
        float predictedRight = localMousePos.x + offset.x + tooltipSize.x;
        if (predictedRight > canvasBounds.xMax - screenPadding)
        {
            pivot.x = 1f;
            offset.x = -Mathf.Abs(cursorOffset.x);
        }
        else
        {
            pivot.x = 0f;
            offset.x = Mathf.Abs(cursorOffset.x);
        }

        // 아래로 넘치면 위쪽 대신 아래쪽 pivot으로 뒤집기
        float predictedBottom = localMousePos.y + offset.y - tooltipSize.y;
        if (predictedBottom < canvasBounds.yMin + screenPadding)
        {
            pivot.y = 0f;
            offset.y = Mathf.Abs(cursorOffset.y);
        }
        else
        {
            pivot.y = 1f;
            offset.y = -Mathf.Abs(cursorOffset.y);
        }

        tooltipRect.pivot = pivot;

        Vector2 targetPos = localMousePos + offset;

        // 최종 clamp
        float minX = canvasBounds.xMin + tooltipSize.x * pivot.x + screenPadding;
        float maxX = canvasBounds.xMax - tooltipSize.x * (1f - pivot.x) - screenPadding;
        float minY = canvasBounds.yMin + tooltipSize.y * pivot.y + screenPadding;
        float maxY = canvasBounds.yMax - tooltipSize.y * (1f - pivot.y) - screenPadding;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        tooltipRect.anchoredPosition = targetPos;
    }
}