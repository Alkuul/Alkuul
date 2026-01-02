using UnityEngine;
using Alkuul.Domain;

public class Bottle : MonoBehaviour
{
    [Header("Data")]
    public IngredientSO ingredient; // 교체: AlcoholData -> IngredientSO

    private Vector3 originalPosition;
    private bool isHeld = false;
    private Camera mainCam;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    void Start()
    {
        originalPosition = transform.position;
        mainCam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer.sortingOrder;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isHeld)
            {
                HandleInteraction();
            }
            else
            {
                if (IsMouseOver())
                    PickUp();
            }
        }

        if (isHeld)
            MoveWithMouse();
    }

    void PickUp()
    {
        isHeld = true;
        spriteRenderer.sortingOrder = 100;
    }

    void MoveWithMouse()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;
    }

    void HandleInteraction()
    {
        if (ingredient == null)
        {
            Debug.LogWarning("Bottle: ingredient가 비어있습니다(IngredientSO를 할당하세요).");
            ReturnToOriginalPosition();
            return;
        }

        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == this.gameObject) continue;

            if (hit.collider.TryGetComponent(out Jigger jigger))
            {
                bool success = jigger.FillFromBottle(ingredient);
                if (success)
                {
                    Debug.Log($"{ingredient.displayName}을(를) 지거에 따랐습니다.");
                    ReturnToOriginalPosition();
                    return;
                }
            }
        }

        ReturnToOriginalPosition();
    }

    void ReturnToOriginalPosition()
    {
        isHeld = false;
        transform.position = originalPosition;
        spriteRenderer.sortingOrder = originalSortingOrder;
    }

    bool IsMouseOver()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);
        return hitCollider != null && hitCollider.gameObject == this.gameObject;
    }
}
