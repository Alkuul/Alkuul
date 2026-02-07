using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UIGarnishSlotDragProxy : MonoBehaviour,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IInitializePotentialDragHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Child garnish visual to activate/drag")]
    [SerializeField] private GameObject garnishVisual;
    [SerializeField] private bool hideOnStart = true;

    private UIDraggableIcon draggable;
    private bool isDragging;
    private bool isPointerDown;

    private void Awake()
    {
        if (garnishVisual == null && transform.childCount > 0)
        {
            garnishVisual = transform.GetChild(0).gameObject;
        }

        if (garnishVisual != null)
        {
            draggable = garnishVisual.GetComponent<UIDraggableIcon>();

            if (hideOnStart)
            {
                garnishVisual.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        if (garnishVisual != null)
        {
            garnishVisual.SetActive(false);
        }

        isDragging = false;
        isPointerDown = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ActivateVisual();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        ActivateVisual();
        AssignDragTarget(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        if (!isDragging && garnishVisual != null)
        {
            garnishVisual.SetActive(false);
        }
    }


    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        AssignDragTarget(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ActivateVisual();

        if (draggable == null || garnishVisual == null) return;

        isDragging = true;
        AssignDragTarget(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggable == null) return;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggable == null) return;

        bool droppedOnGlass = IsDroppedOnGlass(eventData);

        isDragging = false;
        isPointerDown = false;
        if (garnishVisual != null)
        {
            garnishVisual.SetActive(false);
        }
    }

    private bool IsDroppedOnGlass(PointerEventData eventData)
    {
        if (eventData == null) return false;

        var target = eventData.pointerCurrentRaycast.gameObject;
        if (target == null) return false;

        return target.GetComponentInParent<GlassDropZone>() != null;
    }

    private void AssignDragTarget(PointerEventData eventData)
    {
        if (eventData == null || garnishVisual == null) return;

        eventData.pointerDrag = garnishVisual;
        eventData.pointerPress = garnishVisual;
    }

    private void ActivateVisual()
    {
        if (garnishVisual == null) return;

        if (!garnishVisual.activeSelf)
        {
            garnishVisual.SetActive(true);
        }

        if (draggable == null)
        {
            draggable = garnishVisual.GetComponent<UIDraggableIcon>();
        }
    }
}