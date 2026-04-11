using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("If true, icon center sticks to pointer")]
    [SerializeField] private bool snapCenterToPointer = false;
    [SerializeField] private bool hideSelfOnEndDrag = false;

    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private RectTransform rt;
    private CanvasGroup cg;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector2 originalAnchoredPosition;

    // 포인터(캔버스 로컬)와 아이콘(localPosition) 사이 오프셋
    private Vector2 pointerToIconOffset;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        canvasRect = rootCanvas.GetComponent<RectTransform>();

        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalAnchoredPosition = rt.anchoredPosition;

        // 1) 드래그 중 다른 UI에 가려지지 않게 캔버스 최상단으로(현재 위치 유지)
        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        cg.blocksRaycasts = false;

        // 2) 포인터 위치(캔버스 로컬) 계산
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var pointerLocal);

        // 3) 현재 아이콘 위치(캔버스 로컬) 기준으로 오프셋 계산
        // rt.localPosition은 Vector3이지만 2D로 사용
        var iconLocal = (Vector2)rt.localPosition;

        pointerToIconOffset = snapCenterToPointer ? Vector2.zero : (iconLocal - pointerLocal);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var pointerLocal);

        rt.localPosition = pointerLocal + pointerToIconOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;

        // 항상 원래 슬롯으로 복귀
        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);

        // 슬롯 중앙에 스냅
        rt.anchoredPosition = originalAnchoredPosition;

        // 가니쉬 프록시에서 쓰는 자식 비주얼이면 드래그 종료 후 다시 숨김
        if (hideSelfOnEndDrag)
        {
            gameObject.SetActive(false);
        }
    }
}
