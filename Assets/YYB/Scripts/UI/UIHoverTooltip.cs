using TMPro;
using UnityEngine;

public class UIHoverTooltip : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform panel;

    [Header("Texts")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text abvText;
    [SerializeField] private TMP_Text emotionText;

    [Header("Follow")]
    [SerializeField] private Vector2 screenOffset = new Vector2(16f, -16f);
    [SerializeField] private bool followMouse = true;

    private Canvas rootCanvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        if (panel == null) panel = GetComponent<RectTransform>();

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null) canvasRect = rootCanvas.GetComponent<RectTransform>();

        // 툴팁이 레이캐스트 막으면 호버가 끊길 수 있으니 차단 해제
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        Hide();
    }

    private void Update()
    {
        if (!followMouse || !gameObject.activeSelf) return;
        if (rootCanvas == null || canvasRect == null) return;

        Vector2 screenPos = (Vector2)Input.mousePosition + screenOffset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out var localPos
        );

        panel.anchoredPosition = localPos;
    }

    public void Show(string name, string abv, string emotions)
    {
        if (nameText != null) nameText.text = name ?? "";
        if (abvText != null) abvText.text = abv ?? "";
        if (emotionText != null) emotionText.text = emotions ?? "";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}