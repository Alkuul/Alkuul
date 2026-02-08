using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Alkuul.Domain;

public class UIDraggablePortrait : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CustomerPortraitView view;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Drag Settings")]
    [SerializeField] private bool dragOnlyWhenWasted = true;

    private RectTransform _rt;
    private Vector2 _startPos;

    // 드롭 결과를 밖으로 전달
    public System.Action OnDroppedToDoor;
    public System.Action OnDroppedToBed;

    // 현재 드래그 모드: "evict" 또는 "sleep" (내쫓기/재우기)
    public string dragMode = "evict";

    private void Awake()
    {
        if (view == null) view = GetComponent<CustomerPortraitView>();
        _rt = GetComponent<RectTransform>();
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (view == null || !view.HasSet) return;
        if (dragOnlyWhenWasted && view.CurrentStage != IntoxStage.Wasted) return;

        _startPos = _rt.anchoredPosition;
        canvasGroup.blocksRaycasts = false; // 드롭존이 레이캐스트 받게
        view.SetDragVisual(dragMode);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvasGroup.blocksRaycasts) return; // BeginDrag 조건 통과 못했으면 무시

        // Canvas 스케일 고려한 이동
        var delta = eventData.delta / rootCanvas.scaleFactor;
        _rt.anchoredPosition += delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup.blocksRaycasts) return;

        canvasGroup.blocksRaycasts = true;

        // 드롭 성공 여부는 DropZone이 호출해줌(아래 UIDropZone)
        // 여기서는 실패 시 원위치
        _rt.anchoredPosition = _startPos;
        view.ClearDragVisual();
    }

    // DropZone에서 호출
    public void NotifyDroppedToDoor()
    {
        OnDroppedToDoor?.Invoke();
    }

    public void NotifyDroppedToBed()
    {
        OnDroppedToBed?.Invoke();
    }
}
